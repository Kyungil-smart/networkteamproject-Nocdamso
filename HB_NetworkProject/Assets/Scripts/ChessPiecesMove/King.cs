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

        if (!isMoved.Value && distanceX == 2 && distanceZ == 0)
        {
            int startY = isWhite ? 0 : 7;
            if (GridPos.Value.y != startY) return false;
            
            return CanCastling(targetPos);
        }

        return false;
    }    

    // 캐슬링 로직
    private bool CanCastling(Vector2Int targetPos)
    {
        // 기본 정보 설정
        int direction = (targetPos.x > GridPos.Value.x) ? 1 : -1;
        int rookX = (direction > 0) ? 7 : 0;
        Vector2Int currentPos = GridPos.Value;
        TeamColor enemyColor = isWhite ? TeamColor.Black : TeamColor.White;

        // 경로 및 체크 상태 확인 캐슬링 규칙
        if (ChessGameManager.instance.IsKingAttacked(currentPos, enemyColor)) return false;
        

        // 룩 존재 여부 및 상태 확인
        ChessPieceManager rook = ChessGameManager.instance.boardLayout[rookX, currentPos.y];

        if (rook == null)
        {
            foreach (var piece in FindObjectsOfType<ChessPieceManager>())
            {
                if (piece.pieceType == ChessPieces.Rook && piece.isWhite == this.isWhite && piece.GridPos.Value == new Vector2Int(rookX, currentPos.y))
                {
                    rook = piece;
                    break;
                }
            }
        }

        // 룩이 없거나, 타입이 룩이 아니거나, 이미 움직였다면 불가
        if (rook == null || rook.pieceType != ChessPieces.Rook || rook.isMoved.Value)
        {           
            return false;
        }

        // 4. 경로에 장애물이 있는지 확인
        if (isPathBlocked(new Vector2Int(rookX, currentPos.y)))
        {
            return false;
        }

        Vector2Int middlePos = new Vector2Int(currentPos.x + direction, currentPos.y);
        if (ChessGameManager.instance.IsKingAttacked(middlePos, enemyColor)) return false;
        if (ChessGameManager.instance.IsKingAttacked(targetPos, enemyColor)) return false;
        
        // 모든 조건을 통과하면 성공
        return true;
    }
}
