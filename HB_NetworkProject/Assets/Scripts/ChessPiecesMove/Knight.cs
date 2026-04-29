using UnityEngine;

public class Knight : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        // 현재 위치와 목표 위치 차이 계산
        int distanceX = Mathf.Abs(targetPos.x - GridPos.x);
        int distanceY = Mathf.Abs(targetPos.y - GridPos.y);

        // 제자리 클릭 방지
        if (distanceX == 0 && distanceY == 0) return false;

        // X로 2칸 가면 Y로는 1칸 움직임, Y로 2칸가면 X로 1칸 움직임 (diffX == 2 && diffY == 1) || (diffX == 1 && diffY == 2)
        if (distanceX * distanceY == 2)
        {
            return true;
        }

        return false;
    }    
}
