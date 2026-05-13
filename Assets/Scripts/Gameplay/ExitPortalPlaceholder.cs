using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class ExitPortalPlaceholder : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool isUnlocked;
        [SerializeField] private bool hideWhileLocked = true;
        [Header("Locked Visual")]
        [SerializeField] private Color lockedColor = new Color(0.2f, 0.24f, 0.3f, 0.25f);

        [Header("Unlocked Visual")]
        [SerializeField] private Color unlockedColorA = new Color(0.48f, 0.95f, 0.58f, 0.95f);
        [SerializeField] private Color unlockedColorB = new Color(0.22f, 0.7f, 1f, 0.9f);
        [SerializeField] private float pulseSpeed = 1.8f;
        [SerializeField] private float colorShiftSpeed = 1.35f;
        [SerializeField] private float bobAmplitude = 0.08f;
        [SerializeField] private float bobSpeed = 1.6f;
        [SerializeField] private float pulseScale = 0.08f;
        [SerializeField] private float appearDuration = 0.65f;
        [SerializeField] private float spinSpeed = 55f;

        private Collider2D portalCollider;
        private SpriteRenderer[] portalRenderers;
        private ScenePortal scenePortal;
        private bool wasUnlocked;
        private Transform visualRoot;
        private Vector3 visualRootStartPosition;
        private Vector3[] originalScales;
        private float unlockTime = -1f;

        private void Awake()
        {
            portalCollider = GetComponent<Collider2D>();
            portalRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            scenePortal = GetComponent<ScenePortal>();
            visualRoot = transform;
            visualRootStartPosition = visualRoot.localPosition;
            wasUnlocked = isUnlocked;
            originalScales = new Vector3[portalRenderers.Length];

            for (var i = 0; i < portalRenderers.Length; i++)
            {
                if (portalRenderers[i] == null)
                {
                    continue;
                }

                originalScales[i] = portalRenderers[i].transform.localScale;
            }

            ApplyVisualState();
        }

        private void Update()
        {
            if (!isUnlocked || portalRenderers == null || portalRenderers.Length == 0)
            {
                return;
            }

            var elapsed = unlockTime < 0f ? appearDuration : Time.time - unlockTime;
            var appearT = appearDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / appearDuration);
            var easedAppear = Mathf.SmoothStep(0f, 1f, appearT);
            var pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
            var colorWave = 0.5f + 0.5f * Mathf.Sin(Time.time * colorShiftSpeed);
            var baseColor = Color.Lerp(unlockedColorA, unlockedColorB, colorWave);
            var scaleMultiplier = 1f + pulseScale * pulse;
            var bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;

            visualRoot.localPosition = visualRootStartPosition + new Vector3(0f, bobOffset * easedAppear, 0f);

            for (var i = 0; i < portalRenderers.Length; i++)
            {
                var portalRenderer = portalRenderers[i];
                if (portalRenderer == null)
                {
                    continue;
                }

                portalRenderer.enabled = true;
                var rendererColor = baseColor;
                rendererColor.a *= easedAppear;
                portalRenderer.color = rendererColor;

                var targetScale = originalScales[i] == Vector3.zero ? Vector3.one : originalScales[i];
                portalRenderer.transform.localScale = targetScale * Mathf.Lerp(0.55f, scaleMultiplier, easedAppear);
                portalRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, Time.time * spinSpeed * (i % 2 == 0 ? 1f : -1f));
            }
        }

        public void SetUnlocked(bool unlocked)
        {
            if (!wasUnlocked && unlocked)
            {
                GameAudio.PlayPortalUnlock();
                unlockTime = Time.time;
            }

            isUnlocked = unlocked;
            wasUnlocked = unlocked;
            ApplyVisualState();
        }

        private void ApplyVisualState()
        {
            if (portalCollider != null)
            {
                portalCollider.enabled = isUnlocked;
                portalCollider.isTrigger = true;
            }

            if (scenePortal != null)
            {
                scenePortal.SetPortalEnabled(isUnlocked);
            }

            if (portalRenderers == null)
            {
                return;
            }

            for (var i = 0; i < portalRenderers.Length; i++)
            {
                var portalRenderer = portalRenderers[i];
                if (portalRenderer == null)
                {
                    continue;
                }

                if (!isUnlocked)
                {
                    portalRenderer.transform.localRotation = Quaternion.identity;
                    var targetScale = originalScales != null && i < originalScales.Length && originalScales[i] != Vector3.zero
                        ? originalScales[i]
                        : Vector3.one;
                    portalRenderer.transform.localScale = targetScale;
                }

                portalRenderer.enabled = isUnlocked || !hideWhileLocked;
                portalRenderer.color = isUnlocked ? unlockedColorA : lockedColor;
            }

            if (!isUnlocked)
            {
                unlockTime = -1f;
                visualRoot.localPosition = visualRootStartPosition;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isUnlocked || !other.CompareTag(playerTag))
            {
                return;
            }

            Debug.Log("[ExitPortalPlaceholder] Portal entered. Hook your next scene load here later."
            );
        }
    }
}
