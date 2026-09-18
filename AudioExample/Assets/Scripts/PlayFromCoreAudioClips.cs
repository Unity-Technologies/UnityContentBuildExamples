using System.Collections;
using Unity.Loading;
using UnityEngine;

// Demonstrates loading individual clips from named fields on a root asset, rather than from a
// collection like AudioResources.
[RequireComponent(typeof(AudioSource))]
public class PlayFromCoreAudioClips : MonoBehaviour
{
    AudioSource audioSource;
    Loadable<AudioClip> startupClip;
    Loadable<AudioClip> secondClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // CoreAudioClips is the single root asset the Core content directory provides for startup.
        var coreAudioClips = ContentLoadManager.GetRootAssets<CoreAudioClips>()[0];

        // Copy the Loadables out of the root asset, so this component loads and releases them
        // independently of anything else reading the same ScriptableObject.
        startupClip = new Loadable<AudioClip>(coreAudioClips.startupSound.LoadableObjectId);
        secondClip = new Loadable<AudioClip>(coreAudioClips.secondSound.LoadableObjectId);

        audioSource.PlayOneShot(startupClip.Load());
        StartCoroutine(PlaySecondSound());
    }

    IEnumerator PlaySecondSound()
    {
        yield return new WaitForSeconds(1.0f);
        audioSource.PlayOneShot(secondClip.Load());
    }

    void OnDestroy()
    {
        startupClip.Release();
        secondClip.Release();
    }
}
