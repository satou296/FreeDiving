using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem; // ★New Input Systemの名前空間

public class InteractableObject : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject interactionPromptUI; // 「Enterで調べる」などのUI表示（任意）

    [Header("実行したい処理")]
    [SerializeField] private UnityEvent onInteract; // Enterを押したときに実行するイベント

    private bool isPlayerNearby = false;

    private void Start()
    {
        // 最初はプロンプトUI（矢印やテキストなど）を非表示にしておく
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    private void Update()
    {
        // New Input System方式でEnterキー（Return / テンキーEnter）を判定
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool enterPressed = keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;

        // プレイヤーが近くにいて、かつEnterキー（Returnキー）が押された場合
        if (isPlayerNearby && enterPressed)
        {
            TriggerInteraction();
        }
    }

    // 新しいInput Systemのメッセージ（Player InputのBehaviorがSend Messagesの場合）に対応させる場合
    // プレイヤー側ではなく、このオブジェクト単体で完結させたい場合は上記のUpdateで十分動作します。

    private void TriggerInteraction()
    {
        Debug.Log("オブジェクトとインタラクトしました！");
        // インスペクターで設定した処理（画面遷移など）を実行
        onInteract.Invoke();
    }

    // プレイヤーが範囲内に入ったとき
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;

            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(true); // 吹き出しなどを表示
            }
        }
    }

    // プレイヤーが範囲内から出たとき
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false); // 吹き出しを非表示
            }
        }
    }
}