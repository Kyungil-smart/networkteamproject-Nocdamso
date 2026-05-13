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

    // ---------------- 게임 UI -----------------------


    [ClientRpc]
    public void ShowGameOverClientRpc(string winnerName)
    {
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
            WinnerText.text = winnerName.Contains("Disconnected") ? winnerName : winnerName + " Win";

            AudioManager.instance.Play(SoundType.Victory);
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

    // ---------------- 체스 게임 로직 ----------------
    public void ChangeTurn()
    {
        // 앙파상은 해당 턴에만 가능하므로 초기화
        enPassantTarget = null;

        if (!IsServer) return;

        // 턴 넘기기 전 다음 턴 플레이어의 상태 확인
        bool nextTurnIsWhite = !isWhiteTurn.Value;

        if (IsCheckmate(nextTurnIsWhite))
        {
            // 체크메이트! 현재 플레이어 색상 전달
            string winner = isWhiteTurn.Value ? "White" : "Black";
            ShowGameOverClientRpc(winner + " (Checkmate)");
            // 게임 끝났으니 턴 넘기지 않음
            return;
        }

        // 체크메이트가 아니면 턴 교체
        isWhiteTurn.Value = nextTurnIsWhite;
    }

    // 체스보드를 보고 체크 상태인지 확인
    public void CheckStatus()
    {
        if (!IsServer) return;


        Vector2Int whiteKingPos = GetKingPosition(true);
        Vector2Int blackKingPos = GetKingPosition(false);

        bool whiteInCheck = false;
        bool blackInCheck = false;

        for (int x = 0; x < 8; x++)
        {
            for (int z= 0; z < 8; z++)
            {
                var piece = boardLayout[x, z];
                if (piece == null) continue;

                // 상대 기물이 우리 킹을 공격할 수 있는지
                if (piece.teamColor.Value == TeamColor.White && piece.CanMove(blackKingPos)) blackInCheck = true;
                if (piece.teamColor.Value == TeamColor.Black && piece.CanMove(whiteKingPos)) whiteInCheck = true;
            }
        }

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
        AudioManager.instance.Play(SoundType.Check);
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

    // 특정 기물을 옮겼을 때 우리 킹이 안전한지 가상 시뮬레이션
    public bool SimulateMoveSafe(ChessPieceManager piece, Vector2Int targetPos)
    {
        Vector2Int originalPos = piece.GridPos.Value;
        ChessPieceManager capturedPiece = boardLayout[targetPos.x, targetPos.y];
        bool isSafe = false;

        // 가상으로 움직임
        boardLayout[originalPos.x, originalPos.y] = null;
        boardLayout[targetPos.x, targetPos.y] = piece;

        // 우리 킹 위치 찾고 공격 여부확인
        TeamColor myColor = piece.teamColor.Value;
        
        TeamColor enemyColor = (myColor == TeamColor.White) ? TeamColor.Black : TeamColor.White;
        Vector2Int kingPos = GetKingPosition(myColor == TeamColor.White);

        if (!IsKingAttacked(kingPos, enemyColor))
        {
            isSafe = true;
        }

        // 위치 원복
        boardLayout[originalPos.x, originalPos.y] = piece;
        boardLayout[targetPos.x, targetPos.y] = capturedPiece;

        return isSafe;
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

    public bool IsCheckmate(bool isWhite)
    {
        // 현재 체크상태가 아니면 체크메이트도 아님
        CheckStatus();
        if (isWhite ? !IsWhiteChecked : !IsBlackChecked) return false;

        // 현재 모든 기물 루프 돌며 체크를 피할 수 있는지 확인
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPieceManager piece = boardLayout[x, y];

                // 현재 턴인 팀의 기물만 확인
                if (piece != null && piece.isWhite == isWhite)
                {
                    // 이 기물이 갈 수 있는 모든 칸 시뮬레이션
                    for (int tryX = 0; tryX < 8; tryX++)
                    {
                        for (int tryY = 0; tryY < 8; tryY++)
                        {
                            Vector2Int targetPos = new Vector2Int(tryX, tryY);

                            // 기물의 기본 이동 규칙상 이동 가능 + 그 수를 뒀을 때 킹이 안전하다면
                            if (piece.CanMove(targetPos))
                            {
                                if (SimulateMoveSafe(piece, targetPos))
                                {
                                    // 벗어날 수 있다면 체크메이트가 아님
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        }

        // 탈출구가 없다면 체크메이트
        return true;
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
                if (piece.CanMove(new Vector2Int(x, z)))
                {
                    allTiles[x, z].SetTileHighlighter(true);
                }
            }
        }
    }
}
