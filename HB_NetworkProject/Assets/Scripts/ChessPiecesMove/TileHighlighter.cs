using UnityEngine;

public class TileHighlighter : MonoBehaviour
{
    public Vector2Int GridPos;                  // 타일의 좌표
    public GameObject MoveToTileHighlighter;    // 이동 가능한 타일 표시

    private void Start()
    {
        if (TileConverter.Instance != null)
        {
            GridPos = TileConverter.Instance.WorldToGrid(transform.position);
        }

        if (MoveToTileHighlighter != null)
        {
            MoveToTileHighlighter.SetActive(false);
        }
    }

    public void SetTileHighlighter(bool canMove)
    {
        if (MoveToTileHighlighter == null)
        {
            return;
        }
        
        MoveToTileHighlighter.SetActive(canMove);
    }
}
