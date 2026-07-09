using UnityEngine;

public class Harpoon : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float followSpeed = 5f;     // 追従するときのスピード
    [SerializeField] private Vector2 followOffset = new Vector2(-0.5f, 0.5f); // プレイヤーからどれくらい離すか（例: 左上に少し離す）

    private Rigidbody2D rb;
    private bool isStuck = false;
    private bool isFollowing = false; // プレイヤーに回収されてついていっているか
    private Transform playerTransform; // 追従対象のプレイヤー

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // プレイヤー追従モードの場合
        if (isFollowing && playerTransform != null)
        {
            // プレイヤーの向き（transform.rightの正負など）に合わせてオフセットの左右を反転させると自然になります
            Vector3 targetOffset = followOffset;
            if (playerTransform.right.x < 0) 
            {
                targetOffset.x *= -1; // 左を向いていたらオフセットも右側にする
            }

            // 目標位置（プレイヤーの位置 + オフセット）
            Vector3 targetPosition = playerTransform.position + targetOffset;

            // スムーズに目標位置へ移動させる
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
            
            // プレイヤーと同じ回転に合わせる（必要に応じて）
            transform.rotation = playerTransform.rotation;
        }
    }

    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Fish"))
        {
            Debug.Log("魚にモリが当たりました！");
            Destroy(collision.gameObject);
            Destroy(gameObject); // 魚に当たった時は消す（あるいはここでも回収待ちにするかはお好みで）
        }
        else if (collision.CompareTag("Obstacle"))
        {
            Debug.Log("壁に刺さりました");
            isStuck = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        // 壁に刺さっている、または「すでに追従して浮いている状態」で再度プレイヤーが触れたとき
        else if (collision.CompareTag("Player"))
        {
            PlayerHarpoon playerWeapon = collision.GetComponent<PlayerHarpoon>();
            
            // まだプレイヤーがモリを持っていない（hasHarpoon == false）場合のみ回収できる
            if (playerWeapon != null && !playerWeapon.hasHarpoon)
            {
                Debug.Log("モリがプレイヤーの追従を開始しました！");
                playerWeapon.CatchHarpoon(); // プレイヤー側のフラグを「持っている」にする
                
                // 【変更点】Destroyせず、追従モードをONにする
                isStuck = false;
                isFollowing = true;
                playerTransform = collision.transform;

                // 物理演算が邪魔をしないようにトリガー化し、速度を完全にクリア
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}