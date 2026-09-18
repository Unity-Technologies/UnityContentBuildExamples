using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ContentDirectoryProfile))]
internal sealed class ContentDirectoryProfileEditor : Editor
{
    static readonly string kProjectRootPath = Directory.GetParent(Application.dataPath).FullName;

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);
        root.Add(new Button(OnBrowseButtonClicked) { text = "Browse for Output Path" });
        root.Add(new Button(OnBuildButtonClicked) { text = "Build Content Directory" });
        return root;
    }

    // Perform a content directory build with the build configuration stored on the asset.
    private void OnBuildButtonClicked()
    {
        var profile = (ContentDirectoryProfile)target;

        if (!string.IsNullOrEmpty(profile.outputPath))
        {
            profile.BuildContentDirectory();
            return;
        }

        string oneTimeOutputPath = EditorUtility.SaveFolderPanel("Choose Content Directory output location", kProjectRootPath, "");
        if (!string.IsNullOrEmpty(oneTimeOutputPath))
            profile.BuildContentDirectory(oneTimeOutputPath);
    }

    private void OnBrowseButtonClicked()
    {
        var profile = (ContentDirectoryProfile)target;
        string displayPath = Directory.Exists(profile.outputPath) ? profile.outputPath : kProjectRootPath;

        string path = EditorUtility.SaveFolderPanel("Choose Content Directory output location", displayPath, "");
        if (string.IsNullOrEmpty(path))
            return;

        // Keep the path project relative when it points inside the project, with forward slashes so the
        // stored path works on every platform.
        var relative = Path.GetRelativePath(kProjectRootPath, path);
        var outputPath = relative.StartsWith("..") ? path : relative;

        Undo.RecordObject(profile, "Modify ContentDirectoryProfile output path");
        profile.outputPath = outputPath.Replace('\\', '/');
        EditorUtility.SetDirty(profile);
    }
}
