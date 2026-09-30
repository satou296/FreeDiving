using UnityEngine;

public class EnemyFish : MonoBehaviour
{
    public enum FishType
    {
        Passive, // 攻撃してこない（左右に泳ぐだけ）
        Aggressive // 攻撃してくる（プレイヤーを追尾する）
    }

    [Header("敵のタイプ設定")]
    [SerializeField] private FishType fishType = FishType.Passive;

    [Header("ステータス設定")]
    [SerializeField] private int maxHp = 30;         // 魚の最大体力
    [SerializeField] private int attackDamage = 20;  // プレイヤーへの攻撃力（Aggressive用）

    [Header("捕獲・ストレージ情報")]
    [SerializeField] private string fishName = "イワシ"; // 魚の名前
    [SerializeField] private float fishWeight = 1.0f;   // 魚の重さ（kg）
    [SerializeField] private int fishValue = 100;       // 売却価格・スコア
    [SerializeField] private Sprite fishIcon;           // UI用アイコン（任意）

    [Header("移動パラメータ")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f; // 往復する幅
    [SerializeField] private float detectRange = 5f;    // プレイヤーを見つける索敵範囲

    [Header("画面外での自動消滅設定")]
    [SerializeField] private bool autoDestroyOffscreen = true; // 画面外で消去するか
    [SerializeField] private float offscreenDistance = 15f;    // カメラからこれ以上離れたら画面外と判定する距離
    [SerializeField] private float destroyDelay = 3f;          // 画面外に出てから消滅するまでの猶予時間（秒）

    private int currentHp;
    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 startPosition;
    private int moveDirection = 1; // 1: 右, -1: 左

    // ★モリが刺さって停止したかどうかのフラグ
    private bool isCaptured = false;

    // ★画面外判定用タイマーとカメラのTransform
    private float offscreenTimer = 0f;
    private Transform mainCameraTransform;

    public int AttackDamage => attackDamage;
    public bool IsCaptured => isCaptured;

    // ★ストレージ用プロパティ
    public string FishName => fishName;
    public float FishWeight => fishWeight;
    public int FishValue => fishValue;
    public Sprite FishIcon => fishIcon;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // 重力を無効化
        startPosition = transform.position;
        currentHp = maxHp; // HP初期化

        // メインカメラのTransformを取得
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        // プレイヤーオブジェクトを検索
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        // 画面外に出てしばらくしたら破棄する処理
        HandleOffscreenDestroy();
    }

    private void FixedUpdate()
    {
        // ★捕獲されたら一切動かない
        if (isCaptured) return;

        switch (fishType)
        {
            case FishType.Passive:
                PatrolMovement();
                break;

            case FishType.Aggressive:
                AggressiveMovement();
                break;
        }
    }

    // ★画面外判定と消滅処理
    private void HandleOffscreenDestroy()
    {
        if (!autoDestroyOffscreen || mainCameraTransform == null) return;

        // 【最重要】モリが刺さっている（捕獲済み）の魚は、プレイヤーが回収に戻る可能性があるため消さない
        if (isCaptured) return;

        // カメラ（画面中心）との直線距離を測定
        float distanceToCamera = Vector2.Distance(transform.position, mainCameraTransform.position);

        if (distanceToCamera > offscreenDistance)
        {
            // 画面外にいる間、タイマーを進める
            offscreenTimer += Time.deltaTime;

            if (offscreenTimer >= destroyDelay)
            {
                // 一定時間画面外に留まり続けたため消滅
                Destroy(gameObject);
            }
        }
        else
        {
            // 再び画面内（または近く）に戻ってきた場合はタイマーをリセット
            offscreenTimer = 0f;
        }
    }

    // 攻撃してこない敵の動き：一定区間を左右に往復する
    private void PatrolMovement()
    {
        // 始点からの移動距離をチェック
        float distanceFromStart = transform.position.x - startPosition.x;

        if (distanceFromStart > patrolDistance)
        {
            moveDirection = -1;
            FlipSprite(false);
        }
        else if (distanceFromStart < -patrolDistance)
        {
            moveDirection = 1;
            FlipSprite(true);
        }

        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, 0f);
    }

    // 攻撃してくる敵の動き：プレイヤーが近づいたら突進してくる
    private void AggressiveMovement()
    {
        if (playerTransform == null)
        {
            PatrolMovement();
            return;
        }

        // プレイヤーとの距離を測る
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectRange)
        {
            // プレイヤーに向かって一直線に泳ぐ
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * (moveSpeed * 1.5f);

            // 進行方向に向きを変える
            FlipSprite(direction.x > 0);
        }
        else
        {
            // 索敵範囲外なら通常巡回
            PatrolMovement();
        }
    }

    // 進行方向に応じてスプライトを左右反転させる
    private void FlipSprite(bool faceRight)
    {
        Vector3 scale = transform.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // ★モリからダメージを受ける処理。力尽きたら true を返す
    public bool TakeDamage(int damage)
    {
        if (isCaptured) return false;

        currentHp -= damage;
        Debug.Log($"{gameObject.name} に {damage} ダメージ！ 残りHP: {currentHp}");

        if (currentHp <= 0)
        {
            currentHp = 0;
            OnHarpooned(); // 倒れたので停止・捕獲可能状態にする
            return true;
        }

        return false; // まだ生きている
    }

    // ★モリが刺さったときにHarpoonスクリプトから呼ばれる停止処理
    public void OnHarpooned()
    {
        isCaptured = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 速度をゼロにして静止
            rb.bodyType = RigidbodyType2D.Kinematic; // 物理挙動を無効化
        }

        // 攻撃する敵（Enemy）だった場合、刺さった後に激突してゲームオーバーにならないようタグを無害化する
        gameObject.tag = "Untagged";
    }

    // 索敵範囲をSceneビュー上で確認するための補助線
    private void OnDrawGizmosSelected()
    {
        if (fishType == FishType.Aggressive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectRange);
        }
    }
}