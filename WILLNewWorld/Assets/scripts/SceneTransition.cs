using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("触发场景切换的按钮")]
    public Button transitionButton;
    
    [Tooltip("用于渐变效果的Canvas Group（通常挂载在Canvas上）")]
    public CanvasGroup canvasGroup;
    
    [Tooltip("要跳转到的场景名称")]
    public string targetSceneName;
    
    [Header("Animation Settings")]
    [Tooltip("淡出持续时间（秒）")]
    [Range(0.1f, 5f)]
    public float fadeOutDuration = 1f;
    
    [Tooltip("淡入持续时间（秒）")]
    [Range(0.1f, 5f)]
    public float fadeInDuration = 1f;
    
    [Tooltip("淡出和加载场景之间的延迟（秒）")]
    [Range(0f, 2f)]
    public float loadDelay = 0.1f;
    
    private bool isTransitioning = false;  // 防止重复触发

    void Start()
    {
        // 如果未手动指定按钮，尝试从当前GameObject获取
        if (transitionButton == null)
        {
            transitionButton = GetComponent<Button>();
        }
        
        // 如果未手动指定CanvasGroup，尝试从父对象或当前对象获取
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInParent<CanvasGroup>();
            if (canvasGroup == null)
            {
                // 尝试从当前对象获取
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
        
        // 为按钮添加点击监听
        if (transitionButton != null)
        {
            transitionButton.onClick.AddListener(OnTransitionButtonClicked);
        }
        else
        {
            Debug.LogWarning("SceneTransition: 未找到按钮引用，请在Inspector中指定transitionButton");
        }
        
        // 初始化CanvasGroup（如果存在）
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;  // 确保初始状态为完全不透明
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            Debug.LogWarning("SceneTransition: 未找到CanvasGroup引用，渐变效果可能无法正常工作");
        }
    }

    /// <summary>
    /// 按钮点击事件处理
    /// </summary>
    private void OnTransitionButtonClicked()
    {
        if (isTransitioning)
        {
            return;  // 防止重复触发
        }
        
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("SceneTransition: 目标场景名称未设置！");
            return;
        }
        
        // 检查场景是否存在
        if (!SceneExists(targetSceneName))
        {
            Debug.LogError($"SceneTransition: 场景 '{targetSceneName}' 不存在！请确保场景已添加到Build Settings中。");
            return;
        }
        
        StartCoroutine(TransitionToScene());
    }

    /// <summary>
    /// 场景切换协程
    /// </summary>
    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;
        
        // 禁用按钮交互，防止重复点击
        if (transitionButton != null)
        {
            transitionButton.interactable = false;
        }
        
        // 开始异步加载场景（在淡出的同时进行）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;
        
        // 同时进行淡出和场景加载
        bool fadeOutComplete = false;
        
        // 启动淡出协程
        if (canvasGroup != null)
        {
            StartCoroutine(FadeOutWithCallback(() => fadeOutComplete = true));
        }
        else
        {
            fadeOutComplete = true;  // 如果没有CanvasGroup，直接标记为完成
        }
        
        // 等待场景加载到90%
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        // 等待淡出完成（如果还没完成）
        while (!fadeOutComplete)
        {
            yield return null;
        }
        
        // 短暂延迟
        yield return new WaitForSeconds(loadDelay);
        
        // 激活场景
        asyncLoad.allowSceneActivation = true;
        
        // 等待场景完全加载
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 等待一帧，确保场景对象已创建
        yield return null;
        
        // 查找新场景中的SceneTransition组件（最多尝试几次，确保找到）
        SceneTransition newSceneTransition = null;
        int attempts = 0;
        while (newSceneTransition == null && attempts < 10)
        {
            newSceneTransition = FindObjectOfType<SceneTransition>();
            if (newSceneTransition == null)
            {
                yield return null;
                attempts++;
            }
        }
        
        if (newSceneTransition != null && newSceneTransition.canvasGroup != null)
        {
            // 如果设置了淡入时间，执行淡入动画
            if (newSceneTransition.fadeInDuration > 0.01f)
            {
                // 确保从0开始淡入
                newSceneTransition.canvasGroup.alpha = 0f;
                newSceneTransition.canvasGroup.interactable = false;
                newSceneTransition.canvasGroup.blocksRaycasts = false;
                
                // 等待一帧，确保渲染系统已准备好
                yield return null;
                
                // 执行淡入动画
                yield return newSceneTransition.StartCoroutine(newSceneTransition.FadeIn());
            }
            else
            {
                // 如果没有淡入动画，直接显示
                newSceneTransition.canvasGroup.alpha = 1f;
                newSceneTransition.canvasGroup.interactable = true;
                newSceneTransition.canvasGroup.blocksRaycasts = true;
            }
        }
        
        // 等待几帧，确保所有 Start() 方法都已执行完毕
        yield return null;
        yield return null;
        yield return null;
        yield return null;  // 多等待一帧，确保初始化完成
        
        // 查找新场景中的FlashTitle组件并播放动画
        FlashTitle flashTitle = null;
        int flashTitleAttempts = 0;
        while (flashTitle == null && flashTitleAttempts < 10)
        {
            flashTitle = FindObjectOfType<FlashTitle>();
            if (flashTitle == null)
            {
                yield return null;
                flashTitleAttempts++;
            }
        }
        
        if (flashTitle != null)
        {
            // 先禁用 playOnStart，避免自动播放冲突
            flashTitle.DisablePlayOnStart();
            
            // 等待几帧，确保 Start() 已执行完并初始化完成
            yield return null;
            yield return null;
            
            // 先停止所有可能正在运行的动画（通过 ResetAnimation 内部会调用 StopAllCoroutines）
            // 重置动画状态
            flashTitle.ResetAnimation();
            
            // 等待一帧，确保重置完成
            yield return null;
            
            // 验证位置是否正确初始化
            if (flashTitle.RectTransform != null && flashTitle.CanvasGroup != null)
            {
                // 确保文字在起始位置且透明
                flashTitle.RectTransform.anchoredPosition = flashTitle.StartPosition;
                flashTitle.CanvasGroup.alpha = 0f;
            }
            
            // 再等待一帧，确保状态已设置
            yield return null;
            
            // 然后重新播放动画
            flashTitle.StartAnimation();
        }
        else
        {
            Debug.LogWarning("SceneTransition: 未找到 FlashTitle 组件！");
        }
        
        isTransitioning = false;
    }

    /// <summary>
    /// 淡出动画（带回调）
    /// </summary>
    private IEnumerator FadeOutWithCallback(System.Action onComplete = null)
    {
        if (canvasGroup == null)
        {
            onComplete?.Invoke();
            yield break;
        }
        
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        onComplete?.Invoke();
    }

    /// <summary>
    /// 淡出动画
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 淡入动画
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// 检查场景是否存在
    /// </summary>
    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneNameInBuild == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 公共方法：通过代码触发场景切换
    /// </summary>
    /// <param name="sceneName">目标场景名称</param>
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        
        targetSceneName = sceneName;
        OnTransitionButtonClicked();
    }

    void OnDestroy()
    {
        // 清理事件监听
        if (transitionButton != null)
        {
            transitionButton.onClick.RemoveListener(OnTransitionButtonClicked);
        }
    }
}


