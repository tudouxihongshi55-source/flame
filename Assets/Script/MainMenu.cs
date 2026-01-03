using UnityEngine;
using UnityEngine.SceneManagement; // 必须引用：场景管理
using UnityEngine.UI; // 必须引用：UI

public class MainMenu : MonoBehaviour
{
    [Header("关于页面")]
    public GameObject aboutPanel;

    [Header("按钮引用(可选，用于代码控制)")]
    public Button btnContinue;

    void Start()
    {
        // 检查是否有存档 (简单用PlayerPrefs判断)
        // 如果没有玩过，"继续游戏"按钮应该是灰的或隐藏的
        if (!PlayerPrefs.HasKey("HasSavedGame"))
        {
            if (btnContinue != null) btnContinue.interactable = false;
        }
    }

    // --- 按钮点击事件 ---

    public void OnNewGameClicked()
    {
        // 这里需要清空旧存档(如果有的话)
        PlayerPrefs.DeleteKey("SavedScene");
        PlayerPrefs.DeleteKey("RespawnX");
        // ... 其他存档Key

        // 加载第一关 (假设你的关卡场景名叫 "Level1")
        SceneManager.LoadScene("Level1");
    }

    public void OnContinueClicked()
    {
        // 读取存档中记录的关卡
        // 假设我们存了一个 string 叫 "SavedScene"
        string sceneName = PlayerPrefs.GetString("SavedScene", "Level1");
        SceneManager.LoadScene(sceneName);
    }

    public void OnAboutClicked()
    {
        aboutPanel.SetActive(true);
    }

    public void OnCloseAboutClicked()
    {
        aboutPanel.SetActive(false);
    }

    public void OnQuitClicked()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }
}