using UnityEngine;

public class King : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        return true;
    }    
}
