using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AudioResources", menuName = "Scriptable Objects/AudioResources")]
public class AudioResources : ScriptableObject
{
#if UNITY_EDITOR
    // The project relative folder that fills the clips below.  Editor only, because at runtime the
    // clips are all that matter and the folder they were gathered from is no longer part of the build.
    public string clipFolder;
#endif

    // A serialized Dictionary configured to draw a two-column name/clip editor in the Inspector.
    [SerializeField]
    [DictionaryDisplay(keyLabel = "Name", valueLabel = "Audio Clip")]
    public Dictionary<string, Loadable<AudioClip>> clips = new();

#if UNITY_EDITOR

    // Replaces the clips with every AudioClip found in clipFolder, keyed by asset name.  Saving is left
    // to the caller, because the folder watcher runs during an import where saving everything is too broad.
    public void PopulateFromFolder()
    {
        if (!AssetDatabase.IsValidFolder(clipFolder))
        {
            Debug.LogWarning($"{name} tracks {clipFolder}, which is not a folder in this project");
            return;
        }

        clips.Clear();

        foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { clipFolder }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            var id = LoadableObjectIdEditorUtility.CreateLoadableObjectId(clip);

            if (!clips.TryAdd(clip.name, new Loadable<AudioClip>(id)))
                // FindAssets searches subfolders, so two clips can arrive with the same name.
                Debug.LogWarning($"{path} has the same name as an earlier clip, so it was skipped");
        }

        Debug.Log($"Tracked {clips.Count} clips from {clipFolder} in {AssetDatabase.GetAssetPath(this)}");

        EditorUtility.SetDirty(this);
    }
#endif
}
