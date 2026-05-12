using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MarkOfAscension.Gameplay;
using MarkOfAscension.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private string lobbySceneName = "SC_Lobby";

    private void Awake()
    {
        GameAudio.EnsureInstance();

        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
    }

    public void StartGame()
    {
        PersistentPlayer.DestroyPersistentInstance();
        PowerRewardNotificationUI.DestroyInstance();
        SceneManager.LoadScene(lobbySceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
