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
    [SerializeField] private Vector2 followOffset = new Vector2(-0.5f, 0.5f); 

    [Header("引き抜き設定")]
    [SerializeField] private float retrievalTimeFromFish = 1.5f; // 魚からモリを引き抜くのに必要な秒数
    [SerializeField] private float retrievalTimeFromWall = 1.0f; // ★壁からモリを引き抜くのに必要な秒数

    [Header("引き抜いた後の魚の挙動")]
    [SerializeField] private bool destroyFishOnRetrieval = true; // 引き抜いた時に魚を消すかどうか（残すならチェックを外す）

    private Rigidbody2D rb;
    
    // 【修正】初期状態を Flying ではなく Stuck（停止・回収待ち状態）にする
    private HarpoonState currentState = HarpoonState.Stuck; 
    private Transform playerTransform; 

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
            
            // 念のため、トリガー判定（すり抜け接触）が有効になっているか確認
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }
    }

    private void Update()
    {
        // ★刺さっている状態でプレイヤーが触れている場合、長押し入力で回収タイマーを加算
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

        if (currentState == HarpoonState.Following && playerTransform != null)
        {
            Vector3 targetOffset = followOffset;
            
            // プレイヤーの向きによって左右のオフセットを反転
            if (playerTransform.right.x < 0) 
            {
                targetOffset.x *= -1; 
            }

            // Lerpを使わず、計算した位置に直接ワープ（固定）させることで距離を完全に一定にする
            transform.position = playerTransform.position + targetOffset;
            
            // 向きも完全にプレイヤーに同期させる場合
            transform.rotation = playerTransform.rotation;
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
            Debug.Log("魚にモリが刺さりました！");
            
            // 魚側の停止メソッドを呼び出す
            EnemyFish fish = collision.GetComponent<EnemyFish>();
            if (fish != null)
            {
                fish.OnHarpooned();
            }

            // モリ自体の物理を止めて刺さった状態にする
            currentState = HarpoonState.Stuck;
            isStuckInFish = true; // ★魚に刺さったフラグをON
            currentRetrievalTimer = 0f;

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            // 魚の子オブジェクトにして位置を固定（魚と一緒に居続ける）
            transform.SetParent(collision.transform);

            // ※以前の即時削除処理は無効化
            // Destroy(collision.gameObject);
            // Destroy(gameObject); 
        }
        // 2. 壁などに当たった場合（本当に飛んでいるときだけ判定）
        else if (currentState == HarpoonState.Flying && collision.CompareTag("Obstacle"))
        {
            Debug.Log("壁に刺さりました");
            currentState = HarpoonState.Stuck; 
            isStuckInFish = false; // 壁なので魚フラグはOFF
            currentRetrievalTimer = 0f;
            
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // 物理演算を止めて固定
        }
        // 3. 「刺さっている（停止している）状態」でプレイヤーが触れたら回収準備
        else if (currentState == HarpoonState.Stuck && collision.CompareTag("Player"))
        {
            PlayerHarpoon playerWeapon = collision.GetComponentInParent<PlayerHarpoon>();
            
            if (playerWeapon != null && !playerWeapon.hasHarpoon)
            {
                // ★壁・魚を問わずプレイヤーの接触を記録し、長押し回収の受け付けを開始
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

                // ※以前の壁への即時回収処理は無効化
                // ExecuteImmediateCatch(playerWeapon);
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
        }

        currentState = HarpoonState.Following;
        isStuckInFish = false;
        isPlayerTouching = false;
        currentRetrievalTimer = 0f;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // ★切り離しが完了した後に、設定に応じて魚を削除する
        if (fishToDestroy != null && destroyFishOnRetrieval)
        {
            Destroy(fishToDestroy);
        }
    }
}