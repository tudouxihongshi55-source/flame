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

        // 跳跃触发
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && !isPreparingJump)
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

        // --- 更新 Animator 参数 ---
        if (anim != null)
        {
            float vy = rb.velocity.y;
            
            // 下落状态: 速度 < -0.1 (且没在准备跳跃)
            bool isFalling = vy < -0.1f && !isPreparingJump;
            anim.SetBool(paramFalling, isFalling);

            // 上升状态: 速度 > 0.1 (且没在准备跳跃)
            bool isRising = vy > 0.1f && !isPreparingJump;
            anim.SetBool(paramRising, isRising);
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
            
            // 掉落保护 (意外离开平台)
            if (!IsGrounded())
            {
                isPreparingJump = false;
                if (anim != null) 
                {
                    // 1. 强制重置参数
                    anim.SetBool(paramPreparing, false);
                    anim.SetBool(paramFalling, true);
                    
                    // 2. 强制播放 Down 动画 (最暴力也最有效的修正)
                    anim.Play(stateDown);
                }
                yield break; // 退出协程
            }
            yield return null;
        }

        // 执行起跳
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
    }
}
