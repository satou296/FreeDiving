using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject pauseCanvas; // インスペクターでポーズUIのCanvas（またはパネル）を割り当てる

    [Header("制限設定")]
    [SerializeField] private string startSceneName = "StartScene"; // ポーズを禁止するスタート画面のシーン名

    private bool isPaused = false;
    private bool canPauseInThisScene = true;

    private void Start()
    {
        // 現在のシーン名を取得
        string currentSceneName = SceneManager.GetActiveScene().name;

        // スタート画面であれば、このシーンでのポーズ機能を無効化する
        if (currentSceneName == startSceneName)
        {
            canPauseInThisScene = false;
        }

        // ゲーム開始時はポーズ画面を確実に非表示にしておく
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        // スタート画面なら入力を一切受け付けない
        if (!canPauseInThisScene) return;

        // EscapeキーまたはPキーでポーズを切り替え
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // ゲームを一時停止する
    public void Pause()
    {
        // ゲームオーバー画面やセレクト画面など、元々時間が止まっている、
        // あるいは物理演算がない画面でもUIだけは表示できるようにする
        isPaused = true;
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(true);
        }
        
        // 念のため時間を止める（ステージ1・2用）
        Time.timeScale = 0f; 
    }

    // ゲームを再開する（ポーズ画面の「戻る」ボタンからも呼び出せる）
    public void Resume()
    {
        isPaused = false;
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
        
        // 時間の流れを元に戻す
        Time.timeScale = 1f; 
    }
}