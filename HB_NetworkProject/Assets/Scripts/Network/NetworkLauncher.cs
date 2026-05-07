using System;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLauncher : MonoBehaviour
{
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;
    [SerializeField] private Button _copyButton;
    [SerializeField] private Button _startButton;
    [SerializeField] private TMP_InputField _joinCodeDisplay;
    [SerializeField] private TMP_InputField _codeInput;


    private async void Start()
    {
        try
        {
            // 유니티 엔진 서비스 초기화
            await UnityServices.InitializeAsync();

            // 로그인이 안 되어 있다면 익명 로그인 시도
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log ($"[Launcher] 로그인 성공, ID: {AuthenticationService.Instance.PlayerId}");
        }

        catch (Exception e)
        {
            Debug.LogError($"[Launcher] 로그인 중 에러: {e.Message}");
        }
    }

    private void OnEnable()
    {
        if (_hostButton != null) _hostButton.onClick.AddListener(StartHost);
        if (_clientButton != null) _clientButton.onClick.AddListener(CodeInputUI);
        if (_copyButton != null) _copyButton.onClick.AddListener(CopyCode);
        if (_startButton != null)
        {
            _startButton.onClick.AddListener(SceneChange);
            _startButton.gameObject.SetActive(false);
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientStatusChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientStatusChanged;
        }

        UpdateStartButton();
    }
    
    private void OnDisable()
    {
        if (_hostButton != null) _hostButton.onClick.RemoveListener(StartHost);
        if (_clientButton != null) _clientButton.onClick.RemoveListener(CodeInputUI);
        if (_copyButton != null) _copyButton.onClick.RemoveListener(CopyCode);
        if (_startButton != null) _startButton.onClick.RemoveListener(SceneChange);
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= ClientStatusChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= ClientStatusChanged;
        }
    }

    // 호스트로 시작
    public async void StartHost()
    {
        // RelayServiceManager에 방 생성 요청
        string code = await RelayServiceManager.Instance.CreateRelayAsync();

        // 코드가 발급됐다면
        if (!string.IsNullOrEmpty(code))
        {
            Debug.Log($"[Launcher] 방 생성 완료, 코드: {code}");

            if (_joinCodeDisplay != null)
            {
                _joinCodeDisplay.text = code;
            }

            NetworkManager.Singleton.StartHost();

            if (_startButton != null) _startButton.gameObject.SetActive(true);
        }

    }

    // 클라이언트로 시작
    public async void StartClient(string inputCode)
    {
        // 입력받은 코드가 6자리인지
        if (string.IsNullOrEmpty(inputCode))
        {
            return;
        }

        // 코드에 맞는 방 주소를 찾기
        bool success = await RelayServiceManager.Instance.JoinRelayAsync(inputCode);

        // 주소를 찾았다면
        if (success)
        {
            Debug.Log($"[Launcher] {inputCode} 방 접속");

            NetworkManager.Singleton.StartClient();
        }
    }

    public async void CodeInputUI()
    {
        Debug.Log("버튼이 클릭되었습니다");

        // 입력창에 적힌 글자를 변수에 담음
        string code = _codeInput.text;
        Debug.Log($"코드 값: {code}");

        if (!string.IsNullOrEmpty(code))
        {
            Debug.Log("접속 중");
            bool success = await RelayServiceManager.Instance.JoinRelayAsync(code);

            if (success)
            {
                Debug.Log("접속 완료");
                NetworkManager.Singleton.StartClient();
            }
        }
    }

    private void CopyCode()
    {
        if (_joinCodeDisplay != null)
        {
            GUIUtility.systemCopyBuffer = _joinCodeDisplay.text;
            Debug.Log("코드 복사 완료");
        }
    }

    private void SceneChange()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);            
        }
    }

    // 클라이언트 접속/해제 시 실행
    private void ClientStatusChanged(ulong clientID)
    {
        UpdateStartButton();
    }

    private void UpdateStartButton()
    {
        if(NetworkManager.Singleton == null) return;

        if (_startButton == null) return;

        // 호스트만 버튼 활성화
        if (NetworkManager.Singleton.IsServer)
        {
            // 호스트 + 클라이언트 = 2명일 경우 버튼 활성화
            int clientCount = NetworkManager.Singleton.ConnectedClients.Count;
            _startButton.interactable = (clientCount == 2);

            _startButton.gameObject.SetActive(true);

            Debug.Log($"접속 인원 : {clientCount}");
        }

        else
        {
            // 클리이언트는 버튼은 보이나
            _startButton.gameObject.SetActive(true);

            // 클릭할 순 없음
            _startButton.interactable = false;
        }
    }
}
