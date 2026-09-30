using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    // ストレージに格納する魚の簡易データ
    [System.Serializable]
    public class CaughtFish
    {
        public string fishName;
        public float weight;
        public int value;

        public CaughtFish(string name, float weight, int value)
        {
            this.fishName = name;
            this.weight = weight;
            this.value = value;
        }
    }

    [Header("ストレージ容量設定")]
    [SerializeField] private float maxWeightCapacity = 20.0f; // 持てる最大重量（kg）
    [SerializeField] private int maxSlotCount = 10;           // 持てる最大匹数

    // ★画面遷移しても破棄されない静的（static）な魚リストと重量
    private static List<CaughtFish> persistentStorageList = new List<CaughtFish>();
    private static float persistentCurrentWeight = 0f;

    [Header("インスペクター確認用（表示のみ）")]
    [SerializeField] private List<CaughtFish> storageList = new List<CaughtFish>();

    public float CurrentWeight => persistentCurrentWeight;
    public float MaxWeightCapacity => maxWeightCapacity;
    public int CurrentCount => persistentStorageList.Count;
    public int MaxSlotCount => maxSlotCount;
    public IReadOnlyList<CaughtFish> StorageList => persistentStorageList;

    private void Awake()
    {
        // シーン遷移時にインスペクター上の表示リストを最新の永続データと同期
        SyncInspectorList();
    }

    // 魚を追加できるかチェックして追加する処理
    public bool TryAddFish(EnemyFish fish)
    {
        if (fish == null) return false;

        // 匹数制限チェック
        if (persistentStorageList.Count >= maxSlotCount)
        {
            Debug.LogWarning("ストレージが満杯です！（最大匹数オーバー）");
            return false;
        }

        // 重量制限チェック
        if (persistentCurrentWeight + fish.FishWeight > maxWeightCapacity)
        {
            Debug.LogWarning("ストレージの重量制限を超えています！");
            return false;
        }

        // ストレージへ追加
        CaughtFish newFish = new CaughtFish(fish.FishName, fish.FishWeight, fish.FishValue);
        persistentStorageList.Add(newFish);
        persistentCurrentWeight += fish.FishWeight;

        // インスペクター確認用リストも更新
        SyncInspectorList();

        Debug.Log($"【捕獲成功】{newFish.fishName} ({newFish.weight}kg) を収納しました。 現在の総重量: {persistentCurrentWeight:F1}/{maxWeightCapacity}kg");
        return true;
    }

    // 帰還時などに合計売却価格を計算する
    public int CalculateTotalValue()
    {
        int total = 0;
        foreach (var f in persistentStorageList)
        {
            total += f.value;
        }
        return total;
    }

    // ストレージのクリア（帰還売却時やゲームオーバー時など）
    public void ClearInventory()
    {
        persistentStorageList.Clear();
        persistentCurrentWeight = 0f;
        SyncInspectorList();
        Debug.Log("ストレージの魚をすべて精算／クリアしました。");
    }

    // インスペクター上でも内容が見えるように同期する補助処理
    private void SyncInspectorList()
    {
        storageList.Clear();
        storageList.AddRange(persistentStorageList);
    }
}