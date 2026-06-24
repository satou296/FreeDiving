using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float baseMoveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 水中移動を表現するため、重力を0にするか適切に調整してください
        rb.gravityScale = 0f; 
    }

    private void Update()
    {
        // 入力の受付 (上下方向の矢印キーまたはWSキー)
        float moveY = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal"); // 左右移動も考慮
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    private void FixedUpdate()
    {
        // PressureManagerから現在の水圧を取得し、速度を減衰させる
        float currentPressure = PressureManager.Instance != null ? PressureManager.Instance.CurrentPressure : 0f;
        
        // 水圧が高くなるほど移動速度が遅くなる（最低速度を1に制限）
        float adjustedSpeed = Mathf.Max(1f, baseMoveSpeed - (currentPressure * 0.1f));

        rb.linearVelocity = moveInput * adjustedSpeed;
    }
}