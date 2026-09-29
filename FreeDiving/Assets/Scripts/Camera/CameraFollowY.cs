using UnityEngine;

public class CameraFollowY : MonoBehaviour
{
    [Header("追従対象")]
    [SerializeField] private Transform targetPlayer; // プレイヤーのTransform

    [Header("追従の滑らかさ設定")]
    [SerializeField] private float smoothTime = 0.25f; // 目標位置に達するまでの目安時間（小さいほど素早く追従）

    [Header("オフセット設定")]
    [SerializeField] private float offsetY = -1.0f; // プレイヤーより少し下（進行方向）を映したい場合に調整
    [SerializeField] private float fixedX = 0f;     // カメラのX座標を固定する場合の値

    [Header("追従制限設定")]
    [SerializeField] private bool onlyFollowDownwards = false; // trueにすると下方向にのみ追従（上に戻ってもカメラが上がらない仕様）

    private float currentVelocityY; // SmoothDamp内部で使用する速度計算用変数
    private float lowestY;

    private void Start()
    {
        // プレイヤーがインスペクターで割り当てられていない場合、タグから自動検索
        if (targetPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
        }

        lowestY = transform.position.y;
    }

    [Header("海底でのカメラ停止設定")]
[SerializeField] private float minCameraY = -48f; // カメラが下がる限界のY座標

    // プレイヤーの移動処理（FixedUpdate/Update）が完了した後にカメラを動かすため LateUpdate を使用
    private void LateUpdate()
    {
        if (targetPlayer == null) return;

        // 目標とするY座標の計算
        float targetY = targetPlayer.position.y + offsetY;

        // 「下方向にのみ追従する」オプションがONの場合
        if (onlyFollowDownwards)
        {
            if (targetY < lowestY)
            {
                lowestY = targetY;
            }
            targetY = lowestY;
        }

        // SmoothDampを使って現在のカメラY座標から目標Y座標へ滑らかに補間
        float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref currentVelocityY, smoothTime);

        newY = Mathf.Max(newY, minCameraY);

        // カメラのZ座標（通常は -10）を維持したまま位置を更新
        transform.position = new Vector3(fixedX, newY, transform.position.z);
    }
}