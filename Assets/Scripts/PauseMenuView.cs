using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Runtime pause view only. MainMenu and LevelSelect are authored in their scenes.
public sealed class PauseMenuView
{
    public static readonly Color Ink = new Color32(17, 25, 30, 255);
    public static readonly Color Mint = new Color32(183, 237, 151, 255);
    public static readonly Color Muted = new Color32(159, 180, 181, 255);
    public static readonly Color Paper = new Color32(245, 245, 229, 255);
    private static readonly Color Surface = new Color32(37, 51, 56, 255);
    private readonly RectTransform canvas;
    private readonly RectTransform card;
    private readonly CanvasGroup group;
    private readonly TMP_FontAsset font;
    private readonly Sprite rounded;
    private readonly Vector2 size;
    private readonly bool overlay;
    private float elapsed;
    private Button primary;
    private bool focused;

    public PauseMenuView(GameObject root, Vector2 cardSize, bool overlay)
    {
        this.overlay = overlay;
        size = cardSize;
        canvas = root.GetComponent<RectTransform>();
        TMP_Text oldText = root.GetComponentInChildren<TMP_Text>(true);
        font = oldText != null ? oldText.font : TMP_Settings.defaultFontAsset;
        foreach (Image old in root.GetComponentsInChildren<Image>(true))
            if (old.sprite != null) { rounded = old.sprite; break; }
        foreach (Transform child in root.transform) child.gameObject.SetActive(false);
        root.GetComponent<Canvas>().sortingOrder = overlay ? 80 : 0;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
        Image backdrop = Box(root.transform, "Backdrop", Vector2.zero, Vector2.zero,
            overlay ? Color.clear : new Color32(10, 18, 22, 255), false);
        backdrop.rectTransform.anchorMin = Vector2.zero;
        backdrop.rectTransform.anchorMax = Vector2.one;
        backdrop.raycastTarget = true;
        card = Rect(root.transform, "Adventure menu", Vector2.zero, size);
        group = card.gameObject.AddComponent<CanvasGroup>();
        Box(card, "Shadow", new Vector2(0, -10), size + new Vector2(16, 14), new Color(0, 0, 0, 0.3f));
        Box(card, "Rim", Vector2.zero, size + Vector2.one * 2, new Color32(78, 104, 99, 255));
        Box(card, "Surface", Vector2.zero, size - Vector2.one * 2, Ink);
        Box(card, "Accent", new Vector2(0, size.y / 2 - 2), new Vector2(120, 3), Mint, false);
    }

    public void BuildPause(Sprite snail, UnityAction resume, UnityAction levels)
    {
        Label(card, "SNAIL ADVENTURES  /  TAKE A BREATHER", new Vector2(0, 197), new Vector2(490, 24), 11, Muted, FontStyles.Bold, 2);
        Box(card, "Emblem", new Vector2(0, 126), new Vector2(96, 86), Surface);
        if (snail != null) Portrait(card, snail, new Vector2(0, 129), new Vector2(68, 68));
        else
        {
            Box(card, "Pause left", new Vector2(-12, 126), new Vector2(10, 32), Mint);
            Box(card, "Pause right", new Vector2(12, 126), new Vector2(10, 32), Mint);
        }
        Label(card, "PAUSED", new Vector2(0, 48), new Vector2(480, 62), 49, Paper, FontStyles.Bold, 1);
        Label(card, "Your adventure can wait a moment.", new Vector2(0, 4), new Vector2(480, 28), 17, Muted);
        Box(card, "Divider", new Vector2(0, -31), new Vector2(464, 1), Surface, false);
        primary = Action(card, "RESUME GAME", new Vector2(0, -77), new Vector2(464, 52), true, resume);
        Action(card, "LEVEL SELECT", new Vector2(0, -141), new Vector2(464, 52), false, levels);
        Label(card, "ESC  /  BACK TO THE ADVENTURE", new Vector2(0, -200), new Vector2(480, 24), 10, Muted, FontStyles.Normal, 2);
    }

    public void Show()
    {
        elapsed = 0;
        focused = false;
        group.alpha = 0;
        group.interactable = false;
        Tick();
    }

    public void Tick()
    {
        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / 0.32f);
        float eased = 1 - Mathf.Pow(1 - t, 3);
        group.alpha = Mathf.SmoothStep(0, 1, t);
        group.interactable = t >= 1;
        float fraction = overlay ? 0.78f : 0.88f;
        float fit = Mathf.Min(1f, canvas.rect.width * fraction / (size.x + 20), canvas.rect.height * fraction / (size.y + 24));
        card.localScale = Vector3.one * fit * Mathf.Lerp(0.96f, 1, eased);
        card.anchoredPosition = new Vector2(0, Mathf.Lerp(-16, 0, eased));
        if (t >= 1 && !focused)
        {
            focused = true;
            if (primary != null) primary.Select();
        }
    }

    private static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 dimensions)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        return rect;
    }

    private Image Box(Transform parent, string name, Vector2 position, Vector2 dimensions, Color color, bool round = true)
    {
        Image image = Rect(parent, name, position, dimensions).gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        if (round)
        {
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            // Different scenes use different sliced sprites. Normalize their
            // corner size so pause buttons do not turn into oversized pills.
            if (rounded != null)
                image.pixelsPerUnitMultiplier = Mathf.Max(1f,
                    rounded.border.x * 100f / Mathf.Max(1f, rounded.pixelsPerUnit) / 8f);
        }
        return image;
    }

    private void Portrait(Transform parent, Sprite sprite, Vector2 position, Vector2 dimensions)
    {
        Image image = Box(parent, "Snail portrait", position, dimensions, Color.white, false);
        image.sprite = sprite;
        image.preserveAspect = true;
    }

    private TMP_Text Label(Transform parent, string text, Vector2 position, Vector2 dimensions, float fontSize, Color color,
        FontStyles style = FontStyles.Normal, float spacing = 0, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var label = Rect(parent, text, position, dimensions).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.characterSpacing = spacing;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    private Button Action(Transform parent, string text, Vector2 position, Vector2 dimensions, bool prominent, UnityAction callback)
    {
        Image image = Box(parent, text + " button", position, dimensions, prominent ? Mint : Surface);
        image.raycastTarget = true;
        Button button = image.gameObject.AddComponent<Button>();
        StyleButton(button, image);
        button.onClick.AddListener(callback);
        Label(image.transform, text, Vector2.zero, dimensions - new Vector2(16, 4), 16, prominent ? Ink : Paper, FontStyles.Bold, 1.5f);
        return button;
    }

    private static void StyleButton(Button button, Image image)
    {
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        colors.selectedColor = new Color(1.08f, 1.08f, 1.08f);
        colors.pressedColor = new Color(0.78f, 0.84f, 0.78f);
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;
    }
}
