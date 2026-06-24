using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject pauseCanvas; // インスペクターでポーズUIのCanvasを割り当てる

    private bool isPaused = false;

    private void Start()
    {
        // ゲーム開始時はポーズ画面を非表示にしておく
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
    }

    private void Update()
    {
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
        isPaused = true;
        pauseCanvas.SetActive(true);
        Time.timeScale = 0f; // ゲーム内の時間の流れを止める
    }

    // ゲームを再開する（ポーズ画面の「戻る」ボタンからも呼び出せる）
    public void Resume()
    {
        isPaused = false;
        pauseCanvas.SetActive(false);
        Time.timeScale = 1f; // 時間の流れを元に戻す
    }
}