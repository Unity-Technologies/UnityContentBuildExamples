using Unity.Loading;
using UnityEngine;

// Hands out the PhrasebookCatalog root asset.  This is the only code that differs between the Editor
// and a player.
//
// In Play mode the catalog is the asset in the project, loaded through the AssetDatabase, and the
// Loadable fields reached from it resolve to the project's assets.  No content directory build is
// needed to run the example in the Editor.
//
// In a player the catalog comes from the content directory that the player build copied into
// StreamingAssets, which is registered here and unregistered in Release.
public static class CatalogProvider
{
    public const string ContentDirectoryName = "Phrasebook";

#if UNITY_EDITOR
    const string k_CatalogAssetPath = "Assets/RootAssets/PhrasebookCatalog.asset";

    public static PhrasebookCatalog Get()
    {
        var catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<PhrasebookCatalog>(k_CatalogAssetPath);
        if (catalog == null)
            Debug.LogError($"No PhrasebookCatalog at {k_CatalogAssetPath}");
        return catalog;
    }

    public static void Release()
    {
        // Content loaded through the AssetDatabase is not reference counted, so there is nothing to do.
    }
#else
    static ContentDirectoryHandle s_Handle;

    static string ContentDirectoryPath =>
#if UNITY_WEBGL
        // Preloaded StreamingAssets files are in the Emscripten virtual file system at this fixed path.
        "/vfs_streamingassets/" + ContentDirectoryName;
#else
        Application.streamingAssetsPath + "/" + ContentDirectoryName;
#endif

    public static PhrasebookCatalog Get()
    {
        s_Handle = ContentLoadManager.RegisterContentDirectory(ContentDirectoryPath);

        var catalogs = ContentLoadManager.GetRootAssets<PhrasebookCatalog>();
        if (catalogs.Length == 0)
        {
            Debug.LogError($"No PhrasebookCatalog root asset in {ContentDirectoryPath}");
            return null;
        }

        return catalogs[0];
    }

    public static void Release()
    {
        ContentLoadManager.UnregisterContentDirectory(s_Handle);
    }
#endif
}
