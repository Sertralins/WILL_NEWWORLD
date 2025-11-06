using UnityEngine;
using UnityEditor;

/// <summary>
/// 自动检测并修复场景中多个 Audio Listener 的问题
/// </summary>
public class AudioListenerFixer : EditorWindow
{
    [MenuItem("Tools/修复 Audio Listener 问题")]
    public static void ShowWindow()
    {
        GetWindow<AudioListenerFixer>("Audio Listener 修复工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("Audio Listener 检测和修复工具", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 检测当前场景中的 Audio Listener
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        GUILayout.Label($"当前场景中找到 {listeners.Length} 个 Audio Listener:", EditorStyles.label);
        GUILayout.Space(5);

        if (listeners.Length == 0)
        {
            EditorGUILayout.HelpBox("场景中没有找到 Audio Listener。", MessageType.Warning);
        }
        else if (listeners.Length == 1)
        {
            EditorGUILayout.HelpBox("✓ 场景中只有一个 Audio Listener，配置正确！", MessageType.Info);
            GUILayout.Label($"位置: {listeners[0].gameObject.name}", EditorStyles.label);
        }
        else
        {
            EditorGUILayout.HelpBox($"警告: 场景中有 {listeners.Length} 个 Audio Listener！应该只保留一个。", MessageType.Warning);
            GUILayout.Space(10);

            // 显示所有 Audio Listener
            for (int i = 0; i < listeners.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField($"Audio Listener {i + 1}:", listeners[i], typeof(AudioListener), true);
                
                // 标记主摄像机
                if (listeners[i].GetComponent<Camera>() != null && listeners[i].GetComponent<Camera>().tag == "MainCamera")
                {
                    GUILayout.Label("(主摄像机)", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            // 自动修复按钮
            if (GUILayout.Button("自动修复 (保留主摄像机的 Audio Listener)"))
            {
                FixAudioListeners();
            }

            GUILayout.Space(5);

            // 手动选择要保留的 Audio Listener
            EditorGUILayout.HelpBox("或者手动选择要保留的 Audio Listener，然后点击下面的按钮移除其他的。", MessageType.Info);
            
            selectedListener = EditorGUILayout.ObjectField("要保留的 Audio Listener:", selectedListener, typeof(AudioListener), true) as AudioListener;
            
            if (selectedListener != null && GUILayout.Button($"移除其他 Audio Listener (保留 {selectedListener.gameObject.name})"))
            {
                RemoveOtherListeners(selectedListener);
            }
        }
    }

    private AudioListener selectedListener;

    private void FixAudioListeners()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        if (listeners.Length <= 1)
        {
            EditorUtility.DisplayDialog("提示", "场景中 Audio Listener 数量正常，无需修复。", "确定");
            return;
        }

        // 优先保留主摄像机的 Audio Listener
        AudioListener mainCameraListener = null;
        foreach (AudioListener listener in listeners)
        {
            Camera cam = listener.GetComponent<Camera>();
            if (cam != null && cam.tag == "MainCamera")
            {
                mainCameraListener = listener;
                break;
            }
        }

        // 如果没有找到主摄像机，保留第一个
        if (mainCameraListener == null)
        {
            mainCameraListener = listeners[0];
        }

        // 移除其他的 Audio Listener
        int removedCount = 0;
        foreach (AudioListener listener in listeners)
        {
            if (listener != mainCameraListener)
            {
                DestroyImmediate(listener);
                removedCount++;
            }
        }

        EditorUtility.DisplayDialog("修复完成", 
            $"已移除 {removedCount} 个 Audio Listener。\n保留的 Audio Listener: {mainCameraListener.gameObject.name}", 
            "确定");
        
        Debug.Log($"Audio Listener 修复完成: 移除了 {removedCount} 个，保留了 {mainCameraListener.gameObject.name}");
    }

    private void RemoveOtherListeners(AudioListener keepListener)
    {
        if (keepListener == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择要保留的 Audio Listener。", "确定");
            return;
        }

        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        int removedCount = 0;

        foreach (AudioListener listener in listeners)
        {
            if (listener != keepListener)
            {
                DestroyImmediate(listener);
                removedCount++;
            }
        }

        EditorUtility.DisplayDialog("修复完成", 
            $"已移除 {removedCount} 个 Audio Listener。\n保留的 Audio Listener: {keepListener.gameObject.name}", 
            "确定");
        
        Debug.Log($"Audio Listener 修复完成: 移除了 {removedCount} 个，保留了 {keepListener.gameObject.name}");
    }
}

