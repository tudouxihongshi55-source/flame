using UnityEngine;
using System.Collections;

public class DeathHintZone : MonoBehaviour
{
    [Header("UI设置")]
    [Tooltip("玩家如果在这里死掉，要显示的那个UI图片/文字")]
    public GameObject hintUI;

    [Header("延迟设置")]
    [Tooltip("离开区域后，提示还会保留多少秒？")]
    public float clearDelay = 3.0f;

    private Coroutine clearCoroutine;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 调试：看看是谁进来了
        Debug.Log($"[触发器] 物体进入: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("[触发器] 玩家进入！提示UI已准备就绪。");
            
            // 只要进入区域，立刻停止“清除倒计时”
            if (clearCoroutine != null) StopCoroutine(clearCoroutine);

            if (GameManager.Instance != null && hintUI != null)
            {
                GameManager.Instance.SetDeathHint(hintUI);
            }
            else
            {
                if (GameManager.Instance == null) Debug.LogError("[触发器] 找不到 GameManager!");
                if (hintUI == null) Debug.LogError("[触发器] Hint UI 没拖进去!");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[触发器] 玩家离开，将在 {clearDelay} 秒后清除提示...");
            // 离开区域时，不立刻清除，而是启动倒计时
            clearCoroutine = StartCoroutine(ClearHintDelayed());
        }
    }

    IEnumerator ClearHintDelayed()
    {
        yield return new WaitForSeconds(clearDelay);
        
        Debug.Log("[触发器] 时间到，提示已清除。如果现在死，不会显示提示。");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearDeathHint();
        }
    }
}
