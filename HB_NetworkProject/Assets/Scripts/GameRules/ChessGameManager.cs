using Unity.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ChessGameManager : NetworkBehaviour
{
    public static ChessGameManager instance;

    // 기물 위치 관리 배열
    public ChessPieceManager[,] boardLayout = new ChessPieceManager[8, 8];

    // 각 칸의 하이라이트를 관리하는 배열
    public TileHighlighter[,] allTiles = new TileHighlighter[8, 8];

    public NetworkVariable<bool> isWhiteTurn = new NetworkVariable<bool>(true);

    [Header("게임 UI")]
    public GameObject GameOverPanel;
    public TextMeshProUGUI WinnerText;
    public Button LobbyButton;

    private Coroutine _victoryCountdownRoutine;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0; 
        Application.targetFrameRate = 60;
        instance = this;
    }

    private void Start()
    {
        if(LobbyButton != null)
        {
            LobbyButton.onClick.AddListener(ReturnToLobby);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnected;
            NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if(NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerDisconnected;
            NetworkManager.Singleton.OnClientConnectedCallback -= PlayerConnected;
        }
    }

    private void PlayerDisconnected(ulong clientId)
    {
        if (GameOverPanel.activeSelf) return;

        // 내가 호스트일 때 클라이언트 나감
        if (IsServer && clientId != NetworkManager.Singleton.LocalClientId)
        {
            // 5초 대기 후 승리 패널띄우는 코루틴 실행
            if(_victoryCountdownRoutine != null)
            {
                StopCoroutine(_victoryCountdownRoutine);
            }

            _victoryCountdownRoutine = StartCoroutine(VictoryCountdownRoutine());
        }
    }

    private void PlayerConnected(ulong ClientId)
    {
        if(_victoryCountdownRoutine != null)
        {
            StopCoroutine(_victoryCountdownRoutine);
            _victoryCountdownRoutine = null;
        }
    }


    private IEnumerator VictoryCountdownRoutine()
    {
        yield return new WaitForSeconds(5f);

        if (NetworkManager.Singleton.ConnectedClients.Count <= 1)
        {
            ShowVictory("Opponent Disconnected");
        }
    }

    private void ShowVictory(string message)
    {
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
            
            if (WinnerText != null)
            {
                WinnerText.text = message;
            }
        }
    }

    [ClientRpc]
    public void ShowGameOverClientRpc(string winnerName)
    {
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
            WinnerText.text = winnerName +" Win!";
        }
    }

    public void ReturnToLobby()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        StopAllCoroutines();

        SceneManager.LoadScene("InitScene");
    }

    public void ChangeTurn()
    {
        if (!IsServer) return;
        isWhiteTurn.Value = !isWhiteTurn.Value;
    }

    // 모든 타일의 하이라이트를 끔
    public void ClearAllHighlights()
    {
        foreach (var tile in allTiles)
        {
            if (tile != null) tile.SetTileHighlighter(false);
        }
    }

    // 선택한 기물이 갈 수 있는 칸 하이라이트 ON
    public void ShowPossibleMoves(ChessPieceManager piece)
    {
        ClearAllHighlights();

        for (int x= 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                if (piece.CanMove(new Vector2Int(x, z)))
                {
                    allTiles[x, z].SetTileHighlighter(true);
                }
            }
        }
    }
}
