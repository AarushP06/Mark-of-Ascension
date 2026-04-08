using Cainos.PixelArtTopDown_Basic;
using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    public class StagePlayerBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private string spawnObjectName = "PlayerSpawn";

        private void Awake()
        {
            if (PersistentPlayer.Instance != null || playerPrefab == null)
            {
                return;
            }

            var spawn = GameObject.Find(spawnObjectName);
            var position = spawn != null ? spawn.transform.position : Vector3.zero;
            var player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.name = "Player";
            player.tag = "Player";

            if (player.GetComponent<PersistentPlayer>() == null)
            {
                player.AddComponent<PersistentPlayer>();
            }

            if (player.GetComponent<TopDownCharacterController>() == null)
            {
                Debug.LogWarning("[StagePlayerBootstrap] Instantiated player prefab is missing TopDownCharacterController.");
            }
        }
    }
}
