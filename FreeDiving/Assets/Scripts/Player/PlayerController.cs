using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float baseMoveSpeed = 5f;

    [Header("体力設定")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private float invincibilityDuration = 1.0f; // 被弾後の無敵時間（秒）

    [Header("潜水時間（酸素）設定")]
    [SerializeField] private float maxOxygenTime = 30f;       // 潜っていられる最大時間（秒）
    [SerializeField] private float surfaceThresholdY = 0f;    // 水面とみなすY座標（これ以上なら回復）
    [SerializeField] private int suffocationDamage = 10;      // 酸素切れ時のスリップダメージ量
    [SerializeField] private float damageInterval = 1.0f;     // スリップダメージが発生する間隔（秒）
    [SerializeField] private bool recoverInstantlyAtSurface = true; // 水面で即全回復するか

    private int currentHp;
    private float invincibilityTimer = 0f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    // ★潜水時間管理用変数
    private float currentOxygenTime;
    private float suffocationTimer = 0f;

    public Vector2 LastAimDirection { get; private set; } = Vector2.right;

    // 外部（UI表示など）から参照できるプロパティ
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public float CurrentOxygenTime => currentOxygenTime;
    public float MaxOxygenTime => maxOxygenTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        currentHp = maxHp; // 体力初期化
        currentOxygenTime = maxOxygenTime; // 酸素初期化
    }

    private void Update()
    {
        // 1. 無敵時間のカウントダウン
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }

        // 2. 潜水時間（酸素）の処理
        HandleOxygen();
    }

    // 酸素・潜水時間およびスリップダメージの管理
    private void HandleOxygen()
    {
        // 水面より上にいる場合：酸素回復
        if (transform.position.y >= surfaceThresholdY)
        {
            if (recoverInstantlyAtSurface)
            {
                currentOxygenTime = maxOxygenTime;
            }
            else
            {
                // 徐々に回復させたい場合（毎秒2倍速で回復）
                currentOxygenTime = Mathf.Min(maxOxygenTime, currentOxygenTime + Time.deltaTime * 2f);
            }

            suffocationTimer = 0f; // スリップダメージ用タイマーをリセット
            return;
        }

        // 水中にいる場合：酸素を消費
        if (currentOxygenTime > 0f)
        {
            currentOxygenTime -= Time.deltaTime;
            if (currentOxygenTime < 0f)
            {
                currentOxygenTime = 0f;
            }
        }
        else
        {
            // ★酸素が0になった後のスリップダメージ処理
            suffocationTimer += Time.deltaTime;
            if (suffocationTimer >= damageInterval)
            {
                suffocationTimer = 0f;
                Debug.Log("酸素が尽きました！窒息ダメージを受けます。");
                // 無敵時間を無視して直接減算、または無敵時間付きダメージを呼ぶ
                TakeSuffocationDamage(suffocationDamage);
            }
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput.sqrMagnitude > 0.01f)
        {
            LastAimDirection = moveInput.normalized;
        }
    }

    private void FixedUpdate()
    {
        float currentPressure = PressureManager.Instance != null ? PressureManager.Instance.CurrentPressure : 0f;
        float adjustedSpeed = Mathf.Max(1f, baseMoveSpeed - (currentPressure * 0.1f));

        rb.linearVelocity = moveInput * adjustedSpeed;
    }

    // 敵からの通常ダメージ処理
    public void TakeDamage(int damage)
    {
        if (invincibilityTimer > 0f) return;

        currentHp -= damage;
        invincibilityTimer = invincibilityDuration;
        Debug.Log($"プレイヤーがダメージを受けました！ 残りHP: {currentHp}");

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
    }

    // ★窒息専用のスリップダメージ処理（被弾時の点滅・ノックバック等と差別化できるように独立）
    private void TakeSuffocationDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"窒息スリップダメージ！ 残りHP: {currentHp}");

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("プレイヤーが力尽きました。");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    // --------------------------------------------------
    // 衝突判定（物理コライダー: isTrigger = false の場合）
    // --------------------------------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            ApplyEnemyDamage(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            ApplyEnemyDamage(collision.gameObject);
        }
    }

    // --------------------------------------------------
    // トリガー判定（すり抜けコライダー: isTrigger = true の場合）
    // --------------------------------------------------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            ApplyEnemyDamage(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            ApplyEnemyDamage(collision.gameObject);
        }
    }

    private void ApplyEnemyDamage(GameObject enemyObj)
    {
        EnemyFish enemy = enemyObj.GetComponent<EnemyFish>();
        if (enemy != null && !enemy.IsCaptured)
        {
            TakeDamage(enemy.AttackDamage);
        }
    }
}