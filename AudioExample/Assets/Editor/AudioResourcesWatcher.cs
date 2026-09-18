using UnityEditor;
using UnityEngine;

// Refills every AudioResources asset whose tracked folder changed, so its clips never fall behind the
// audio clips on disk.  Once a folder is chosen in the Inspector, adding, removing, renaming or moving
// a clip inside it is enough: no build step and no button press.
public class AudioResourcesWatcher : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths, bool didDomainReload)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:AudioResources"))
        {
            var audioResources = AssetDatabase.LoadAssetAtPath<AudioResources>(AssetDatabase.GUIDToAssetPath(guid));
            if (string.IsNullOrEmpty(audioResources.clipFolder))
                continue;

            // Moving or renaming the tracked folder leaves the clips inside it, so follow the folder
            // rather than leaving the asset pointed at a path that no longer exists.
            var folder = FollowMove(audioResources.clipFolder, movedAssets, movedFromAssetPaths);

            if (folder == audioResources.clipFolder &&
                !HasClipChange(folder, importedAssets) && !HasClipChange(folder, deletedAssets) &&
                !HasClipChange(folder, movedAssets) && !HasClipChange(folder, movedFromAssetPaths))
                continue;

            audioResources.clipFolder = folder;
            audioResources.PopulateFromFolder();

            // Saving reimports the asset, which runs this method again with the asset's own path.  That
            // path is not an audio clip inside the tracked folder, so the second run stops above.
            AssetDatabase.SaveAssetIfDirty(audioResources);
        }
    }

    static bool HasClipChange(string folder, string[] changedPaths)
    {
        foreach (var path in changedPaths)
        {
            if (path.StartsWith(folder + "/") && MayBeClip(path))
                return true;
        }

        return false;
    }

    // A deleted or moved-away asset has no type left to check, so only a path that still resolves to
    // something other than an audio clip is ruled out.
    static bool MayBeClip(string path)
    {
        var type = AssetDatabase.GetMainAssetTypeAtPath(path);
        return type == null || type == typeof(AudioClip);
    }

    // Maps a folder through the moves in this import batch, including a move of one of its parents.
    static string FollowMove(string folder, string[] movedAssets, string[] movedFromAssetPaths)
    {
        for (int i = 0; i < movedFromAssetPaths.Length; i++)
        {
            if (folder == movedFromAssetPaths[i])
                return movedAssets[i];

            if (folder.StartsWith(movedFromAssetPaths[i] + "/"))
                return movedAssets[i] + folder.Substring(movedFromAssetPaths[i].Length);
        }

        return folder;
    }
}
