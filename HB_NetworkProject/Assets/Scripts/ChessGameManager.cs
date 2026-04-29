using Unity.Collections;
using UnityEngine;

public class ChessGameManager : MonoBehaviour
{
    public static ChessGameManager instance;
    public ChessPieceManager[,] boardLayout = new ChessPieceManager[8, 8];

    private void Awake()
    {
        instance = this;
    }
}
