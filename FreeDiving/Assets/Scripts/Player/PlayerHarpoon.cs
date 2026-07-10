using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerHarpoon : MonoBehaviour
{
    [Header("モリの設定")]
    [SerializeField] private GameObject harpoonPrefab;
    [SerializeField] private Transform shotPoint;
    
    public bool hasHarpoon = true;
    private GameObject activeHarpoon; // 現在プレイヤーについてきているモリの記憶用

    private void Update()
    {
        if (hasHarpoon && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ShootHarpoon();
        }
    }

    private void ShootHarpoon()
    {
        hasHarpoon = false;

        // もし古いモリ（ついてきているモリ）があれば、それを画面から消す
        if (activeHarpoon != null)
        {
            Destroy(activeHarpoon);
        }

        // 新しいモリを生成して発射
        GameObject projectedHarpoon = Instantiate(harpoonPrefab, shotPoint.position, shotPoint.rotation);
        
        // 今投げたモリを記憶しておく（次に拾うか投げる時用）
        activeHarpoon = projectedHarpoon;

        Harpoon harpoonScript = projectedHarpoon.GetComponent<Harpoon>();
        if (harpoonScript != null)
        {
            harpoonScript.Launch(transform.right);
        }
    }

    // モリ側から「自分自身（caughtHarpoon）」を渡してもらうように変更
    public void CatchHarpoon(GameObject caughtHarpoon)
    {
        hasHarpoon = true;
        
        // 【追加】回収したモリがどれなのかをしっかり記憶する
        activeHarpoon = caughtHarpoon; 
    }
}