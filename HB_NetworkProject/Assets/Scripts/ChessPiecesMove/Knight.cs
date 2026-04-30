using UnityEngine;

public class Knight : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        // 보드 밖, 제자리, 아군 위치 체크 and 거리 계산
        if (!IsMoveValid(targetPos, out int distanceX, out int distanceZ))
        {
            return false;
        }

        // X로 2칸 가면 Y로는 1칸 움직임, Y로 2칸가면 X로 1칸 움직임 (diffX == 2 && diffY == 1) || (diffX == 1 && diffY == 2)
        bool isKnightMove = (distanceX * distanceZ == 2);

        return isKnightMove;
    }    
}
