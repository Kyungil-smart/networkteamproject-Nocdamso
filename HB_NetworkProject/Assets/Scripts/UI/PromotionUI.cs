using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PromotionUI : MonoBehaviour
{
    public static PromotionUI Instance;

    [Header("UI 구성 요소")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI timerText; // 5초 카운트다운 표시
    
    [Header("프로모션 선택 버튼들")]
    [SerializeField] private Button queenButton;
    [SerializeField] private Button rookButton;
    [SerializeField] private Button knightButton;
    [SerializeField] private Button bishopButton;

    private Vector2Int pendingPos;
    private ChessPieceManager promotingPawn;
    private Coroutine timerCoroutine;

    void Awake()
    {
        Instance = this;

        if (queenButton != null) queenButton.onClick.AddListener(() => SendPromotion(ChessPieces.Queen));
        if (rookButton != null) rookButton.onClick.AddListener(() => SendPromotion(ChessPieces.Rook));
        if (knightButton != null) knightButton.onClick.AddListener(() => SendPromotion(ChessPieces.Knight));
        if (bishopButton != null) bishopButton.onClick.AddListener(() => SendPromotion(ChessPieces.Bishop));

        if (panel != null) panel.SetActive(false);
    }

    public void Show(Vector2Int pos, ChessPieceManager pawn)
    {
        pendingPos = pos;
        promotingPawn = pawn;
        
        if (panel != null) 
        {
            panel.SetActive(true);
            // 기존에 돌아가던 타이머가 있다면 새로 시작
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(PromotionTimerRoutine());
        }
    }

    private IEnumerator PromotionTimerRoutine()
    {
        float timeLeft = 5f;

        while (timeLeft > 0)
        {
            if (timerText != null)
            {
                // 초 단위 올림 표시
                timerText.text = Mathf.CeilToInt(timeLeft).ToString();               
            }
            
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        // 5초가 지나면 랜덤으로 하나 선택
        Debug.Log("시간 초과, 랜덤 프로모션을 실행");
        SelectRandomPiece();
    }

    private void SelectRandomPiece()
    {
        ChessPieces[] options = { ChessPieces.Queen, ChessPieces.Rook, ChessPieces.Knight, ChessPieces.Bishop };
        ChessPieces randomChoice = options[Random.Range(0, options.Length)];
        SendPromotion(randomChoice);
    }

    private void SendPromotion(ChessPieces type)
    {
        // 선택이 완료되면 타이머 중지
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (promotingPawn != null)
        {
            promotingPawn.RequestPromotionServerRpc(type, pendingPos);
        }
        
        if (panel != null) panel.SetActive(false);
        promotingPawn = null;
    }
}
