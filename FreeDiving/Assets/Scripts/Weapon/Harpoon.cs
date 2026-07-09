using UnityEngine;

public class Harpoon : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    
    private Rigidbody2D rb;
    private bool isStuck = false; // 壁に刺さっているかどうかのフラグ

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
    }

    // プレイヤーから呼び出されて飛ぶ方向を決める
    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction * speed;
    }

    // 2Dの当たり判定（トリガー判定）
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 魚に当たった場合
        if (collision.CompareTag("Fish"))
        {
            Debug.Log("魚にモリが当たりました！");
            Destroy(collision.gameObject); // 魚を消す
            Destroy(gameObject);           // モリも消す（魚に当たった時は消して良い場合）
        }
        // 2. 壁などに当たった場合
        else if (collision.CompareTag("Obstacle"))
        {
            Debug.Log("壁に刺さりました");
            isStuck = true; 
            
            rb.linearVelocity = Vector2.zero; // 停止させる
            rb.bodyType = RigidbodyType2D.Kinematic; // 物理演算を止めて固定
        }
        // 3. 壁に刺さっている状態でプレイヤーが触れた場合
        else if (isStuck && collision.CompareTag("Player"))
        {
            PlayerHarpoon playerWeapon = collision.GetComponent<PlayerHarpoon>();
            if (playerWeapon != null)
            {
                Debug.Log("モリを回収しました！");
                playerWeapon.CatchHarpoon(); // プレイヤーを「モリを持っている状態」に戻す
                Destroy(gameObject);         // 回収されたので、画面上からモリを消す
            }
        }
    }
}