using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // New Input System対応
using System.Text; // StringBuilderを使用するために必要
using TMPro; // TextMeshProを使用するために必要

public class StorageUI : MonoBehaviour
{
    [Header("UI要素の参照")]
    [SerializeField] private GameObject storagePanel;               // ストレージ画面の親パネル
    [SerializeField] private TextMeshProUGUI capacityText;          // 重量・匹数表示用テキスト
    [SerializeField] private TextMeshProUGUI fishListText;          // 捕獲した魚の一覧表示用テキスト
    [SerializeField] private TextMeshProUGUI moneyText;             // 所持金表示用テキスト

    [Header("プレイヤー参照")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("動作設定")]
    [SerializeField] private bool pauseGameWhileOpen = true; // 開いている間ゲームを一時停止（ポーズ）するか

    private bool isOpen = false;

    private void Start()
    {
        // プレイヤーが未設定なら検索
        if (playerInventory == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerInventory = playerObj.GetComponent<PlayerInventory>();
            }
        }

        // 初期状態は確実に閉じておく
        if (storagePanel != null)
        {
            storagePanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Tabキー または Iキー で開閉トグル
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.tabKey.wasPressedThisFrame || keyboard.iKey.wasPressedThisFrame)
            {
                ToggleStorage();
            }
            // 開いている時にEscapeキーでも閉じられるようにする
            else if (isOpen && keyboard.escapeKey.wasPressedThisFrame)
            {
                CloseStorage();
            }
        }
    }

    // 開閉の切り替え
    public void ToggleStorage()
    {
        if (isOpen)
        {
            CloseStorage();
        }
        else
        {
            OpenStorage();
        }
    }

    // ストレージ画面を開く
    public void OpenStorage()
    {
        if (storagePanel == null) return;

        isOpen = true;
        storagePanel.SetActive(true);

        // 魚リストおよび所持金の表示を最新状態に更新
        UpdateStorageDisplay();

        // 必要に応じてゲーム内時間を止める
        if (pauseGameWhileOpen)
        {
            Time.timeScale = 0f;
        }
    }

    // ストレージ画面を閉じる
    public void CloseStorage()
    {
        if (storagePanel == null) return;

        isOpen = false;
        storagePanel.SetActive(false);

        // ゲーム内時間を再開
        if (pauseGameWhileOpen)
        {
            Time.timeScale = 1f;
        }
    }

    // 画面のテキスト内容を最新化
    private void UpdateStorageDisplay()
    {
        if (playerInventory == null) return;

        // 1. 容量テキストの更新（重量と匹数）
        if (capacityText != null)
        {
            capacityText.text = $"gross weight: {playerInventory.CurrentWeight:F1} / {playerInventory.MaxWeightCapacity:F1} kg   " +
                                $"Number of animals: {playerInventory.CurrentCount} / {playerInventory.MaxSlotCount} ";
        }

        // 2. 所持金テキストの更新
        if (moneyText != null)
        {
            moneyText.text = $"Money: {playerInventory.CurrentMoney:#,##0} G";
        }

        // 3. 捕獲した魚一覧テキストの構築
        if (fishListText != null)
        {
            if (playerInventory.CurrentCount == 0)
            {
                fishListText.text = "No fish have been caught so far.\n（You can retrieve the fish after spearing and pulling it out.）";
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("【List of Captured Fish】");
            sb.AppendLine("--------------------------------------------------");

            var list = playerInventory.StorageList;
            for (int i = 0; i < list.Count; i++)
            {
                var fish = list[i];
                sb.AppendLine($"{i + 1}. {fish.fishName}  |  weight: {fish.weight:F1} kg  |  Estimated value: {fish.value} G");
            }

            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"Total estimated sale price: {playerInventory.CalculateTotalValue():#,##0} G");

            fishListText.text = sb.ToString();
        }
    }

    private void OnDestroy()
    {
        // シーン終了時などに万が一止まっていたら時間を戻す
        if (pauseGameWhileOpen)
        {
            Time.timeScale = 1f;
        }
    }
}