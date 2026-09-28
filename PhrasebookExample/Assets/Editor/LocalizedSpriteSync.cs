using System.Collections.Generic;
using System.IO;
using Unity.Loading;
using UnityEditor;
using UnityEngine;

// Keeps one LocalizedSprite asset per phrase folder in step with the card images in that folder.
//
// The convention is Assets/Cards/<Phrase>/<Phrase>.<lang>.png, and the asset is written next to the
// images as <Phrase>.asset.  Runs after every import that touches a card image, and from the menu for
// a full pass.  It also warns about a missing or unknown language, which is the point where a gap in
// the localized content becomes visible.
public class LocalizedSpriteSync : AssetPostprocessor
{
    public const string CardsFolder = "Assets/Cards";

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var folders = new HashSet<string>();
        CollectCardFolders(importedAssets, folders);
        CollectCardFolders(deletedAssets, folders);
        CollectCardFolders(movedAssets, folders);
        CollectCardFolders(movedFromAssetPaths, folders);

        if (folders.Count == 0)
            return;

        foreach (var folder in folders)
            if (AssetDatabase.IsValidFolder(folder))
                SyncFolder(folder);

        AssetDatabase.SaveAssets();
    }

    // The postprocessor only runs when a card image is imported, so it cannot see a change to
    // LanguageSetting.Available.  Run this after adding or removing a language there, so every
    // LocalizedSprite is checked against the new list and the warnings show what is missing.
    [MenuItem("Example/Sync Localized Sprites")]
    public static void SyncAll()
    {
        foreach (var folder in AssetDatabase.GetSubFolders(CardsFolder))
            SyncFolder(folder);

        AssetDatabase.SaveAssets();
    }

    static void CollectCardFolders(string[] paths, HashSet<string> folders)
    {
        foreach (var path in paths)
            if (path.StartsWith(CardsFolder + "/") && path.EndsWith(".png"))
                folders.Add(Path.GetDirectoryName(path).Replace('\\', '/'));
    }

    static void SyncFolder(string folder)
    {
        var phraseId = Path.GetFileName(folder);
        var assetPath = $"{folder}/{phraseId}.asset";

        var asset = AssetDatabase.LoadAssetAtPath<LocalizedSprite>(assetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<LocalizedSprite>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        asset.languages.Clear();

        foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { folder }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var parts = Path.GetFileNameWithoutExtension(path).Split('.');

            if (parts.Length != 2 || parts[0] != phraseId || !LanguageSetting.IsAvailable(parts[1]))
            {
                Debug.LogWarning($"{path} does not follow the {phraseId}.<language>.png convention and was skipped");
                continue;
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var id = LoadableObjectIdEditorUtility.CreateLoadableObjectId(sprite);
            asset.languages[parts[1]] = new Loadable<Sprite>(id);
        }

        foreach (var language in LanguageSetting.Available)
            if (!asset.languages.ContainsKey(language.Code))
                Debug.LogWarning($"{phraseId} has no card for '{language.Code}'; the fallback language will be shown instead");

        EditorUtility.SetDirty(asset);
    }
}
