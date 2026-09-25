using System.IO;
using UnityEditor;
using UnityEditor.Build;

// Runs at the start of every player build: builds the content directory for the same platform, then
// adds it to the player's StreamingAssets folder.  A player therefore always ships with content that
// matches it, whether the build was started from the Build Profiles window or from the Example menu.
class ContentDirectoryDeployment : BuildPlayerProcessor
{
    // A Web build preloads the files listed by every manifest in this folder into its virtual file system.
    const string k_WebPreloadFolder = "Library/PlayerDataCache/WebGLPreloadedStreamingAssets";
    const string k_WebPreloadManifest = k_WebPreloadFolder + "/phrasebook.manifest";

    public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
    {
        var report = BuildAll.BuildContentDirectory();

        // Unity copies the contents of this folder into the root of StreamingAssets, so the content
        // directory arrives as StreamingAssets/Phrasebook, which is where CatalogProvider looks.
        buildPlayerContext.AddAdditionalPathToStreamingAssets(BuildAll.ContentBuildsPath);

        // Preserve the types used by the content so that managed code stripping does not remove them.
        if (BuildHistory.TryGetBuildReportDirectory(report.summary.buildSessionGuid, out var reportDirectory))
            buildPlayerContext.AddPreviousBuildReportDirectory(reportDirectory);

        if (buildPlayerContext.BuildPlayerOptions.target == BuildTarget.WebGL)
            WriteWebPreloadManifest();
        else if (File.Exists(k_WebPreloadManifest))
            File.Delete(k_WebPreloadManifest);
    }

    // The Web platform serves StreamingAssets over HTTP, which a browser cannot read synchronously.
    // RegisterContentDirectory needs synchronous access, so every file is listed for the build to preload.
    static void WriteWebPreloadManifest()
    {
        Directory.CreateDirectory(k_WebPreloadFolder);

        using var writer = new StreamWriter(k_WebPreloadManifest, false);
        foreach (var file in Directory.GetFiles(BuildAll.ContentDirectoryPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = CatalogProvider.ContentDirectoryName + "/" + Path.GetRelativePath(BuildAll.ContentDirectoryPath, file);
            writer.WriteLine(relativePath.Replace('\\', '/'));
        }
    }
}
