using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class CustomScrollView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("ScrollView Settings")]
    public RectTransform content;          // 包含图片的Content
    public RectTransform viewport;         // ScrollView的视口
    [Range(0f, 1f)]
    public float elasticStrength = 0.3f;   // 回弹强度（0-1，值越大回弹越明显）
    
    private ScrollRect scrollRect;
    private RectTransform scrollRectTransform;
    private Vector2 contentStartPosition;
    private Vector2 dragStartPosition;     // 开始拖动时的鼠标屏幕位置
    private bool isDragging = false;
    private Vector2 velocity = Vector2.zero;
    private float smoothTime = 0.1f;
    [Header("Runtime State")]
    public bool interactionLocked = false; // 外部可锁定交互与惯性

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        scrollRectTransform = GetComponent<RectTransform>();
        
        // 如果content和viewport未手动指定，尝试从ScrollRect获取
        if (content == null && scrollRect != null)
        {
            content = scrollRect.content;
        }
        if (viewport == null && scrollRect != null)
        {
            viewport = scrollRect.viewport;
        }
        
        // 禁用默认的滚动，使用自定义拖动
        scrollRect.enabled = false;
    }

    void Update()
    {
        if (interactionLocked)
        {
            velocity = Vector2.zero;
            return;
        }

        if (!isDragging)
        {
            // 应用惯性效果
            if (velocity.magnitude > 0.01f)
            {
                content.anchoredPosition += velocity * Time.deltaTime;
                velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * 5f);
                EnsureBounds();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (interactionLocked) return;
        isDragging = true;
        contentStartPosition = content.anchoredPosition;
        dragStartPosition = eventData.position;
        velocity = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (interactionLocked) return;
        if (content == null || viewport == null) return;
        
        // 将屏幕坐标转换为viewport的本地空间坐标
        Vector2 currentLocalPoint, startLocalPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewport, eventData.position, eventData.pressEventCamera, out currentLocalPoint) &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewport, dragStartPosition, eventData.pressEventCamera, out startLocalPoint))
        {
            // 计算本地空间偏移
            Vector2 localDelta = currentLocalPoint - startLocalPoint;
            
            // 根据ScrollRect的滚动方向调整
            if (!scrollRect.horizontal) localDelta.x = 0;
            if (!scrollRect.vertical) localDelta.y = 0;
            
            // 移动content（跟随鼠标拖动方向）
            content.anchoredPosition = contentStartPosition + localDelta;
            
            // 确保在边界内，并应用回弹效果
            EnsureBounds();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (interactionLocked) return;
        isDragging = false;
        
        // 计算拖动结束时的速度（简化版本）
        if (eventData.delta != Vector2.zero)
        {
            velocity = eventData.delta / Time.deltaTime * 0.1f;
        }
    }

    public void SetInteractionLocked(bool locked)
    {
        interactionLocked = locked;
        if (locked)
        {
            velocity = Vector2.zero;
        }
    }

    private void EnsureBounds()
    {
        if (content == null || viewport == null) return;
        
        Vector2 contentSize = content.rect.size;
        Vector2 viewportSize = viewport.rect.size;
        
        // 如果content比viewport小，不需要滚动限制
        bool canScrollX = contentSize.x > viewportSize.x;
        bool canScrollY = contentSize.y > viewportSize.y;
        
        Vector2 contentPos = content.anchoredPosition;
        
        // 计算边界（根据ScrollRect的标准锚点设置：左上角为(0,1)）
        // 对于标准ScrollRect，content的锚点通常在左上角
        float minX = canScrollX ? viewportSize.x - contentSize.x : 0f;
        float maxX = 0f;
        float minY = canScrollY ? viewportSize.y - contentSize.y : 0f;
        float maxY = 0f;
        
        // 限制X轴位置并应用回弹效果
        if (scrollRect.horizontal || !scrollRect.vertical)
        {
            if (contentPos.x > maxX)
            {
                float overshoot = contentPos.x - maxX;
                float resistance = 1f - (elasticStrength * Mathf.Clamp01(overshoot / 100f));
                contentPos.x = maxX + overshoot * resistance;
                if (isDragging) velocity.x *= 0.3f;
            }
            else if (contentPos.x < minX)
            {
                float overshoot = contentPos.x - minX;
                float resistance = 1f - (elasticStrength * Mathf.Clamp01(Mathf.Abs(overshoot) / 100f));
                contentPos.x = minX + overshoot * resistance;
                if (isDragging) velocity.x *= 0.3f;
            }
        }
        
        // 限制Y轴位置并应用回弹效果
        if (scrollRect.vertical || !scrollRect.horizontal)
        {
            if (contentPos.y > maxY)
            {
                float overshoot = contentPos.y - maxY;
                float resistance = 1f - (elasticStrength * Mathf.Clamp01(overshoot / 100f));
                contentPos.y = maxY + overshoot * resistance;
                if (isDragging) velocity.y *= 0.3f;
            }
            else if (contentPos.y < minY)
            {
                float overshoot = contentPos.y - minY;
                float resistance = 1f - (elasticStrength * Mathf.Clamp01(Mathf.Abs(overshoot) / 100f));
                contentPos.y = minY + overshoot * resistance;
                if (isDragging) velocity.y *= 0.3f;
            }
        }
        
        content.anchoredPosition = contentPos;
    }

    // 公共方法：重置位置到中心
    public void ResetToCenter()
    {
        if (content == null || viewport == null) return;
        
        Vector2 contentSize = content.rect.size;
        Vector2 viewportSize = viewport.rect.size;
        
        Vector2 centeredPosition = new Vector2(
            (viewportSize.x - contentSize.x) * 0.5f,
            (contentSize.y - viewportSize.y) * 0.5f
        );
        
        content.anchoredPosition = centeredPosition;
        velocity = Vector2.zero;
    }
}