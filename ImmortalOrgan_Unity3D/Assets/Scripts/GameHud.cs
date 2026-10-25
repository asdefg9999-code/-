using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ImmortalOrgan
{
    /// <summary>Mobile-friendly HUD created entirely at runtime; no UI prefab required.</summary>
    public sealed class GameHud : MonoBehaviour
    {
        private GameRuntime runtime;
        private Canvas canvas;
        private Font font;
        private Text hpText, energyText, erosionText, objectiveText, nearbyText, messageTitle, messageBody, floatingText;
        private Image hpFill, energyFill, erosionFill;
        private GameObject introPanel, resultPanel, messagePanel;
        private Text resultTitle, resultBody;
        private float messageEnd, floatingEnd;
        private bool pausedByUser;

        private static readonly Color Ink = new Color(.08f, .15f, .18f, .84f);
        private static readonly Color Cream = new Color(.99f, .95f, .89f);
        private static readonly Color Blood = new Color(.88f, .31f, .43f);
        private static readonly Color Aqua = new Color(.43f, .86f, .82f);

        public void Initialize(GameRuntime game)
        {
            runtime = game;
            font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Noto Sans CJK KR", "Arial Unicode MS", "Arial" }, 22);
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            EnsureEventSystem();
            GameObject ui = new GameObject("IMMORTAL ORGAN | HUD", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            ui.transform.SetParent(transform, false);
            canvas = ui.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            CanvasScaler scaler = ui.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .50f;

            // A transparent drag surface receives camera orbit gestures. Buttons live above it.
            GameObject look = Fullscreen("Camera orbit touch field", ui.transform, new Color(0, 0, 0, 0));
            look.AddComponent<TouchOrbitField>().Initialize(game.Orbit);

            MakeText(ui.transform, "Game ID", "LRC  /  LAZARUS REGENERATIVE CELL  /  PROJECT 01",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(660, 38), new Vector2(28, -18), 22,
                new Color(.98f, .86f, .68f), TextAnchor.MiddleLeft);
            objectiveText = MakeText(ui.transform, "Objective", "L-Cell 잔향을 추적하라",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(690, 56), new Vector2(28, -67), 31,
                Cream, TextAnchor.MiddleLeft);

            MakePanel(ui.transform, "Player status backdrop", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(470, 160), new Vector2(28, -134), Ink);
            hpText = MakeText(ui.transform, "Vital status", "생명력 100 / 100",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(420, 35), new Vector2(45, -147), 24,
                Cream, TextAnchor.MiddleLeft);
            hpFill = Bar(ui.transform, "Health", new Vector2(45, -192), new Vector2(410, 15), Blood);
            energyText = MakeText(ui.transform, "Energy status", "L-Cell 35 / 100",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(420, 34), new Vector2(45, -215), 22,
                new Color(.75f, .95f, .93f), TextAnchor.MiddleLeft);
            energyFill = Bar(ui.transform, "L-Cell", new Vector2(45, -255), new Vector2(410, 12), Aqua);
            erosionText = MakeText(ui.transform, "Erosion status", "침식도 0 / 100",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(420, 32), new Vector2(45, -270), 21,
                new Color(1f, .77f, .80f), TextAnchor.MiddleLeft);
            erosionFill = Bar(ui.transform, "Erosion", new Vector2(45, -304), new Vector2(410, 11),
                new Color(.86f, .28f, .66f));

            MakePortrait(ui.transform);
            nearbyText = MakeText(ui.transform, "Nearby interaction", "",
                new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(650, 47),
                new Vector2(0, 256), 26, Cream, TextAnchor.MiddleCenter);

            GameObject stick = MakePanel(ui.transform, "Move joystick", new Vector2(0, 0), new Vector2(.5f, .5f),
                new Vector2(230, 230), new Vector2(155, 165), new Color(.14f, .40f, .44f, .52f));
            var knob = MakePanel(stick.transform, "Knob", new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                new Vector2(98, 98), Vector2.zero, new Color(.58f, .83f, .83f, .8f));
            stick.AddComponent<TouchJoystick>().Initialize(runtime.Player, knob.GetComponent<RectTransform>());

            MakeButton(ui.transform, "공격", new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(132, 132), new Vector2(-92, 145), Blood, runtime.Combat.LightAttack);
            MakeButton(ui.transform, "혈류 개방", new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(112, 112), new Vector2(-98, 293), new Color(.61f, .24f, .52f), runtime.Combat.BloodRend);
            MakeButton(ui.transform, "회피", new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(105, 105), new Vector2(-251, 145), new Color(.31f, .65f, .71f), runtime.Player.RequestDash);
            MakeButton(ui.transform, "점프", new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(105, 105), new Vector2(-405, 138), new Color(.30f, .56f, .62f), runtime.Player.RequestJump);
            MakeButton(ui.transform, "재생", new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(104, 104), new Vector2(-251, 292), new Color(.34f, .64f, .53f), runtime.Combat.CellRegeneration);
            MakeButton(ui.transform, "조사", new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(97, 97), new Vector2(-405, 295), new Color(.76f, .60f, .33f), runtime.Quest.TryCollectNearby);

            MakeButton(ui.transform, "메뉴", new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(120, 58), new Vector2(-35, -35), Ink, TogglePause);

            floatingText = MakeText(ui.transform, "Combat feedback", "",
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(820, 62),
                new Vector2(0, 180), 32, new Color(1f, .79f, .82f), TextAnchor.MiddleCenter);

            CreateMessagePanel(ui.transform);
            CreateIntro(ui.transform);
            CreateResult(ui.transform);
        }

        private void Update()
        {
            if (runtime == null || hpText == null) return;
            hpText.text = $"생명력  {Mathf.CeilToInt(runtime.Combat.Health)} / 100";
            energyText.text = $"L-Cell  {Mathf.CeilToInt(runtime.Combat.Charge)} / 100";
            erosionText.text = $"침식도  {Mathf.CeilToInt(runtime.Combat.Erosion)} / 100";
            ScaleBar(hpFill, runtime.Combat.Health / 100f);
            ScaleBar(energyFill, runtime.Combat.Charge / 100f);
            ScaleBar(erosionFill, runtime.Combat.Erosion / 100f);
            objectiveText.text = runtime.Quest.Objective;
            nearbyText.text = runtime.Quest.HasPickupNearby() ? "✦  L-Cell 잔향 발견 · [조사 / E]" : "";
            if (messagePanel != null && messagePanel.activeSelf && Time.unscaledTime > messageEnd)
                messagePanel.SetActive(false);
            if (floatingText != null && Time.unscaledTime > floatingEnd)
                floatingText.text = "";
            if (GameInput.Pressed(KeyCode.Escape) && !runtime.Finished && introPanel != null && !introPanel.activeSelf)
                TogglePause();
        }

        public void ShowMessage(string title, string body, float seconds)
        {
            messageTitle.text = title;
            messageBody.text = body;
            messagePanel.SetActive(true);
            messageEnd = Time.unscaledTime + seconds;
        }

        public void ShowFloatingText(string words)
        {
            floatingText.text = words;
            floatingEnd = Time.unscaledTime + 1.65f;
        }

        public void ShowResult(bool victory, string title, string body)
        {
            resultTitle.text = title;
            resultBody.text = body;
            resultPanel.SetActive(true);
        }

        private static void ScaleBar(Image fill, float value)
        {
            if (fill == null) return;
            fill.rectTransform.localScale = new Vector3(Mathf.Clamp01(value), 1f, 1f);
        }

        private void TogglePause()
        {
            if (runtime.Finished || introPanel.activeSelf) return;
            pausedByUser = !pausedByUser;
            runtime.Playing = !pausedByUser;
            ShowMessage(pausedByUser ? "일시정지" : "다시 탐험", pausedByUser ?
                "메뉴 버튼을 다시 누르면 탐험을 계속합니다." : "운명이 이어집니다.", 2f);
        }

        private void MakePortrait(Transform parent)
        {
            var texture = Resources.Load<Texture2D>("MinhoConcept");
            if (texture == null) return;
            GameObject go = new GameObject("Choi Min-ho anime concept art", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(204, 310);
            rt.anchoredPosition = new Vector2(-28, -105);
            RawImage raw = go.GetComponent<RawImage>();
            raw.texture = texture;
            raw.raycastTarget = false;
            raw.color = new Color(1, 1, 1, .94f);
        }

        private void CreateIntro(Transform parent)
        {
            introPanel = Fullscreen("Opening title", parent, new Color(.035f, .065f, .09f, .94f));
            MakeText(introPanel.transform, "Chapter caption", "THE IMMORTAL ORGAN  ·  ORIGINAL IP",
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(1200, 60),
                new Vector2(0, 260), 32, new Color(.93f, .78f, .66f), TextAnchor.MiddleCenter);
            MakeText(introPanel.transform, "Game title", "불멸의 기관",
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(1400, 125),
                new Vector2(0, 140), 95, Cream, TextAnchor.MiddleCenter);
            MakeText(introPanel.transform, "Subtitle", "제1장  ·  침식의 숲",
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(1100, 60),
                new Vector2(0, 36), 39, new Color(.97f, .58f, .65f), TextAnchor.MiddleCenter);
            MakeText(introPanel.transform, "Intro", "생명은 재생할 수 있다. 그러나 선택까지 복제할 수는 없다.",
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(1500, 80),
                new Vector2(0, -58), 27, Cream, TextAnchor.MiddleCenter);
            MakeButton(introPanel.transform, "탐험 시작", new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                new Vector2(350, 95), new Vector2(0, -182), Blood, () =>
                {
                    introPanel.SetActive(false);
                    runtime.StartAdventure();
                });
            MakeText(introPanel.transform, "Controls", "이동 WASD · 카메라 우클릭 드래그 · 점프 SPACE · 회피 SHIFT · 공격 J · 혈류 Q · 재생 R · 조사 E",
                new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(1680, 65),
                new Vector2(0, 50), 23, new Color(.73f, .82f, .82f), TextAnchor.MiddleCenter);
        }

        private void CreateResult(Transform parent)
        {
            resultPanel = Fullscreen("Chapter result", parent, new Color(.045f, .07f, .10f, .93f));
            resultTitle = MakeText(resultPanel.transform, "Result title", "",
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(1350, 125),
                new Vector2(0, 118), 70, Cream, TextAnchor.MiddleCenter);
            resultBody = MakeText(resultPanel.transform, "Result description", "",
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(1300, 170),
                new Vector2(0, -12), 32, new Color(.88f, .84f, .84f), TextAnchor.MiddleCenter);
            MakeButton(resultPanel.transform, "다시 도전", new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                new Vector2(350, 90), new Vector2(0, -190), Blood, runtime.Restart);
            resultPanel.SetActive(false);
        }

        private void CreateMessagePanel(Transform parent)
        {
            messagePanel = MakePanel(parent, "Quest notification", new Vector2(.5f, 1), new Vector2(.5f, 1),
                new Vector2(760, 127), new Vector2(0, -55), Ink);
            messageTitle = MakeText(messagePanel.transform, "Message headline", "",
                new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(720, 50),
                new Vector2(0, -7), 30, new Color(1f, .76f, .68f), TextAnchor.MiddleCenter);
            messageBody = MakeText(messagePanel.transform, "Message detail", "",
                new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(710, 63),
                new Vector2(0, -60), 23, Cream, TextAnchor.MiddleCenter);
            messagePanel.SetActive(false);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            var module = go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            module.AssignDefaultActions();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            DontDestroyOnLoad(go);
        }

        private static GameObject Fullscreen(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return go;
        }

        private GameObject MakePanel(Transform parent, string name, Vector2 anchor, Vector2 pivot,
            Vector2 size, Vector2 offset, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = offset;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private Text MakeText(Transform parent, string name, string label, Vector2 anchor, Vector2 pivot,
            Vector2 size, Vector2 offset, int fontSize, Color color, TextAnchor alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = offset;
            Text text = go.GetComponent<Text>();
            text.text = label;
            text.font = font;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = fontSize;
            text.raycastTarget = false;
            return text;
        }

        private Image Bar(Transform parent, string name, Vector2 offset, Vector2 size, Color color)
        {
            MakePanel(parent, name + " background", new Vector2(0, 1), new Vector2(0, 1),
                size, offset, new Color(.07f, .15f, .17f, .92f));
            GameObject fill = MakePanel(parent, name + " fill", new Vector2(0, 1), new Vector2(0, 1),
                size, offset, color);
            Image image = fill.GetComponent<Image>();
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            return image;
        }

        private Button MakeButton(Transform parent, string label, Vector2 anchor, Vector2 pivot,
            Vector2 size, Vector2 offset, Color color, UnityEngine.Events.UnityAction action)
        {
            GameObject panel = MakePanel(parent, label + " button", anchor, pivot, size, offset, color);
            Button button = panel.AddComponent<Button>();
            button.onClick.AddListener(action);
            MakeText(panel.transform, label + " label", label, new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                size - new Vector2(10, 10), Vector2.zero, 28, Cream, TextAnchor.MiddleCenter);
            return button;
        }
    }

    public sealed class TouchJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private PlayerMotor player;
        private RectTransform baseRect, thumb;
        private float radius;

        public void Initialize(PlayerMotor motor, RectTransform knob)
        {
            player = motor;
            thumb = knob;
            baseRect = GetComponent<RectTransform>();
            radius = baseRect.sizeDelta.x * .30f;
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);
        public void OnDrag(PointerEventData eventData)
        {
            if (player == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect,
                    eventData.position, eventData.pressEventCamera, out Vector2 local)) return;
            Vector2 limited = Vector2.ClampMagnitude(local, radius);
            thumb.anchoredPosition = limited;
            player.MobileMove = limited / radius;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (player != null) player.MobileMove = Vector2.zero;
            if (thumb != null) thumb.anchoredPosition = Vector2.zero;
        }
    }

    public sealed class TouchOrbitField : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private OrbitCamera orbit;
        private int pointerId = int.MinValue;
        private Vector2 last;

        public void Initialize(OrbitCamera cameraController) => orbit = cameraController;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.position.x < Screen.width * .39f) return;
            pointerId = eventData.pointerId;
            last = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (orbit == null || eventData.pointerId != pointerId) return;
            Vector2 delta = eventData.position - last;
            last = eventData.position;
            orbit.AddTouchOrbit(delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerId) pointerId = int.MinValue;
        }
    }
}
