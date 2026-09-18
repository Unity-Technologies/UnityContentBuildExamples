using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;

// Runs during player builds to copy the built content directories and their support files into the
// player's StreamingAssets folder.
class ContentDirectoryDeployment : BuildPlayerProcessor
{
    // A Web build preloads the files listed by every manifest in this folder into its virtual file system.
    const string k_WebPreloadFolder = "Library/PlayerDataCache/WebGLPreloadedStreamingAssets";
    const string k_WebPreloadManifest = k_WebPreloadFolder + "/content-directories.manifest";

    public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
    {
        var buildsPath = ContentDirectoryManager.ContentBuildsPath;

        // Skip if no content directories exist, so other builds in the same project can proceed without them.
        if (!Directory.Exists(buildsPath))
            return;

        var directories = Directory.GetDirectories(buildsPath);
        if (directories.Length == 0)
            return;

        // Sorted for a deterministic registration order.
        Array.Sort(directories, StringComparer.Ordinal);

        // The listing is required on platforms that cannot enumerate the StreamingAssets folder through the
        // regular file system.  For simplicity every platform uses it.
        WriteListing(buildsPath, directories);

        // Unity copies the contents of buildsPath into the root of StreamingAssets, so each content directory
        // keeps its own folder name and the listing file keeps its fixed name.
        buildPlayerContext.AddAdditionalPathToStreamingAssets(buildsPath);

        // Preserve the types used by the content so that managed code stripping does not remove them.
        foreach (var directory in directories)
        {
            if (BuildHistory.TryGetBuildSummaryForOutputPath(directory, out var summary) &&
                BuildHistory.TryGetBuildReportDirectory(summary.BuildSessionGUID, out var reportDirectory))
                buildPlayerContext.AddPreviousBuildReportDirectory(reportDirectory);
        }

        if (buildPlayerContext.BuildPlayerOptions.target == BuildTarget.WebGL)
            WriteWebPreloadManifest(buildsPath, directories);
        else if (File.Exists(k_WebPreloadManifest))
            File.Delete(k_WebPreloadManifest);
    }

    // Writes one content directory name per line.
    static void WriteListing(string buildsPath, string[] directories)
    {
        var directoryNames = new List<string>();
        foreach (var path in directories)
            directoryNames.Add(Path.GetFileName(path));

        File.WriteAllLines(buildsPath + "/" + ContentDirectoryManager.ListingFileName, directoryNames);
    }

    // The Web platform serves StreamingAssets over HTTP, which a browser cannot read synchronously.
    // RegisterContentDirectory needs synchronous access, so every file is listed for the build to preload.
    static void WriteWebPreloadManifest(string buildsPath, string[] directories)
    {
        Directory.CreateDirectory(k_WebPreloadFolder);

        using var writer = new StreamWriter(k_WebPreloadManifest, false);
        foreach (var directory in directories)
        {
            var directoryName = Path.GetFileName(directory);
            foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                var relativePath = directoryName + "/" + Path.GetRelativePath(directory, file);
                writer.WriteLine(relativePath.Replace('\\', '/'));
            }
        }

        // The listing file itself is read the same way.
        writer.WriteLine(ContentDirectoryManager.ListingFileName);
    }
}
