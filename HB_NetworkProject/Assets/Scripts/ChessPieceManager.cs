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
        // 이동
        transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z);


        this.GridPos = gridPos;
        this.isMoved = true;
    }

    // 각 기물별 이동 규칙
    public abstract bool CanMove(Vector2Int targetPos);
}
