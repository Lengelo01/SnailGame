using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Builds a compact, resolution-independent card inside the existing game-over canvas.
// The original authored children stay intact and are only hidden at runtime.
public sealed class GameOverView
{
    private readonly RectTransform canvasRect;
    private readonly RectTransform card;
    private readonly CanvasGroup group;
    private readonly Image portrait;
    private readonly TMP_Text cause;
    private readonly TMP_Text description;
    private readonly Button retry;
    private readonly Sprite rounded;
    private readonly TMP_FontAsset font;
    private float reveal = 1f;
    private static readonly Color Ink = PauseMenuView.Ink;
    private static readonly Color Muted = PauseMenuView.Muted;
    private static readonly Color Mint = PauseMenuView.Mint;

    public GameOverView(GameObject root, UnityAction restart, UnityAction levels)
    {
        canvasRect = root.GetComponent<RectTransform>();
        TMP_Text oldText = root.GetComponentInChildren<TMP_Text>(true);
        font = oldText != null ? oldText.font : TMP_Settings.defaultFontAsset;
        foreach (Image oldImage in root.GetComponentsInChildren<Image>(true))
            if (oldImage.sprite != null) { rounded = oldImage.sprite; break; }
        foreach (Transform child in root.transform) child.gameObject.SetActive(false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.sortingOrder = 100;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        // This transparent input shield does not cover the visible game with a dimmer.
        Image shield = Box(root.transform, "Input shield", Vector2.zero, Vector2.zero, Color.clear, false);
        shield.rectTransform.anchorMin = Vector2.zero;
        shield.rectTransform.anchorMax = Vector2.one;
        shield.raycastTarget = true;
        card = Rect(root.transform, "Defeat card", Vector2.zero, new Vector2(560, 510));
        group = card.gameObject.AddComponent<CanvasGroup>();
        Box(card, "Shadow", new Vector2(0, -10), new Vector2(576, 524), new Color(0, 0, 0, 0.3f));
        Box(card, "Outer rim", Vector2.zero, new Vector2(562, 512), new Color32(78, 104, 99, 255));
        Box(card, "Surface", Vector2.zero, new Vector2(558, 508), Ink);
        Box(card, "Top accent", new Vector2(0, 253), new Vector2(120, 3), Mint, false);
        Label(card, "SNAIL ADVENTURES  /  " + SceneLabel(), new Vector2(0, 223), new Vector2(480, 24), 12, Muted, FontStyles.Bold, 3);
        Box(card, "Portrait rim", new Vector2(0, 139), new Vector2(112, 102), new Color32(110, 53, 55, 255));
        Box(card, "Portrait background", new Vector2(0, 139), new Vector2(108, 98), new Color32(48, 29, 35, 255));
        portrait = Box(card, "Ladybug portrait", new Vector2(0, 145), new Vector2(88, 68), Color.white, false);
        portrait.preserveAspect = true;
        cause = Label(card, "RUN ENDED", new Vector2(0, 101), new Vector2(130, 20), 10, new Color32(255, 149, 143, 255), FontStyles.Bold, 2);
        Label(card, "YOU DIED", new Vector2(0, 48), new Vector2(480, 64), 49, new Color32(245, 245, 229, 255), FontStyles.Bold, 1);
        Label(card, "Small shell. Another big adventure.", new Vector2(0, 1), new Vector2(480, 28), 17, Muted);
        Box(card, "Divider", new Vector2(0, -34), new Vector2(464, 1), new Color32(51, 66, 69, 255), false);
        description = Label(card, "The ladybug caught you. Time your next move.", new Vector2(0, -57), new Vector2(480, 28), 14, Muted);
        retry = Action(card, "TRY AGAIN", new Vector2(0, -112), Mint, Ink, restart);
        Action(card, "LEVEL SELECT", new Vector2(0, -176), new Color32(37, 51, 56, 255), new Color32(216, 229, 220, 255), levels);
        Label(card, "ENTER  /  TRY AGAIN", new Vector2(0, -229), new Vector2(460, 20), 10, Muted, FontStyles.Normal, 2);
        FitToScreen();
    }

    private static string SceneLabel()
    {
        string name = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return name.Replace("Level", "LEVEL ").ToUpperInvariant();
    }

    public void SetPortrait(Sprite sprite)
    {
        portrait.sprite = sprite;
        portrait.enabled = sprite != null;
    }

    public void SetCause(DefeatCause defeatCause)
    {
        if (defeatCause == DefeatCause.ArmoredFrog)
        {
            cause.text = "ARMORED FROG";
            description.text = "Its spit pierces your shell. Dodge it and avoid contact.";
            return;
        }
        if (defeatCause == DefeatCause.Frog)
        {
            cause.text = "FROG";
            description.text = "Hide in your shell to reflect shots. Two hits defeat the frog.";
            return;
        }
        if (defeatCause == DefeatCause.Fire)
        {
            cause.text = "FIRE";
            description.text = "Caught in the flames. Hide in your shell or wait.";
            return;
        }
        bool spike = defeatCause == DefeatCause.Spike;
        bool spider = defeatCause == DefeatCause.Spider;
        cause.text = spider ? "SPIDER" : spike ? "SPIKE" : "RUN ENDED";
        description.text = spider
            ? "Caught in the spider's web. Plan your escape."
            : spike
            ? "Caught by the spikes. Wait for them to retract."
            : "The ladybug caught you. Time your next move.";
    }

    public void SetReveal(float progress)
    {
        reveal = progress;
        group.alpha = Mathf.SmoothStep(0f, 1f, progress);
        group.interactable = progress >= 1f;
        card.anchoredPosition = new Vector2(0, Mathf.Lerp(-22f, 0f, 1f - Mathf.Pow(1f - progress, 3)));
        FitToScreen();
    }

    public void FitToScreen()
    {
        // Leave at least 11% of the game visible on every side, even in a small Game view.
        float fit = Mathf.Min(1f, canvasRect.rect.width * 0.78f / 580f, canvasRect.rect.height * 0.78f / 534f);
        card.localScale = Vector3.one * fit * Mathf.Lerp(0.94f, 1f, 1f - Mathf.Pow(1f - reveal, 3));
    }

    public void FocusRetry() { retry.Select(); }

    private static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private Image Box(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool round = true)
    {
        Image image = Rect(parent, name, position, size).gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        if (round) { image.sprite = rounded; image.type = Image.Type.Sliced; }
        return image;
    }

    private TMP_Text Label(Transform parent, string text, Vector2 position, Vector2 size, float fontSize,
        Color color, FontStyles style = FontStyles.Normal, float spacing = 0)
    {
        var label = Rect(parent, text, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.characterSpacing = spacing;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    private Button Action(Transform parent, string text, Vector2 position, Color background, Color foreground, UnityAction action)
    {
        Image image = Box(parent, text + " button", position, new Vector2(464, 52), background);
        image.raycastTarget = true;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
        colors.selectedColor = Color.white;
        colors.pressedColor = new Color(0.78f, 0.84f, 0.78f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        button.onClick.AddListener(action);
        Label(image.transform, text, Vector2.zero, new Vector2(440, 44), 16, foreground, FontStyles.Bold, 1.5f);
        return button;
    }
}
