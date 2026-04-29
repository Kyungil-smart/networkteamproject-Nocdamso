using UnityEngine;

[CreateAssetMenu(fileName = "ChessPiecesSO", menuName = "Scriptable Objects/ChessPiecesSO")]
public class ChessPiecesSO : ScriptableObject
{
    public string PieceName;                // 기물 이름
    public ChessPieces Type;                // Enum Type
    public GameObject ModelPrefab;          // 기물 에셋 프리팹
    
    [Header("이동 규칙")]
    public int MoveDistance;                // 이동 거리
    public bool CanMoveDiagonally;          // 대각선 이동 가능?
    public bool CanJump;                    // 다른 기물 넘기 가능?

    [Header("특수 이동 규칙")]
    public bool CanDoubleMoveOnFirst;       // 처음에 2칸 움직일 수 있나(폰)
    public bool CanEnPassant;               // 앙파상(폰)
    public bool CanCaptureDiagonally;       // 폰 상대 기물 잡을 시 대각선 이동
    public bool CanCastling;                // 룩캐슬링 용도(킹, 룩)
}
