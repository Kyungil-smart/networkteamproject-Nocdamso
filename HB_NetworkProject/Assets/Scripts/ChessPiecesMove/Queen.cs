using UnityEngine;

public class Queen : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        // 보드 밖, 제자리, 아군 위치 체크 and 거리 계산
        if (!IsMoveValid(targetPos, out int distanceX, out int distanceZ))
        {
            return false;
        }

        bool isStraight = (distanceX == 0 || distanceZ == 0);
        bool isDiagonal = (distanceX == distanceZ);

        if (!isStraight && !isDiagonal)
        {
            return false;
        }

        // 경로상 다른 기물이 없어야 함
        return !isPathBlocked(targetPos);
    }    
}
