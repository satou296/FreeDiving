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

    [Header("初期所持金（初回起動時のみ）")]
    [SerializeField] private int initialMoney = 0;

    // 画面遷移しても破棄されない静的（static）な魚リストと重量
    private static List<CaughtFish> persistentStorageList = new List<CaughtFish>();
    private static float persistentCurrentWeight = 0f;

    // 画面遷移しても破棄されない静的（static）な所持金
    private static int persistentMoney = 0;
    private static bool isInitialized = false;

    // 画面遷移しても破棄されない装備アップグレードレベル
    private static int harpoonLevel = 1;     // モリのレベル（初期1）
    private static int oxygenTankLevel = 1;  // 酸素タンクのレベル（初期1）

    [Header("インスペクター確認用（表示のみ）")]
    [SerializeField] private List<CaughtFish> storageList = new List<CaughtFish>();
    [SerializeField] private int currentMoneyDisplay = 0; // インスペクター確認用
    [SerializeField] private int currentHarpoonLevelDisplay = 1;
    [SerializeField] private int currentOxygenLevelDisplay = 1;

    public float CurrentWeight => persistentCurrentWeight;
    public float MaxWeightCapacity => maxWeightCapacity;
    public int CurrentCount => persistentStorageList.Count;
    public int MaxSlotCount => maxSlotCount;
    public IReadOnlyList<CaughtFish> StorageList => persistentStorageList;

    // 所持金・装備レベルの公開プロパティ
    public int CurrentMoney => persistentMoney;
    public static int HarpoonLevel => harpoonLevel;
    public static int OxygenTankLevel => oxygenTankLevel;

    private void Awake()
    {
        // ゲーム起動時の初期所持金設定
        if (!isInitialized)
        {
            persistentMoney = initialMoney;
            isInitialized = true;
        }

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

    // すべての魚を一括売却する処理
    public int SellAllFish()
    {
        int totalEarnings = CalculateTotalValue();
        if (totalEarnings > 0)
        {
            AddMoney(totalEarnings);
            ClearInventory();
            Debug.Log($"魚をすべて売却し、{totalEarnings} G を獲得しました！");
        }
        else
        {
            Debug.Log("売却できる魚を持っていません。");
        }
        return totalEarnings;
    }

    // モリのアップグレード購入処理
    public bool TryUpgradeHarpoon(int cost)
    {
        if (TrySpendMoney(cost))
        {
            harpoonLevel++;
            SyncInspectorList();
            Debug.Log($"モリをレベル {harpoonLevel} にアップグレードしました！");
            return true;
        }
        return false;
    }

    // 酸素タンクのアップグレード購入処理
    public bool TryUpgradeOxygenTank(int cost)
    {
        if (TrySpendMoney(cost))
        {
            oxygenTankLevel++;
            SyncInspectorList();
            Debug.Log($"酸素タンクをレベル {oxygenTankLevel} にアップグレードしました！");
            return true;
        }
        return false;
    }

    // 所持金の追加（魚の売却、報酬獲得時など）
    public void AddMoney(int amount)
    {
        persistentMoney += amount;
        SyncInspectorList();
        Debug.Log($"{amount} G を獲得しました！ 現在の所持金: {persistentMoney} G");
    }

    // 所持金の消費（装備購入、アップグレード時など）
    public bool TrySpendMoney(int amount)
    {
        if (persistentMoney >= amount)
        {
            persistentMoney -= amount;
            SyncInspectorList();
            Debug.Log($"{amount} G を消費しました。 残り所持金: {persistentMoney} G");
            return true;
        }

        Debug.LogWarning("所持金が足りません！");
        return false;
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
        currentMoneyDisplay = persistentMoney;
        currentHarpoonLevelDisplay = harpoonLevel;
        currentOxygenLevelDisplay = oxygenTankLevel;
    }
}