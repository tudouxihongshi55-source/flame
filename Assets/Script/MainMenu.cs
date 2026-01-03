using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("关于页面")]
    public GameObject aboutPanel;

    [Header("按钮引用")]
    public Button btnContinue;

    void Start()
    {
        if (!PlayerPrefs.HasKey("HasSavedGame"))
        {
            if (btnContinue != null) btnContinue.interactable = false; 
        }
    }

    // --- 按钮点击事件 ---

    public void OnNewGameClicked()
    {
        // 播放音效 (如果按钮上没绑 ButtonSound 脚本，这里也可以手动播一下)
        // if (GameManager.Instance != null) GameManager.Instance.PlayButtonSound();

        // 开启协程，延迟加载
        StartCoroutine(LoadSceneDelay("Level1", true));
    }

    public void OnContinueClicked()
    {
        string sceneName = PlayerPrefs.GetString("SavedScene", "Level1");
        StartCoroutine(LoadSceneDelay(sceneName, false));
    }

    public void OnAboutClicked()
    {
        if (aboutPanel != null) aboutPanel.SetActive(true);
    }

    public void OnCloseAboutClicked()
    {
        if (aboutPanel != null) aboutPanel.SetActive(false);
    }

    public void OnQuitClicked()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }

    // --- 延迟加载的协程 ---
    IEnumerator LoadSceneDelay(string sceneName, bool isNewGame)
    {
        // 等待 0.3 秒，让点击音效播出来
        yield return new WaitForSeconds(0.3f);

        if (isNewGame)
        {
            PlayerPrefs.DeleteKey("SavedScene");
            PlayerPrefs.DeleteKey("RespawnX");
        }

        SceneManager.LoadScene(sceneName);
    }
}
