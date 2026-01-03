using UnityEngine;

public class WaterMonster : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 2f;
    public float moveDistance = 3f; // 向左右各移动多少距离

    [Header("Debuff 参数")]
    public float debuffDuration = 3f;     // 持续时间
    [Range(0.1f, 1f)]
    public float shrinkRatio = 0.5f;      // 光圈缩小倍率 (0.5表示变成一半大)
    
    [Header("效果")]
    public AudioClip hitSound;            // 碰撞音效

    private Vector3 startPos;
    private bool movingRight = true;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // --- 左右巡逻逻辑 ---
        if (movingRight)
        {
            transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
            // 翻转图片朝右
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
            
            if (transform.position.x >= startPos.x + moveDistance)
                movingRight = false;
        }
        else
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
            // 翻转图片朝左
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);

            if (transform.position.x <= startPos.x - moveDistance)
                movingRight = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 播放声音 (通过 GameManager 播，因为自己马上要死了)
            if (GameManager.Instance != null)
            {
                if (hitSound != null) GameManager.Instance.PlaySFX(hitSound);
                
                // 2. 触发光圈缩小 (交给 GameManager 处理)
                GameManager.Instance.TriggerLightDebuff(debuffDuration, shrinkRatio);
            }

            // 3. 自我销毁
            Destroy(gameObject);
        }
    }
}
