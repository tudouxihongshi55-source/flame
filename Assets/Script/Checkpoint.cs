using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("蜡烛设置")]
    [Tooltip("点亮后的图片")]
    public Sprite litSprite; 
    [Tooltip("点亮时的光效物体(可选)")]
    public GameObject lightObj; 
    
    [Header("复活位置微调")]
    [Tooltip("请在蜡烛下创建一个空物体作为复活点，并拖到这里")]
    public Transform spawnPointTransform; 
    
    private bool isLit = false;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if(lightObj != null) lightObj.SetActive(false); 
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isLit)
        {
            ActivateCheckpoint();
        }
    }

    void ActivateCheckpoint()
    {
        isLit = true;
        
        if (litSprite != null) sr.sprite = litSprite;
        if (lightObj != null) lightObj.SetActive(true);

        // 优先使用 spawnPointTransform 的位置，如果没有赋值，就用蜡烛自己的位置
        Vector3 targetPos = transform.position;
        if (spawnPointTransform != null)
        {
            targetPos = spawnPointTransform.position;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateRespawnPoint(targetPos);
        }
        
        Debug.Log("存档点已激活！");
    }
}
