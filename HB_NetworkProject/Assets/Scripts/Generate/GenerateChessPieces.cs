using Unity.Netcode;
using UnityEngine;

public class GenerateChessPieces : MonoBehaviour
{
    public GenerateChessBoard BoardScript;

    [Header("백 (0:폰, 1:룩, 2:나이트, 3:비숍, 4:퀸, 5:킹)")]
    public GameObject[] WhitePieces;

    [Header("흑 (0:폰, 1:룩, 2:나이트, 3:비숍, 4:퀸, 5:킹)")]
    public GameObject[] BlackPieces;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GeneratePieces();            
        }
    }

    public void GeneratePieces()
    {
        // layout에 기물 위치 선정
        int[,] layout =
        {
            { 2, 3, 4, 5, 6, 4, 3, 2},
            { 1, 1, 1, 1, 1, 1, 1, 1},
            { 0, 0, 0, 0, 0, 0, 0, 0},
            { 0, 0, 0, 0, 0, 0, 0, 0},
            { 0, 0, 0, 0, 0, 0, 0, 0},
            { 0, 0, 0, 0, 0, 0, 0, 0},
            { 1, 1, 1, 1, 1, 1, 1, 1},
            { 2, 3, 4, 5, 6, 4, 3, 2}
        };

        var converter = TileConverter.Instance;

        for (int x = 0; x < 8; x++)
        {
            for (int z = 0; z < 8; z++)
            {
                // 기물의 위치 숫자를 읽어와서 해당 칸이 비었다면 다음으로
                ChessPieces type = (ChessPieces)layout[z, x];
                if (type == ChessPieces.Empty) continue;

                // 타일 컨버터에서 좌표 계산
                Vector3 spawnPos = converter.GridToWorld(x, z, 1f);

                // z가 2보다 작으면 흑색 기물, 그 외면 백색 기물
                bool isWhiteTeam = (z < 2);
                GameObject[] chessTeamPiece = (z < 2) ? WhitePieces : BlackPieces;
                GameObject chessPiece = chessTeamPiece[(int)type - 1];

                if (chessPiece != null)
                {
                    // 유니티 좌표에 기물 프리팹 생성 후 관리하기 편하게 정리
                    GameObject gameObject = Instantiate(chessPiece, spawnPos, Quaternion.identity, this.transform);

                    // 흑색 기물은 180도 회전 시켜서 생성
                    if (z > 5) gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    else gameObject.transform.rotation = Quaternion.identity;

                    ChessPieceManager chessPieceManager = gameObject.GetComponent<ChessPieceManager>();
                    if (chessPieceManager != null)
                    {
                        chessPieceManager.teamColor.Value = isWhiteTeam ? TeamColor.White : TeamColor.Black;
                        chessPieceManager.pieceType = type;

                        chessPieceManager.GridPos.Value = new Vector2Int(x, z);

                        ChessGameManager.instance.boardLayout[x, z] = chessPieceManager;
                    }

                    if (gameObject.TryGetComponent(out NetworkObject networkObject))
                    {
                        networkObject.Spawn();
                    }            
                }
            }
        }
    }
}
