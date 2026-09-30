using UnityEngine;
using UnityEngine.InputSystem; // ★New Input System対応

public class Harpoon : MonoBehaviour
{
    private enum HarpoonState
    {
        Flying,    // 飛んでいる
        Stuck,     // 壁に刺さっている（または最初から地面に落ちている）
        Following, // プレイヤーに追従している
        Sinking    // ★水中で沈降中（敵を一撃で倒せなかった時）
    }

    [SerializeField] private float speed = 10f;
    //[SerializeField] private float followSpeed = 5f;     追従させる際に滑らかにするために使用していた。
    [SerializeField] private Vector2 followOffset = Vector2.zero; // 手の位置を使うためオフセットは基本0でOK

    [Header("攻撃力設定")]
    [SerializeField] private int damage = 30; // ★モリの攻撃力

    [Header("引き抜き設定")]
    [SerializeField] private float retrievalTimeFromFish = 1.5f; // 魚からモリを引き抜くのに必要な秒数
    [SerializeField] private float retrievalTimeFromWall = 1.0f; // ★壁からモリを引き抜くのに必要な秒数

    [Header("引き抜いた後の魚の挙動")]
    [SerializeField] private bool destroyFishOnRetrieval = false; // 引き抜いた時に魚を消すかどうか（残すならチェックを外す）

    [Header("刺さり具合の調整")]
    [SerializeField] private float penetrationDepth = 0.2f; // 魚の体にどれくらいモリの先端をめり込ませるか

    [Header("水中落下（沈降）物理パラメータ")]
    [SerializeField] private float sinkingGravityScale = 0.3f; // 水中でゆっくり沈む重力スケール
    [SerializeField] private float waterLinearDrag = 2.0f;     // 水の抵抗（速度の減速）
    [SerializeField] private float waterAngularDrag = 3.0f;    // 水の回転抵抗（揺れの収束）
    [SerializeField] private float bounceDamping = 0.3f;       // 弾かれた後の勢い残し（0.0〜1.0）

    private Rigidbody2D rb;
    
    // 【修正】初期状態を Flying ではなく Stuck（停止・回収待ち状態）にする
    private HarpoonState currentState = HarpoonState.Stuck; 
    private Transform playerTransform; 
    private Transform targetHandPoint; // ★手の位置を追従先として保持（エラー解消のための変数宣言）

