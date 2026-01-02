using UnityEngine;
using UnityEngine.UI; // 引用UI
using System.Collections;
using TMPro; // 如果你用TextMeshPro，需要这个

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("复活流程设置")]
    public float deathAnimDuration = 1.0f; // 死亡动画大概多长
    public float fadeDuration = 1.0f;      // 黑屏渐变时间
    public float darkTime = 0.7f;          // 黑屏保持时间
    public float cameraMoveSpeed = 50f;    // 镜头飞向复活点的速度 (如果是手动控制)

    [Header("UI引用")]
    public Image blackScreen;      // 黑屏遮罩
    public GameObject respawnText; // 复活提示文字物体

    [Header("音效文件")]
    public AudioClip sfxJump;
    public AudioClip sfxFallStart;
    public AudioClip sfxLand;
    public AudioClip sfxDeath;
    public AudioClip sfxCheckpoint;
    public AudioClip sfxWin;
    public AudioClip sfxRespawn; // 复活音效

    private Vector3 currentRespawnPoint;
    private GameObject player;
    private Animator playerAnim;
    private Rigidbody2D playerRb;
    private AudioSource playerAudio;
    private bool isDead = false;

    // 摄像机跟随目标 (用于控制Cinemachine)
    // 假设 Cinemachine Follow 的是这个物体，或者是主角
    // 如果是 Cinemachine，我们需要操作 Virtual Camera
    // 为了通用，我们这里直接操作主角的位置，让摄像机跟着主角飞过去
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerAnim = player.GetComponent<Animator>();
            playerRb = player.GetComponent<Rigidbody2D>();
            playerAudio = player.GetComponent<AudioSource>();
            currentRespawnPoint = player.transform.position;
        }

        // 初始化UI
        if (blackScreen != null) 
        {
            Color c = blackScreen.color;
            c.a = 0;
            blackScreen.color = c;
        }
        if (respawnText != null) respawnText.SetActive(false);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && playerAudio != null)
        {
            playerAudio.PlayOneShot(clip);
        }
    }

    public void UpdateRespawnPoint(Vector3 newPos)
    {
        currentRespawnPoint = newPos;
        PlaySFX(sfxCheckpoint);
    }

    public void TriggerDeath()
    {
        if (isDead) return;
        StartCoroutine(DeathSequence());
    }

    public void TriggerWin()
    {
        PlaySFX(sfxWin);
    }

    // --- 新的死亡流程 ---
    IEnumerator DeathSequence()
    {
        isDead = true;
        PlaySFX(sfxDeath);

        // 1. 禁用控制，清理动画参数
        DisablePlayerControl();
        ResetAnimParameters();

        // 2. 播放死亡动画
        if (playerAnim != null) playerAnim.Play("Death", 0, 0f);

        // 3. 等待死亡动画播完
        yield return new WaitForSeconds(deathAnimDuration);

        // 4. 屏幕慢慢变黑
        yield return StartCoroutine(FadeScreen(0, 1, fadeDuration));

        // --- 此时全黑 ---

        // 5. 瞬间把主角移到复活点 (摄像机如果跟着主角，也会瞬间过去)
        // 或者是让摄像机慢慢飞过去？你说"视角迅速移动到复活点"
        // 如果想让玩家看到移动过程，我们得先让屏幕变亮，再移动
        // 但你说"屏幕变亮时视角要移动"，这意味着我们得先把主角挪过去，或者控制摄像机目标
        
        // 简单做法：把主角瞬移到复活点，然后让摄像机跟着
        player.transform.position = currentRespawnPoint;
        
        // 重置主角状态为"尸体" (为了不穿帮，先保持Death或者Idle)
        if (playerAnim != null) playerAnim.Play("Idle"); // 或者专门做一个躺尸的动画

        // 6. 保持黑屏一小会儿
        yield return new WaitForSeconds(darkTime);

        // 7. 屏幕慢慢变亮 (内容是灰色的 - 这里我们只做变亮，变灰需要PostProcessing)
        yield return StartCoroutine(FadeScreen(1, 0, fadeDuration));

        // 8. 显示"按空格复活"
        if (respawnText != null) respawnText.SetActive(true);

        // 9. 等待玩家按空格
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        // 10. 玩家按了空格 -> 开始复活
        if (respawnText != null) respawnText.SetActive(false);
        PlaySFX(sfxRespawn);

        // 播放复活动画 (如果有) -> 这里假设用 Idle 代替，或者你有专门的 Respawn 动画
        // 如果有专门的复活动画:
        // if (playerAnim != null) playerAnim.Play("Respawn");
        // yield return new WaitForSeconds(1.0f); // 等待复活动画
        
        // 恢复控制
        EnablePlayerControl();
        isDead = false;
    }

    // UI 渐变协程
    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (blackScreen == null) yield break;

        float elapsed = 0f;
        Color c = blackScreen.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            blackScreen.color = c;
            yield return null;
        }
        c.a = endAlpha;
        blackScreen.color = c;
    }

    void DisablePlayerControl()
    {
        player.GetComponent<Running>().enabled = false;
        player.GetComponent<Jumping>().enabled = false;
        playerRb.velocity = Vector2.zero;
        playerRb.simulated = false; // 禁用物理
    }

    void EnablePlayerControl()
    {
        playerRb.simulated = true;
        player.GetComponent<Running>().enabled = true;
        player.GetComponent<Jumping>().enabled = true;
        
        if (playerAnim != null)
        {
            playerAnim.Play("Idle");
            playerAnim.ResetTrigger("Die");
        }
    }

    void ResetAnimParameters()
    {
        if (playerAnim != null)
        {
            playerAnim.SetBool("isMoving", false);
            playerAnim.SetBool("isPreparing", false);
            playerAnim.SetBool("isRising", false);
            playerAnim.SetBool("isFalling", false);
        }
    }
}
