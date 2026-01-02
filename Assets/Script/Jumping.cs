using UnityEngine;
using System.Collections;

public class Jumping : MonoBehaviour
{
    [Header("跳跃物理参数")]
    [Tooltip("最大上升速度 (此参数决定了'上升有多慢')")]
    public float maxRiseSpeed = 5f;
    [Tooltip("最大跳跃高度")]
    public float maxJumpHeight = 9f;
    [Tooltip("下落时的重力倍率 (较大值 = 气球泄气/快速坠落)")]
    public float fallGravityScale = 25f;

    [Header("动画控制")]
    [Tooltip("气球充气动画(Pre-Jump)持续时间 (秒)")]
    public float preJumpDuration = 1.0f;
    
    // 动画参数名
    private string paramPreparing = "isPreparing"; // Pre-jump
    private string paramRising = "isRising";       // Jumping
    private string paramFalling = "isFalling";     // Down
    // 强制状态名 (Animator中必须有这个状态，名字必须一致)
    private string stateDown = "Down";

    [Header("空中移动参数")]
    [Tooltip("空中加速度 (控制在空中的移动灵敏度)")]
    public float airAcceleration = 150f;
    [Tooltip("空中控制力 (0-1之间，1为完全控制，0为无法改变方向)")]
    [Range(0f, 1f)]
    public float airControl = 0.037f;
    [Tooltip("空中刹车 (阻力，值越大空中停得越快)")]
    public float airDrag = 1f;
    [Tooltip("最大空中水平速度 (防止在空中无限加速)")]
    public float maxAirSpeed = 8f;

    [Header("可变高度跳跃")]
    public bool enableVariableHeight = true;
    [Tooltip("跳跃截断系数 (0-1)，值越大松手后上升截断越明显")]
    [Range(0f, 1f)]
    public float jumpCutoff = 0.684f;

    // 内部计算的上升重力
    private float calculatedRiseGravityScale;

    private Rigidbody2D rb;
    private Animator anim;
    private float moveInput;
    private bool isPreparingJump = false;
    private bool wasFalling = false; // 用于音效去重
    private bool wasInAir = false;   // 用于落地音效

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        RecalculatePhysics();
    }

    void OnValidate()
    {
        RecalculatePhysics();
    }

    void RecalculatePhysics()
    {
        if (maxJumpHeight > 0.1f && maxRiseSpeed > 0.1f)
        {
            float requiredGravity = (maxRiseSpeed * maxRiseSpeed) / (2f * maxJumpHeight);
            float baseGravity = Mathf.Abs(Physics2D.gravity.y);
            if (baseGravity > 0.001f)
            {
                calculatedRiseGravityScale = requiredGravity / baseGravity;
            }
            else
            {
                calculatedRiseGravityScale = 1f;
            }
        }
    }

    void Update()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput -= 1f;
        if (Input.GetKey(KeyCode.D)) moveInput += 1f;

        bool isGrounded = IsGrounded();

        // 跳跃触发
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isPreparingJump)
        {
            StartCoroutine(PrepareJumpRoutine());
        }

        // Variable Height (松手截断)
        if (enableVariableHeight && Input.GetKeyUp(KeyCode.Space))
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * (1f - jumpCutoff));
            }
        }

        // --- 更新 Animator 参数 & 音效检测 ---
        float vy = rb.velocity.y;
        
        bool isFalling = vy < -0.1f && !isPreparingJump;
        bool isRising = vy > 0.1f && !isPreparingJump;

        if (anim != null)
        {
            anim.SetBool(paramFalling, isFalling);
            anim.SetBool(paramRising, isRising);
        }

        // 1. 开始下落音效
        if (isFalling && !wasFalling)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PlaySFX(GameManager.Instance.sfxFallStart);
        }
        wasFalling = isFalling;

        // 2. 落地音效
        if (!isGrounded)
        {
            wasInAir = true;
        }
        else
        {
            if (wasInAir) // 刚才在空中，现在落地了
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.PlaySFX(GameManager.Instance.sfxLand);
                wasInAir = false;
            }
        }
    }

    bool IsGrounded()
    {
        return Mathf.Abs(rb.velocity.y) < 0.05f;
    }

    IEnumerator PrepareJumpRoutine()
    {
        isPreparingJump = true;
        if (anim != null) anim.SetBool(paramPreparing, true);

        // 等待前摇
        float elapsed = 0f;
        while (elapsed < preJumpDuration)
        {
            elapsed += Time.deltaTime;
            
            // 掉落保护
            if (!IsGrounded())
            {
                isPreparingJump = false;
                if (anim != null) 
                {
                    anim.SetBool(paramPreparing, false);
                    anim.SetBool(paramFalling, true);
                    anim.Play(stateDown);
                }
                yield break;
            }
            yield return null;
        }

        bool isHoldingSpace = Input.GetKey(KeyCode.Space);
        Jump(isHoldingSpace); 

        if (anim != null) anim.SetBool(paramPreparing, false);
        
        yield return new WaitForSeconds(0.1f);
        isPreparingJump = false;
    }

    void FixedUpdate()
    {
        float velX = rb.velocity.x;
        float velY = rb.velocity.y;

        // 垂直运动处理
        if (velY > 0) 
        {
            rb.gravityScale = calculatedRiseGravityScale;
            if (velY > maxRiseSpeed)
            {
                velY = maxRiseSpeed;
            }
        }
        else 
        {
            rb.gravityScale = fallGravityScale;
        }

        // 空中水平移动控制
        if (Mathf.Abs(velY) > 0.01f) 
        {
            if (moveInput != 0)
            {
                bool movingSameDir = Mathf.Sign(moveInput) == Mathf.Sign(velX);
                bool overSpeed = Mathf.Abs(velX) > maxAirSpeed;

                if (!overSpeed || !movingSameDir)
                {
                    float targetVelX = moveInput * maxAirSpeed;
                    float nextSpeedX = Mathf.MoveTowards(velX, targetVelX, airAcceleration * airControl * Time.fixedDeltaTime);
                    velX = nextSpeedX;
                }
            }
            else
            {
                velX = Mathf.MoveTowards(velX, 0, airDrag * Time.fixedDeltaTime);
            }
        }

        rb.velocity = new Vector2(velX, velY);
    }

    void Jump(bool fullJump)
    {
        if (fullJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxRiseSpeed);
        }
        else
        {
            float reducedSpeed = maxRiseSpeed * (1f - jumpCutoff);
            rb.velocity = new Vector2(rb.velocity.x, reducedSpeed);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.PlaySFX(GameManager.Instance.sfxJump);
    }

    // 修改：增加跳跃高度和速度 (叠加模式)
    public void AddJumpAbility(float heightAdd, float speedAdd)
    {
        maxJumpHeight += heightAdd;
        maxRiseSpeed += speedAdd;
        RecalculatePhysics();
        Debug.Log($"跳跃升级！新高度: {maxJumpHeight}, 新速度: {maxRiseSpeed}");
    }
}