    // ★魚に刺さっているかどうかの判定フラグ
    private bool isStuckInFish = false;
    private float currentRetrievalTimer = 0f;
    private bool isPlayerTouching = false;
    private PlayerHarpoon touchingPlayerHarpoon = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // ゲーム開始時に配置されている場合、勝手に動かないように物理演算を止めておく
        if (currentState == HarpoonState.Stuck)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            
            // 念のため、トリガー判定（すり抜け接触）が有効になっているか確認
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }
    }

    private void Update()
    {
        // ★刺さっている、または水中に落ちて回収待ちの状態でプレイヤーが触れている場合、長押し入力で回収タイマーを加算
        if ((currentState == HarpoonState.Stuck || currentState == HarpoonState.Sinking) && isPlayerTouching && touchingPlayerHarpoon != null)
        {
            if (!touchingPlayerHarpoon.hasHarpoon)
            {
                var keyboard = Keyboard.current;
                
                // 長押し対象キーの判定（Enterキー、Eキー、スペースキーのいずれかを押し続けているか）
                bool isHoldingKey = keyboard != null && (
                    keyboard.enterKey.isPressed || 
                    keyboard.numpadEnterKey.isPressed ||
                    keyboard.eKey.isPressed ||
                    keyboard.spaceKey.isPressed
                );

                if (isHoldingKey)
                {
                    currentRetrievalTimer += Time.deltaTime;

                    // 対象（魚か壁か、または水中に浮いているか）に応じた必要時間を取得
                    // ※水中に漂っている時は引き抜く必要がないため即座または短い時間（0.3秒など）で回収可能
                    float requiredTime = isStuckInFish ? retrievalTimeFromFish : 
                                         (currentState == HarpoonState.Sinking ? 0.3f : retrievalTimeFromWall);

                    // モリを引き抜く時間を達成したか確認
                    if (currentRetrievalTimer >= requiredTime)
                    {
                        CompleteRetrieval();
                    }
                }
                else
                {
                    // キーを途中で離したらタイマーをリセット（やり直し）
                    currentRetrievalTimer = 0f;
                }
            }
        }

        // ★手の位置に固定・追従させる処理
        if (currentState == HarpoonState.Following)
        {
            Transform followTarget = targetHandPoint != null ? targetHandPoint : playerTransform;

            if (followTarget != null)
            {
                // 手の座標にピッタリ合わせる
                transform.position = followTarget.position;
                
                // 手・プレイヤーの回転に同期させる
                transform.rotation = followTarget.rotation;
            }
        }
    }

    // プレイヤーが「スペースキー」で投げた時にこれが呼ばれる
    public void Launch(Vector2 direction)
    {
        // 親子関係を解除（魚に刺さっていた場合などに備えてルートへ出す）
        transform.SetParent(null);

        // 各種状態のリセット
        isStuckInFish = false;
        currentRetrievalTimer = 0f;
        isPlayerTouching = false;
        touchingPlayerHarpoon = null;
        targetHandPoint = null;

        // 投げる瞬間に初めて物理演算を有効にし、状態を「Flying」にする
        rb.bodyType = RigidbodyType2D.Dynamic; 
        rb.gravityScale = 0f; // 飛行中は重力なし
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.linearVelocity = direction * speed;
        currentState = HarpoonState.Flying;

        // 飛んでいく方向を計算して、モリの向き（Z軸の回転）を合わせる
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 回転を適用する
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 魚または敵に当たった場合（飛んでいる時のみ判定）
        if (currentState == HarpoonState.Flying && (collision.CompareTag("Fish") || collision.CompareTag("Enemy")))
        {
            EnemyFish fish = collision.GetComponent<EnemyFish>();
            bool isDefeated = false;

            if (fish != null)
            {
                // 魚にダメージを与える
                isDefeated = fish.TakeDamage(damage);
            }

            // 魚の体力が0になった場合、魚に突き刺さる
            if (isDefeated)
            {
                Debug.Log("魚を仕留めました！ モリが刺さりました。");
                currentState = HarpoonState.Stuck;
                isStuckInFish = true;
                currentRetrievalTimer = 0f;

                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;

                // ★進行方向に少しだけ踏み込ませて「めり込み」を表現
                if (rb.linearVelocity.sqrMagnitude > 0.001f)
                {
                    transform.position += (Vector3)(rb.linearVelocity.normalized * penetrationDepth);
                }
                else
                {
                    transform.position += transform.up * penetrationDepth;
                }

                // ★重要: 第2引数に true を渡すことでワールド座標・角度・スケールを維持したまま子オブジェクト化する
                transform.SetParent(collision.transform, true);
            }
            else
            {
                // ★まだHPが残っている場合：静止せず、水中浮力・抵抗に従って沈み始める
                Debug.Log("魚にダメージを与えましたが、まだ耐えています！ モリが水中を沈みます。");
                StartSinkingInWater();
            }
        }
        // 2. 壁などに当たった場合（Obstacle に加えて Wall も判定）
        else if ((currentState == HarpoonState.Flying || currentState == HarpoonState.Sinking) && 
                 (collision.CompareTag("Obstacle") || collision.CompareTag("Wall")))
        {
            Debug.Log("壁に刺さりました");
            currentState = HarpoonState.Stuck; 
            isStuckInFish = false;
            currentRetrievalTimer = 0f;
            
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
        // 3. 回収準備（プレイヤーとの接触）
        else if ((currentState == HarpoonState.Stuck || currentState == HarpoonState.Sinking) && collision.CompareTag("Player"))
        {
            PlayerHarpoon playerWeapon = collision.GetComponentInParent<PlayerHarpoon>();
            
            if (playerWeapon != null && !playerWeapon.hasHarpoon)
            {
                isPlayerTouching = true;
                touchingPlayerHarpoon = playerWeapon;

                if (isStuckInFish)
                {
                    Debug.Log("魚からモリを引き抜くにはキーを長押ししてください...");
                }
                else if (currentState == HarpoonState.Sinking)
                {
                    Debug.Log("沈んでいるモリを回収するにはキーを押してください...");
                }
                else
                {
                    Debug.Log("壁からモリを引き抜くにはキーを長押ししてください...");
                }
            }
        }
    }

    // ★敵に弾かれた後に水中でゆっくり沈降する挙動へ移行するメソッド
    private void StartSinkingInWater()
    {
        currentState = HarpoonState.Sinking;
        isStuckInFish = false;

        // 物理挙動を維持しつつ、水中の抵抗と重力を設定
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = sinkingGravityScale; // ゆっくり落ちる重力
        rb.linearDamping = waterLinearDrag;    // 水の粘性抵抗で前進速度を急速に落とす
        rb.angularDamping = waterAngularDrag;  // 回転を抑える

        // 当たった勢いを減衰させつつ、少し後方へ跳ね返るような微小な反動
        rb.linearVelocity = -rb.linearVelocity * bounceDamping;

        // 水中を漂うように少し回転トルクを加える
        rb.AddTorque(Random.Range(-5f, 5f));
    }

    // プレイヤーが途中で離れたらタイマーをリセット
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerTouching = false;
            touchingPlayerHarpoon = null;
            currentRetrievalTimer = 0f;
        }
    }

    // 壁刺さり時の即時回収処理（※現在は長押し回収に統合されたため未使用）
    private void ExecuteImmediateCatch(PlayerHarpoon playerWeapon)
    {
        Debug.Log("モリがプレイヤーの追従を開始しました！");
        
        transform.SetParent(null);
        playerWeapon.CatchHarpoon(gameObject); 
        
        playerTransform = playerWeapon.transform;
        targetHandPoint = playerWeapon.ShotPoint; // ★手の位置を保存
        currentState = HarpoonState.Following; 

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    // 長押し達成時の回収処理
    private void CompleteRetrieval()
    {
        GameObject fishToDestroy = null;
        EnemyFish caughtFish = null;

        if (isStuckInFish)
        {
            Debug.Log("魚からモリを引き抜きました！");

            // 刺さっていた魚の参照を退避しておく
            if (transform.parent != null)
            {
                fishToDestroy = transform.parent.gameObject;
                caughtFish = fishToDestroy.GetComponent<EnemyFish>();
            }
        }
        else if (currentState == HarpoonState.Sinking)
        {
            Debug.Log("水中に漂うモリを拾い上げました！");
        }
        else
        {
            Debug.Log("壁からモリを引き抜きました！");
        }

        // ★重要: 先にモリを親（魚）から完全に切り離す
        transform.SetParent(null);

        // ★プレイヤーへモリを引き渡して追従開始
        if (touchingPlayerHarpoon != null)
        {
            touchingPlayerHarpoon.CatchHarpoon(gameObject);
            playerTransform = touchingPlayerHarpoon.transform;
            targetHandPoint = touchingPlayerHarpoon.ShotPoint; // ★手の位置を保存

            // ★プレイヤーのストレージに魚を追加する処理
            if (caughtFish != null)
            {
                PlayerInventory inventory = touchingPlayerHarpoon.GetComponent<PlayerInventory>();
                if (inventory != null)
                {
                    bool isStored = inventory.TryAddFish(caughtFish);
                    if (isStored)
                    {
                        // ストレージに無事収納できた場合、シーン内の魚を消滅（捕獲）させる
                        Destroy(fishToDestroy);
                    }
                    else
                    {
                        // 容量オーバーの場合は魚を消さず、その場に残す（再捕獲可能）
                        Debug.Log("容量オーバーのため魚を持ち帰れませんでした。");
                    }
                }
                else
                {
                    // インベントリが無い場合は通常破棄
                    Destroy(fishToDestroy);
                }
            }
        }

        currentState = HarpoonState.Following;
        isStuckInFish = false;
        isPlayerTouching = false;
        currentRetrievalTimer = 0f;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // ★魚を消したくないため、以下の削除処理をコメントアウトとして保持
        /*
        if (fishToDestroy != null && destroyFishOnRetrieval)
        {
            Destroy(fishToDestroy);
        }
        */
    }
}