using UnityEngine;

public class FallDamage : MonoBehaviour
{
    [Header("摔落设置")]
    [Tooltip("致死下落高度 (米)")]
    public float lethalFallHeight = 15f;

    [Header("调试信息 (只读)")]
    public float currentHighestY; // 当前记录的最高点
    public bool isAirborne;       // 是否在空中
    public float lastGroundedY;   // 上一次在地面时的Y坐标

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastGroundedY = transform.position.y;
    }

    void Update()
    {
        // 1. 判定是否在地面
        // 这里的 0.1f 是容差，比 0.05 更宽松一点，防止浮点数误差
        bool isGrounded = Mathf.Abs(rb.velocity.y) < 0.1f;

        if (!isGrounded)
        {
            // --- 在空中 ---
            if (!isAirborne)
            {
                // 刚起飞/刚掉落瞬间：初始化状态
                isAirborne = true;
                currentHighestY = transform.position.y;
            }

            // 持续更新最高点：如果你往上飞，最高点就跟着涨
            if (transform.position.y > currentHighestY)
            {
                currentHighestY = transform.position.y;
            }
        }
        else
        {
            // --- 在地面 ---
            if (isAirborne)
            {
                // 刚落地瞬间：结算伤害
                CalculateFallDamage();
                isAirborne = false;

                // 归位，防止重复计算
                currentHighestY = transform.position.y;
            }
            else
            {
                // 一直在地面走：同步最高点为当前高度
                // 这样当你直接走出悬崖时，起点就是悬崖边缘的高度
                currentHighestY = transform.position.y;
            }
        }
    }

    void CalculateFallDamage()
    {
        // 落差 = 最高点 - 当前落地点的Y
        float fallDistance = currentHighestY - transform.position.y;

        Debug.Log($"落地结算：最高点 {currentHighestY:F2}, 落地 {transform.position.y:F2}, 落差 {fallDistance:F2}");

        if (fallDistance >= lethalFallHeight)
        {
            Debug.Log(">>> 触发摔死逻辑！ <<<");

            // 双重保险：防止 GameManager 为空
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerDeath();
            }
            else
            {
                Debug.LogError("找不到 GameManager 实例！请检查场景里有没有 GameManager 物体。");
            }
        }
    }
}