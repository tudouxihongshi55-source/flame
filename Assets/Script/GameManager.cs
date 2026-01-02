using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 单例模式，方便别人调用

    [Header("复活设置")]
    public float respawnDelay = 1.5f; // 死亡后黑屏或等待的时间

    private Vector3 currentRespawnPoint;
    private GameObject player;
    private Animator playerAnim;
    private Rigidbody2D playerRb;

    // 状态锁，防止死两次
    private bool isDead = false;

    void Awake()
    {
        // 单例初始化
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
            // 初始复活点就是游戏开始的位置
            currentRespawnPoint = player.transform.position;
        }
    }

    // 供存档点调用
    public void UpdateRespawnPoint(Vector3 newPos)
    {
        currentRespawnPoint = newPos;
    }

    // 供玩家脚本调用
    public void TriggerDeath()
    {
        if (isDead) return;
        StartCoroutine(DeathProcess());
    }

    IEnumerator DeathProcess()
    {
        isDead = true;

        // 1. 禁用控制脚本
        player.GetComponent<Running>().enabled = false;
        player.GetComponent<Jumping>().enabled = false;
        playerRb.velocity = Vector2.zero;
        playerRb.simulated = false;

        // 2. 【关键新增】清理 Animator 参数，防止干扰
        if (playerAnim != null)
        {
            playerAnim.SetBool("isMoving", false);
            playerAnim.SetBool("isPreparing", false);
            playerAnim.SetBool("isRising", false);
            playerAnim.SetBool("isFalling", false);
            // 确保没有其他状态干扰
        }

        // 3. 强制播放死亡动画
        if (playerAnim != null)
        {
            // Play(状态名, 层级, 归一化时间)
            // 0f 表示从头开始播
            playerAnim.Play("Death", 0, 0f);
            Debug.Log("已强制播放 Death 动画");
        }

        // 4. 等待
        yield return new WaitForSeconds(respawnDelay);

        // 5. 复活
        Respawn();
    }

    void Respawn()
    {
        // 重置位置
        player.transform.position = currentRespawnPoint;

        // 重置状态
        if (playerAnim != null)
        {
            playerAnim.Play("Idle"); // 强行切回Idle
            playerAnim.ResetTrigger("Die");
        }

        // 恢复控制
        playerRb.simulated = true;
        player.GetComponent<Running>().enabled = true;
        player.GetComponent<Jumping>().enabled = true;

        isDead = false;
    }
}