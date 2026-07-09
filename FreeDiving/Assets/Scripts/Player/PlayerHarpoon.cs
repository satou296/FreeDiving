using UnityEngine;

public class PlayerHarpoon : MonoBehaviour
{
    [Header("モリの設定")]
    [SerializeField] private GameObject harpoonPrefab;
    [SerializeField] private Transform shotPoint;
    
    public bool hasHarpoon = true;
    private GameObject activeHarpoon; // 現在プレイヤーについてきているモリの記憶用

    private void Update()
    {
        if (hasHarpoon && Input.GetKeyDown(KeyCode.Space))
        {
            ShootHarpoon();
        }
    }

    private void ShootHarpoon()
    {
        hasHarpoon = false;

        // もし古いモリがついてきていたら、それを削除してから新しく生成する
        if (activeHarpoon != null)
        {
            Destroy(activeHarpoon);
        }

        GameObject projectedHarpoon = Instantiate(harpoonPrefab, shotPoint.position, shotPoint.rotation);
        
        // 生成したモリを記憶しておく
        activeHarpoon = projectedHarpoon;

        Harpoon harpoonScript = projectedHarpoon.GetComponent<Harpoon>();
        if (harpoonScript != null)
        {
            harpoonScript.Launch(transform.right);
        }
    }

    // モリ側から呼び出される
    public void CatchHarpoon()
    {
        hasHarpoon = true;
    }
}