using System.Collections;
using UnityEngine;

namespace MarkOfAscension.UI
{
    public class PowerRewardNotificationUI : MonoBehaviour
    {
        public static PowerRewardNotificationUI Instance { get; private set; }

        [SerializeField] private float displayDuration = 4f;

        private Coroutine displayRoutine;
        private string currentTitle;
        private string currentBody;
        private bool isVisible;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private Texture2D backgroundTexture;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (backgroundTexture != null)
            {
                Destroy(backgroundTexture);
            }
        }

        public static void DestroyInstance()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        public void ShowMessage(string title, string body)
        {
            currentTitle = title;
            currentBody = body;

            if (displayRoutine != null)
            {
                StopCoroutine(displayRoutine);
            }

            displayRoutine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            isVisible = true;
            yield return new WaitForSecondsRealtime(displayDuration);
            isVisible = false;
            displayRoutine = null;
        }

        private void OnGUI()
        {
            if (!isVisible)
            {
                return;
            }

            if (panelStyle == null || titleStyle == null || bodyStyle == null)
            {
                CreateStyles();
            }

            var width = 760f;
            var height = 280f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            var panelRect = new Rect(x, y, width, height);
            var titleRect = new Rect(x + 40f, y + 28f, width - 80f, 64f);
            var bodyRect = new Rect(x + 50f, y + 105f, width - 100f, 130f);

            GUI.Box(panelRect, GUIContent.none, panelStyle);
            GUI.Label(titleRect, currentTitle, titleStyle);
            GUI.Label(bodyRect, currentBody, bodyStyle);
        }

        private void CreateStyles()
        {
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0.04f, 0.06f, 0.05f, 0.96f));
            backgroundTexture.Apply();

            panelStyle = new GUIStyle();
            panelStyle.normal.background = backgroundTexture;
            panelStyle.border = new RectOffset(6, 6, 6, 6);

            titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontSize = 30;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.82f, 1f, 0.82f, 1f);

            bodyStyle = new GUIStyle();
            bodyStyle.alignment = TextAnchor.UpperCenter;
            bodyStyle.fontSize = 22;
            bodyStyle.wordWrap = true;
            bodyStyle.richText = true;
            bodyStyle.normal.textColor = Color.white;
        }
    }
}
