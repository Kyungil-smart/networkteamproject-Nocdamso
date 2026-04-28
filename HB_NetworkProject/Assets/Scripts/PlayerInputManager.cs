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
            if (hitObj.TryGetComponent(out ChessPieceManager piece))
            {
                SelectPiece(piece);
            }            
        }


        // 타일을 클릭했는지
        else if (hitLayer == _tileLayer && SelectedPiece != null)
        {
            if (hitObj.TryGetComponent(out TileHighlighter tile))
            {
                if (SelectedPiece.CanMove(tile.GridPos))
                {
                    SelectedPiece.MovePiece(tile.transform.position, tile.GridPos);
                    Deselect();
                }
            }
        }
    }

    private void SelectPiece(ChessPieceManager piece)
    {
        Deselect();

        SelectedPiece = piece;
        SelectedPiece.SetHighlight(true);

    }

    private void Deselect()
    {
        if (SelectedPiece != null)
        {
            SelectedPiece.SetHighlight(false);
            SelectedPiece = null;
        }
    }
}
