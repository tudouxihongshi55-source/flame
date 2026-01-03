using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject pauseMenuUI; 

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if(pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        if(pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; 
        isPaused = true;
    }

    public void OnRestartClicked()
    {
        StartCoroutine(DelayLoad(SceneManager.GetActiveScene().name));
    }

    public void OnMainMenuClicked()
    {
        StartCoroutine(DelayLoad("MainMenu")); // 确保这里是你主菜单场景的名字
    }

    IEnumerator DelayLoad(string sceneName)
    {
        // 恢复时间流速，否则 WaitForSecondsRealtime 没问题，但加载后的场景可能还是暂停的
        Time.timeScale = 1f; 

        // 使用 Realtime，因为刚才游戏是暂停的，如果用普通的 WaitForSeconds 可能会出问题
        yield return new WaitForSecondsRealtime(0.3f);

        // 如果是重新开始，顺便重置一下 GameManager 的状态
        if (GameManager.Instance != null && sceneName == SceneManager.GetActiveScene().name)
        {
            // 这里可以加重置逻辑，目前先不动
        }

        SceneManager.LoadScene(sceneName);
    }
}
