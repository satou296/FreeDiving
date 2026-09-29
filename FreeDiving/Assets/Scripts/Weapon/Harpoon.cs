using UnityEngine;
using UnityEngine.InputSystem; // ★New Input System対応

public class Harpoon : MonoBehaviour
{
    private enum HarpoonState
    {
        Flying,    // 飛んでいる
        Stuck,     // 壁に刺さっている（または最初から地面に落ちている）
        Following  // プレイヤーに追従している
    }

    [SerializeField] private float speed = 10f;
    //[SerializeField] private float followSpeed = 5f;     追従させる際に滑らかにするために使用していた。
    [SerializeField] private Vector2 followOffset = Vector2.zero; // 手の位置を使うためオフセットは基本0でOK

    [Header("攻撃力設定")]
    [SerializeField] private int damage = 30; // モリの攻撃力

    [Header("引き抜き設定")]
    [SerializeField] private float retrievalTimeFromFish = 1.5f; // 魚からモリを引き抜くのに必要な秒数
    [SerializeField] private float retrievalTimeFromWall = 1.0f; // 壁からモリを引き抜くのに必要な秒数

    [Header("引き抜いた後の魚の挙動")]
    [SerializeField] private bool destroyFishOnRetrieval = false; // 引き抜いた時に魚を消すかどうか（残すならチェックを外す）

    [Header("刺さり具合の調整")]
    [SerializeField] private float penetrationDepth = 0.2f; // 魚の体にどれくらいモリの先端をめり込ませるか

    private Rigidbody2D rb;
    
    // 初期状態を Flying ではなく Stuck（停止・回収待ち状態）にする
    private HarpoonState currentState = HarpoonState.Stuck; 
    private Transform playerTransform; 
    private Transform targetshotPoint; // 手の位置を追従先として保持

    // 魚に刺さっているかどうかの判定フラグ
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
            
            // 念のため、トリガー判定（すり抜け接触）が有効になっているか確認
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }
    }

    private void Update()
    {
        // 刺さっている状態でプレイヤーが触れている場合、長押し入力で回収タイマーを加算
        if (currentState == HarpoonState.Stuck && isPlayerTouching && touchingPlayerHarpoon != null)
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

                    // 対象（魚か壁か）に応じた必要時間を取得
                    float requiredTime = isStuckInFish ? retrievalTimeFromFish : retrievalTimeFromWall;

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

        // 手の位置に固定・追従させる処理
        if (currentState == HarpoonState.Following)
        {
            Transform followTarget = targetshotPoint != null ? targetshotPoint : playerTransform;

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
        targetshotPoint = null;

        // 投げる瞬間に初めて物理演算を有効にし、状態を「Flying」にする
        rb.bodyType = RigidbodyType2D.Dynamic; 
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

                // 進行方向に少しだけ踏み込ませて「めり込み」を表現
                if (rb.linearVelocity.sqrMagnitude > 0.001f)
                {
                    transform.position += (Vector3)(rb.linearVelocity.normalized * penetrationDepth);
                }
                else
                {
                    transform.position += transform.up * penetrationDepth;
                }

                // 重要: 第2引数に true を渡すことでワールド座標・角度・スケールを維持したまま子オブジェクト化する
                transform.SetParent(collision.transform, true);
            }
            else
            {
                // まだHPが残っている場合、モリはその場で停止して回収待ちにする
                Debug.Log("魚にダメージを与えましたが、まだ耐えています！");
                currentState = HarpoonState.Stuck;
                isStuckInFish = false; // 魚の体には固定せずその場に落とす/止める
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
        // 2. 壁などに当たった場合
        else if (currentState == HarpoonState.Flying && collision.CompareTag("Obstacle"))
        {
            Debug.Log("壁に刺さりました");
            currentState = HarpoonState.Stuck; 
            isStuckInFish = false;
            currentRetrievalTimer = 0f;
            
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        // 3. 回収準備（プレイヤーとの接触）
        else if (currentState == HarpoonState.Stuck && collision.CompareTag("Player"))
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
                else
                {
                    Debug.Log("壁からモリを引き抜くにはキーを長押ししてください...");
                }
            }
        }
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
        targetshotPoint = playerWeapon.ShotPoint; // 手の位置を保存
        currentState = HarpoonState.Following; 

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
    }

    // 長押し達成時の回収処理
    private void CompleteRetrieval()
    {
        GameObject fishToDestroy = null;

        if (isStuckInFish)
        {
            Debug.Log("魚からモリを引き抜きました！");

            // 刺さっていた魚の参照を退避しておく
            if (transform.parent != null)
            {
                fishToDestroy = transform.parent.gameObject;
            }
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
            targetshotPoint = touchingPlayerHarpoon.ShotPoint; // 手の位置を保存
        }

        currentState = HarpoonState.Following;
        isStuckInFish = false;
        isPlayerTouching = false;
        currentRetrievalTimer = 0f;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // ★魚を消したくないため、以下の削除処理をコメントアウトとして保持
        /*
        if (fishToDestroy != null && destroyFishOnRetrieval)
        {
            Destroy(fishToDestroy);
        }
        */
    }
}