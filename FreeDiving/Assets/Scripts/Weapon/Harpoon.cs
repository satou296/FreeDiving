using UnityEngine;

public class Harpoon : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 3f; // 画面外などで消えるまでの時間
    
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // 一定時間経ったら自動で削除
        Destroy(gameObject, lifeTime);
    }

    // プレイヤーから呼び出されて飛ぶ方向を決める
    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction * speed;
    }

    // 2Dの当たり判定（トリガー判定）
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 当たった相手のタグが「Fish（魚）」だった場合
        if (collision.CompareTag("Fish"))
        {
            // 魚にダメージを与える、あるいは魚を消す処理
            Debug.Log("魚にモリが当たりました！");
            Destroy(collision.gameObject); // 魚を消す
            Destroy(gameObject);           // モリも消す
        }
        // 壁などに当たった場合
        else if (collision.CompareTag("Obstacle"))
        {
            Debug.Log("壁に刺さりました");
            rb.linearVelocity = Vector2.zero; // 停止させる
            rb.bodyType = RigidbodyType2D.Kinematic; // 物理演算を止めて固定
        }
    }
}