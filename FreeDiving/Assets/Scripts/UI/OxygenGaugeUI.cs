using UnityEngine;
using UnityEngine.UI;

public class OxygenGaugeUI : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Slider oxygenSlider;      // 連動させるSliderコンポーネント
    [SerializeField] private PlayerController player; // 酸素情報を取得するプレイヤー

    private void Start()
    {
        // もしインスペクターでSliderが指定されていない場合、自身から自動取得を試みる
        if (oxygenSlider == null)
        {
            oxygenSlider = GetComponent<Slider>();
        }

        // プレイヤーが未割り当ての場合、タグから自動検索
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
            }
        }
    }

    private void Update()
    {
        if (oxygenSlider == null || player == null) return;

        // 最大酸素時間が0でないことを確認して、割合（0.0 〜 1.0）を計算
        if (player.MaxOxygenTime > 0f)
        {
            float fillRatio = player.CurrentOxygenTime / player.MaxOxygenTime;
            
            // スライダーの現在値（0〜1）に反映
            oxygenSlider.value = Mathf.Clamp01(fillRatio);
        }
    }
}