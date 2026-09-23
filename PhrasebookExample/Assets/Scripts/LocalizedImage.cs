using Unity.Loading;
using UnityEngine;
using UnityEngine.UI;

// Shows the sprite for the current language and swaps it when the language changes.
//
// The prefab holds a direct reference to a LocalizedSprite asset rather than to a Sprite, so the
// prefab is not tied to any one language and loading it does not load any card image.
[RequireComponent(typeof(Image))]
public class LocalizedImage : MonoBehaviour
{
    public LocalizedSprite source;

    Image m_Image;
    Loadable<Sprite> m_Loaded;
    int m_RefreshCount;

    void Awake()
    {
        m_Image = GetComponent<Image>();
    }

    void OnEnable()
    {
        LanguageSetting.Changed += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        LanguageSetting.Changed -= Refresh;
    }

    void OnDestroy()
    {
        m_Loaded?.Release();
        m_Loaded = null;
    }

    async void Refresh()
    {
        if (source == null)
            return;

        var next = source.Resolve(LanguageSetting.Current);
        if (next == null)
        {
            Debug.LogWarning($"{source.name} has no sprite for '{LanguageSetting.Current}' or the fallback language", this);
            return;
        }

        var refreshCount = ++m_RefreshCount;
        var sprite = await next.LoadAsync();

        // The language changed again, or this object was destroyed, while the sprite was loading.
        if (refreshCount != m_RefreshCount || this == null)
        {
            next.Release();
            return;
        }

        m_Image.sprite = sprite;

        // Release the previous language's sprite only after the new one is showing.
        m_Loaded?.Release();
        m_Loaded = next;
    }
}
