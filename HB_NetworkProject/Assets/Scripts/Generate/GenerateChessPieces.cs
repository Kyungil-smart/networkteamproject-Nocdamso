using System;
using UnityEngine;

public class GenerateChessPieces : MonoBehaviour
{
    public GenerateChessBoard BoardScript;

    [Header("백 (0:폰, 1:룩, 2:나이트, 3:비숍, 4:퀸, 5:킹)")]
    public GameObject[] WhitePieces;

    [Header("흑 (0:폰, 1:룩, 2:나이트, 3:비숍, 4:퀸, 5:킹)")]
    public GameObject[] BlackPieces;

    private void Start()
    {
        GeneratePieces();
    }

    public void GeneratePieces()
    {
        int[,] layout =
        {
            { 2, 3, 4, 5, 6, 4, 3, 2},
            { 1, 1, 1, 1, 1, 1, 1, 1},
            { 0, 0, 0, 0, 0, 0, 0, 0},
            { 0, 0, 0, 0, 0, 0, 0, 0},
            { 0, 0, 0, 0, 0, 0, 0, 0},
            { 0, 0, 0, 0, 0, 0, 0, 0},
            { 1, 1, 1, 1, 1, 1, 1, 1},
            { 2, 3, 4, 5, 6, 4, 3, 2}
        };

        for (int z = 0; z < 8; z++)
        {
            for (int x = 0; x < 8; x++)
            {
                ChessPieces type = (ChessPieces)layout[z, x];
                if (type == ChessPieces.Empty) continue;

                Vector3 spawnPos = TilePos(x, z);

                GameObject[] chessTeamPiece = (z < 2) ? BlackPieces : WhitePieces;
                GameObject chessPiece = chessTeamPiece[(int)type - 1];

                if (chessPiece != null)
                {
                    GameObject gameObject = Instantiate(chessPiece, spawnPos, Quaternion.identity);
                    gameObject.transform.parent = this.transform;

                    if (z > 5) gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                }
            }
        }
    }

    private Vector3 TilePos(int x, int z)
    {
        float size = BoardScript.TileSize;

        float xPos = x * size;
        float zPos = z * size;

        return new Vector3(xPos, 1f, zPos);
    }
}
