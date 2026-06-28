using UnityEngine;
using UnityEngine.InputSystem; // ★これを追加

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float baseMoveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; 
    }

    // ★新しいInput Systemから入力を受け取るためのメソッド
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        float currentPressure = PressureManager.Instance != null ? PressureManager.Instance.CurrentPressure : 0f;
        float adjustedSpeed = Mathf.Max(1f, baseMoveSpeed - (currentPressure * 0.1f));

        rb.linearVelocity = moveInput * adjustedSpeed;
    }
}