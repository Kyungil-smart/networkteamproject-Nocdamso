using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscMenuController : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject _escMenuPanel;
    [SerializeField] private GameObject _gameOverPanel;

    [Header("메뉴 버튼")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _lobbyButton;
    [SerializeField] private Button _quitButton;

    private void Awake()
    {
        _continueButton.onClick.AddListener(ToggleMenu);
        _lobbyButton.onClick.AddListener(GoToLobby);
        _quitButton.onClick.AddListener(QuitGame);

        _escMenuPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // 현재 씬 이름 가져오기
            string currentScene = SceneManager.GetActiveScene().name;

            // Init, Game Scene일 때만 ESC 입력 감지
            if (currentScene == "GameScene")
            {
                if (_gameOverPanel == null || !_gameOverPanel.activeSelf)
                {
                    ToggleMenu();                    
                }
            }                  
        }
    }

    private void ToggleMenu()
    {
        bool isActive = !_escMenuPanel.activeSelf;
        _escMenuPanel.SetActive(isActive);
    }

    private void GoToLobby()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene("InitScene");
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        // 에디터 플레이모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드 게임 종료
        Application.Quit();
#endif
    }
}
