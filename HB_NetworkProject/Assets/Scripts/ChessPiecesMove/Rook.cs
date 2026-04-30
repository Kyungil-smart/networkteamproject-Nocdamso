using UnityEngine;

public class Rook : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        // 보드 밖, 제자리, 아군 위치 체크 and 거리 계산
        if (!IsMoveValid(targetPos, out int distanceX, out int distanceZ))
        {
            return false;
        }

        // 가로나 세로 둘 중 하나가 0 이어야 직선 이동
        if (distanceX != 0 && distanceZ != 0)
        {
            return false;
        }

        // 경로상 다른 기물이 없어야 함
        return !isPathBlocked(targetPos);
    }
}
