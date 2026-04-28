using UnityEngine;

public class Knight : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        return true;
    }    
}
