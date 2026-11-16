using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class RoadRushUI : MonoBehaviour
{
    private static readonly Color Ink = new(.022f, .043f, .065f, .96f);
    private static readonly Color Aqua = new(.36f, .94f, .87f);
    private static readonly Color White = new(.94f, .97f, 1f);
    private static readonly Color Muted = new(.66f, .76f, .82f);
    private static readonly Color Alert = new(1f, .49f, .30f);

    private readonly RectTransform[] pages = new RectTransform[5];
    private readonly Button[] firstButtons = new Button[5];
    private readonly Image[] lanes = new Image[3];
    private GameManager game;
    private Font font;
    private EventSystem events;
    private Text scoreLabel, bestLabel, speedLabel, resultLabel, recordLabel, menuRecord;
    private int displayedScore = -1, displayedBest = -1, displayedSpeed = -1, displayedLane = -1;

    public void Create(GameManager manager, string[] credits)
    {
        game = manager;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var input = new GameObject("Keyboard and Mouse", typeof(EventSystem), typeof(InputSystemUIInputModule));
        input.transform.SetParent(transform);
        events = input.GetComponent<EventSystem>();

        var root = new GameObject("ROAD RUSH UI", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform);
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = .5f;
        var canvas = root.GetComponent<RectTransform>();
        foreach (GameManager.ScreenState state in Enum.GetValues(typeof(GameManager.ScreenState)))
        {
            var page = Node(canvas, state.ToString());
            page.anchorMax = Vector2.one;
            page.offsetMin = page.offsetMax = Vector2.zero;
            pages[(int)state] = page;
        }

        var menu = pages[(int)GameManager.ScreenState.Menu];
        var cover = Fill(menu, "Backdrop", new Color(.015f, .035f, .05f, .26f));
        cover.raycastTarget = false;
        var hero = Panel(menu, new Vector2(0, .5f), 48, -305, 450, 610);
        Block(hero, "Accent", 30, 561, 36, 3, Aqua);
        Label(hero, "KOMPUTER GRAFIS  /  3D", 21, 31, 520, 370, 33, Aqua, true);
        Label(hero, "ROAD", 88, 25, 412, 410, 105, White, true);
        Label(hero, "RUSH", 88, 27, 330, 410, 104, Aqua, true);
        Label(hero, "Hindari traffic di tiga jalur.", 22, 33, 295, 380, 30, White);
        menuRecord = Label(hero, "", 20, 33, 253, 380, 28, Muted, true);
        firstButtons[0] = Action(hero, "MULAI BALAP", 32, 172, 386, 56, game.StartRun, true);
        Action(hero, "Credits", 32, 109, 188, 48, game.Credits);
        Action(hero, "Keluar", 230, 109, 188, 48, game.Quit);
        Label(hero, "A / D atau panah     •     Esc untuk jeda", 18, 33, 46, 388, 28, Muted);

        var credit = Modal(GameManager.ScreenState.Credits, 590, 440);
        Label(credit, "Credits", 51, 32, 337, 526, 75, White, true);
        Label(credit, "ROAD RUSH  /  PROYEK KOMPUTER GRAFIS", 17, 35, 296, 520, 31, Aqua, true);
        var entries = Label(credit, string.Join("\n", credits), 22, 35, 132, 520, 150, White);
        entries.name = "Team credits";
        entries.alignment = TextAnchor.UpperLeft;
        firstButtons[1] = Action(credit, "Kembali", 34, 33, 522, 56, game.Menu, true);

        var hud = pages[(int)GameManager.ScreenState.Running];
        var scores = Panel(hud, new Vector2(0, 1), 28, -112, 300, 88);
        Label(scores, "SKOR", 15, 17, 54, 126, 20, Aqua, true);
        scoreLabel = Label(scores, "", 33, 15, 13, 125, 39, White, true);
        Label(scores, "REKOR", 15, 160, 54, 125, 20, Muted, true);
        bestLabel = Label(scores, "", 33, 158, 13, 128, 39, White, true);
        var race = Panel(hud, new Vector2(1, 1), -209, -112, 181, 88);
        Label(race, "KM/J", 15, 112, 35, 63, 20, Muted, true);
        speedLabel = Label(race, "", 37, 19, 23, 91, 49, White, true);
        var controls = Panel(hud, new Vector2(.5f, 0), -177, 20, 354, 46);
        for (int i = 0; i < lanes.Length; i++)
            lanes[i] = Block(controls, "Lane " + i, 16 + i * 25, 19, 18, 8, Muted);
        Label(controls, "A / D   Pindah jalur    •    Esc   Jeda", 15, 101, 8, 245, 30, White);
        var pauseButton = Node(hud, "Pause anchor");
        pauseButton.anchorMin = pauseButton.anchorMax = new Vector2(1, 0);
        pauseButton.sizeDelta = Vector2.zero;
        Action(pauseButton, "Jeda  Esc", -161, 20, 133, 46, game.Pause);

        var pause = Modal(GameManager.ScreenState.Paused, 490, 324);
        Label(pause, "Dijeda", 56, 30, 222, 432, 71, White, true);
        Label(pause, "Jalur dan skor berhenti sementara.", 20, 33, 179, 420, 34, Muted);
        firstButtons[3] = Action(pause, "Lanjutkan", 32, 101, 426, 52, game.Resume, true);
        Action(pause, "Menu utama", 32, 33, 426, 51, game.Menu);

        var over = Modal(GameManager.ScreenState.GameOver, 540, 385, Alert);
        Label(over, "TABRAKAN", 17, 33, 312, 430, 30, Alert, true);
        Label(over, "Game Over", 60, 29, 235, 480, 79, White, true);
        resultLabel = Label(over, "", 30, 33, 176, 473, 46, White, true);
        recordLabel = Label(over, "", 18, 35, 136, 473, 30, Aqua, true);
        firstButtons[4] = Action(over, "Main lagi   R", 32, 72, 476, 52, game.StartRun, true);
        Action(over, "Menu utama", 32, 17, 476, 45, game.Menu);
    }

    public void Show(GameManager.ScreenState state)
    {
        for (int i = 0; i < pages.Length; i++) pages[i].gameObject.SetActive(i == (int)state);
        var first = firstButtons[(int)state];
        events.SetSelectedGameObject(first == null ? null : first.gameObject);
        menuRecord.text = $"REKOR LOKAL   {game.Record:N0}";
        if (state != GameManager.ScreenState.GameOver) return;
        resultLabel.text = $"Skor   {game.Score:N0}";
        recordLabel.text = game.NewRecord ? "REKOR BARU" : $"Rekor lokal   {game.Record:N0}";
    }

    public void Refresh(int score, int best, float speed, int lane)
    {
        if (score != displayedScore) scoreLabel.text = (displayedScore = score).ToString("N0");
        if (best != displayedBest) bestLabel.text = (displayedBest = best).ToString("N0");
        int kmh = Mathf.RoundToInt(speed * 3.6f);
        if (kmh != displayedSpeed) speedLabel.text = (displayedSpeed = kmh).ToString();
        if (lane == displayedLane) return;
        displayedLane = lane;
        for (int i = 0; i < lanes.Length; i++) lanes[i].color = i == lane ? Aqua : new Color(.26f, .36f, .44f);
    }

    private RectTransform Modal(GameManager.ScreenState state, float width, float height, Color? accent = null)
    {
        var page = pages[(int)state];
        Fill(page, "Backdrop", new Color(.01f, .02f, .035f, .63f));
        var panel = Panel(page, new Vector2(.5f, .5f), -width / 2, -height / 2, width, height);
        Block(panel, "Top rule", 0, height - 3, width, 3, accent ?? Aqua);
        return panel;
    }

    private static RectTransform Panel(RectTransform parent, Vector2 anchor, float x, float y, float width, float height)
    {
        var r = Place(parent, "Panel", x, y, width, height);
        r.anchorMin = r.anchorMax = anchor;
        r.gameObject.AddComponent<Image>().color = Ink;
        return r;
    }

    private Text Label(RectTransform parent, string content, int size, float x, float y, float w, float h,
        Color color, bool bold = false)
    {
        var text = Place(parent, "Text", x, y, w, h).gameObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        text.color = color;
        text.text = content;
        text.alignment = TextAnchor.MiddleLeft;
        text.raycastTarget = false;
        return text;
    }

    private Button Action(RectTransform parent, string title, float x, float y, float w, float h,
        Action call, bool primary = false)
    {
        var r = Place(parent, title, x, y, w, h);
        var graphic = r.gameObject.AddComponent<Image>();
        var button = r.gameObject.AddComponent<Button>();
        button.targetGraphic = graphic;
        button.colors = new ColorBlock
        {
            normalColor = primary ? Aqua : new Color(.095f, .15f, .20f),
            highlightedColor = primary ? new Color(.6f, 1f, .94f) : new Color(.21f, .34f, .40f),
            selectedColor = primary ? new Color(.50f, 1f, .90f) : new Color(.16f, .33f, .36f),
            pressedColor = primary ? new Color(.21f, .69f, .62f) : new Color(.12f, .24f, .29f),
            disabledColor = Muted, colorMultiplier = 1, fadeDuration = .12f
        };
        var label = Label(r, title, 19, 13, 0, w - 26, h, primary ? Ink : White, true);
        label.alignment = TextAnchor.MiddleCenter;
        button.onClick.AddListener(() => call());
        return button;
    }

    private static Image Fill(RectTransform parent, string name, Color color)
    {
        var r = Node(parent, name);
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        var image = r.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static Image Block(RectTransform parent, string name, float x, float y, float w, float h, Color color)
    {
        var image = Place(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static RectTransform Place(RectTransform parent, string name, float x, float y, float w, float h)
    {
        var rect = Node(parent, name);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(w, h);
        return rect;
    }

    private static RectTransform Node(RectTransform parent, string name)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        return rect;
    }
}
