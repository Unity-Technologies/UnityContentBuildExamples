using Unity.Loading;
using UnityEngine;

// Example of a ScriptableObject that can be used as a root asset
[CreateAssetMenu(fileName = "CoreAudioClips", menuName = "Scriptable Objects/CoreAudioClips")]
public class CoreAudioClips : ScriptableObject
{
    // Scene that is in the content directory.  Loaded by the bootstrap scene, see RegisterAndLaunch
    public LoadableSceneId startupScene;

    public Loadable<AudioClip> startupSound;
    public Loadable<AudioClip> secondSound;
}
