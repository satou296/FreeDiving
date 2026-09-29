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

    [Header("移動パラメータ")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f; // 往復する幅
    [SerializeField] private float detectRange = 5f;    // プレイヤーを見つける索敵範囲

    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 startPosition;
    private int moveDirection = 1; // 1: 右, -1: 左

    // ★モリが刺さって停止したかどうかのフラグ
    private bool isCaptured = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // 重力を無効化
        startPosition = transform.position;

        // プレイヤーオブジェクトを検索
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
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