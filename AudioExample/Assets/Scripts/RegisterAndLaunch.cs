using System.Collections;
using Unity.Loading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Lives on the "bootstrap" scene that ships in the player, and owns the content directories for the
// whole run: it registers them, launches the real startup scene out of the Core directory, and
// unregisters everything on the way out.
public class RegisterAndLaunch : MonoBehaviour
{
    readonly ContentDirectoryManager contentDirectories = new();
    Scene startScene;
    bool tearingDown;
    bool readyToQuit;

    void Start()
    {
        Application.wantsToQuit += OnWantsToQuit;
        StartCoroutine(RegisterAndLoadStartScene());
    }

    IEnumerator RegisterAndLoadStartScene()
    {
        yield return StartCoroutine(contentDirectories.RegisterAll());

        // Based on the design, there is only a single instance of the CoreAudioClips root asset
        var coreRootAssets = ContentLoadManager.GetRootAssets<CoreAudioClips>();
        if (coreRootAssets.Length == 0)
        {
            Debug.LogError("No CoreAudioClips root asset found. Is the Core content directory built?");
            yield break;
        }

        var coreRootAsset = coreRootAssets[0];
        Debug.Log($"Loading startup scene (guid: {coreRootAsset.startupScene})");

        yield return SceneManager.LoadSceneAsync(coreRootAsset.startupScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive });

        startScene = SceneManager.GetSceneByLoadableSceneId(coreRootAsset.startupScene);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
#if UNITY_EDITOR
            // Application.Quit is ignored in the Editor, so leave Play mode instead.
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }

    // Runs before anything is destroyed, it gives a chance for orderly cleanup on shutdown.
    // Note: technically its not always necessary to clean everything explicitly, given that the app
    // is shutting down. But this demonstrates techniques for an orderly cleanup, including releasing
    // loaded content before a content directory is unregistered.
    bool OnWantsToQuit()
    {
        if (readyToQuit)
            return true;

        if (!tearingDown)
        {
            tearingDown = true;
            StartCoroutine(TearDownAndQuit());
        }
        // Refer the quit until TearDownAndQuit has finished
        return false;
    }

    IEnumerator TearDownAndQuit()
    {
        Debug.Log("Closing Startup Scene and content directories");

        // The startup scene came out of the Core content directory, so it has to finish unloading
        // before anything is unregistered.  Unloading it also destroys AudioManager, which releases
        // the clips it loaded.
        if (startScene.isLoaded)
            yield return SceneManager.UnloadSceneAsync(startScene);

        contentDirectories.UnregisterAll();

        readyToQuit = true;
        Application.Quit();
    }

    void OnDestroy()
    {
        Application.wantsToQuit -= OnWantsToQuit;
    }
}
