using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;
using UnityEngine.UI;

// Drives the scene: fills the language dropdown and instantiates one card per prefab in the catalog.
public class PhrasebookController : MonoBehaviour
{
    public Dropdown languageDropdown;
    public RectTransform cardParent;

    readonly List<Loadable<GameObject>> m_Cards = new();

    async void Start()
    {
        languageDropdown.ClearOptions();
        var names = new List<string>();
        var currentIndex = 0;
        for (var i = 0; i < LanguageSetting.Available.Length; i++)
        {
            names.Add(LanguageSetting.Available[i].Name);
            if (LanguageSetting.Available[i].Code == LanguageSetting.Current)
                currentIndex = i;
        }
        languageDropdown.AddOptions(names);
        languageDropdown.SetValueWithoutNotify(currentIndex);

        // Hook up event so that the dropdown controls the active language.
        languageDropdown.onValueChanged.AddListener(index => LanguageSetting.Current = LanguageSetting.Available[index].Code);

        var catalog = CatalogProvider.Get();
        if (catalog == null)
            return;

        // Each card is loaded through its own Loadable, which is released in OnDestroy.
        foreach (var card in catalog.cards)
        {
            var loadable = new Loadable<GameObject>(card.LoadableObjectId);
            m_Cards.Add(loadable);

            var prefab = await loadable.LoadAsync();
            if (prefab == null)
            {
                Debug.LogError($"Failed to load card {card.LoadableObjectId}");
                continue;
            }

            Instantiate(prefab, cardParent);
        }

        Debug.Log($"Phrasebook ready with {cardParent.childCount} cards");
    }

    void OnDestroy()
    {
        foreach (var card in m_Cards)
            card.Release();
        m_Cards.Clear();

        CatalogProvider.Release();
    }
}
