using UnityEngine;

public class King : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        // 보드 밖, 제자리, 아군 위치 체크 and 거리 계산
        if (!IsMoveValid(targetPos, out int distanceX, out int distanceZ))
        {
            return false;
        }

        // 모든 방향으로 1칸 이동(1칸이기 때문에 경로는 탐색할 필요없음)
        if(distanceX <= 1 && distanceZ <= 1)
        {
            return true;
        }

        return false;
    }    
}
