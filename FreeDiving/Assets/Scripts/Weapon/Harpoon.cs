using UnityEngine;

public class Harpoon : MonoBehaviour
{
    // モリの状態を定義
    private enum HarpoonState
    {
        Flying,    // 飛んでいる
        Stuck,     // 壁に刺さっている
        Following  // プレイヤーに追従している
    }

    [SerializeField] private float speed = 10f;
    [SerializeField] private float followSpeed = 5f;     // 追従するときのスピード
    [SerializeField] private Vector2 followOffset = new Vector2(-0.5f, 0.5f); // プレイヤーからの距離

    private Rigidbody2D rb;
    private HarpoonState currentState = HarpoonState.Flying; // 初期状態は「飛んでいる」
    private Transform playerTransform; // 追従対象のプレイヤー

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // プレイヤー追従モードの場合
        if (currentState == HarpoonState.Following && playerTransform != null)
        {
            Vector3 targetOffset = followOffset;
            if (playerTransform.right.x < 0) 
            {
                targetOffset.x *= -1; // 左を向いていたらオフセットも反転
            }

            Vector3 targetPosition = playerTransform.position + targetOffset;

            // スムーズに目標位置へ移動
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
            transform.rotation = playerTransform.rotation;
        }
    }

    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction * speed;
        currentState = HarpoonState.Flying;
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
        // 2. 壁などに当たった場合（まだ飛んでいるときだけ判定）
        else if (currentState == HarpoonState.Flying && collision.CompareTag("Obstacle"))
        {
            Debug.Log("壁に刺さりました");
            currentState = HarpoonState.Stuck; // 状態を「刺さっている」に変更
            
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        // 3. 「壁に刺さっている状態」でプレイヤーが触れた場合のみ回収できる
        else if (currentState == HarpoonState.Stuck && collision.CompareTag("Player"))
        {
            PlayerHarpoon playerWeapon = collision.GetComponent<PlayerHarpoon>();
            
            if (playerWeapon != null && !playerWeapon.hasHarpoon)
            {
                Debug.Log("モリがプレイヤーの追従を開始しました！");
                playerWeapon.CatchHarpoon(); // プレイヤー側のフラグを戻す
                
                playerTransform = collision.transform;
                currentState = HarpoonState.Following; // 状態を「追従中」に変更

                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
            }
        }
        // 「壁に刺さっていない状態」でもプレイヤーは回収できる
        else if (collision.CompareTag("Player"))
        {
            PlayerHarpoon playerWeapon = collision.GetComponent<PlayerHarpoon>();
            
            if (playerWeapon != null && !playerWeapon.hasHarpoon)
            {
                Debug.Log("モリがプレイヤーの追従を開始しました！");
                playerWeapon.CatchHarpoon(); // プレイヤー側のフラグを戻す
                
                playerTransform = collision.transform;
                currentState = HarpoonState.Following; // 状態を「追従中」に変更

                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}