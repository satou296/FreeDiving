using UnityEngine;
using UnityEngine.InputSystem; // New Input System対応

public class PlayerHarpoon : MonoBehaviour
{
    [Header("モリの設定")]
    [SerializeField] private GameObject harpoonPrefab; // 投げるモリのプレハブ
    [SerializeField] private Transform shotPoint;     // モリを発射する位置
    public Transform ShotPoint => shotPoint != null ? shotPoint : transform;  //外部アクセスを可能にする
    
    public bool hasHarpoon = true; // モリを持っているかどうかのフラグ
    private GameObject currentHarpoonObj; // 追従中のモリがある場合に保持する変数
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // New Input System方式でスペースキーを監視
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // モリを持っていて、スペースキーが押されたら投げる
        if (hasHarpoon && keyboard.spaceKey.wasPressedThisFrame)
        {
            ShootHarpoon();
        }
    }

    private void ShootHarpoon()
    {
        hasHarpoon = false; // 手放す

        // ★PlayerControllerから入力された上下左右の向きを取得（取得できない場合は右向きデフォルト）
        Vector2 throwDirection = Vector2.right;
        if (playerController != null)
        {
            throwDirection = playerController.LastAimDirection;
        }

        // 発射位置の決定
        Vector3 spawnPos = shotPoint != null ? shotPoint.position : transform.position;

        // モリの生成または追従していた既存モリの射出
        GameObject projectedHarpoon;
        if (currentHarpoonObj != null)
        {
            // 既に回収して追従中だったモリを再利用して投げる場合
            projectedHarpoon = currentHarpoonObj;
            currentHarpoonObj = null;
        }
        else
        {
            // 新規にプレハブから生成する場合
            projectedHarpoon = Instantiate(harpoonPrefab, spawnPos, Quaternion.identity);
        }
        
        Harpoon harpoonScript = projectedHarpoon.GetComponent<Harpoon>();
        if (harpoonScript != null)
        {
            // ★計算した上下左右の方向ベクトルをLaunchに渡す
            harpoonScript.Launch(throwDirection);
        }
    }

    // モリを再回収するためのメソッド（引数付き）
    public void CatchHarpoon(GameObject harpoon)
    {
        hasHarpoon = true;
        currentHarpoonObj = harpoon;
    }

    // 既存の引数なし版も互換性保持のために残す
    public void CatchHarpoon()
    {
        hasHarpoon = true;
    }
}