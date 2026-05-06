using System;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class NetworkLauncher : MonoBehaviour
{
    public TextMeshProUGUI joinCodeDisplay;

    public TMP_InputField codeInput;

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

    // 호스트로 시작
    public async void StartHost()
    {
        // RelayServiceManager에 방 생성 요청
        string code = await RelayServiceManager.Instance.CreateRelayAsync();

        // 코드가 발급됐다면
        if (!string.IsNullOrEmpty(code))
        {
            Debug.Log($"[Launcher] 방 생성 완료, 코드: {code}");

            if (joinCodeDisplay != null)
            {
                joinCodeDisplay.text = $"방 코드: {code}";
            }

            NetworkManager.Singleton.StartHost();
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
        // 입력창에 적힌 글자를 변수에 담음
        string code = codeInput.text;

        if (!string.IsNullOrEmpty(code))
        {
            bool success = await RelayServiceManager.Instance.JoinRelayAsync(code);
            if (success)
            {
                NetworkManager.Singleton.StartClient();
            }
        }
    }
}
