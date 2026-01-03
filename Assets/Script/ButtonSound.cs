using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    private Button btn;

    void Start()
    {
        btn = GetComponent<Button>();
        // 自动给按钮添加一个点击事件
        btn.onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        // 尝试找 GameManager 并播放
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayButtonSound();
        }
        else
        {
            // 如果是在主菜单，可能还没有 GameManager？
            // 这种情况下，你可能需要在主菜单也放一个临时的 SoundManager
            // 或者把 GameManager 做成 DontDestroyOnLoad 并从一开始就加载
            Debug.LogWarning("找不到 GameManager 实例来播放音效");
        }
    }
}