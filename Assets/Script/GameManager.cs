using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using TMPro; 
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("复活流程设置")]
    public float deathAnimDuration = 1.0f;
    public float respawnAnimDuration = 1.0f; 
    public float fadeDuration = 1.0f;      
    public float darkTime = 0.7f;          

    [Header("UI引用")]
    public Image blackScreen;      
    public GameObject respawnText; 
    public GameObject winScreen; 

    [Header("音效文件")]
    public AudioClip sfxJump;
    public AudioClip sfxFallStart;
    public AudioClip sfxLand;
    public AudioClip sfxDeath;
    public AudioClip sfxCheckpoint;
    public AudioClip sfxWin;
    public AudioClip sfxRespawn; 
    public AudioClip sfxButton; 

    private Vector3 currentRespawnPoint;
    private GameObject player;
    private Animator playerAnim;
    private Rigidbody2D playerRb;
    
    private AudioSource audioSource; 

    private bool isDead = false;
    public GameObject currentDeathHint; // 【新增】当前需要显示的死亡提示UI

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerAnim = player.GetComponent<Animator>();
            playerRb = player.GetComponent<Rigidbody2D>();
            currentRespawnPoint = player.transform.position;
        }

        if (blackScreen != null) 
        {
            Color c = blackScreen.color;
            c.a = 0;
            blackScreen.color = c;
        }
        if (respawnText != null) respawnText.SetActive(false);
        if (winScreen != null) winScreen.SetActive(false);
    }

    // 【新增】设置死亡提示 (由触发器调用)
    public void SetDeathHint(GameObject uiObj)
    {
        currentDeathHint = uiObj;
    }

    // 【新增】清除死亡提示
    public void ClearDeathHint()
    {
        currentDeathHint = null;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayButtonSound()
    {
        if (sfxButton != null)
        {
            PlaySFX(sfxButton);
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
        if (winScreen != null && winScreen.activeSelf) return;

        PlaySFX(sfxWin);
        StartCoroutine(WinSequence());
    }

    // 【新增】触发光照Debuff
    public void TriggerLightDebuff(float duration, float shrinkRatio)
    {
        StartCoroutine(LightDebuffRoutine(duration, shrinkRatio));
    }

    IEnumerator LightDebuffRoutine(float duration, float shrinkRatio)
    {
        if (player == null) yield break;

        Light2D playerLight = player.GetComponentInChildren<Light2D>();
        
        if (playerLight != null)
        {
            float originalRadius = playerLight.pointLightOuterRadius; 
            float targetRadius = originalRadius * shrinkRatio;      

            playerLight.pointLightOuterRadius = targetRadius;

            yield return new WaitForSeconds(duration);

            playerLight.pointLightOuterRadius = originalRadius;
        }
    }

    IEnumerator WinSequence()
    {
        DisablePlayerControl();
        yield return new WaitForSeconds(1.0f);
        if (winScreen != null) winScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnNextLevelClicked()
    {
        Debug.Log("下一关还没做呢！");
    }

    public void OnRestartClicked()
    {
        StartCoroutine(DelayLoad(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
    }

    public void OnMainMenuClicked()
    {
        StartCoroutine(DelayLoad("MainMenu"));
    }

    IEnumerator DelayLoad(string sceneName)
    {
        Time.timeScale = 1f; 
        yield return new WaitForSeconds(0.3f); 
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    IEnumerator DeathSequence()
    {
        isDead = true;
        PlaySFX(sfxDeath);

        DisablePlayerControl();
        ResetAnimParameters();

        if (playerAnim != null) playerAnim.Play("Death", 0, 0f);

        yield return new WaitForSeconds(deathAnimDuration);

        yield return StartCoroutine(FadeScreen(0, 1, fadeDuration));

        if (player != null)
            player.transform.position = currentRespawnPoint + Vector3.up * 0.5f; 
        
        yield return new WaitForSeconds(darkTime);

        yield return StartCoroutine(FadeScreen(1, 0, fadeDuration));

        if (respawnText != null) respawnText.SetActive(true);
        
        // 【新增】如果有特殊提示，显示它
        if (currentDeathHint != null) currentDeathHint.SetActive(true); 

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        if (respawnText != null) respawnText.SetActive(false);
        // 【新增】关闭提示
        if (currentDeathHint != null) currentDeathHint.SetActive(false); 
        
        PlaySFX(sfxRespawn);

        if (playerAnim != null)
        {
            playerAnim.Play("Respawn", 0, 0f); 
        }
        
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
        if (player != null)
        {
            Running r = player.GetComponent<Running>();
            if (r != null) r.enabled = false;
            
            Jumping j = player.GetComponent<Jumping>();
            if (j != null) j.enabled = false;
        }

        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.simulated = false; 
        }
    }

    void EnablePlayerControl()
    {
        if (playerRb != null) playerRb.simulated = true;
        
        if (player != null)
        {
            Running r = player.GetComponent<Running>();
            if (r != null) r.enabled = true;

            Jumping j = player.GetComponent<Jumping>();
            if (j != null) j.enabled = true;
        }
        
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
