using System.Collections.Generic;
using System.Text;
using Unity.Loading;
using UnityEngine;
using UnityEngine.UI;

// Drives the scene: fills the language dropdown, instantiates one card per prefab in the catalog,
// and reports which sprites the cards are currently showing.
public class PhrasebookController : MonoBehaviour
{
    public Dropdown languageDropdown;
    public Text statusText;
    public RectTransform cardParent;

    readonly List<Loadable<GameObject>> m_Cards = new();
    readonly StringBuilder m_Status = new();

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

    void Update()
    {
        // The sprite names are the card image file names, so this line shows which language each
        // card is showing.
        m_Status.Clear();
        m_Status.Append("Language: ").Append(LanguageSetting.Current).Append("    Showing: ");
        foreach (var image in cardParent.GetComponentsInChildren<LocalizedImage>())
        {
            var sprite = image.GetComponent<Image>().sprite;
            m_Status.Append(sprite != null ? sprite.name : "(loading)").Append("  ");
        }
        statusText.text = m_Status.ToString();
    }

    void OnDestroy()
    {
        foreach (var card in m_Cards)
            card.Release();
        m_Cards.Clear();

        CatalogProvider.Release();
    }
}
