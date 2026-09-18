using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Loading;
using UnityEngine;
using UnityEngine.Networking;

// Registers every content directory that the content build produced, and unregisters them again on
// shutdown.
public class ContentDirectoryManager
{
    // Build output location for the content directories, relative to the project folder.  It is outside
    // the Assets folder so Unity does not import the built content.
    public const string ContentBuildsPath = "Build/ContentDirectoryBuilds";

    // Fixed name for the listing file, so that a player can find it without enumerating the
    // StreamingAssets folder.  Necessary on platforms like Android and Web, where StreamingAssets is not
    // a local file system path.
    public const string ListingFileName = "content-directories.txt";

    readonly List<ContentDirectoryHandle> handles = new();

    // In play mode the content directories are read from the build output location.  In a player they
    // are read from the StreamingAssets folder, where the player build copied them.
    //
    // An alternative for play mode would be to skip content directories and use the Editor project's
    // assets directly, e.g. the AudioResources ScriptableObjects.  This example instead loads content
    // directories in both play mode and the player, so the two behave the same way.
    public static string ContentRootPath
    {
        get
        {
#if UNITY_EDITOR
            return ContentBuildsPath;
#elif UNITY_WEBGL
            // Preloaded StreamingAssets files are in the Emscripten virtual file system at this fixed path.
            return "/vfs_streamingassets";
#else
            return Application.streamingAssetsPath;
#endif
        }
    }

    public IEnumerator RegisterAll()
    {
        var directoryNames = new List<string>();
        yield return LoadDirectoryNames(directoryNames);

        foreach (var directoryName in directoryNames)
            handles.Add(ContentLoadManager.RegisterContentDirectory(ContentRootPath + "/" + directoryName));
    }

    public void UnregisterAll()
    {
        for (int i = handles.Count - 1; i >= 0; i--)
            ContentLoadManager.UnregisterContentDirectory(handles[i]);

        handles.Clear();
    }

    static IEnumerator LoadDirectoryNames(List<string> directoryNames)
    {
#if UNITY_EDITOR
        if (!Directory.Exists(ContentBuildsPath))
        {
            Debug.LogWarning($"No content directories in {ContentBuildsPath}.  Build the content directories first.");
            yield break;
        }

        foreach (var path in Directory.GetDirectories(ContentBuildsPath))
            directoryNames.Add(Path.GetFileName(path));
#else
        var listingPath = ContentRootPath + "/" + ListingFileName;
        string listing;

#if UNITY_ANDROID
        // On Android the StreamingAssets folder is inside the APK (a jar: URL), which file system APIs
        // cannot read, so download the listing.
        var request = UnityWebRequest.Get(listingPath);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to read {listingPath}: {request.error}");
            yield break;
        }

        listing = request.downloadHandler.text;
#else
        // On Web, ContentRootPath is the virtual file system path where the preloaded files are mounted.
        // Other platforms use Application.streamingAssetsPath.  Both are readable with file system APIs.
        listing = File.ReadAllText(listingPath);
#endif

        foreach (var line in listing.Split('\n'))
        {
            var directoryName = line.Trim();
            if (directoryName.Length > 0)
                directoryNames.Add(directoryName);
        }

        yield break;
#endif
    }
}
