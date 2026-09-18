using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// Adds a button that chooses the folder this asset tracks.  Once a folder is set, AudioResourcesWatcher
// keeps the clips up to date, so the button is only needed when the folder changes.
// The dictionary itself needs no custom UI: the built-in serialized Dictionary editor draws it as a
// two-column list, laid out by the [DictionaryDisplay] attribute on the field.
[CustomEditor(typeof(AudioResources))]
public class AudioResourcesEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);
        root.Add(new Button(OnSelectFolderClicked) { text = "Track a Directory of Audio Clips" });
        return root;
    }

    private void OnSelectFolderClicked()
    {
        var inspectedObject = (AudioResources)target;

        string scriptObjectAssetPath = AssetDatabase.GetAssetPath(inspectedObject);
        string defaultPath = scriptObjectAssetPath.Substring(0, scriptObjectAssetPath.LastIndexOf('/'));

        string path = EditorUtility.OpenFolderPanel("Select a Directory", defaultPath, "");
        if (string.IsNullOrEmpty(path))
            return;

        // The directory picker returns absolute paths, but AssetDatabase requires project relative.
        string directory = FileUtil.GetProjectRelativePath(path);
        if (string.IsNullOrEmpty(directory))
        {
            Debug.LogWarning($"Selected folder {path} is not inside the project");
            return;
        }

        Undo.RecordObject(inspectedObject, "Track a directory of audio clips");
        inspectedObject.clipFolder = directory;
        inspectedObject.PopulateFromFolder();
        AssetDatabase.SaveAssetIfDirty(inspectedObject);
    }
}
