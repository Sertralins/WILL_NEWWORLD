using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonFous : MonoBehaviour
{
    [Header("Button References")]
    public Button firstButton;           // 第一个按钮
    public GameObject secondButton;      // 第二个按钮（需要显示/隐藏的对象）
    
    [Header("ScrollView Reference")]
    public CustomScrollView scrollView;  // 自定义ScrollView组件引用
    
    [Header("Transparency Settings")]
    [Range(0f, 1f)]
    public float dimmedAlpha = 0.3f;     // 其他组件的透明度（0-1，值越小越透明）
    
    [Header("Selected Elements")]
    [Tooltip("当前选中的元素列表（只读）")]
    public List<GameObject> selectedElements = new List<GameObject>();  // 选中的元素列表
    
    private bool isSecondButtonVisible = false;  // 第二个按钮的显示状态
    private bool isDimmed = false;               // 当前是否处于降低透明度状态

    void Start()
    {
        // 如果未手动指定第一个按钮，尝试从当前GameObject获取
        if (firstButton == null)
        {
            firstButton = GetComponent<Button>();
        }
        
        // 为第一个按钮添加点击监听
        if (firstButton != null)
        {
            firstButton.onClick.AddListener(OnFirstButtonClicked);
        }
        
        // 初始化：确保第二个按钮初始状态为隐藏
        if (secondButton != null)
        {
            secondButton.SetActive(false);
            isSecondButtonVisible = false;
        }
    }

    /// <summary>
    /// 第一个按钮点击事件处理
    /// </summary>
    private void OnFirstButtonClicked()
    {
        // 切换第一个按钮的选中状态
        ToggleElementSelection(firstButton?.gameObject);
        
        // 切换第二个按钮的显示状态
        isSecondButtonVisible = !isSecondButtonVisible;
        
        if (secondButton != null)
        {
            secondButton.SetActive(isSecondButtonVisible);
        }
        
        // 切换其他组件的透明度
        ToggleOtherComponentsTransparency();
        
        // 将视角中心集中在第一个按钮的中心
        FocusOnFirstButton();
    }

    /// <summary>
    /// 将ScrollView的视角中心集中在第一个按钮的中心
    /// </summary>
    private void FocusOnFirstButton()
    {
        if (firstButton == null || scrollView == null) return;
        
        RectTransform firstButtonRect = firstButton.GetComponent<RectTransform>();
        if (firstButtonRect == null) return;
        
        // 获取ScrollView的content和viewport
        RectTransform content = scrollView.content;
        RectTransform viewport = scrollView.viewport;
        
        if (content == null || viewport == null) return;
        
        // 获取ScrollRect组件以检查滚动方向
        ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
        if (scrollRect == null) return;
        
        // 获取Canvas，用于坐标转换
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        
        // 方法：将按钮和viewport中心都转换到content的本地坐标系，然后计算偏移
        // 将按钮的中心点从屏幕坐标转换为content的本地坐标
        Vector2 buttonScreenPos = RectTransformUtility.WorldToScreenPoint(cam, firstButtonRect.position);
        Vector2 buttonLocalPosInContent;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content, 
            buttonScreenPos, 
            cam, 
            out buttonLocalPosInContent);
        
        // 将viewport的中心点从屏幕坐标转换为content的本地坐标
        Vector2 viewportCenterScreenPos = RectTransformUtility.WorldToScreenPoint(cam, viewport.position);
        Vector2 viewportCenterInContent;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content,
            viewportCenterScreenPos,
            cam,
            out viewportCenterInContent);
        
        // 计算按钮相对于viewport中心的偏移（在content的本地坐标系中）
        Vector2 offsetInContent = buttonLocalPosInContent - viewportCenterInContent;
        
        // 根据ScrollRect的滚动方向调整
        if (!scrollRect.horizontal) offsetInContent.x = 0;
        if (!scrollRect.vertical) offsetInContent.y = 0;
        
        // 检查按钮是否已经在中心位置（偏移量很小，不需要移动）
        float threshold = 5f; // 允许的误差范围（像素）
        if (Mathf.Abs(offsetInContent.x) < threshold && Mathf.Abs(offsetInContent.y) < threshold)
        {
            // 按钮已经在中心位置，不需要移动
            return;
        }
        
        // 在ScrollView中，要将按钮居中，需要反向移动content
        // 如果按钮在viewport中心的右边（offsetInContent.x > 0），需要向左移动content（减小x），这样按钮会向右移动并居中
        // 如果按钮在viewport中心的上边（offsetInContent.y > 0），需要向下移动content（减小y），这样按钮会向上移动并居中
        Vector2 newPosition = content.anchoredPosition - offsetInContent;
        
        // 限制在边界内（使用ScrollView的边界检查逻辑）
        newPosition = ClampPositionToBounds(newPosition, content, viewport);
        
        // 平滑移动到新位置
        StartCoroutine(SmoothMoveToPosition(content, newPosition));
    }

    /// <summary>
    /// 平滑移动content到指定位置
    /// </summary>
    private System.Collections.IEnumerator SmoothMoveToPosition(RectTransform target, Vector2 targetPosition)
    {
        Vector2 startPosition = target.anchoredPosition;
        float duration = 0.3f;  // 动画持续时间
        float elapsed = 0f;
        
        // 锁定交互，防止拖动干扰动画
        if (scrollView != null)
        {
            scrollView.SetInteractionLocked(true);
        }
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            target.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        target.anchoredPosition = targetPosition;
        
        // 解锁交互
        if (scrollView != null)
        {
            scrollView.SetInteractionLocked(false);
        }
    }

    /// <summary>
    /// 将位置限制在ScrollView的边界内
    /// </summary>
    private Vector2 ClampPositionToBounds(Vector2 position, RectTransform content, RectTransform viewport)
    {
        Vector2 contentSize = content.rect.size;
        Vector2 viewportSize = viewport.rect.size;
        
        // 如果content比viewport小，不需要滚动限制
        bool canScrollX = contentSize.x > viewportSize.x;
        bool canScrollY = contentSize.y > viewportSize.y;
        
        float minX = canScrollX ? viewportSize.x - contentSize.x : 0f;
        float maxX = 0f;
        float minY = canScrollY ? viewportSize.y - contentSize.y : 0f;
        float maxY = 0f;
        
        // 限制位置
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        
        return position;
    }

    /// <summary>
    /// 切换元素的选中状态
    /// </summary>
    /// <param name="element">要切换选中状态的元素</param>
    private void ToggleElementSelection(GameObject element)
    {
        if (element == null) return;
        
        if (selectedElements.Contains(element))
        {
            // 如果已选中，则取消选中
            selectedElements.Remove(element);
        }
        else
        {
            // 如果未选中，则添加到选中列表
            selectedElements.Add(element);
        }
    }
    
    /// <summary>
    /// 添加元素到选中列表
    /// </summary>
    /// <param name="element">要添加的元素</param>
    public void AddToSelected(GameObject element)
    {
        if (element != null && !selectedElements.Contains(element))
        {
            selectedElements.Add(element);
        }
    }
    
    /// <summary>
    /// 从选中列表移除元素
    /// </summary>
    /// <param name="element">要移除的元素</param>
    public void RemoveFromSelected(GameObject element)
    {
        if (element != null && selectedElements.Contains(element))
        {
            selectedElements.Remove(element);
        }
    }
    
    /// <summary>
    /// 清空选中列表
    /// </summary>
    public void ClearSelected()
    {
        selectedElements.Clear();
    }
    
    /// <summary>
    /// 获取选中元素列表（只读）
    /// </summary>
    /// <returns>选中元素列表的只读副本</returns>
    public List<GameObject> GetSelectedElements()
    {
        return new List<GameObject>(selectedElements);
    }
    
    /// <summary>
    /// 检查元素是否被选中
    /// </summary>
    /// <param name="element">要检查的元素</param>
    /// <returns>如果元素在选中列表中返回true，否则返回false</returns>
    public bool IsElementSelected(GameObject element)
    {
        return element != null && selectedElements.Contains(element);
    }
    
    /// <summary>
    /// 切换其他组件的透明度
    /// </summary>
    private void ToggleOtherComponentsTransparency()
    {
        if (scrollView == null || scrollView.content == null) return;
        
        RectTransform content = scrollView.content;
        isDimmed = !isDimmed;
        
        // 遍历content下的所有子对象
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            GameObject childObj = child.gameObject;
            
            // 跳过选中的元素和第二个按钮
            if (selectedElements.Contains(childObj) || childObj == secondButton)
            {
                continue;
            }
            
            // 设置透明度
            SetGameObjectAlpha(childObj, isDimmed ? dimmedAlpha : 1f);
        }
    }
    
    /// <summary>
    /// 设置GameObject的透明度（通过CanvasGroup或Image/Text的color）
    /// </summary>
    private void SetGameObjectAlpha(GameObject obj, float alpha)
    {
        if (obj == null) return;
        
        // 优先使用CanvasGroup（如果存在）
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            return;
        }
        
        // 如果没有CanvasGroup，尝试通过Image和Text组件设置透明度
        Image image = obj.GetComponent<Image>();
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
        
        Text text = obj.GetComponent<Text>();
        if (text != null)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
        
        // 递归处理子对象（如果需要）
        // 注意：这里只处理直接子对象，如果需要递归处理所有子对象，可以取消下面的注释
        /*
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            SetGameObjectAlpha(obj.transform.GetChild(i).gameObject, alpha);
        }
        */
    }

    void OnDestroy()
    {
        // 清理事件监听
        if (firstButton != null)
        {
            firstButton.onClick.RemoveListener(OnFirstButtonClicked);
        }
    }
}

