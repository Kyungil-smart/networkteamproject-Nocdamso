using Unity.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Data.Common;

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
    public GameObject CheckPanel;
    public GameObject MyTurnPanel;

    [Header("체크 상태")]
    public bool IsWhiteChecked = false;
    public bool IsBlackChecked = false;

    [Header("킹 위치 추적")]
    public Vector2Int WhiteKingPos;
    public Vector2Int BlackKingPos;

    // 이번 턴에 두 칸 전진한 폰을 저장
    public ChessPieceManager enPassantTarget { get; set; }

    [System.Serializable]
    public struct PiecePrefabMap
    {
        public ChessPieces type;
        public GameObject whitePrefab;
        public GameObject blackPrefab;
    }

    [Header("기물 프리팹 설정")]
    public System.Collections.Generic.List<PiecePrefabMap> piecePrefabs;

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

    // ---------- 네트워크 이벤트 관리 ------------

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton != null)
        {
            // 클라이언트 접속/해제 이벤트 구독
            NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnected;
            NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnected;

            isWhiteTurn.OnValueChanged += (oldVal, newVal) =>
            {
                if(IsServer) CheckStatus();    

                PlayMyTurnSound(newVal);
            };
        }
    }

    public override void OnNetworkDespawn()
    {
        if(NetworkManager.Singleton != null)
        {
            // 구독해제
            NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerDisconnected;
            NetworkManager.Singleton.OnClientConnectedCallback -= PlayerConnected;
        }
    }

    // 상대방 나갔을 때
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
        // 상대가 다시 들어오면 카운트다운 중지
        if(_victoryCountdownRoutine != null)
        {
            StopCoroutine(_victoryCountdownRoutine);
            _victoryCountdownRoutine = null;
        }
    }


    private IEnumerator VictoryCountdownRoutine()
    {
        yield return new WaitForSeconds(5f);

        // 5초 후 혼자라면 기권승
        if (NetworkManager.Singleton.ConnectedClients.Count <= 1)
        {
            ShowGameOverClientRpc("Opponent Disconnected");
        }
    }

    private void PlayMyTurnSound(bool currentTurnIsWhite)
    {
        // 로컬 플레이어의 팀 컬러 정보 가져오기
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerTeamColor>();
        if (localPlayer == null) return;

        TeamColor myColor = localPlayer.PlayerColor.Value;

        // 현재 턴의 색상과 내 색상이 일치하는지 확인
        bool isMyTurn = (currentTurnIsWhite && myColor == TeamColor.White) ||
                        (!currentTurnIsWhite && myColor == TeamColor.Black);

        if (isMyTurn)
        {
            if (AudioManager.instance != null)
            {
                // 내 차례 알림음 재생
                AudioManager.instance.Play(SoundType.MyTurn);       
            }

            StartCoroutine(ShowMyTurnUICoroutine());
        }
    }

    // ---------------- 게임 UI -----------------------


    [ClientRpc]
    public void ShowGameOverClientRpc(string winnerName)
    {
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);

            // 기권승/패
            if (winnerName.Contains("Disconnected"))
            {
                WinnerText.text = winnerName;
                // 기권인 경우 승리 사운드 재생
                AudioManager.instance.Play(SoundType.Victory);
            }
            else
            {
                WinnerText.text = winnerName + " Wins!";

                //승리/패배 사운드 분기
                var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerTeamColor>();
                string myColorStr = localPlayer.PlayerColor.Value == TeamColor.White ? "White" : "Black";

                if (AudioManager.instance != null)
                {
                    // 승자 이름과 내 컬러가 같으면 승, 아니면 패
                    if (winnerName == myColorStr)
                    {
                        AudioManager.instance.Play(SoundType.Victory);
                    }

                    else
                    {
                        AudioManager.instance.Play(SoundType.Defeat);
                    }
                }
            }
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

    private IEnumerator ShowMyTurnUICoroutine()
{
    if (MyTurnPanel != null)
    {
        MyTurnPanel.SetActive(true);
        yield return new WaitForSeconds(0.5f); // 0.5초 대기
        MyTurnPanel.SetActive(false);
    }
}

    // ---------------- 체스 게임 로직 ----------------
    public void ChangeTurn()
    {
        if (!IsServer) return;
        isWhiteTurn.Value = !isWhiteTurn.Value;
    }

    // 체스보드를 보고 체크 상태인지 확인
    public void CheckStatus()
    {
        if (!IsServer) return;

        bool whiteInCheck = IsKingAttacked(WhiteKingPos, TeamColor.Black);
        bool blackInCheck = IsKingAttacked(BlackKingPos, TeamColor.White);

        // 체크 됐을 때 사운드 재생
        if ((whiteInCheck && !IsWhiteChecked) || (blackInCheck && !IsBlackChecked))
        {
            PlayCheckSoundClientRpc();
        }

        IsWhiteChecked = whiteInCheck;
        IsBlackChecked = blackInCheck;
    }

    [ClientRpc]
    private void PlayCheckSoundClientRpc()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play(SoundType.Check);   
        }

        StartCoroutine (ShowCheckUICoroutine());
    }

    private IEnumerator ShowCheckUICoroutine()
    {
        if (CheckPanel != null)
        {
            CheckPanel.SetActive(true);
            yield return new WaitForSeconds(1f);
            CheckPanel.SetActive(false);
        }
    }

    // 킹의 위치 찾기
    public Vector2Int GetKingPosition(bool isWhite)
    {
        return isWhite ? WhiteKingPos : BlackKingPos;
    }   

    public void UpdateKingPosition(bool isWhite, Vector2Int newPos)
    {
        if (isWhite) WhiteKingPos = newPos;
        else BlackKingPos = newPos;
    }

    // 특정 칸이 상대에게 공격받고 있는지 확인
    public bool IsKingAttacked(Vector2Int targetPos, TeamColor attackerColor)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPieceManager piece = boardLayout[x, y];

                // 상대팀의 기물을 찾으면, 그 기물이 targetPos로 갈 수 있는지
                if (piece != null && piece.teamColor.Value == attackerColor)
                {
                    if (piece.CanMove(targetPos))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    } 

    // 프로모션 등 기물 생성이 필요할 때 프리팹을 반환하는 함수
    public GameObject GetPiecePrefab(ChessPieces type, bool isWhite)
    {
        foreach (var map in piecePrefabs)
        {
            if (map.type == type)
            {
                return isWhite ? map.whitePrefab : map.blackPrefab;
            }
        }
        Debug.LogError($"[ChessGameManager] {type}에 해당하는 프리팹을 찾을 수 없습니다!");
        return null;
    }

    // ------------- 타일 하이라이트 --------------

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
                Vector2Int targetPos = new Vector2Int(x, z);
                if (piece.CanMove(new Vector2Int(x, z)))
                {
                    allTiles[x, z].SetTileHighlighter(true);
                }
            }
        }
    }
}
