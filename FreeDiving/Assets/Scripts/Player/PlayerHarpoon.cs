/*using UnityEngine;
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
        
        // 回収したモリがどれなのかをしっかり記憶する
        activeHarpoon = caughtHarpoon; 
    }
}*/

/*
using UnityEngine;
using UnityEngine.InputSystem; // New Input System対応

public class PlayerHarpoon : MonoBehaviour
{
    [Header("モリの設定")]
    [SerializeField] private GameObject harpoonPrefab; // 投げるモリのプレハブ
    [SerializeField] private Transform shotPoint;     // モリを発射する位置
    
    public bool hasHarpoon = true; // モリを持っているかどうかのフラグ
    private GameObject currentHarpoonObj; // 追従中のモリがある場合に保持する変数

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

    /*private void ShootHarpoon()
    {
        hasHarpoon = false; // 手放す

        // ★プレイヤーの向きを判定する（ScaleのXがプラスなら右向き、マイナスなら左向き）
        Vector2 throwDirection = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;

        // ※もし ShotPoint が設定されている場合、ShotPointの向きを使うことも可能です
        // Vector2 throwDirection = shotPoint != null ? (Vector2)shotPoint.right : (Vector2)transform.right;

        // 発射位置の決定（ShotPointが設定されていない場合はプレイヤー自身の位置）
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
            // ★プレイヤーの向きに応じたベクトルをLaunchに渡す
            harpoonScript.Launch(throwDirection);
        }
    }*/

    /*private void ShootHarpoon()
    {
        hasHarpoon = false; // 手放す

        // SpriteRendererのコンポーネントを取得
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // flipXがtrueなら左向き、falseなら右向きと判定する
        Vector2 throwDirection = Vector2.right;
        if (sr != null && sr.flipX)
        {
            throwDirection = Vector2.left;
        }
        else if (transform.localScale.x < 0)
        {
            // localScaleで反転している場合も考慮
            throwDirection = Vector2.left;
        }

        // 発射位置の決定
        Vector3 spawnPos = shotPoint != null ? shotPoint.position : transform.position;

        // モリの生成または追従していた既存モリの射出
        GameObject projectedHarpoon;
        if (currentHarpoonObj != null)
        {
            projectedHarpoon = currentHarpoonObj;
            currentHarpoonObj = null;
        }
        else
        {
            projectedHarpoon = Instantiate(harpoonPrefab, spawnPos, Quaternion.identity);
        }
        
        Harpoon harpoonScript = projectedHarpoon.GetComponent<Harpoon>();
        if (harpoonScript != null)
        {
            harpoonScript.Launch(throwDirection);
        }
    }*/
/*
    private void ShootHarpoon()
    {
        hasHarpoon = false;

        // PlayerControllerから向いている（移動していた）方向を取得
        PlayerController controller = GetComponent<PlayerController>();
        Vector2 throwDirection = Vector2.right;

        if (controller != null)
        {
            // 左右のみにしたい場合
            throwDirection = controller.LastMoveDirection.x >= 0 ? Vector2.right : Vector2.left;

            // ※もし上下や斜めにも投げたい場合は以下のようにそのまま渡せます
            // throwDirection = controller.LastMoveDirection;
        }

        Vector3 spawnPos = shotPoint != null ? shotPoint.position : transform.position;

        GameObject projectedHarpoon = currentHarpoonObj != null ? currentHarpoonObj : Instantiate(harpoonPrefab, spawnPos, Quaternion.identity);
        currentHarpoonObj = null;

        Harpoon harpoonScript = projectedHarpoon.GetComponent<Harpoon>();
        if (harpoonScript != null)
        {
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
}*/

using UnityEngine;
using UnityEngine.InputSystem; // New Input System対応

public class PlayerHarpoon : MonoBehaviour
{
    [Header("モリの設定")]
    [SerializeField] private GameObject harpoonPrefab; // 投げるモリのプレハブ
    [SerializeField] private Transform shotPoint;     // モリを発射する位置
    
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