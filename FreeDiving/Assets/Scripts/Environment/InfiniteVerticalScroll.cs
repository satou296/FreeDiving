using UnityEngine;

public class InfiniteVerticalScroll : MonoBehaviour
{
    [Header("監視対象のカメラ")]
    [SerializeField] private Transform cameraTransform;

    [Header("背景画像の高さ")]
    [SerializeField] private float backgroundHeight = 10f; // 画像の縦幅に合わせて設定

    [Header("水深による色のグラデーション設定")]
    [SerializeField] private Color surfaceColor = new Color(0.2f, 0.6f, 1f, 1f);   // 水面付近の明るい青
    [SerializeField] private Color deepSeaColor = new Color(0.02f, 0.05f, 0.2f, 1f); // 深海の濃紺・暗黒
    [SerializeField] private float surfaceY = 0f;    // 水面のY座標
    [SerializeField] private float maxDepthY = -50f; // 最も暗くなる水深のY座標

    [Header("海底到達時の制限設定")]
    [SerializeField] private bool stopAtSeabed = true; // 海底でループを停止するかどうか
    [SerializeField] private float seabedY = -50f;     // 海底（地面）のY座標

    [Header("水面（上方向）の制限設定")]
    [SerializeField] private bool stopAtSurface = false; // 水面より上には背景をループさせない場合はtrue（無限にループさせるならfalse）
    [SerializeField] private float surfaceLimitY = 10f;  // 水面の上限Y座標（stopAtSurfaceがtrueの時のみ使用）

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        // 1. 水深に応じた色の変更
        UpdateBackgroundColor();

        if (cameraTransform == null) return;

        // ★下方向に潜っている場合の処理（カメラがこの背景の下端を通り過ぎたとき）
        if (cameraTransform.position.y < transform.position.y - backgroundHeight)
        {
            // 次にワープする予定のY座標（下方向）を計算
            float nextPosY = transform.position.y - (backgroundHeight * 2f);

            // 海底制限が有効で、次にワープする位置が海底を超えてしまう場合はループを止める
            if (stopAtSeabed && nextPosY < seabedY)
            {
                return;
            }

            // 背景2枚分の距離だけ「下」へワープさせる
            Vector3 newPos = transform.position;
            newPos.y = nextPosY;
            transform.position = newPos;
        }
        // ★上方向に浮上している場合の処理（カメラがこの背景の上端を通り過ぎたとき）
        else if (cameraTransform.position.y > transform.position.y + backgroundHeight)
        {
            // 次にワープする予定のY座標（上方向）を計算
            float nextPosY = transform.position.y + (backgroundHeight * 2f);

            // 水面上限の制限が有効な場合
            if (stopAtSurface && nextPosY > surfaceLimitY)
            {
                return;
            }

            // 背景2枚分の距離だけ「上」へワープさせる
            Vector3 newPos = transform.position;
            newPos.y = nextPosY;
            transform.position = newPos;
        }
    }

    // 現在のオブジェクトのY座標（水深）に合わせて色を補間する
    private void UpdateBackgroundColor()
    {
        if (spriteRenderer == null) return;

        // 水面から最深部までの進捗割合（0.0: 水面 〜 1.0: 最深部）を計算
        float depthRatio = Mathf.InverseLerp(surfaceY, maxDepthY, transform.position.y);

        // 線形補間（Lerp）で浅い色から濃紺へブレンド
        spriteRenderer.color = Color.Lerp(surfaceColor, deepSeaColor, depthRatio);
    }
}