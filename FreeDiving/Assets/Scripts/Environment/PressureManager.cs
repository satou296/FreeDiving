using UnityEngine;

public class PressureManager : MonoBehaviour
{
    public static PressureManager Instance;

    [Header("設定")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float waterSurfaceY = 0f; // 水面のY座標
    [SerializeField] private float pressureFactor = 0.5f; // 水深1mあたりの水圧上昇率

    // 他のスクリプトから現在の水圧を取得するためのプロパティ
    public float CurrentPressure { get; private set; }
    public float CurrentDepth { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // プレイヤーが水面より下にいる場合、水深を計算 (下に行くほどYはマイナス)
        if (playerTransform.position.y < waterSurfaceY)
        {
            CurrentDepth = waterSurfaceY - playerTransform.position.y;
            CurrentPressure = CurrentDepth * pressureFactor;
        }
        else
        {
            CurrentDepth = 0f;
            CurrentPressure = 0f;
        }
    }
}