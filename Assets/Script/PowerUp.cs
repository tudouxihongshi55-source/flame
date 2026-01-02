using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("强化属性 (叠加)")]
    [Tooltip("增加的高度值 (比如填3，就是原高度+3)")]
    public float heightAmount = 3f; 
    [Tooltip("增加的速度值 (比如填1，就是原速度+1)")]
    public float speedAmount = 1f; 
    
    [Header("视觉效果")]
    [Tooltip("拾取时的音效(可选)")]
    public AudioClip pickupSFX;
    [Tooltip("拾取时的特效(可选)")]
    public GameObject pickupVFX;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Jumping jumpScript = other.GetComponent<Jumping>();
            if (jumpScript != null)
            {
                // 调用叠加方法
                jumpScript.AddJumpAbility(heightAmount, speedAmount);
                
                if (GameManager.Instance != null && pickupSFX != null)
                {
                    GameManager.Instance.PlaySFX(pickupSFX);
                }
                
                if (pickupVFX != null)
                {
                    Instantiate(pickupVFX, transform.position, Quaternion.identity);
                }
                
                Destroy(gameObject);
            }
        }
    }
}
