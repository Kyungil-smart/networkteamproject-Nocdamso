using Unity.Netcode;
using UnityEngine;

public abstract class ChessPieceManager : NetworkBehaviour
{
    [Header("기물SO")]
    public ChessPiecesSO PiecesSO;

    [Header("기물 정보")]
    public ChessPieces pieceType;

    public NetworkVariable<Vector2Int> GridPos = new NetworkVariable<Vector2Int>(
        new Vector2Int(-1, -1), 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
    
    public NetworkVariable<TeamColor> teamColor = new NetworkVariable<TeamColor>(
        TeamColor.White, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);                             // 현재 보드상 좌표
    public NetworkVariable<bool> isMoved = new NetworkVariable<bool>(
    false, 
    NetworkVariableReadPermission.Everyone, 
    NetworkVariableWritePermission.Server);                                  // 이동 여부
    public bool isWhite => teamColor.Value == TeamColor.White;              // 기물 색 판별

    public GameObject SelectionPiece;  // 기물 발밑 스프라이트

    protected virtual void Start()
    {
        if(SelectionPiece != null)
        {
            SelectionPiece.SetActive(false);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (ChessGameManager.instance != null && GridPos.Value.x >= 0)
        {
            RegisterToBoard(GridPos.Value);        
        }

        // 서버에서 좌표 설정하거나 바꿀 때마다 실행
        GridPos.OnValueChanged += HandleGridPosChanged;
              
        // 초기화
        UpdateWorldPosition(GridPos.Value);
    }

    private void HandleGridPosChanged(Vector2Int oldPos, Vector2Int newPos)
    {
        UpdateWorldPosition(newPos);

        if (ChessGameManager.instance != null)
        {
            if (oldPos.x >= 0 && oldPos.x < 8 && oldPos.y >= 0 && oldPos.y < 8)
            {
                if (ChessGameManager.instance.boardLayout[oldPos.x, oldPos.y] == this)
                {
                    ChessGameManager.instance.boardLayout[oldPos.x, oldPos.y] = null;
                }
            }

            if (newPos.x >= 0 && newPos.x < 8)
            {
                RegisterToBoard(newPos);
            }
        }
    }

    private void RegisterToBoard(Vector2Int pos)
    {
        if (ChessGameManager.instance == null) return;

        var existingPiece = ChessGameManager.instance.boardLayout[pos.x, pos.y];
        if (existingPiece != null && existingPiece != this)
        {
            Debug.LogWarning($"[Register] {pos} 위치에 이미 {existingPiece.pieceType}이(가) 있습니다. {this.pieceType}으로 교체합니다.");
        }

        ChessGameManager.instance.boardLayout[pos.x, pos.y] = this;

        if (IsServer && pieceType == ChessPieces.King)
        {
            ChessGameManager.instance.UpdateKingPosition(isWhite, pos);
        }
    }

    // 데이터가 변경될 때마다 실제 게임 화면의 위치를 옮겨줌
    private void OnGridPosChanged(Vector2Int oldPos, Vector2Int newPos)
    {
        UpdateWorldPosition(newPos);
    }

    // 그리드 좌표를 실제 3D 월드 좌표로 변환하여 적용
    private void UpdateWorldPosition(Vector2Int gridPos)
    {
        if (TileConverter.Instance != null)
        {
            transform.position = TileConverter.Instance.GridToWorld(gridPos.x, gridPos.y);
        }
    }

    // 기물은 서버가 소유함, 그러므로 이동요청
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestMoveServerRpc(Vector2Int targetGridPos, RpcParams rpcParams = default)
    {
        Debug.Log($"이동 요청 수신: {name} -> {targetGridPos}");

        // 서버에서 이동이 가능한지 검증요청
        if (CanMove(targetGridPos))
        {
            // 앙파상
            if (pieceType == ChessPieces.Pawn && Mathf.Abs(targetGridPos.x - GridPos.Value.x) == 1 &&
                ChessGameManager.instance.boardLayout[targetGridPos.x, targetGridPos.y] == null)
            {
                // 대각선 이동인데 목적지가 비어있다면
                Vector2Int capturePos = new Vector2Int(targetGridPos.x, GridPos.Value.y);
                // 옆에 있는 폰 잡음
                CaptureTargetClientRpc(capturePos);
            }

            else
            {
                // 이동할 곳에 상대 기물이 있다면 잡기
                ChessPieceManager target = ChessGameManager.instance.boardLayout[targetGridPos.x, targetGridPos.y];
                if (target != null && target.isWhite != this.isWhite)
                {
                    CaptureTargetClientRpc(targetGridPos);
                }    
            }
            
            int moveDistanceY = Mathf.Abs(targetGridPos.y - GridPos.Value.y);
            if (pieceType == ChessPieces.Pawn && moveDistanceY == 2)
            {
                ChessGameManager.instance.enPassantTarget = this;
            }
            
            // 킹이 2칸 이동했다면 캐슬링으로 간주
            if (pieceType == ChessPieces.King && Mathf.Abs(targetGridPos.x - GridPos.Value.x) == 2)
            {
                PerformCastling(targetGridPos);
            }

            // 이동가능하면 모든 클라이언트에게 이동 명령
            MoveClientRpc(targetGridPos);

            bool IsPromotion = (pieceType == ChessPieces.Pawn && (isWhite ? targetGridPos.y == 7 : targetGridPos.y == 0));
            
            if (IsPromotion)
            {
                ShowPromotionUIClientRpc(targetGridPos, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
            }
            else
            {
                ChessGameManager.instance.ChangeTurn();
            }
        }

        else
        {
            Debug.LogWarning("이동 검증 실패!");
        }
    }

    private void PerformCastling(Vector2Int kingTargetPos)
    {
        int rookPosX = (kingTargetPos.x > GridPos.Value.x) ? 7 : 0;
        int rookTargetX = (kingTargetPos.x > GridPos.Value.x) ? 5 : 3;

        ChessPieceManager rook = ChessGameManager.instance.boardLayout[rookPosX, GridPos.Value.y];
        if (rook != null)
        {
            rook.MoveClientRpc(new Vector2Int(rookTargetX, GridPos.Value.y));
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestPromotionServerRpc(ChessPieces pieceType, Vector2Int pos)
    {
        // 폰이 있던 자리를 보드 레이아웃에서 비움
        GameObject newPrefab = ChessGameManager.instance.GetPiecePrefab(pieceType, isWhite);

        if (newPrefab == null) return;

        // 새 기물 생성 및 스폰
        GameObject newPiece = Instantiate(newPrefab);
        var netObj = newPiece.GetComponent<NetworkObject>();
        netObj.Spawn();

        // 데이터 초기화
        var manager = newPiece.GetComponent<ChessPieceManager>();
        manager.GridPos.Value = pos;
        manager.teamColor.Value = this.teamColor.Value;
        manager.isMoved.Value = true;
        
        // 보드 레이아웃에 새 기물 등록
        ChessGameManager.instance.boardLayout[pos.x, pos.y] = manager;

        // 폰 제거
        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }

        // 턴 교대
        ChessGameManager.instance.ChangeTurn();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ShowPromotionUIClientRpc(Vector2Int targetPos, RpcParams rpcParams = default)
    {
        if (PromotionUI.Instance != null)
        {
            PromotionUI.Instance.Show(targetPos, this);
        }
    }

    [ClientRpc]
    private void CaptureTargetClientRpc(Vector2Int pos)
    {
        ChessPieceManager target = ChessGameManager.instance.boardLayout[pos.x, pos.y];
        if (target != null)
        {
            if (AudioManager.instance != null) AudioManager.instance.Play(SoundType.Capture);

            // 만약 킹을 잡는 로직이 호출되었다면 게임 종료만 처리
            if (target.pieceType == ChessPieces.King)
            {
                // 체크메이트 로직이 정상 작동한다면 이 코드는 예외 방지
                return; 
            }

            ChessGameManager.instance.boardLayout[pos.x, pos.y] = null;
            Destroy(target.gameObject);
        }
    }

    // 서버가 모든 클라이언트 화면 갱신
    [ClientRpc]
    private void MoveClientRpc(Vector2Int targetGridPos)
    {
        // 기존 위치 비우기
        Vector2Int oldPos = GridPos.Value;
        if (oldPos.x >= 0 && oldPos.y >= 0)
        {
            if (ChessGameManager.instance.boardLayout[oldPos.x, oldPos.y] == this)
                ChessGameManager.instance.boardLayout[oldPos.x, oldPos.y] = null;
        }

        // 실제 좌표 및 데이터 갱신
        if (IsServer)
        {
            GridPos.Value = targetGridPos;
            isMoved.Value = true;
            if (pieceType == ChessPieces.King)
                ChessGameManager.instance.UpdateKingPosition(isWhite, targetGridPos);
        }

        // 이동 및 새 위치 등록
        transform.position = TileConverter.Instance.GridToWorld(targetGridPos.x, targetGridPos.y, transform.position.y);
        ChessGameManager.instance.boardLayout[targetGridPos.x, targetGridPos.y] = this;

        // 효과음 및 체크 상태 확인
        if (AudioManager.instance != null) AudioManager.instance.Play(SoundType.Move);
        if (IsServer) ChessGameManager.instance.CheckStatus();
    }

    private void MovePieceRpc(Vector2Int gridPos)
    {
        // 기물 옮기기 전 자리 비우기
        Vector2Int previousPos = this.GridPos.Value;

        if (previousPos.x >= 0 && previousPos.x < 8)
        {
            if (ChessGameManager.instance.boardLayout[previousPos.x, previousPos.y] == this)
            {
                ChessGameManager.instance.boardLayout[previousPos.x, previousPos.y] = null;
            }
        }

        // 데이터 갱신
        if (IsServer)
        {
            this.GridPos.Value = gridPos;   

            this.isMoved.Value = true;

            if (pieceType == ChessPieces.King)
            {
                ChessGameManager.instance.UpdateKingPosition(isWhite, gridPos);
            }
        }
        
        transform.position = TileConverter.Instance.GridToWorld(gridPos.x, gridPos.y, transform.position.y);
        ChessGameManager.instance.boardLayout[gridPos.x, gridPos.y] = this;

        RegisterToBoard(gridPos);
    }

    
    // 각 기물별 이동 규칙
    public abstract bool CanMove(Vector2Int targetPos);


    // 출발지와 목적지 확인
    public bool IsMoveValid(Vector2Int targetPos, out int distanceX, out int distanceZ)
    {
        // 보드 범위 밖인가
        if (targetPos.x < 0 || targetPos.x >= 8 || targetPos.y < 0 || targetPos.y >= 8)
        {
            distanceX = distanceZ = 0;
            return false;
        }

        // 제자리인가
        if (targetPos == GridPos.Value)
        {
            distanceX = distanceZ = 0;
            return false;
        }

        // 목표 지점에 아군이 있나
        if (isAlly(targetPos.x, targetPos.y))
        {
            distanceX = distanceZ = 0;
            return false;
        }

        // 이동 거리 계산 (절대값)
        distanceX = Mathf.Abs(targetPos.x - GridPos.Value.x);
        distanceZ = Mathf.Abs(targetPos.y - GridPos.Value.y);

        return true;
    }

    // 이동 경로 확인
    public bool isPathBlocked(Vector2Int targetPos)
    {
        // 이동 거리(방향) 계산
        int distanceX = targetPos.x - GridPos.Value.x;
        int distanceZ = targetPos.y - GridPos.Value.y;

        // 한 칸씩 이동할 방향
        // 거리가 양수면 +1, 음수면 -1, 0이면 0
        int oneStepX = (distanceX == 0) ? 0 : (distanceX > 0 ? 1 : -1);
        int oneStepZ = (distanceZ == 0) ? 0 : (distanceZ > 0 ? 1 : -1);

        // 현재 위치 바로 다음 칸 체크
        int checkX = GridPos.Value.x + oneStepX;
        int checkZ = GridPos.Value.y + oneStepZ;

        // 목적지까지 한 칸씩 체크
        while (checkX != targetPos.x || checkZ != targetPos.y)
        {
            // 해당 경로에 기물이 있다면 막힘
            if (ChessGameManager.instance.boardLayout[checkX, checkZ] != null)
            {
                return true;
            }

            // 좌표 갱신
            checkX += oneStepX;
            checkZ += oneStepZ;
        }

        return false;
    }
    public virtual void SetHighlight(bool isOn)
    {
        if (SelectionPiece != null)
        {
            SelectionPiece.SetActive(isOn);
        }
    }

    public bool isAlly(int targetX, int targetZ)
    {
        ChessPieceManager targetPiece = ChessGameManager.instance.boardLayout[targetX, targetZ];

        if (targetPiece != null)
        {
            if (targetPiece.isWhite == this.isWhite)
            {
                return true;
            }
        }

        return false;
    }
}
