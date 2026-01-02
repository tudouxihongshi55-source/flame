using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlowPulse : MonoBehaviour
{
    [Header("Sprite 闪烁")]
    public float speed = 2f;
    public float minAlpha = 0.6f;
    public float maxAlpha = 1f;

    [Header("Light2D 呼吸")]
    public float minLightIntensity = 0.2f;
    public float maxLightIntensity = 0.6f;

    private SpriteRenderer sr;
    private Light2D light2D;
    private float offset;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        light2D = GetComponentInChildren<Light2D>(); // 子物体上的 Light2D
        offset = Random.Range(0f, 10f);
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed + offset) + 1f) * 0.5f;

        // Sprite 亮度呼吸
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, alpha);

        // Light2D 强度呼吸
        if (light2D != null)
        {
            float intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, t);
            light2D.intensity = intensity;
        }
    }
}
