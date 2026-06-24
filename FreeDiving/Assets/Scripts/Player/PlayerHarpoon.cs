using UnityEngine;

public class PlayerHarpoon : MonoBehaviour
{
    [Header("モリの設定")]
    [SerializeField] private GameObject harpoonPrefab; // 投げるモリのプレハブ
    [SerializeField] private Transform shotPoint;     // モリを発射する位置
    
    public bool hasHarpoon = true; // モリを持っているかどうかのフラグ

    private void Update()
    {
        // モリを持っていて、スペースキーが押されたら投げる
        if (hasHarpoon && Input.GetKeyDown(KeyCode.Space))
        {
            ShootHarpoon();
        }
    }

    private void ShootHarpoon()
    {
        hasHarpoon = false; // 手放す

        // モリの生成
        GameObject projectedHarpoon = Instantiate(harpoonPrefab, shotPoint.position, shotPoint.rotation);
        
        // プレイヤーの向いている方向に合わせてモリを飛ばす（右向きを基準とした例）
        Harpoon harpoonScript = projectedHarpoon.GetComponent<Harpoon>();
        if (harpoonScript != null)
        {
            // プレイヤーの向き（ローカルの右方向）に発射
            harpoonScript.Launch(transform.right);
        }
    }

    // モリを再回収するためのメソッド（必要に応じて）
    public void CatchHarpoon()
    {
        hasHarpoon = true;
    }
}