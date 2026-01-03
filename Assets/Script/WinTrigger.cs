using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerWin();
            }
        }
    }
}

