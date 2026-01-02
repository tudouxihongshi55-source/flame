using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("蜡烛设置")]
    [Tooltip("点亮后的图片")]
    public Sprite litSprite;
    [Tooltip("点亮时的光效物体(可选)")]
    public GameObject lightObj;

    private bool isLit = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (lightObj != null) lightObj.SetActive(false); // 默认熄灭
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 只有主角能点亮，且没点亮过才触发
        if (other.CompareTag("Player") && !isLit)
        {
            ActivateCheckpoint();
        }
    }

    void ActivateCheckpoint()
    {
        isLit = true;

        // 1. 视觉变化
        if (litSprite != null) sr.sprite = litSprite;
        if (lightObj != null) lightObj.SetActive(true);

        // 2. 通知管理器更新复活点
        GameManager.Instance.UpdateRespawnPoint(transform.position);

        Debug.Log("存档点已激活！");
    }
}