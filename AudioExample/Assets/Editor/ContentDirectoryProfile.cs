using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Object = UnityEngine.Object;

// A persisted definition for a content directory build, similar to a Build Profile.  Assets of this type
// are edited in the Inspector, which can also launch the build directly without use of a build script.
// Note: Builds always use the platform selected by the current build profile settings.
// Note: An alternative to using an asset like this is for a custom build script to determine the
// arguments to BuildContentDirectory() on its own, e.g. potentially hard-coding or programmatically determining
// the root assets.
[CreateAssetMenu(fileName = "ContentDirectoryProfile", menuName = "Scriptable Objects/Content Directory Profile")]
public sealed class ContentDirectoryProfile : ScriptableObject
{
    [SerializeField]
    internal string outputPath;
    [SerializeField]
    internal List<Object> rootAssets;
    [SerializeField]
    internal BuildContentOptions options;
    [SerializeField]
    internal CompressionType compressionType;
    [SerializeField]
    internal string[] extraScriptingDefines;

    internal BuildCompression compression => compressionType switch
    {
        // Note: LZMA is not recommended for content directories - when used the archive
        // will need to be fully decompressed to memory prior to loading anything.
        CompressionType.Lzma => BuildCompression.LZMA,
        CompressionType.Lz4 => BuildCompression.LZ4Runtime,
        CompressionType.Lz4HC => BuildCompression.LZ4,
        _ => BuildCompression.Uncompressed,
    };

    public BuildReport BuildContentDirectory(string oneTimeOutputPath = null)
    {
        return BuildPipeline.BuildContentDirectory(new BuildContentDirectoryParameters
        {
            outputPath = oneTimeOutputPath ?? outputPath,
            rootAssetPaths = rootAssets.ConvertAll(AssetDatabase.GetAssetPath).ToArray(),
            options = options,
            compression = compression,
            extraScriptingDefines = extraScriptingDefines,
            name = name,
        });
    }
}
