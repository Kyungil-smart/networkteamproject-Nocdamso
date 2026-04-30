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
        
        // 기물을 클릭했는지
        if (hitLayer == _pieceLayer)
        {
            if (hitObj.TryGetComponent(out ChessPieceManager clickedPiece))
            {
                // 자신의 기물을 선택한 상태에서 상대 기물 클릭했다면
                if (SelectedPiece != null && clickedPiece.isWhite != SelectedPiece.isWhite)
                {
                    // 상대 기물의 좌표를 가져와서 이동 시도
                    TryMove(clickedPiece.GridPos, clickedPiece);
                }

                // 내 기물을 클릭하거나 클릭하기 전 이라면 해당 기물 선택
                else if (clickedPiece.isWhite == ChessGameManager.instance.isWhiteTurn)
                {
                    SelectPiece(clickedPiece);
                }
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
        if (SelectedPiece.CanMove(targetGridPos))
        {
            // 내 기물 선택 후 상대 기물을 클릭하면 그 기물의 좌표 사용, 타일 클릭했다면 배열에서 가져옴
            ChessPieceManager target = enemyPiece ?? ChessGameManager.instance.boardLayout[targetGridPos.x, targetGridPos.y];
        
            // 적 기물이라면 잡기
            if (target != null && target.isWhite != SelectedPiece.isWhite)
            {
                if (TryGetComponent(out CaptureManager captureManager))
                {
                    captureManager.Capture(target);
                }
            }
            // 이동
            SelectedPiece.MovePiece(targetGridPos);

            ChessGameManager.instance.ChangeTurn();
            Deselect();                     
        }

        // 이동할 수 없는 곳 클릭 시 선택 해제
        else
        {
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
