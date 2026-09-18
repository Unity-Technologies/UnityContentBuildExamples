using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Draws the on-screen key legend: the special keys, then the clip keys grouped by the content
// directory that provided them.  Using IMGUI to keep things self-contained.
public class AudioKeyLegend
{
    const float k_Margin = 16f;
    const float k_Padding = 14f;
    const float k_Width = 380f;
    const float k_CollapsedWidth = 200f;
    const float k_LineHeight = 19f;
    const float k_KeyCapWidth = 28f;
    const int k_KeyCapsPerRow = 10;

    static readonly (string key, string action)[] k_SpecialKeys =
    {
        ("Esc", "Quit"),
        ("Del", "Stop playback"),
        ("F1", "Show / hide this list"),
    };

    public bool visible = true;

    readonly List<(string source, List<string> keyLabels)> groups = new();
    GUIStyle panel, title, section, body, hint, keyCap;

    // Grouped once, since the bindings only change when a content directory is registered or released.
    public void SetBindings(IEnumerable<ClipBinding> bindings)
    {
        groups.Clear();
        groups.AddRange(bindings
            .GroupBy(binding => binding.source)
            .Select(group => (group.Key, group.Select(binding => binding.keyLabel).ToList())));
    }

    public void Draw()
    {
        EnsureStyles();

        if (!visible)
        {
            BeginPanel(k_CollapsedWidth);
            DrawEntry("F1", "Show key list", k_CollapsedWidth);
            EndPanel();
            return;
        }

        BeginPanel(k_Width);

        GUILayout.Label("Content Directory Audio Example", title);
        GUILayout.Space(6f);

        foreach (var (key, action) in k_SpecialKeys)
            DrawEntry(key, action, k_Width);

        foreach (var (source, keyLabels) in groups)
        {
            GUILayout.Space(6f);
            GUILayout.Label(source, section);
            DrawKeyCaps(keyLabels);
        }

        GUILayout.Space(6f);
        GUILayout.Label(groups.Count == 0
            ? "No clips found.  Build the content directories first."
            : "Each clip is named after the key that plays it.", hint);

        EndPanel();
    }

    // The area is given the full height available and the panel inside it hugs its content, with the
    // trailing flexible space absorbing whatever is left over.
    void BeginPanel(float width)
    {
        GUILayout.BeginArea(new Rect(k_Margin, k_Margin, width, Screen.height - 2 * k_Margin));
        GUILayout.BeginVertical(panel);
    }

    static void EndPanel()
    {
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }

    void DrawKeyCaps(List<string> keyLabels)
    {
        for (int i = 0; i < keyLabels.Count; i += k_KeyCapsPerRow)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(k_LineHeight));
            for (int column = 0; column < k_KeyCapsPerRow && i + column < keyLabels.Count; column++)
                GUILayout.Label(keyLabels[i + column], keyCap, GUILayout.Width(k_KeyCapWidth));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    void DrawEntry(string keyLabel, string description, float panelWidth)
    {
        GUILayout.BeginHorizontal(GUILayout.Height(k_LineHeight));
        GUILayout.Label(keyLabel, keyCap, GUILayout.Width(k_KeyCapWidth));
        GUILayout.Label(description, body, GUILayout.Width(panelWidth - 2 * k_Padding - k_KeyCapWidth));
        GUILayout.EndHorizontal();
    }

    void EnsureStyles()
    {
        if (panel != null)
            return;

        // Zero margins keep the rows tight; the default label margins add uneven spacing.
        var flush = new RectOffset(0, 0, 0, 0);

        panel = new GUIStyle { padding = new RectOffset((int)k_Padding, (int)k_Padding, (int)k_Padding, (int)k_Padding) };
        panel.normal.background = SolidTexture(new Color(0.08f, 0.09f, 0.11f, 0.92f));

        title = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, margin = flush };
        title.normal.textColor = Color.white;

        section = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold, margin = flush };
        section.normal.textColor = new Color(0.45f, 0.75f, 1f);

        body = new GUIStyle(GUI.skin.label) { fontSize = 12, margin = flush, padding = flush, alignment = TextAnchor.MiddleLeft };
        body.normal.textColor = new Color(0.85f, 0.87f, 0.9f);

        hint = new GUIStyle(GUI.skin.label) { fontSize = 11, margin = flush };
        hint.normal.textColor = new Color(0.55f, 0.58f, 0.62f);

        keyCap = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(0, 6, 2, 2),
            padding = flush,
        };
        keyCap.normal.background = SolidTexture(new Color(0.83f, 0.85f, 0.88f, 1f));
        keyCap.normal.textColor = new Color(0.1f, 0.11f, 0.13f);
    }

    static Texture2D SolidTexture(Color color)
    {
        var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
