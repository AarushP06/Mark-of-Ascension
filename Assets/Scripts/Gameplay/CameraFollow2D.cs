using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private float orthographicSize = 9f;
        [SerializeField] private bool clampX;
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private bool clampY = true;
        [SerializeField] private float minY = -8.5f;
        [SerializeField] private float maxY = 20.5f;

        private Camera cachedCamera;
        private Transform target;

        private void Awake()
        {
            ApplyCameraSettings();
        }

        private void OnValidate()
        {
            ApplyCameraSettings();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                var targetObject = GameObject.FindGameObjectWithTag(targetTag);
                if (targetObject != null)
                {
                    target = targetObject.transform;
                }
            }

            if (target == null)
            {
                return;
            }

            var desiredPosition = target.position + offset;
            var verticalHalfSize = cachedCamera.orthographicSize;
            var horizontalHalfSize = verticalHalfSize * cachedCamera.aspect;

            if (clampX)
            {
                desiredPosition.x = ClampAxis(desiredPosition.x, minX, maxX, horizontalHalfSize);
            }

            if (clampY)
            {
                desiredPosition.y = ClampAxis(desiredPosition.y, minY, maxY, verticalHalfSize);
            }

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );
        }

        private void ApplyCameraSettings()
        {
            if (cachedCamera == null)
            {
                cachedCamera = GetComponent<Camera>();
            }

            if (cachedCamera == null)
            {
                return;
            }

            cachedCamera.orthographic = true;
            cachedCamera.orthographicSize = orthographicSize;
        }

        private static float ClampAxis(float value, float min, float max, float halfSize)
        {
            var minCenter = min + halfSize;
            var maxCenter = max - halfSize;

            if (minCenter > maxCenter)
            {
                return (min + max) * 0.5f;
            }

            return Mathf.Clamp(value, minCenter, maxCenter);
        }
    }
}
