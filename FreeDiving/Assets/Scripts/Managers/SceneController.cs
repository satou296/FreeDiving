using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // 指定した名前のシーンに遷移する
    public void LoadScene(string sceneName)
    {
        // ポーズ中に遷移する場合を考慮し、時間を通常速度に戻しておく
        Time.timeScale = 1f; 
        SceneManager.LoadScene(sceneName);
    }

    // アプリケーション（ゲーム）を終了する（タイトル画面用）
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}