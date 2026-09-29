using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReturnTriggerManager : MonoBehaviour
{
    [Header("監視対象")]
    [SerializeField] private Transform playerTransform;

    [Header("帰還判定Y座標")]
    [SerializeField] private float returnThresholdY = 5f; // このY座標以上になったら帰還ダイアログ表示

    [Header("UI設定")]
    [SerializeField] private GameObject returnConfirmPanel; // 確認ダイアログのパネル
    [SerializeField] private Button confirmReturnButton;    // 帰還決定ボタン
    [SerializeField] private Button cancelButton;           // キャンセル（戻る）ボタン

    [Header("遷移先シーン名")]
    [SerializeField] private string stageSelectSceneName = "StageSelectScene";

    private bool hasTriggered = false; // 一度開いたら連続で開かないようにするフラグ

    private void Start()
    {
        // プレイヤーが未割り当ての場合、タグから検索
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        // 初期状態でパネルを確実に閉じておく
        if (returnConfirmPanel != null)
        {
            returnConfirmPanel.SetActive(false);
        }

        // ボタンのクリックイベントを登録
        if (confirmReturnButton != null)
        {
            confirmReturnButton.onClick.AddListener(OnConfirmReturn);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelReturn);
        }
    }

    private void Update()
    {
        if (hasTriggered || playerTransform == null) return;

        // プレイヤーが指定したY座標以上に浮上したら実行
        if (playerTransform.position.y >= returnThresholdY)
        {
            ShowReturnConfirm();
        }
    }

    // 確認ダイアログを表示し、ゲーム内時間を止める
    private void ShowReturnConfirm()
    {
        hasTriggered = true;

        if (returnConfirmPanel != null)
        {
            returnConfirmPanel.SetActive(true);
        }

        // ゲーム内時間を停止（ポーズ状態）
        Time.timeScale = 0f;
    }

    // 「帰還する」ボタンを押したときの処理
    public void OnConfirmReturn()
    {
        // シーン遷移前に必ず時間を通常速度に戻す
        Time.timeScale = 1f;

        // ステージセレクト画面へシーン遷移
        SceneManager.LoadScene(stageSelectSceneName);
    }

    // 「潜水を続ける」ボタンを押したときの処理
    public void OnCancelReturn()
    {
        if (returnConfirmPanel != null)
        {
            returnConfirmPanel.SetActive(false);
        }

        // 時間を再開
        Time.timeScale = 1f;

        // 画面を閉じた直後に即座に再判定されないよう、プレイヤーを少し下へ押し戻す
        if (playerTransform != null)
        {
            Vector3 pos = playerTransform.position;
            pos.y = returnThresholdY - 0.5f;
            playerTransform.position = pos;
        }

        // 再度浮上したときにまた開くようにフラグを戻す
        hasTriggered = false;
    }

    // オブジェクト破棄時など念のため時間を戻す
    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}