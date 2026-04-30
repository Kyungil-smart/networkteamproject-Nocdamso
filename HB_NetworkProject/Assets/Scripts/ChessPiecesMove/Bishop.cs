using UnityEngine;

public class Bishop : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        // 보드 밖, 제자리, 아군 위치 체크 and 거리 계산
        if (!IsMoveValid(targetPos, out int distanceX, out int distanceZ))
        {
            return false;
        }

        // 가로 세로 이동 거리가 같아야 대각선 이동
        if (distanceX != distanceZ)
        {
            return false;
        }

        // 경로상 다른 기물이 없어야 함
        return !isPathBlocked(targetPos);
    }    
}
