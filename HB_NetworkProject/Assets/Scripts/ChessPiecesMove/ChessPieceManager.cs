using Unity.Netcode;
using UnityEngine;

public abstract class ChessPieceManager : NetworkBehaviour
{
    [Header("기물SO")]
    public ChessPiecesSO PiecesSO;

    [Header("기물 정보")]
    public bool isWhite;               // 기물 색 판별
    public bool isMoved;               // 이동 여부
    public NetworkVariable<Vector2Int> GridPos = new NetworkVariable<Vector2Int>();         // 현재 보드상 좌표

    public GameObject SelectionPiece;  // 기물 발밑 스프라이트

    protected virtual void Start()
    {
        if(SelectionPiece != null)
        {
            SelectionPiece.SetActive(false);
        }
    }

    // 기물은 서버가 소유함, 그러므로 이동요청
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestMoveServerRpc(Vector2Int targetGridPos)
    {
        Debug.Log($"[서버] 이동 요청 수신: {name} -> {targetGridPos}");

        // 서버에서 이동이 가능한지 검증요청
        if (CanMove(targetGridPos))
        {
            Debug.Log("[서버] 이동 검증 성공.");
            // 이동할 곳에 상대 기물이 있다면 잡기
            ChessPieceManager target = ChessGameManager.instance.boardLayout[targetGridPos.x, targetGridPos.y];
            if (target != null && target.isWhite != this.isWhite)
            {
                CaptureTargetClientRpc(targetGridPos);
            }

            // 이동가능하면 모든 클라이언트에게 이동 명령
            MoveClientRpc(targetGridPos);

            // 서버에서 턴 교대
            ChessGameManager.instance.ChangeTurn();
        }

        else
        {
            Debug.LogWarning("[서버] 이동 검증 실패!");
        }
    }

    [ClientRpc]
    private void CaptureTargetClientRpc(Vector2Int pos)
    {
        ChessPieceManager target = ChessGameManager.instance.boardLayout[pos.x, pos.y];
        if (target != null)
        {
            if (target is King)
            {
                string winner = target.isWhite? "Black" : "White";
                
                ChessGameManager.instance.ShowGameOverClientRpc(winner);
            }

            ChessGameManager.instance.boardLayout[pos.x, pos.y] = null;

            Destroy(target.gameObject);
        }
    }

    // 서버가 모든 클라이언트 화면 갱신
    [ClientRpc]
    private void MoveClientRpc(Vector2Int targetGridPos)
    {
        MovePieceRpc(targetGridPos);
    }

    private void MovePieceRpc(Vector2Int gridPos)
    {
        // 기물 옮기기 전 자리 비우기
        Vector2Int previousPos = this.GridPos.Value;

        if (previousPos.x >= 0 && ChessGameManager.instance.boardLayout[previousPos.x, previousPos.y] == this)
        {
            ChessGameManager.instance.boardLayout[previousPos.x, previousPos.y] = null;
        }

        // TileConverter로 좌표 계산
        Vector3 targetWorldPos = TileConverter.Instance.GridToWorld(gridPos.x, gridPos.y, transform.position.y);

        // 계산된 위치로 이동
        transform.position = targetWorldPos;

        // 데이터 갱신
        if (IsServer)
        {
            this.GridPos.Value = gridPos;   
        }

        this.isMoved = true;
        
        ChessGameManager.instance.boardLayout[gridPos.x, gridPos.y] = this;
    }

    public override void OnNetworkSpawn()
    {
        if (ChessGameManager.instance != null)
        {
            Vector2Int currentPos = GridPos.Value;
            
            if (currentPos.x >= 0 && currentPos.x < 8 && currentPos.y >= 0 && currentPos.y < 8)
            {
                ChessGameManager.instance.boardLayout[currentPos.x, currentPos.y] = this;               
            }
        }
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
