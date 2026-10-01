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

    [Header("プレイヤーの向きスプライト設定")]
    [SerializeField] private Sprite idleFrontSprite; // ★何も操作していない時（正面）
    [SerializeField] private Sprite movingDownSprite; // ★下移動時（潜水）
    [SerializeField] private Sprite movingSideSprite; // ★左右移動時（横向き）
    [SerializeField] private Sprite movingUpSprite;   // （任意）上移動時の画像があれば設定

    private int currentHp;
    private float invincibilityTimer = 0f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; // ★スプライト切り替え用
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
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRendererを取得

        currentHp = maxHp; // 体力初期化
        //currentOxygenTime = maxOxygenTime; // 酸素初期化

        // 購入した酸素タンクレベルに応じて最大潜水時間を強化（例: 1レベルごとに +10秒）
        int oxygenBonus = (PlayerInventory.OxygenTankLevel - 1) * 10;
        maxOxygenTime += oxygenBonus;
        currentOxygenTime = maxOxygenTime; // 強化後の最大値で初期化

        // 初期スプライトを正面に設定
        if (idleFrontSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = idleFrontSprite;
        }
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

        // 3. 移動方向に応じたスプライト画像の更新
        UpdatePlayerVisual();
    }

    // ★プレイヤーの移動方向に応じた画像切り替え処理
    private void UpdatePlayerVisual()
    {
        if (spriteRenderer == null) return;

        // 何も入力していない（静止）時：正面画像
        if (moveInput.sqrMagnitude < 0.01f)
        {
            if (idleFrontSprite != null)
            {
                spriteRenderer.sprite = idleFrontSprite;
            }
            spriteRenderer.flipX = false; // 反転解除
            return;
        }

        // 入力がある場合：縦と横の入力強度を比較して優先する方向を決定
        if (Mathf.Abs(moveInput.y) > Mathf.Abs(moveInput.x))
        {
            // 縦方向の入力が強い場合
            if (moveInput.y < 0)
            {
                // 下方向
                if (movingDownSprite != null)
                {
                    spriteRenderer.sprite = movingDownSprite;
                }
                spriteRenderer.flipX = false;
            }
            else
            {
                // 上方向
                if (movingUpSprite != null)
                {
                    spriteRenderer.sprite = movingUpSprite;
                }
                else if (idleFrontSprite != null)
                {
                    spriteRenderer.sprite = idleFrontSprite; // 上画像がなければ正面を流用
                }
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            // 横方向の入力が強い場合
            if (movingSideSprite != null)
            {
                spriteRenderer.sprite = movingSideSprite;
            }

            // 右向きなら通常、左向きならX反転（flipX）
            if (moveInput.x > 0.05f)
            {
                spriteRenderer.flipX = false; // 右向き
            }
            else if (moveInput.x < -0.05f)
            {
                spriteRenderer.flipX = true;  // 左向き
            }
        }
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