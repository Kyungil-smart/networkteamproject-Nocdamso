using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    private PlayerInputActions _control;                            // 뉴 인풋 시스템 C# 스크립트
    private Camera _mainCamera;

    [Header("기물 선택")]
    public ChessPieceManager SelectedPiece { get; private set; }     // 현재 선택된 기물

    private int _pieceLayer;
    private int _tileLayer;
    private int _combineMask;

    private void Awake()
    {
        _control = new PlayerInputActions();
        _mainCamera = Camera.main;

        _pieceLayer = LayerMask.NameToLayer("Piece");
        _tileLayer = LayerMask.NameToLayer("Tile");

        _combineMask = LayerMask.GetMask("Piece", "Tile");
    }

    private void OnEnable() => _control.Enable();
    private void OnDisable() => _control.Disable();

    private void Start()
    {
        _control.Player.Click.performed += OnClickPerformed;
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        // 현재 마우스의 위치
        Vector2 mousePosition = _control.Player.Point.ReadValue<Vector2>();

        // 레이캐스트
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _combineMask))
        {
            Debug.Log($"클릭 성공, 물체 이름 : {hit.collider.name}");
            HitObject(hit.collider.gameObject);
        }

        else
        {
            Deselect();
        }
    }

    private void HitObject(GameObject hitObj)
    {
        int hitLayer = hitObj.layer;
        Debug.Log($"클릭 레이어 번호: {hitLayer}, 설정된 Piece레이어: {_pieceLayer}");
        
        // 기물을 클릭했는지
        if (hitLayer == _pieceLayer)
        {
            if (hitObj.TryGetComponent(out ChessPieceManager clickedPiece))
            {
                // 자신의 기물을 선택한 상태에서 상대 기물 클릭했다면
                if (SelectedPiece != null && clickedPiece.isWhite != SelectedPiece.isWhite)
                {
                    // 상대 기물의 좌표를 가져와서 이동 시도
                    TryMove(clickedPiece.GridPos.Value, clickedPiece);
                    return;
                }

                var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerTeamColor>();
                bool isMyPiece = (localPlayer.PlayerColor.Value == TeamColor.White && clickedPiece.isWhite) ||
                                 (localPlayer.PlayerColor.Value == TeamColor.Black && !clickedPiece.isWhite);
            
                if (isMyPiece && clickedPiece.isWhite == ChessGameManager.instance.isWhiteTurn.Value)
                {
                    Debug.Log("<color=green>최종 성공: SelectPiece 호출됨!</color>");
                    SelectPiece(clickedPiece);
                }
                else
                {
                    
                    var myColor = localPlayer.PlayerColor.Value;
                    var currentTurn = ChessGameManager.instance.isWhiteTurn.Value ? "백" : "흑";
                    var pieceColor = clickedPiece.isWhite ? "백" : "흑";
                    
                    Debug.Log($"<color=red>[판정 실패]</color> 내 컬러: {myColor}, 기물 컬러: {pieceColor}, 현재 턴: {currentTurn}");
                    Debug.Log($"상세조건 - 내 기물인가: {isMyPiece}, 턴이 맞는가: {clickedPiece.isWhite == ChessGameManager.instance.isWhiteTurn.Value}");
                }

            }  

            else
            {
                Debug.LogError("[디버그] ChessPieceManager 컴포넌트를 찾을 수 없습니다!");
            }    
        }

        // 타일을 클릭했는지
        else if (hitLayer == _tileLayer && SelectedPiece != null)
        {
            if (hitObj.TryGetComponent(out TileHighlighter tile))
            {
                TryMove(tile.GridPos);
            }
        }
    }

    private void TryMove(Vector2Int targetGridPos, ChessPieceManager enemyPiece = null)
    {
        // 내 네트워크 플레이어의 팀 컬러 가져옴
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerTeamColor>();
        TeamColor teamColor = localPlayer.PlayerColor.Value;

        // 현재 보드상의 턴 가져옴
        bool isWhiteTurn = ChessGameManager.instance.isWhiteTurn.Value;

        // 내가 조작하려는 기물색 확인
        bool selectedIsWhite = SelectedPiece.isWhite;

        // 내 팀 컬러가 현재 턴이고, 기물도 같은 색이어야 함
        bool isMyTurn = (teamColor == TeamColor.White && isWhiteTurn && selectedIsWhite) ||
                        (teamColor == TeamColor.Black && !isWhiteTurn && !selectedIsWhite);

        if (!isMyTurn)
        {
            Debug.Log("내 턴 아님");
            Deselect();
            return;
        }

        if (SelectedPiece.CanMove(targetGridPos))
        {
            SelectedPiece.RequestMoveServerRpc(targetGridPos);

            Deselect();
        }
    }

    private void SelectPiece(ChessPieceManager piece)
    {
        Deselect();

        SelectedPiece = piece;
        SelectedPiece.SetHighlight(true);

        ChessGameManager.instance.ShowPossibleMoves(piece);
    }

    private void Deselect()
    {
        if (SelectedPiece != null)
        {
            SelectedPiece.SetHighlight(false);
            SelectedPiece = null;

            ChessGameManager.instance.ClearAllHighlights();
        }
    }
}
