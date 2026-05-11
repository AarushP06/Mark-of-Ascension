using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PersistentPlayer : MonoBehaviour
    {
        public static PersistentPlayer Instance { get; private set; }

        private Rigidbody2D body;

        public static void DestroyPersistentInstance()
        {
            if (Instance == null)
            {
                return;
            }

            Destroy(Instance.gameObject);
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            body = GetComponent<Rigidbody2D>();
            gameObject.tag = "Player";
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var spawnName = string.IsNullOrWhiteSpace(SceneTransitionState.NextSpawnName)
                ? FindFallbackSpawnName()
                : SceneTransitionState.NextSpawnName;

            var spawn = GameObject.Find(spawnName);
            if (spawn != null)
            {
                transform.position = spawn.transform.position;
                if (body != null)
                {
                    body.linearVelocity = Vector2.zero;
                }
            }

            SceneTransitionState.NextSpawnName = null;
        }

        private static string FindFallbackSpawnName()
        {
            if (GameObject.Find("PlayerSpawn") != null)
            {
                return "PlayerSpawn";
            }

            return "PlayerSpawnPoint";
        }
    }
}
