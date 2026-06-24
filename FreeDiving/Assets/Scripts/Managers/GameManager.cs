using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // プレイヤー死亡時などに、外部のスクリプト（当たり判定など）から呼び出す
    public void GameOver()
    {
        Debug.Log("ゲームオーバー！ 画面を遷移します。");
        // GameOverSceneへ遷移
        SceneManager.LoadScene("GameOverScene");
    }
}