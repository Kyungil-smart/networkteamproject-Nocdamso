using UnityEngine;

public class Pawn : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        // 보드 밖, 제자리, 아군 위치 체크 and 거리 계산
        if (!IsMoveValid(targetPos, out int distanceX, out int distanceZ))
        {
            return false;
        }

        // 흑, 백에 따른 전진방향 설정
        int direction = isWhite ? -1: 1;
        int forwardDistance = targetPos.y - GridPos.y;

        if (distanceX == 0)
        {
            // 한칸 전진
            if (forwardDistance == direction)
            {
                // 앞에 기물이 없어야 함
                return ChessGameManager.instance.boardLayout[targetPos.x, targetPos.y] == null;
            }

            // 첫 이동 시 두 칸 전진 가능
            if (!isMoved && forwardDistance == direction * 2)
            {
                return !isPathBlocked(targetPos) && 
                ChessGameManager.instance.boardLayout[targetPos.x, targetPos.y] == null;
            }
        }

        // 상대 기물을 잡을 떈 대각선으로 이동
        else if (distanceX == 1 &&forwardDistance == direction)
        {
            // 대각선에 상대 기물이 있어야만 함
            ChessPieceManager target = ChessGameManager.instance.boardLayout[targetPos.x, targetPos.y];
            return target != null && target.isWhite != this.isWhite;
        }

        return false;
    }
}
