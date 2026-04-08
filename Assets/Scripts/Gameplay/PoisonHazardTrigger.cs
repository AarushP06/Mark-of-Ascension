using UnityEngine;

namespace MarkOfAscension.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class PoisonHazardTrigger : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private int damagePerTick = 1;
        [SerializeField] private float tickInterval = 1f;

        private float nextDamageTime;

        private void Reset()
        {
            var trigger = GetComponent<Collider2D>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag))
            {
                return;
            }

            if (Time.time < nextDamageTime)
            {
                return;
            }

            nextDamageTime = Time.time + tickInterval;
            other.gameObject.SendMessage("TakeDamage", damagePerTick, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"[PoisonHazardTrigger] {other.name} touched poison for {damagePerTick} damage."
                + " Add a TakeDamage(int) method later to replace this placeholder log."
            );
        }
    }
}
