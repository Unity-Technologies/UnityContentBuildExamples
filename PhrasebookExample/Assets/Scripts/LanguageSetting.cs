using System;

// The language the phrasebook currently shows.  This is the only place that knows which languages
// exist, so adding one is a single edit here plus the card images.
public static class LanguageSetting
{
    public struct Language
    {
        public string Code;   // lower-case two-letter code, also used as the file name suffix of the card images
        public string Name;   // shown in the dropdown
    }

    public const string Fallback = "en";

    public static readonly Language[] Available =
    {
        new() { Code = "en", Name = "English" },
        new() { Code = "fr", Name = "Français" },
        new() { Code = "es", Name = "Español" },
    };

    static string s_Current = Fallback;

    public static string Current
    {
        get => s_Current;
        set
        {
            if (s_Current == value)
                return;
            s_Current = value;
            Changed?.Invoke();
        }
    }

    // Raised after Current changes.  LocalizedImage listens to this to swap its sprite.
    public static event Action Changed;

    public static bool IsAvailable(string code)
    {
        foreach (var language in Available)
            if (language.Code == code)
                return true;
        return false;
    }
}
