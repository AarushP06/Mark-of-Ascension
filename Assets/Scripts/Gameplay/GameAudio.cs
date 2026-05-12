using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarkOfAscension.Gameplay
{
    public class GameAudio : MonoBehaviour
    {
        public static GameAudio Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource musicSource;
        private AudioClip lobbyMusicClip;
        private AudioClip playerAttackClip;
        private AudioClip enemyHitClip;
        private AudioClip playerHurtClip;
        private AudioClip enemyDeathClip;
        private AudioClip bossEventClip;
        private AudioClip poisonTickClip;
        private AudioClip fireTickClip;
        private AudioClip portalUnlockClip;
        private AudioClip portalEnterClip;
        private AudioClip levelUpClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.28f;

            lobbyMusicClip = LoadClip("lobby_music");
            playerAttackClip = LoadClip("player_attack");
            enemyHitClip = LoadClip("enemy_hit");
            playerHurtClip = LoadClip("player_hurt");
            enemyDeathClip = LoadClip("enemy_death");
            bossEventClip = LoadClip("boss_event");
            poisonTickClip = LoadClip("poison_tick");
            fireTickClip = LoadClip("fire_tick");
            portalUnlockClip = LoadClip("portal_unlock");
            portalEnterClip = LoadClip("portal_enter");
            levelUpClip = LoadClip("level_up");

            SceneManager.sceneLoaded += OnSceneLoaded;
            PlayMusicForScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public static void EnsureInstance()
        {
            if (Instance != null)
            {
                return;
            }

            var audioObject = new GameObject("GameAudio");
            audioObject.AddComponent<GameAudio>();
        }

        public static void PlayPlayerAttack()
        {
            EnsureInstance();
            Instance?.PlayOneShot(Instance.playerAttackClip, 0.55f, 0.96f, 1.04f);
        }

        public static void PlayEnemyHit()
        {
            EnsureInstance();
            Instance?.PlayOneShot(Instance.enemyHitClip, 0.45f, 0.96f, 1.04f);
        }

        public static void PlayPlayerHurt()
        {
            EnsureInstance();
            Instance?.PlayOneShot(Instance.playerHurtClip, 0.65f, 0.98f, 1.02f);
        }

        public static void PlayEnemyDeath(bool boss)
        {
            EnsureInstance();
            var clip = boss ? Instance?.bossEventClip : Instance?.enemyDeathClip;
            var volume = boss ? 0.8f : 0.5f;
            Instance?.PlayOneShot(clip, volume, 0.95f, 1.05f);
        }

        public static void PlayBossSpawn()
        {
            EnsureInstance();
            Instance?.PlayOneShot(Instance.bossEventClip, 0.75f, 0.92f, 0.98f);
        }

        public static void PlayPoisonTick()
        {
            EnsureInstance();
            Instance?.PlayOneShot(Instance.poisonTickClip, 0.24f, 0.98f, 1.03f);
        }

        public static void PlayFireTick()
        {
            EnsureInstance();
            Instance?.PlayOneShot(Instance.fireTickClip, 0.28f, 0.98f, 1.03f);
        }

        public static void PlayPortalUnlock()
        {
            EnsureInstance();
            Instance?.PlayOneShot(Instance.portalUnlockClip, 0.6f, 0.98f, 1.02f);
        }

        public static void PlayPortalEnter()
        {
            EnsureInstance();
            Instance?.PlayOneShot(Instance.portalEnterClip, 0.5f, 0.98f, 1.02f);
        }

        public static void PlayLevelUp()
        {
            EnsureInstance();
            Instance?.PlayOneShot(Instance.levelUpClip, 0.7f, 0.98f, 1.02f);
        }

        private AudioClip LoadClip(string clipName)
        {
            return Resources.Load<AudioClip>($"Audio/{clipName}");
        }

        private void PlayOneShot(AudioClip clip, float volume, float minPitch, float maxPitch)
        {
            if (clip == null || sfxSource == null)
            {
                return;
            }

            sfxSource.pitch = Random.Range(minPitch, maxPitch);
            sfxSource.PlayOneShot(clip, volume);
            sfxSource.pitch = 1f;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayMusicForScene(scene.name);
        }

        private void PlayMusicForScene(string sceneName)
        {
            if (musicSource == null)
            {
                return;
            }

            var clip = sceneName == "MainMenu" || sceneName == "SC_Lobby"
                ? lobbyMusicClip
                : null;

            if (clip == null)
            {
                if (musicSource.isPlaying)
                {
                    musicSource.Stop();
                }

                musicSource.clip = null;
                return;
            }

            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.clip = clip;
            musicSource.Play();
        }
    }
}
