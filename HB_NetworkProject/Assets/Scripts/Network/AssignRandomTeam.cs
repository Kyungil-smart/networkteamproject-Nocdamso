using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AssignRandomTeam : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(WaitAndAssignTeams());
        }
    }

    private IEnumerator WaitAndAssignTeams()
    {
        float timeout = 3f;
        float timer = 0f;

        while (NetworkManager.Singleton.ConnectedClientsList.Count < 2 && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        // NetworkManager가 관리하는 목록에서 플레이어 정보 가져오기
        var clients = NetworkManager.Singleton.ConnectedClientsList;

        if (clients.Count >= 2)
        {
            AssignTeams(clients);
        }

        else
        {
            Debug.LogWarning("상대 접속 실패, 초기 화면으로");

            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("InitScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            
        }
    }

    private void AssignTeams(IReadOnlyList<NetworkClient> clients)
    {
        bool isHostWhite = UnityEngine.Random.value > 0.5f;

        foreach (var client in clients)
        {
            if(client.PlayerObject != null)
            {
                if (client.PlayerObject.TryGetComponent<PlayerTeamColor>(out var playerTeamColor))
                {
                    TeamColor assigned = (client.ClientId == NetworkManager.ServerClientId)
                                        ? (isHostWhite ? TeamColor.White : TeamColor.Black)
                                        : (isHostWhite ? TeamColor.Black : TeamColor.White);

                    playerTeamColor.PlayerColor.Value = assigned;
                }
            }            
        }
    }
}
