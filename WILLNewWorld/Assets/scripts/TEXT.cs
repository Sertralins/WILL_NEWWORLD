using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TEXT : MonoBehaviour
{
    [Header("文字设置")]
    [Tooltip("要显示的文字数组，每个元素代表一行")]
    [TextArea(3, 10)]
    public string[] textLines = new string[] { "第一行文字", "第二行文字", "第三行文字" };
    
    [Header("打字机设置")]
    [Tooltip("每个字符显示的间隔时间（秒）")]
    public float typewriterSpeed = 0.05f;
    
    [Header("组件引用")]
    [Tooltip("显示文字的Text组件数组，每个Text对应一行文字（数量应与textLines数组一致）")]
    public Text[] textDisplays;
    
    [Tooltip("触发显示的按钮")]
    public Button triggerButton;
    
    [Tooltip("失活后要激活的下一个GameObject（可以为空）")]
    public GameObject nextGameObject;
    
    private int currentLineIndex = 0;  // 当前显示的行索引
    private bool isTyping = false;     // 是否正在打字
    private Coroutine typingCoroutine; // 打字协程引用
    
    void Awake()
    {
        // 如果没有指定Text组件数组，尝试从子对象获取所有Text组件
        if (textDisplays == null || textDisplays.Length == 0)
        {
            Text[] allTexts = GetComponentsInChildren<Text>();
            if (allTexts != null && allTexts.Length > 0)
            {
                textDisplays = allTexts;
                Debug.Log("TEXT: 自动找到 " + textDisplays.Length + " 个Text组件");
            }
        }
        
        // 如果没有指定按钮，尝试从子对象获取
        if (triggerButton == null)
        {
            triggerButton = GetComponentInChildren<Button>();
        }
    }
    
    void Start()
    {
        // 如果找到了按钮，添加点击事件
        if (triggerButton != null)
        {
            triggerButton.onClick.AddListener(OnButtonClick);
            Debug.Log("TEXT: 按钮已绑定，按钮名称: " + triggerButton.name);
        }
        else
        {
            Debug.LogWarning("TEXT: 未找到按钮组件！请确保场景中有Button组件。");
        }
        
        // 初始化：清空所有文字
        if (textDisplays != null && textDisplays.Length > 0)
        {
            for (int i = 0; i < textDisplays.Length; i++)
            {
                if (textDisplays[i] != null)
                {
                    textDisplays[i].text = "";
                }
            }
            Debug.Log("TEXT: 找到 " + textDisplays.Length + " 个Text组件，已清空所有文字");
        }
        else
        {
            Debug.LogWarning("TEXT: 未找到Text组件数组！请确保场景中有Text组件。");
        }
        
        // 检查文字数组和Text数组的数量是否匹配
        if (textLines == null || textLines.Length == 0)
        {
            Debug.LogWarning("TEXT: 文字数组为空！请在Inspector中设置textLines数组。");
        }
        else if (textDisplays != null && textDisplays.Length != textLines.Length)
        {
            Debug.LogWarning("TEXT: 文字数组数量(" + textLines.Length + ")与Text组件数量(" + textDisplays.Length + ")不匹配！");
        }
    }
    
    /// <summary>
    /// 按钮点击事件处理
    /// </summary>
    void OnButtonClick()
    {
        Debug.Log("TEXT: 按钮被点击！");
        HandleClick();
    }
    
    /// <summary>
    /// 处理点击逻辑（可以被按钮或鼠标点击调用）
    /// </summary>
    void HandleClick()
    {
        // 如果正在打字，直接完成当前行
        if (isTyping)
        {
            Debug.Log("TEXT: 正在打字，直接完成当前行");
            CompleteCurrentLine();
            return;
        }
        
        // 如果所有文字都显示完了，清空并失活
        if (currentLineIndex >= textLines.Length)
        {
            Debug.Log("TEXT: 所有文字显示完毕，清空并失活");
            ClearAndDeactivate();
            return;
        }
        
        // 否则显示下一行
        Debug.Log("TEXT: 显示下一行，索引: " + currentLineIndex);
        ShowNextLine();
    }
    
    /// <summary>
    /// 公共方法：可以在Inspector中直接绑定到按钮的OnClick事件
    /// </summary>
    public void OnClick()
    {
        Debug.Log("TEXT: OnClick方法被调用");
        HandleClick();
    }
    
    /// <summary>
    /// 鼠标点击事件（需要Collider，主要用于3D对象）
    /// </summary>
    void OnMouseDown()
    {
        Debug.Log("TEXT: GameObject被鼠标点击");
        HandleClick();
    }
    
    /// <summary>
    /// 显示下一行文字（打字机效果）
    /// </summary>
    void ShowNextLine()
    {
        if (currentLineIndex >= textLines.Length)
        {
            return;
        }
        
        // 检查是否有对应的Text组件
        if (currentLineIndex >= textDisplays.Length || textDisplays[currentLineIndex] == null)
        {
            Debug.LogWarning("TEXT: 第 " + currentLineIndex + " 行没有对应的Text组件！");
            currentLineIndex++;
            return;
        }
        
        // 停止之前的协程（如果有）
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // 开始打字机效果，传入对应的Text组件和文字
        typingCoroutine = StartCoroutine(TypewriterEffect(textDisplays[currentLineIndex], textLines[currentLineIndex]));
        currentLineIndex++;
    }
    
    /// <summary>
    /// 打字机效果协程
    /// </summary>
    IEnumerator TypewriterEffect(Text targetText, string text)
    {
        isTyping = true;
        
        if (targetText != null)
        {
            targetText.text = "";
            
            foreach (char c in text)
            {
                targetText.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }
        
        isTyping = false;
    }
    
    /// <summary>
    /// 直接完成当前行的显示
    /// </summary>
    void CompleteCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        int lineIndex = currentLineIndex - 1;
        if (lineIndex >= 0 && lineIndex < textLines.Length && 
            lineIndex < textDisplays.Length && textDisplays[lineIndex] != null)
        {
            textDisplays[lineIndex].text = textLines[lineIndex];
        }
        
        isTyping = false;
    }
    
    /// <summary>
    /// 清空所有文字并失活
    /// </summary>
    void ClearAndDeactivate()
    {
        // 清空所有Text组件的文字
        if (textDisplays != null)
        {
            for (int i = 0; i < textDisplays.Length; i++)
            {
                if (textDisplays[i] != null)
                {
                    textDisplays[i].text = "";
                }
            }
        }
        
        currentLineIndex = 0;
        isTyping = false;
        
        // 失活当前GameObject
        gameObject.SetActive(false);
        
        // 激活下一个GameObject
        if (nextGameObject != null)
        {
            nextGameObject.SetActive(true);
            Debug.Log("TEXT: 已激活下一个GameObject: " + nextGameObject.name);
            
            // 如果下一个GameObject也有TEXT组件，可以自动重置它
            TEXT nextText = nextGameObject.GetComponent<TEXT>();
            if (nextText != null)
            {
                nextText.Reset();
            }
        }
    }
    
    /// <summary>
    /// 重置组件状态（可以在外部调用）
    /// </summary>
    public void Reset()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        currentLineIndex = 0;
        isTyping = false;
        
        // 清空所有Text组件的文字
        if (textDisplays != null)
        {
            for (int i = 0; i < textDisplays.Length; i++)
            {
                if (textDisplays[i] != null)
                {
                    textDisplays[i].text = "";
                }
            }
        }
        
        gameObject.SetActive(true);
    }
    
    void OnDestroy()
    {
        // 清理按钮事件
        if (triggerButton != null)
        {
            triggerButton.onClick.RemoveListener(OnButtonClick);
        }
    }
}

