using UnityEngine;

public class Bishop : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        return true;
    }    
}
