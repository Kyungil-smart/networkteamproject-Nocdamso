using UnityEngine;

public class Rook : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        return true;
    }
}
