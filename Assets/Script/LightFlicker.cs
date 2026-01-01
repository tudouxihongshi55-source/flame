using UnityEngine;
using UnityEngine.Rendering.Universal; // 必须引用这个才能控制 Light 2D

public class LightFlicker : MonoBehaviour
{
    [Header("闪烁参数")]
    [Tooltip("基础亮度")]
    public float baseIntensity = 1.0f;
    [Tooltip("闪烁幅度 (亮度波动范围)")]
    public float flickerAmplitude = 0.2f;
    [Tooltip("闪烁速度 (越快越像风吹)")]
    public float flickerSpeed = 5.0f;

    [Header("微动参数 (可选)")]
    [Tooltip("光圈是否也要微弱缩放")]
    public bool enablePulse = true;
    public float pulseSpeed = 2.0f;
    public float pulseAmount = 0.05f;

    private Light2D light2D;
    private float randomOffset;
    private Vector3 initialScale;

    void Start()
    {
        light2D = GetComponent<Light2D>();
        randomOffset = Random.Range(0f, 100f);
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (light2D == null) return;

        // 1. 亮度闪烁 (使用 Perlin Noise 制造自然的波动)
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);
        // noise 是 0~1，我们要把它映射到 baseIntensity +/- amplitude
        light2D.intensity = baseIntensity + (noise - 0.5f) * 2 * flickerAmplitude;

        // 2. 大小微动 (呼吸感)
        if (enablePulse)
        {
            float scaleNoise = Mathf.PerlinNoise(Time.time * pulseSpeed, randomOffset + 50);
            float scaleFactor = 1.0f + (scaleNoise - 0.5f) * 2 * pulseAmount;
            transform.localScale = initialScale * scaleFactor;
        }
    }
}