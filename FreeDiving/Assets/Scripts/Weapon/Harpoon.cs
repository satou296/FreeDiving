using UnityEngine;

public class Harpoon : MonoBehaviour
{
    private enum HarpoonState
    {
        Flying,    // 飛んでいる
        Stuck,     // 壁に刺さっている（または最初から地面に落ちている）
        Following  // プレイヤーに追従している
    }

    [SerializeField] private float speed = 10f;
    [SerializeField] private float followSpeed = 5f;     
    [SerializeField] private Vector2 followOffset = new Vector2(-0.5f, 0.5f); 

    private Rigidbody2D rb;
    
    // 【修正】初期状態を Flying ではなく Stuck（停止・回収待ち状態）にする
    private HarpoonState currentState = HarpoonState.Stuck; 
    private Transform playerTransform; 

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
        if (currentState == HarpoonState.Following && playerTransform != null)
        {
            Vector3 targetOffset = followOffset;
            if (playerTransform.right.x < 0) 
            {
                targetOffset.x *= -1; 
            }

            Vector3 targetPosition = playerTransform.position + targetOffset;

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
            transform.rotation = playerTransform.rotation;
        }
    }
    // プレイヤーが「スペースキー」で投げた時にこれが呼ばれる
    public void Launch(Vector2 direction)
    {
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
        // 1. 魚に当たった場合
        if (collision.CompareTag("Fish"))
        {
            Debug.Log("魚にモリが当たりました！");
            Destroy(collision.gameObject);
            Destroy(gameObject); 
        }
        // 2. 壁などに当たった場合（本当に飛んでいるときだけ判定）
        else if (currentState == HarpoonState.Flying && collision.CompareTag("Obstacle"))
        {
            Debug.Log("壁に刺さりました");
            currentState = HarpoonState.Stuck; 
            
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // 物理演算を止めて固定
        }
        // 3. 「刺さっている（停止している）状態」でプレイヤーが触れたら回収（追従開始）
        else if (currentState == HarpoonState.Stuck && collision.CompareTag("Player"))
        {
            PlayerHarpoon playerWeapon = collision.GetComponentInParent<PlayerHarpoon>();
            
            if (playerWeapon != null && !playerWeapon.hasHarpoon)
            {
                Debug.Log("モリがプレイヤーの追従を開始しました！");
                
                // 自分自身のゲームオブジェクト(gameObject)を渡して回収してもらう
                playerWeapon.CatchHarpoon(gameObject); 
                
                playerTransform = playerWeapon.transform;
                currentState = HarpoonState.Following; 

                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}