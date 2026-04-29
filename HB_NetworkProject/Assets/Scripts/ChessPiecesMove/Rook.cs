using UnityEngine;

public class Rook : ChessPieceManager
{
    public override bool CanMove(Vector2Int targetPos)
    {
        // 현재 위치와 목표 위치 차이 계산
        int distanceX = Mathf.Abs(targetPos.x - GridPos.x);
        int distanceY = Mathf.Abs(targetPos.y - GridPos.y);

        // 제자리 클릭 방지
        if (distanceX == 0 && distanceY == 0) return false;

        // X가 0이면 수직, Y가 0이면 수평이동 직선으로만 이동
        if (distanceX == 0 || distanceY == 0)
        {
            // TODO: 가는 길에 기물이 있는지 확인 로직 추가
            return true;
        }

        return false;
    }
}
