using UnityEngine;

public class King : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        // 현재 위치와 목표 위치 차이 계산
        int distanceX = Mathf.Abs(targetPos.x - GridPos.x);
        int distanceY = Mathf.Abs(targetPos.y - GridPos.y);

        // 제자리 클릭 방지
        if (distanceX == 0 && distanceY == 0) return false;

        // 어느 방향으로든 1칸만 움직일 수 있음
        if (distanceX <= 1 && distanceY <= 1)
        {
            // TODO: 가는 길에 기물이 있는지 확인 로직 추가
            return true;
        }

        return false;
    }    
}
