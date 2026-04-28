using UnityEngine;

public class Pawn : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        return true;
    }   
}
