using Unity.Collections;
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

    public virtual void MovePiece(Vector3 worldPos, Vector2Int gridPos)
    {
        Vector2Int previousPos = this.GridPos;

        ChessGameManager.instance.boardLayout[previousPos.x, previousPos.y] = null;
        
        // 이동
        transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z);
        this.GridPos = gridPos;
        this.isMoved = true;
        
        Vector2Int currentPos = this.GridPos;
        ChessGameManager.instance.boardLayout[currentPos.x, currentPos.y] = this;
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

    public bool IsMoveValid(Vector2Int targetPos, out int distanceX, out int distanceY)
    {
        // 보드 범위 밖인가
        if (targetPos.x < 0 || targetPos.x >= 8 || targetPos.y < 0 || targetPos.y >= 8)
        {
            distanceX = distanceY = 0;
            return false;
        }

        // 제자리인가
        if (targetPos == GridPos)
        {
            distanceX = distanceY = 0;
            return false;
        }

        // 아군이 있나
        if (isAlly(targetPos.x, targetPos.y))
        {
            distanceX = distanceY = 0;
            return false;
        }

        distanceX = Mathf.Abs(targetPos.x - GridPos.x);
        distanceY = Mathf.Abs(targetPos.y - GridPos.y);

        return true;
    }
}
