using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Loading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// One playable clip.  Clips are named after the key that plays them, so the name is the binding.
public readonly struct ClipBinding
{
    public readonly KeyControl control;
    public readonly string keyLabel;
    public readonly string source;
    public readonly Loadable<AudioClip> clip;

    public ClipBinding(KeyControl control, string source, Loadable<AudioClip> clip)
    {
        this.control = control;
        this.keyLabel = control.displayName.ToUpperInvariant();
        this.source = source;
        this.clip = clip;
    }
}

// Assembles the clips contributed by every registered content directory and plays them from the
// keyboard.  Building another SoundPack content directory adds its clips here with no code change.
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    readonly List<ClipBinding> bindings = new();
    readonly AudioKeyLegend legend = new();
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        CollectClips();
        legend.SetBindings(bindings);

        Debug.Log($"{bindings.Count} clips available");
    }

    void CollectClips()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        // Sorted so the legend below lists the directories and their keys in a stable order.
        foreach (var handle in ContentLoadManager.GetContentDirectories().OrderBy(h => h.BuildName))
        {
            foreach (var asset in ContentLoadManager.GetRootAssets<AudioResources>(handle))
            {
                foreach (var name in asset.clips.Keys.OrderBy(n => n, StringComparer.Ordinal))
                {
                    var control = keyboard.FindKeyOnCurrentKeyboardLayout(name);
                    if (control == null)
                    {
                        Debug.LogWarning($"No key named '{name}' on this keyboard layout, so nothing plays that clip from {handle.BuildName}");
                        continue;
                    }

                    if (bindings.Any(b => b.control == control))
                    {
                        Debug.LogWarning($"Key '{name}' is already taken, ignoring the clip from {handle.BuildName}");
                        continue;
                    }

                    // Copy the Loadable out of the root asset, so this component loads and releases
                    // it independently of anything else reading the same ScriptableObject.
                    bindings.Add(new ClipBinding(control, handle.BuildName, new Loadable<AudioClip>(asset.clips[name].LoadableObjectId)));
                }
            }
        }
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        foreach (var binding in bindings)
        {
            // Load blocks on first use and is a no-op afterwards, so the clip stays in memory for reuse.
            if (binding.control.wasPressedThisFrame)
                audioSource.PlayOneShot(binding.clip.Load());
        }

        if (keyboard.deleteKey.wasPressedThisFrame)
            audioSource.Stop();

        if (keyboard.f1Key.wasPressedThisFrame)
            legend.visible = !legend.visible;
    }

    void OnGUI()
    {
        legend.Draw();
    }

    void OnDestroy()
    {
        // Clips must be released before their content directory is unregistered.  RegisterAndLaunch
        // guarantees that by unloading this scene before it unregisters anything.
        foreach (var binding in bindings)
            binding.clip.Release();
    }
}
