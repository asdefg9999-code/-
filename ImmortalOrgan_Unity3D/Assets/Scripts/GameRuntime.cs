using System;
using UnityEngine;
using UnityEngine.UI;

namespace ImmortalOrgan
{
    /// <summary>
    /// A fully procedural prototype. Even an empty Unity scene is enough to press Play.
    /// No prefab, asset-store dependency, paid package, or external service is required.
    /// </summary>
    public sealed class GameRuntime : MonoBehaviour
    {
        public static GameRuntime Instance { get; private set; }

        public PlayerMotor Player { get; private set; }
        public BloodCombat Combat { get; private set; }
        public OrbitCamera Orbit { get; private set; }
        public QuestDirector Quest { get; private set; }
        public GameHud Hud { get; private set; }

        public bool Playing { get; set; }
        public bool Finished { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreatePrototype()
        {
            if (FindFirstObjectByType<GameRuntime>() != null) return;
            var root = new GameObject("IMMORTAL ORGAN | Runtime");
            root.AddComponent<GameRuntime>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Physics.gravity = new Vector3(0f, -19.6f, 0f);
            if (Application.isMobilePlatform) Screen.orientation = ScreenOrientation.LandscapeLeft;

            // A camera must exist before procedurally generating the world. If
            // initialization throws, the user can still see a diagnostic UI.
            EnsureStartupCamera();
            try
            {
                WorldFactory.ConfigureAtmosphere();
                WorldFactory.BuildLandscape(transform);

                Player = WorldFactory.CreatePlayer(transform);
                Combat = Player.GetComponent<BloodCombat>();
                Quest = gameObject.AddComponent<QuestDirector>();
                Quest.Initialize(this);
                WorldFactory.PlaceChallenges(transform, Quest);

                Orbit = WorldFactory.CreateCamera(transform, Player.transform);
                Player.SetCamera(Orbit);
                Hud = gameObject.AddComponent<GameHud>();
                Hud.Initialize(this);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                ShowStartupError(error);
            }

            Playing = false;
            Finished = false;
        }

        private void EnsureStartupCamera()
        {
            if (Camera.main != null) return;

            var root = new GameObject("IMMORTAL ORGAN | Bootstrap Camera");
            root.tag = "MainCamera";
            root.transform.SetParent(transform, false);
            root.transform.position = new Vector3(0f, 4f, -35f);
            root.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            var camera = root.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.26f, .43f, .45f);
            root.AddComponent<AudioListener>();
        }

        private void ShowStartupError(Exception error)
        {
            Camera camera = Camera.main;
            if (camera != null)
                camera.backgroundColor = new Color(.24f, .05f, .10f);

            try
            {
                var overlay = new GameObject("IMMORTAL ORGAN | Startup Error", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                overlay.transform.SetParent(transform, false);
                var canvas = overlay.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;

                var message = new GameObject("Error Text", typeof(RectTransform), typeof(Text));
                message.transform.SetParent(overlay.transform, false);
                var rect = message.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(.08f, .15f);
                rect.anchorMax = new Vector2(.92f, .85f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var label = message.GetComponent<Text>();
                label.font = Font.CreateDynamicFontFromOSFont("Arial", 32);
                if (label.font == null)
                    label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.color = Color.white;
                label.alignment = TextAnchor.MiddleCenter;
                label.fontSize = 32;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Overflow;
                label.text = "IMMORTAL ORGAN - STARTUP ERROR\n" +
                    error.GetType().Name + ": " + error.Message +
                    "\nPlease capture this screen for troubleshooting.";
            }
            catch (Exception overlayError)
            {
                Debug.LogException(overlayError);
            }
        }

        public void StartAdventure()
        {
            Playing = true;
            Hud.ShowMessage("폐쇄 연구구역 · 외곽 수목원", "세 개의 L-Cell 잔향을 찾아라.", 4f);
        }

        public void Complete(bool victory)
        {
            if (Finished) return;
            Finished = true;
            Playing = false;
            Hud.ShowResult(victory,
                victory ? "제1장 완료 · 기관의 흔적" : "불완전한 재생",
                victory
                    ? "코어 잔향이 소멸했다. 최민호는 더 깊은 연구시설로 향하는 좌표를 확보했다."
                    : "L-Cell이 다시 뛰기를 기다린다. 다른 선택으로 다시 도전하자.");
        }

        public void Restart()
        {
            if (Instance == this) Instance = null;
            int index = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            if (index >= 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(index);
            }
            else
            {
                Destroy(gameObject);
                new GameObject("IMMORTAL ORGAN | Runtime").AddComponent<GameRuntime>();
            }
        }
    }
}
