using UnityEngine;
using UnityEngine.SceneManagement;
using MarkOfAscension.UI;

namespace MarkOfAscension.Gameplay
{
    public class PlayerProgression : MonoBehaviour
    {
        [Header("Stage Rewards")]
        [SerializeField] private int stage01MaxHealthBonus = 2;
        [SerializeField] private int stage02AttackDamageBonus = 1;
        [SerializeField] private float stage02AttackCooldownReduction = 0.05f;

        private PlayerHealth playerHealth;
        private PlayerAttack playerAttack;
        private PowerRewardNotificationUI rewardNotificationUi;
        private bool clearedStage01;
        private bool clearedStage02;
        private bool clearedStage03;
        private string pendingRewardTitle;
        private string pendingRewardBody;

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();
            playerAttack = GetComponent<PlayerAttack>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public bool GrantRewardForStage(string stageName)
        {
            if (string.IsNullOrWhiteSpace(stageName))
            {
                return false;
            }

            switch (stageName)
            {
                case "Stage01":
                    if (clearedStage01)
                    {
                        return false;
                    }

                    clearedStage01 = true;
                    playerHealth?.AddMaxHealthBonus(stage01MaxHealthBonus, true);
                    playerAttack?.UnlockPoisonDamage();
                    QueueRewardNotification(
                        "LEVEL UP",
                        $"New Power Unlocked: <color=#7CFF7C>Poison Strike</color>\nYour attacks now inflict poison damage.\nMax Health +{stage01MaxHealthBonus}");
                    Debug.Log($"[PlayerProgression] Cleared {stageName}. Max health increased by {stage01MaxHealthBonus}.");
                    return true;

                case "Stage02":
                    if (clearedStage02)
                    {
                        return false;
                    }

                    clearedStage02 = true;
                    playerAttack?.AddAttackDamageBonus(stage02AttackDamageBonus);
                    playerAttack?.ReduceAttackCooldown(stage02AttackCooldownReduction);
                    playerAttack?.UnlockFireDamage();
                    playerHealth?.RestoreFullHealth();
                    QueueRewardNotification(
                        "LEVEL UP",
                        $"New Power Unlocked: <color=#FF9B5A>Flame Strike</color>\nYour attacks now inflict fire damage.\nAttack Damage +{stage02AttackDamageBonus}\nAttack Speed Up");
                    Debug.Log($"[PlayerProgression] Cleared {stageName}. Attack damage increased by {stage02AttackDamageBonus} and attack cooldown reduced by {stage02AttackCooldownReduction:0.##}.");
                    return true;

                case "Stage03":
                    if (clearedStage03)
                    {
                        return false;
                    }

                    clearedStage03 = true;
                    Debug.Log($"[PlayerProgression] Cleared {stageName}. Final stage completed.");
                    return true;

                default:
                    return false;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (string.IsNullOrWhiteSpace(pendingRewardTitle) || scene.name == "MainMenu")
            {
                return;
            }

            EnsureNotificationUi();
            rewardNotificationUi?.ShowMessage(pendingRewardTitle, pendingRewardBody);
            pendingRewardTitle = null;
            pendingRewardBody = null;
        }

        private void QueueRewardNotification(string title, string body)
        {
            pendingRewardTitle = title;
            pendingRewardBody = body;
        }

        private void EnsureNotificationUi()
        {
            if (rewardNotificationUi != null)
            {
                return;
            }

            var existingUi = PowerRewardNotificationUI.Instance;
            if (existingUi != null)
            {
                rewardNotificationUi = existingUi;
                return;
            }

            var notificationObject = new GameObject("PowerRewardNotificationUI", typeof(RectTransform));
            rewardNotificationUi = notificationObject.AddComponent<PowerRewardNotificationUI>();
        }
    }
}
