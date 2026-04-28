using UnityEngine;

public class Queen : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        return true;
    }    
}
