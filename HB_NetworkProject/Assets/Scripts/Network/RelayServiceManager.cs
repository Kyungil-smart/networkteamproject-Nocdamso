using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayServiceManager : MonoBehaviour
{
    public static RelayServiceManager Instance { get; private set; }

    private void Awake()
    {
        // 중복 방지
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 파괴되지 않음
        DontDestroyOnLoad(gameObject);
    }

    // 호스트가 방 만들어달라고 요청하는 함수
    public async Task<string> CreateRelayAsync(int maxPlayers = 1)
    {
        try
        {
            // 유니티 서버에 인원수만큼 방 할당요청
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            // 입장 코드 생성
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            // 방의 주소를 서버 데이터로 만듦
            RelayServerData serverData = AllocationUtils.ToRelayServerData(allocation, "dtls");

            // 내 NetworkManager가 이 주소를 사용하도록 함
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);

            // 생성된 입장 코드 보여줌
            return joinCode;
        }

        catch (Exception e)
        {
            Debug.LogError($"[relay] 방 생성 중 에러: {e.Message}");
            return null;
        }
    }

    // 클라이언트가 코드를 가지고 방에 들어가는 함수
    public async Task<bool> JoinRelayAsync(string joinCode)
    {
        try
        {
            // 입장 코드를 서버에 보내 방 찾기 요청
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // 받은 정보를 서버 데이터로 정리
            RelayServerData serverData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");

            // 내 NetworkManager가 이 주로소 접속하도록 함
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);

            return true;
        }

        catch (Exception e)
        {
            Debug.LogError($"[Relay] 방 접속 중 에러: {e.Message}");
            return false;
        }
    }
}
