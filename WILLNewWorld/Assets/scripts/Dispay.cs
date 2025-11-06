using UnityEngine;

using System.Collections.Generic;

/// <summary>
/// 场景显示管理器：当一个场景激活时，自动失活其他场景
/// </summary>
public class Dispay : MonoBehaviour
{
    [Header("场景管理设置")]
    [Tooltip("需要管理的场景对象列表（通常是不同的面板或区域）")]
    public List<GameObject> scenes = new List<GameObject>();
    
    [Tooltip("默认激活的场景索引（-1 表示不激活任何场景）")]
    public int defaultActiveSceneIndex = -1;
    
    [Tooltip("是否在 Start 时激活默认场景")]
    public bool activateDefaultOnStart = true;
    
    private int currentActiveIndex = -1;

    void Start()
    {
        // 初始化：失活所有场景
        DeactivateAllScenes();
        
        // 如果设置了默认激活场景，则激活它
        if (activateDefaultOnStart && defaultActiveSceneIndex >= 0 && defaultActiveSceneIndex < scenes.Count)
        {
            ActivateScene(defaultActiveSceneIndex);
        }
    }

    /// <summary>
    /// 激活指定索引的场景，并失活其他所有场景
    /// </summary>
    /// <param name="index">场景在列表中的索引</param>
    public void ActivateScene(int index)
    {
        // 检查索引是否有效
        if (index < 0 || index >= scenes.Count)
        {
            Debug.LogWarning($"Dispay: 场景索引 {index} 无效！有效范围：0 到 {scenes.Count - 1}");
            return;
        }

        // 检查场景对象是否存在
        if (scenes[index] == null)
        {
            Debug.LogWarning($"Dispay: 场景索引 {index} 的对象为空！");
            return;
        }

        // 失活所有场景
        DeactivateAllScenes();

        // 激活指定场景
        scenes[index].SetActive(true);
        currentActiveIndex = index;

        Debug.Log($"Dispay: 已激活场景索引 {index}，其他场景已失活");
    }

    /// <summary>
    /// 通过场景对象引用激活场景
    /// </summary>
    /// <param name="sceneObject">要激活的场景对象</param>
    public void ActivateScene(GameObject sceneObject)
    {
        if (sceneObject == null)
        {
            Debug.LogWarning("Dispay: 场景对象为空！");
            return;
        }

        // 查找场景对象在列表中的索引
        int index = scenes.IndexOf(sceneObject);
        
        if (index == -1)
        {
            Debug.LogWarning("Dispay: 场景对象不在管理列表中！");
            return;
        }

        ActivateScene(index);
    }

    /// <summary>
    /// 失活所有场景
    /// </summary>
    public void DeactivateAllScenes()
    {
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i] != null)
            {
                scenes[i].SetActive(false);
            }
        }
        currentActiveIndex = -1;
    }

    /// <summary>
    /// 获取当前激活的场景索引
    /// </summary>
    /// <returns>当前激活的场景索引，如果没有激活的场景则返回 -1</returns>
    public int GetCurrentActiveIndex()
    {
        return currentActiveIndex;
    }

    /// <summary>
    /// 获取当前激活的场景对象
    /// </summary>
    /// <returns>当前激活的场景对象，如果没有激活的场景则返回 null</returns>
    public GameObject GetCurrentActiveScene()
    {
        if (currentActiveIndex >= 0 && currentActiveIndex < scenes.Count)
        {
            return scenes[currentActiveIndex];
        }
        return null;
    }

    /// <summary>
    /// 检查指定索引的场景是否处于激活状态
    /// </summary>
    /// <param name="index">场景索引</param>
    /// <returns>如果场景激活则返回 true，否则返回 false</returns>
    public bool IsSceneActive(int index)
    {
        if (index < 0 || index >= scenes.Count || scenes[index] == null)
        {
            return false;
        }
        return scenes[index].activeSelf;
    }
}

