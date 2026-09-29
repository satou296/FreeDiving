using UnityEngine;
using System.Collections.Generic;

public class FishSpawner : MonoBehaviour
{
    // 各魚のスポーンルールを定義する構造体
    [System.Serializable]
    public class FishSpawnData
    {
        public string fishName = "魚の名前";
        public GameObject fishPrefab; // 魚のプレハブ

        [Header("水深の範囲（Y座標）")]
        public float minY = -50f; // 最も深いY座標（例: -50）
        public float maxY = 0f;   // 最も浅いY座標（例: 0）

        [Header("出現確率の重み（大きいほど出やすい）")]
        [Range(0, 100)] public int spawnWeight = 10;
    }

    [Header("ステージ別の設定")]
    [SerializeField] private string stageName = "Stage 1"; // ステージ識別用
    [SerializeField] private List<FishSpawnData> fishSpawnList = new List<FishSpawnData>();

    [Header("スポーン間隔の設定")]
    [SerializeField] private float spawnInterval = 3f; // 何秒ごとに生成を試みるか

    [Header("スポーン範囲（画面幅・カメラ周辺）")]
    [SerializeField] private float spawnWidthX = 8f;   // 画面の横幅範囲（-X 〜 +X）
    [SerializeField] private float spawnOffsetY = 3f;  // カメラより少し画面外（上下）に出すオフセット
    [SerializeField] private Transform cameraTransform;

    [Header("制限設定")]
    [SerializeField] private int maxFishCount = 10; // シーン内に同時に存在できる最大魚数

    private float timer = 0f;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnFish();
        }
    }

    private void TrySpawnFish()
    {
        if (fishSpawnList == null || fishSpawnList.Count == 0) return;

        // シーン内の現在の魚数をタグでカウント
        int currentCount = GameObject.FindGameObjectsWithTag("Fish").Length + 
                           GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (currentCount >= maxFishCount) return;

        // 現在のカメラ（またはプレイヤー）のY座標を基準水深とする
        float currentDepthY = cameraTransform != null ? cameraTransform.position.y : transform.position.y;

        // 1. 現在の水深に出現可能な魚をリストアップし、重みの合計を計算
        List<FishSpawnData> validFishList = new List<FishSpawnData>();
        int totalWeight = 0;

        foreach (var data in fishSpawnList)
        {
            if (data.fishPrefab == null) continue;

            // 現在の水深がこの魚の出現範囲（minY 〜 maxY）内にあるかチェック
            if (currentDepthY >= data.minY && currentDepthY <= data.maxY)
            {
                if (data.spawnWeight > 0)
                {
                    validFishList.Add(data);
                    totalWeight += data.spawnWeight;
                }
            }
        }

        // 出現可能な魚がいない場合は終了
        if (validFishList.Count == 0 || totalWeight == 0) return;

        // 2. 重みに基づく確率抽選（ルーレット方式）
        int randomRoll = Random.Range(0, totalWeight);
        int accumulatedWeight = 0;
        GameObject selectedPrefab = null;

        foreach (var data in validFishList)
        {
            accumulatedWeight += data.spawnWeight;
            if (randomRoll < accumulatedWeight)
            {
                selectedPrefab = data.fishPrefab;
                break;
            }
        }

        if (selectedPrefab == null) return;

        // 3. 画面外（左右または下部）のランダムな位置に生成
        float spawnX = Random.Range(-spawnWidthX, spawnWidthX);
        // カメラの下側（プレイヤーの進行方向画面外）寄りにスポーンさせる
        float spawnY = currentDepthY - spawnOffsetY;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
        Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
    }
}