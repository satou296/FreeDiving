using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // ← 新しいInput Systemを使うために追加

public class PauseManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject pauseCanvas; 

    [Header("制限設定")]
    [SerializeField] private string startSceneName = "Title"; // プロジェクトに合わせて Title や Home に変更してください

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

        // キーボードが接続されていない場合は処理しない（エラー回避）
        if (Keyboard.current == null) return;

        // 新しいInput Systemでのキー入力判定 (Escapeキー または Pキー)
        if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
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
        isPaused = true;
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(true);
        }
        
        Time.timeScale = 0f; 
    }

    // ゲームを再開する
    public void Resume()
    {
        isPaused = false;
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
        
        Time.timeScale = 1f; 
    }

    // 「最初からやり直す」ボタン用の処理
    public void Retry()
    {
        // シーンを再読み込みする前に、必ず時間の流れを元に戻す
        Time.timeScale = 1f; 
        
        // 現在のシーンの名前を取得して、再度ロードする
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 「タイトルへ戻る」ボタン用の処理
    public void GoToTitle()
    {
        // 超重要：シーン遷移する前に、必ず時間の流れを元に戻す
        Time.timeScale = 1f; 
        
        // プロジェクト構成に合わせて "Title" や "StageSelectScene" に変更してください
        SceneManager.LoadScene("Title"); 
    }
}