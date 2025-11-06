using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LineAnimation : MonoBehaviour
{
    [SerializeField] private Button triggerButton;
    [SerializeField] private Image[] lineImages;
    [SerializeField] private float animationDuration = 2f;

    private void Start()
    {
        // 初始化所有图像：设置填充模式，从上到下填充，并初始隐藏
        foreach (Image img in lineImages)
        {
            if (img != null)
            {
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Vertical;
                img.fillOrigin = (int)Image.OriginVertical.Top;
                img.fillAmount = 0f;
            }
        }
        
        // 添加按钮点击事件
        triggerButton.onClick.AddListener(StartLineAnimation);
    }

    private void StartLineAnimation()
    {
        // 禁用按钮，确保只能点击一次
        triggerButton.interactable = false;
        StartCoroutine(AnimateLine());
    }

    private IEnumerator AnimateLine()
    {
        // 检查数组是否有效
        if (lineImages == null || lineImages.Length == 0)
        {
            yield break;
        }
        
        // 计算每个图像的动画时长
        float durationPerImage = animationDuration / lineImages.Length;
        
        // 依次显示每个图像
        for (int i = 0; i < lineImages.Length; i++)
        {
            if (lineImages[i] == null) continue;
            
            // 确保动画开始时图像完全隐藏
            lineImages[i].fillAmount = 0f;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < durationPerImage)
            {
                // 计算填充比例，从上到下逐渐显示
                float fillAmount = elapsedTime / durationPerImage;
                lineImages[i].fillAmount = fillAmount;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保最终完全填充
            lineImages[i].fillAmount = 1f;
        }
    }
}