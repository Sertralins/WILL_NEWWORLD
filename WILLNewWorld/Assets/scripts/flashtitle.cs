using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 文字标题动画组件：场景切换时从左到右滑出，停留后消失
/// </summary>
public class FlashTitle : MonoBehaviour
{
    [Header("动画设置")]
    [Tooltip("是否在 Start 时自动播放动画")]
    public bool playOnStart = true;
    
    [Tooltip("滑出动画持续时间（秒）")]
    [Range(0.1f, 5f)]
    public float slideOutDuration = 1f;
    
    [Tooltip("文字停留时间（秒），滑出后停留多久再消失")]
    [Range(0f, 10f)]
    public float stayDuration = 3f;
    
    [Tooltip("淡出消失持续时间（秒）")]
    [Range(0.1f, 3f)]
    public float fadeOutDuration = 0.5f;
    
    [Tooltip("滑出距离（相对于屏幕宽度的倍数，负数表示从左侧滑出）")]
    [Range(-2f, 2f)]
    public float slideDistance = -1.5f;
    
    [Header("位置设置")]
    [Tooltip("起始位置（文字滑出的目标位置）")]
    public Vector2 startPosition;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Coroutine animationCoroutine;
    private bool isPlayOnStartEnabled = true;

    // 公共属性
    public RectTransform RectTransform => rectTransform;
    public CanvasGroup CanvasGroup => canvasGroup;
    public Vector2 StartPosition => startPosition;

    void Awake()
    {
        // 获取 RectTransform
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("FlashTitle: 需要 RectTransform 组件！");
            return;
        }

        // 尝试获取或添加 CanvasGroup 用于淡出效果
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        // 如果没有手动设置起始位置，使用当前位置
        if (startPosition == Vector2.zero && rectTransform != null)
        {
            startPosition = rectTransform.anchoredPosition;
        }
        
        // 初始化：文字在左侧屏幕外，不透明
        InitializePosition();
        
        // 如果启用了 playOnStart，自动播放动画
        if (playOnStart && isPlayOnStartEnabled)
        {
            // 延迟一帧，确保所有初始化完成
            StartCoroutine(DelayedStartAnimation());
        }
    }

    /// <summary>
    /// 延迟启动动画（确保初始化完成）
    /// </summary>
    private IEnumerator DelayedStartAnimation()
    {
        yield return null;
        StartAnimation();
    }

    /// <summary>
    /// 初始化位置：将文字放在屏幕左侧外
    /// </summary>
    private void InitializePosition()
    {
        if (rectTransform == null)
        {
            return;
        }

        // 计算起始位置（屏幕左侧外）
        Vector2 leftStartPos;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // 对于 ScreenSpaceOverlay，使用屏幕宽度
            float screenWidth = Screen.width;
            leftStartPos = new Vector2(
                startPosition.x + slideDistance * screenWidth,
                startPosition.y
            );
        }
        else
        {
            // 对于其他模式，使用相对位置
            leftStartPos = new Vector2(
                startPosition.x + slideDistance * 1000f,
                startPosition.y
            );
        }

        rectTransform.anchoredPosition = leftStartPos;
        
        // 确保初始状态：不透明
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 禁用 playOnStart（由 SceneTransition 调用）
    /// </summary>
    public void DisablePlayOnStart()
    {
        isPlayOnStartEnabled = false;
    }

    /// <summary>
    /// 重置动画状态
    /// </summary>
    public void ResetAnimation()
    {
        // 停止所有协程
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        StopAllCoroutines();
        
        // 重置位置和透明度
        InitializePosition();
    }

    /// <summary>
    /// 启动动画：从左到右滑出，停留后消失
    /// </summary>
    public void StartAnimation()
    {
        if (rectTransform == null || canvasGroup == null)
        {
            Debug.LogWarning("FlashTitle: 缺少必要的组件！");
            return;
        }

        // 如果正在播放动画，先停止
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        // 启动动画协程
        animationCoroutine = StartCoroutine(SlideOutAndFade());
    }

    /// <summary>
    /// 文字从左到右滑出，然后淡出消失的动画协程
    /// </summary>
    private IEnumerator SlideOutAndFade()
    {
        if (rectTransform == null || canvasGroup == null)
        {
            yield break;
        }

        // 计算起始位置（屏幕左侧外）
        Vector2 leftStartPos;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            float screenWidth = Screen.width;
            leftStartPos = new Vector2(
                startPosition.x + slideDistance * screenWidth,
                startPosition.y
            );
        }
        else
        {
            leftStartPos = new Vector2(
                startPosition.x + slideDistance * 1000f,
                startPosition.y
            );
        }

        // 设置起始位置和透明度
        rectTransform.anchoredPosition = leftStartPos;
        canvasGroup.alpha = 1f;

        // 等待一帧，确保位置已设置
        yield return null;

        // 第一步：从左到右滑出
        float elapsed = 0f;
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideOutDuration);

            // 使用缓动函数使动画更平滑
            t = EaseOutCubic(t);

            // 从左侧位置滑到目标位置
            rectTransform.anchoredPosition = Vector2.Lerp(leftStartPos, startPosition, t);

            yield return null;
        }

        // 确保到达目标位置
        rectTransform.anchoredPosition = startPosition;

        // 第二步：停留一段时间
        yield return new WaitForSeconds(stayDuration);

        // 第三步：淡出消失
        elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        // 确保完全透明
        canvasGroup.alpha = 0f;

        animationCoroutine = null;
    }

    /// <summary>
    /// 缓动函数：三次缓出
    /// </summary>
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    /// <summary>
    /// 在 Inspector 中验证设置
    /// </summary>
    void OnValidate()
    {
        // 如果 RectTransform 已存在，更新起始位置
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (rectTransform != null && startPosition == Vector2.zero)
        {
            startPosition = rectTransform.anchoredPosition;
        }

        // 确保 CanvasGroup 存在
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}

