/*using UnityEngine;
using UnityEngine.InputSystem; // ★これを追加

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float baseMoveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;

    // 最後に動いた方向、または現在の入力方向を外部へ渡す
    public Vector2 LastMoveDirection { get; private set; } = Vector2.right; // 初期値は右向き

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; 
    }

    // ★新しいInput Systemから入力を受け取るためのメソッド
    /*public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }*/

/*
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // 何らかの入力がある場合、最後に移動した方向を更新
        if (moveInput.sqrMagnitude > 0.01f)
        {
            LastMoveDirection = moveInput.normalized;
        }
    }

    private void FixedUpdate()
    {
        float currentPressure = PressureManager.Instance != null ? PressureManager.Instance.CurrentPressure : 0f;
        float adjustedSpeed = Mathf.Max(1f, baseMoveSpeed - (currentPressure * 0.1f));

        rb.linearVelocity = moveInput * adjustedSpeed;
    }
}*/

using UnityEngine;
using UnityEngine.InputSystem; // ★これを追加

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float baseMoveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;

    // ★向いている方向（最後に移動した方向）を外部から取得するためのプロパティ（初期値は右向き）
    public Vector2 LastAimDirection { get; private set; } = Vector2.right;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 水中移動を表現するため、重力を0にするか適切に調整してください
        rb.gravityScale = 0f; 
    }

    // ★新しいInput Systemから入力を受け取るためのメソッド
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // 移動入力がある場合、狙う方向（AimDirection）を更新する
        if (moveInput.sqrMagnitude > 0.01f)
        {
            LastAimDirection = moveInput.normalized;
        }
    }

    /*
    private void Update()
    {
        // 入力の受付 (上下方向の矢印キーまたはWSキー)
        // ※旧Input処理はNew Input System環境で例外エラーとなるためコメントアウトとして保持
        // float moveY = Input.GetAxisRaw("Vertical");
        // float moveX = Input.GetAxisRaw("Horizontal"); // 左右移動も考慮
        // moveInput = new Vector2(moveX, moveY).normalized;
    }
    */

    private void FixedUpdate()
    {
        // PressureManagerから現在の水圧を取得し、速度を減衰させる
        float currentPressure = PressureManager.Instance != null ? PressureManager.Instance.CurrentPressure : 0f;
        
        // 水圧が高くなるほど移動速度が遅くなる（最低速度を1に制限）
        float adjustedSpeed = Mathf.Max(1f, baseMoveSpeed - (currentPressure * 0.1f));

        rb.linearVelocity = moveInput * adjustedSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 「Obstacle（障害物）」や「Enemy（敵）」のタグを持つ物体に激突したらゲームオーバー
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Enemy"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
}