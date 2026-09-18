using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Unity.Loading;
using UnityEngine;

// Note: this example project is designed so it is not necessary to use a custom build script.
// However, these simple build scripts are added to support batchmode builds and agentic access.
static class BuildAll
{
    const string k_ProfileFolder = "Assets/Editor/ContentBuilds";
    const string k_PlayerOutputDirectory = "Build/Player";
    const string k_AppName = "AudioExample";
    const string k_BootstrapScene = "Assets/Scenes/Bootstrap.unity";

    [MenuItem("Example/Build Content Directories")]
    public static void BuildContentDirectories()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:ContentDirectoryProfile", new[] { k_ProfileFolder }))
        {
            var profilePath = AssetDatabase.GUIDToAssetPath(guid);
            var profile = AssetDatabase.LoadAssetAtPath<ContentDirectoryProfile>(profilePath);

            Debug.Log($"Building content directory from {profilePath}");
            var report = profile.BuildContentDirectory();
            if (report == null || report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Content directory build failed for {profilePath}");
        }
    }

    [MenuItem("Example/Build Player")]
    public static void BuildPlayer()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var options = new BuildPlayerOptions
        {
            scenes = new[] { k_BootstrapScene },
            locationPathName = CreateOutputPath(target),
            target = target,
            options = BuildOptions.Development // Development player, remove for release builds
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report == null || report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Player build failed");

        Debug.Log($"Built player to {options.locationPathName}");
    }

    static string CreateOutputPath(BuildTarget target)
    {
        if (!Directory.Exists(k_PlayerOutputDirectory))
            Directory.CreateDirectory(k_PlayerOutputDirectory);

        var path = $"{k_PlayerOutputDirectory}/{k_AppName}";

        // See "Build path requirements for target platforms" in the Unity Manual
        if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows)
            path += ".exe";
        else if (target == BuildTarget.StandaloneOSX)
            path += ".app";
        else if (target == BuildTarget.StandaloneLinux64)
            path += ".x86_64";

        return path;
    }

    // Content directories are built first so that the player build can pick them up from
    // the Build folder and include them in the Player StreamingAssets.
    [MenuItem("Example/Build Everything")]
    public static void BuildEverything()
    {
        BuildContentDirectories();
        BuildPlayer();
    }

    // Useful after a Play mode session that left content registered.  Logs a warning for any
    // Loadable that is still loaded.
    [MenuItem("Example/Close All ContentDirectories")]
    public static void CloseAllContentDirectories()
    {
        var handles = ContentLoadManager.GetContentDirectories();
        foreach (var handle in handles)
            ContentLoadManager.UnregisterContentDirectory(handle);

        Debug.Log($"Closed {handles.Length} content directories");
    }
}
