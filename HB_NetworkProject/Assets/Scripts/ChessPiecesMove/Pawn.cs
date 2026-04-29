using UnityEngine;

public class Pawn : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        if (isAlly(targetPos.x, targetPos.y))
        {
            return false;
        }

        int direction = isWhite ? -1: 1;

        // 폰은 앞으로만 움직이기 때문에 절대값X
        int distanceX = targetPos.x - GridPos.x;
        int distanceY = targetPos.y - GridPos.y;

        if (distanceX == 0 && distanceY == direction)
        {
            if (ChessGameManager.instance.boardLayout[targetPos.x, targetPos.y] == null)
            {
                return true;
            }
            
        }

        if (!isMoved && distanceX == 0 && distanceY == direction * 2)
        {
            // 1칸 앞 좌표
            int frontPos = GridPos.y + direction;

            // 1칸 앞이 비어있는지
            bool isOneSquareEmpty = ChessGameManager.instance.boardLayout[targetPos.x, frontPos] == null;
            // 2칸 앞이 비어있는지
            bool isTwoSquareEmpty = ChessGameManager.instance.boardLayout[targetPos.x, targetPos.y] == null;

            if (isOneSquareEmpty && isTwoSquareEmpty)
            {
                return true;
            }
        }

        if (Mathf.Abs(distanceX) == 1 && distanceY == direction)
        {
            if (ChessGameManager.instance.boardLayout[targetPos.x, targetPos.y] != null && !isAlly(targetPos.x, targetPos.y))
            {
                return true;
            }            
        }

        return false;
    }
}
