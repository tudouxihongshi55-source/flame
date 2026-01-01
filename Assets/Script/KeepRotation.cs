using UnityEngine;

public class KeepRotation : MonoBehaviour
{
    private Vector3 initialScale;
    private Quaternion initialRotation;

    void Start()
    {
        // 记录初始大小和旋转
        initialScale = transform.lossyScale; // lossyScale 是全局缩放
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // 强制每一帧都重置为初始的世界旋转和缩放
        // 这样父物体怎么翻，我都岿然不动
        transform.rotation = initialRotation;

        // 处理缩放：如果父物体翻转了(x为负)，lossyScale.x 也会变负
        // 我们需要把它纠正回来
        Vector3 parentScale = transform.parent.lossyScale;

        // 计算需要的局部缩放，以抵消父物体的翻转
        // 如果父是-1，我就设为-1，负负得正，或者直接用绝对值逻辑
        // 最简单的方法：
        Vector3 newLocalScale = transform.localScale;
        newLocalScale.x = Mathf.Abs(newLocalScale.x) * (parentScale.x > 0 ? 1 : -1);
        // 哎等等，上面这个逻辑是跟随翻转。
        // 你想要的是“不翻转”，所以应该是：

        if (transform.parent.localScale.x < 0)
            newLocalScale.x = -Mathf.Abs(initialScale.x); // 反向抵消
        else
            newLocalScale.x = Mathf.Abs(initialScale.x);

        // 其实有个更简单的傻瓜办法：
        // 直接在 Update 里把 transform.rotation = Quaternion.identity; (不旋转)
        // 至于翻转，如果不规则形状不对称，翻转确实难看。
    }
}