using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Custom build script to build the example.  The build profiles UI could also be used
// this scripts builds with recommended build settings and is convenient for testing from the CLI.
static class BuildAll
{
    public const string RootAssetPath = "Assets/RootAssets/PhrasebookCatalog.asset";

    // Content directory builds are written outside the Assets folder so Unity does not import them.
    // The player build copies the contents of this folder into StreamingAssets, so the content
    // directory keeps its folder name there.
    public const string ContentBuildsPath = "Build/ContentDirectoryBuilds";
    public const string ContentDirectoryPath = ContentBuildsPath + "/" + CatalogProvider.ContentDirectoryName;

    const string k_PlayerOutputDirectory = "Build/Player";
    const string k_AppName = "PhrasebookExample";
    const string k_Scene = "Assets/Scenes/Phrasebook.unity";

    [MenuItem("Example/Build Player")]
    public static void BuildPlayer()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var options = new BuildPlayerOptions
        {
            scenes = new[] { k_Scene },
            locationPathName = CreatePlayerOutputPath(target),
            target = target,
            options = BuildOptions.Development, // Development player, remove for release builds
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report == null || report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Player build failed");

        Debug.Log($"Built player to {options.locationPathName}");
    }

    // Called automatically during the player build.
    public static BuildReport BuildContentDirectory()
    {
        Directory.CreateDirectory(ContentDirectoryPath);

        var report = BuildPipeline.BuildContentDirectory(new BuildContentDirectoryParameters
        {
            outputPath = ContentDirectoryPath,
            rootAssetPaths = new[] { RootAssetPath },
            name = CatalogProvider.ContentDirectoryName,
        });

        if (report == null || report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Content directory build failed");

        Debug.Log($"Built content directory to {ContentDirectoryPath}");
        return report;
    }

    static string CreatePlayerOutputPath(BuildTarget target)
    {
        Directory.CreateDirectory(k_PlayerOutputDirectory);

        var path = $"{k_PlayerOutputDirectory}/{k_AppName}";

        // See "Build path requirements for target platforms" in the Unity Manual.
        if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows)
            path += ".exe";
        else if (target == BuildTarget.StandaloneOSX)
            path += ".app";
        else if (target == BuildTarget.StandaloneLinux64)
            path += ".x86_64";

        return path;
    }
}
