using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class ShopManager : MonoBehaviour
{
    [Header("パネルの参照")]
    [SerializeField] private GameObject mainShopPanel; // メインメニュー画面（Sale/Buyボタンが置いてある親パネル）
    [SerializeField] private GameObject salePanel;      // 売却画面
    [SerializeField] private GameObject buyPanel;       // 購入画面

    [Header("初期表示設定")]
    [SerializeField] private bool openSaleOnStart = false; // 最初からSaleを開いておくかどうか

    [Header("共通UI表示")]
    [SerializeField] private TextMeshProUGUI currentMoneyText; // 現在の所持金テキスト

    [Header("売却（Sale）画面UI")]
    [SerializeField] private TextMeshProUGUI saleFishListText; // 持っている魚の一覧
    [SerializeField] private TextMeshProUGUI totalSellPriceText; // 合計売却額
    [SerializeField] private Button sellAllButton;              // 一括売却ボタン

    [Header("購入（Buy）画面UI - モリ")]
    [SerializeField] private TextMeshProUGUI harpoonStatusText; // モリの現在Lv・説明
    [SerializeField] private TextMeshProUGUI harpoonCostText;   // モリの価格
    [SerializeField] private Button buyHarpoonButton;
    [SerializeField] private int harpoonBaseCost = 300;         // モリ強化の基本価格

    [Header("購入（Buy）画面UI - 酸素タンク")]
    [SerializeField] private TextMeshProUGUI oxygenStatusText;  // 酸素の現在Lv・説明
    [SerializeField] private TextMeshProUGUI oxygenCostText;    // 酸素タンクの価格
    [SerializeField] private Button buyOxygenButton;
    [SerializeField] private int oxygenBaseCost = 250;          // 酸素強化の基本価格

    [Header("プレイヤーインベントリ参照")]
    [SerializeField] private PlayerInventory playerInventory;

    private void Start()
    {
        // プレイヤーインベントリが未割り当てなら検索
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }

        // 初期表示の制御
        if (openSaleOnStart)
        {
            OpenSalePanel();
        }
        else
        {
            // 最初はメインメニューを表示し、各パネルは閉じておく
            BackToMainMenu();
        }

        UpdateMoneyUI();
    }

    // メインメニュー画面に戻る（Sale/Buyパネルを閉じてMainPanelを表示）
    public void BackToMainMenu()
    {
        if (mainShopPanel != null) mainShopPanel.SetActive(true);
        if (salePanel != null) salePanel.SetActive(false);
        if (buyPanel != null) buyPanel.SetActive(false);
        UpdateMoneyUI();
    }

    // Saleボタンを押した時（MainPanelを消してSalePanelを表示）
    public void OpenSalePanel()
    {
        if (mainShopPanel != null) mainShopPanel.SetActive(false); // 非表示にする
        if (salePanel != null) salePanel.SetActive(true);
        if (buyPanel != null) buyPanel.SetActive(false);
        
        UpdateSaleUI();
        UpdateMoneyUI();
    }

    // Buyボタンを押した時（MainPanelを消してBuyPanelを表示）
    public void OpenBuyPanel()
    {
        if (mainShopPanel != null) mainShopPanel.SetActive(false); // 非表示にする
        if (salePanel != null) salePanel.SetActive(false);
        if (buyPanel != null) buyPanel.SetActive(true);
        
        UpdateBuyUI();
        UpdateMoneyUI();
    }

    // 両方のパネルを閉じる（互換性のために保持）
    public void CloseAllPanels()
    {
        BackToMainMenu();
    }

    // 魚をすべて売却するボタンを押した時
    public void OnClickSellAll()
    {
        if (playerInventory != null)
        {
            playerInventory.SellAllFish();
            UpdateSaleUI();
            UpdateMoneyUI();
        }
    }

    // モリ購入ボタンを押した時
    public void OnClickBuyHarpoon()
    {
        int cost = GetHarpoonUpgradeCost();
        if (playerInventory != null && playerInventory.TryUpgradeHarpoon(cost))
        {
            UpdateBuyUI();
            UpdateMoneyUI();
        }
    }

    // 酸素タンク購入ボタンを押した時
    public void OnClickBuyOxygen()
    {
        int cost = GetOxygenUpgradeCost();
        if (playerInventory != null && playerInventory.TryUpgradeOxygenTank(cost))
        {
            UpdateBuyUI();
            UpdateMoneyUI();
        }
    }

    // 強化費用計算（レベルが上がるごとに価格上昇）
    private int GetHarpoonUpgradeCost()
    {
        return harpoonBaseCost * PlayerInventory.HarpoonLevel;
    }

    private int GetOxygenUpgradeCost()
    {
        return oxygenBaseCost * PlayerInventory.OxygenTankLevel;
    }

    // 所持金テキストの更新
    private void UpdateMoneyUI()
    {
        if (currentMoneyText != null && playerInventory != null)
        {
            currentMoneyText.text = $"Cash on hand: {playerInventory.CurrentMoney:#,##0} G";
        }
    }

    // 売却画面の更新
    private void UpdateSaleUI()
    {
        if (playerInventory == null) return;

        var fishList = playerInventory.StorageList;
        int totalValue = playerInventory.CalculateTotalValue();

        if (saleFishListText != null)
        {
            if (fishList.Count == 0)
            {
                saleFishListText.text = "There are no fish available to sell.";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < fishList.Count; i++)
                {
                    sb.AppendLine($"{i + 1}. {fishList[i].fishName} ({fishList[i].weight:F1}kg) - {fishList[i].value} G");
                }
                saleFishListText.text = sb.ToString();
            }
        }

        if (totalSellPriceText != null)
        {
            totalSellPriceText.text = $"Total sale price: {totalValue:#,##0} G";
        }

        if (sellAllButton != null)
        {
            sellAllButton.interactable = (fishList.Count > 0);
        }
    }

    // 購入画面の更新
    private void UpdateBuyUI()
    {
        if (playerInventory == null) return;

        int harpoonCost = GetHarpoonUpgradeCost();
        int oxygenCost = GetOxygenUpgradeCost();

        // モリ表示更新
        if (harpoonStatusText != null)
        {
            harpoonStatusText.text = $"Reinforced Moly (Lv.{PlayerInventory.HarpoonLevel})\nIncreased power(+15)";
        }
        if (harpoonCostText != null)
        {
            harpoonCostText.text = $"Required amount: {harpoonCost:#,##0} G";
        }
        if (buyHarpoonButton != null)
        {
            buyHarpoonButton.interactable = (playerInventory.CurrentMoney >= harpoonCost);
        }

        // 酸素タンク表示更新
        if (oxygenStatusText != null)
        {
            oxygenStatusText.text = $"High-capacity oxygen cylinder(Lv.{PlayerInventory.OxygenTankLevel})\nIncreased diving time(+10秒)";
        }
        if (oxygenCostText != null)
        {
            oxygenCostText.text = $"Required amount: {oxygenCost:#,##0} G";
        }
        if (buyOxygenButton != null)
        {
            buyOxygenButton.interactable = (playerInventory.CurrentMoney >= oxygenCost);
        }
    }

    public void UpdateAllUI()
    {
        UpdateMoneyUI();
        UpdateSaleUI();
        UpdateBuyUI();
    }
}