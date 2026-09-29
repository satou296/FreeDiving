using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenEdgeColliders : MonoBehaviour
{
    [Header("壁の厚み")]
    [SerializeField] private float wallThickness = 1.0f;

    [Header("適用するタグ")]
    [SerializeField] private string wallTag = "Obstacle";

    private void Start()
    {
        GenerateEdgeColliders();
    }

    private void GenerateEdgeColliders()
    {
        Camera cam = GetComponent<Camera>();

        // カメラの撮影範囲の高さと幅をワールド座標で計算
        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;

        // 左の透明な壁
        CreateWall("LeftWall", 
            new Vector2(-screenWidth / 2f - wallThickness / 2f, 0f), 
            new Vector2(wallThickness, screenHeight * 2f)); // スクロールブレを考慮して縦長に

        // 右の透明な壁
        CreateWall("RightWall", 
            new Vector2(screenWidth / 2f + wallThickness / 2f, 0f), 
            new Vector2(wallThickness, screenHeight * 2f));
    }

    private void CreateWall(string name, Vector2 localPosition, Vector2 size)
    {
        // カメラの子オブジェクトとして空のゲームオブジェクトを作成
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(transform);
        wall.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);

        // レイヤーを「ScreenWall」に設定
        wall.layer = LayerMask.NameToLayer("ScreenWall");

        // タグを設定（モリの刺さり判定用）
        if (!string.IsNullOrEmpty(wallTag))
        {
            wall.tag = wallTag;
        }

        // コライダーを追加してサイズを設定（SpriteRendererはつけないため完全に見えない）
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = size;
    }
}