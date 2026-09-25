using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;

// One phrase, in every language it has been drawn in.  Each entry is a Loadable, so building this
// asset into a content directory builds every language's sprite, while loading the asset loads none
// of them.  The sprite for the current language is loaded only when a LocalizedImage asks for it.
//
// The assets of this type are kept in step with the card images by LocalizedSpriteSync.
[CreateAssetMenu(fileName = "LocalizedSprite", menuName = "Scriptable Objects/Localized Sprite")]
public class LocalizedSprite : ScriptableObject
{
    // Dictionary fields are serialized only when marked [SerializeField], even when public.
    [SerializeField]
    [DictionaryDisplay(keyLabel = "Language", valueLabel = "Sprite")]
    public Dictionary<string, Loadable<Sprite>> languages = new();

    // Returns a new Loadable for the requested language, or for the fallback language when the
    // requested one is missing.  A new instance is returned so that each caller holds its own
    // reference and releasing one does not unload the sprite for another.
    public Loadable<Sprite> Resolve(string language)
    {
        if (!languages.TryGetValue(language, out var loadable) &&
            !languages.TryGetValue(LanguageSetting.Fallback, out loadable))
            return null;

        return new Loadable<Sprite>(loadable.LoadableObjectId);
    }
}
