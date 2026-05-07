using Unity.Netcode;
using UnityEngine;

public class PlayerTeamColor : NetworkBehaviour
{
    public NetworkVariable<TeamColor> PlayerColor = new NetworkVariable<TeamColor>();

    public override void OnNetworkSpawn()
    {
        // 팀 값이 정해지면 카메라 업데이트
        PlayerColor.OnValueChanged += (oldValue, newValue) =>
        {
            ViewPoint(newValue);    
        };

        // 값이 할당되었는데 늦게 스폰된 경우
        if (IsOwner)
        {
            ViewPoint(PlayerColor.Value);
        }
    }

    private void ViewPoint(TeamColor teamColor)
    {
        // 내 화면만 움직여야함
        if (!IsOwner) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        if (teamColor == TeamColor.White)
        {
            mainCam.transform.position = new Vector3(8, 16, 19);
            mainCam.transform.rotation = Quaternion.Euler(60, 180, 0);
            Debug.Log("<color=white>백, 선공!");
        }

        else if (teamColor == TeamColor.Black)
        {
            mainCam.transform.position = new Vector3(8, 16, -3);
            mainCam.transform.rotation = Quaternion.Euler(60, 0, 0);
            Debug.Log("<color=white>흑, 후공!");
        }
    }
}
