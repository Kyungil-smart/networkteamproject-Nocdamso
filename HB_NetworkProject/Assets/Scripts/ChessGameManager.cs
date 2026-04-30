using Unity.Collections;
using UnityEngine;

public class ChessGameManager : MonoBehaviour
{
    public static ChessGameManager instance;

    // 기물 위치 관리 배열
    public ChessPieceManager[,] boardLayout = new ChessPieceManager[8, 8];

    // 각 칸의 하이라이트를 관리하는 배열
    public TileHighlighter[,] allTiles = new TileHighlighter[8, 8];

    public bool isWhiteTurn = true;

    private void Awake()
    {
        instance = this;
    }

    public void ChangeTurn()
    {
        isWhiteTurn = !isWhiteTurn;
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
