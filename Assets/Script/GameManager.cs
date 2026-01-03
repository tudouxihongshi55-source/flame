using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using TMPro; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("复活流程设置")]
    public float deathAnimDuration = 1.0f; // 死亡动画时长
    public float respawnAnimDuration = 1.0f; // 【新增】复活动画时长
    public float fadeDuration = 1.0f;      // 黑屏渐变时长
    public float darkTime = 0.7f;          // 黑屏保持时长

    [Header("UI引用")]
    public Image blackScreen;      // 黑屏遮罩
    public GameObject respawnText; // 复活提示文字/图片

    [Header("音效文件")]
    public AudioClip sfxJump;
    public AudioClip sfxFallStart;
    public AudioClip sfxLand;
    public AudioClip sfxDeath;
    public AudioClip sfxCheckpoint;
    public AudioClip sfxWin;
    public AudioClip sfxRespawn; 

    private Vector3 currentRespawnPoint;
    private GameObject player;
    private Animator playerAnim;
    private Rigidbody2D playerRb;
    private AudioSource playerAudio;
    private bool isDead = false;

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

    // --- 死亡流程 ---
    IEnumerator DeathSequence()
    {
        isDead = true;
        PlaySFX(sfxDeath);

        DisablePlayerControl();
        ResetAnimParameters();

        // 播放死亡动画
        if (playerAnim != null) playerAnim.Play("Death", 0, 0f);

        // 等待死亡动画
        yield return new WaitForSeconds(deathAnimDuration);

        // 屏幕变黑
        yield return StartCoroutine(FadeScreen(0, 1, fadeDuration));

        // 瞬移到复活点
        player.transform.position = currentRespawnPoint;
        
        // 保持黑屏
        yield return new WaitForSeconds(darkTime);

        // 屏幕变亮
        yield return StartCoroutine(FadeScreen(1, 0, fadeDuration));

        // 显示提示
        if (respawnText != null) respawnText.SetActive(true);

        // 等待按键
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        // --- 开始复活 ---
        if (respawnText != null) respawnText.SetActive(false);
        PlaySFX(sfxRespawn);

        // 【修改点】播放复活动画并等待
        if (playerAnim != null)
        {
            // 确保你动画状态机里有个状态叫 "Respawn"
            playerAnim.Play("Respawn", 0, 0f); 
        }
        
        // 等待复活动画播完
        yield return new WaitForSeconds(respawnAnimDuration);
        
        EnablePlayerControl();
        isDead = false;
    }

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
        playerRb.simulated = false; 
    }

    void EnablePlayerControl()
    {
        playerRb.simulated = true;
        player.GetComponent<Running>().enabled = true;
        player.GetComponent<Jumping>().enabled = true;
        
        // 【修改点】这里不再强制播放 Idle，而是相信 Animator 的连线会从 Respawn 自动切回 Idle
        // 只要重置一下可能残留的 Trigger 即可
        if (playerAnim != null)
        {
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
