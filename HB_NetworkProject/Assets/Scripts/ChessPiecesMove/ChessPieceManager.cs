using UnityEngine;

public abstract class ChessPieceManager : MonoBehaviour
{
    [Header("기물SO")]
    public ChessPiecesSO PiecesSO;

    [Header("기물 정보")]
    public bool isWhite;               // 기물 색 판별
    public bool isMoved;               // 이동 여부
    public Vector2Int GridPos;         // 현재 보드상 좌표

    public GameObject SelectionPiece;  // 기물 발밑 스프라이트

    protected virtual void Start()
    {
        if(SelectionPiece != null)
        {
            SelectionPiece.SetActive(false);
        }
    }
    
    public virtual void SetHighlight(bool isOn)
    {
        if (SelectionPiece != null)
        {
            SelectionPiece.SetActive(isOn);
        }
    }

    public virtual void MovePiece(Vector2Int gridPos)
    {
        // 이전 자리 비우기
        Vector2Int previousPos = this.GridPos;
        ChessGameManager.instance.boardLayout[previousPos.x, previousPos.y] = null;
        
        // TileConverter로 좌표 계산
        Vector3 targetWorldPos = TileConverter.Instance.GridToWorld(gridPos.x, gridPos.y, transform.position.y);

        // 계산된 위치로 이동
        transform.position = targetWorldPos;

        // 데이터 갱신
        this.GridPos = gridPos;
        this.isMoved = true;
        
        ChessGameManager.instance.boardLayout[gridPos.x, gridPos.y] = this;
    }

    // 각 기물별 이동 규칙
    public abstract bool CanMove(Vector2Int targetPos);

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
        if (targetPos == GridPos)
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
        distanceX = Mathf.Abs(targetPos.x - GridPos.x);
        distanceZ = Mathf.Abs(targetPos.y - GridPos.y);

        return true;
    }

    // 이동 경로 확인
    public bool isPathBlocked(Vector2Int targetPos)
    {
        // 이동 거리(방향) 계산
        int distanceX = targetPos.x - GridPos.x;
        int distanceZ = targetPos.y - GridPos.y;

        // 한 칸씩 이동할 방향
        // 거리가 양수면 +1, 음수면 -1, 0이면 0
        int oneStepX = (distanceX == 0) ? 0 : (distanceX > 0 ? 1 : -1);
        int oneStepZ = (distanceZ == 0) ? 0 : (distanceZ > 0 ? 1 : -1);

        // 현재 위치 바로 다음 칸 체크
        int checkX = GridPos.x + oneStepX;
        int checkZ = GridPos.y + oneStepZ;

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
}
