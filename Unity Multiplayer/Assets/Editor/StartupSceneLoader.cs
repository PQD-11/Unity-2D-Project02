using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad]
/*
[InitializeOnLoad]: Đây là một thuộc tính được sử dụng trong Unity
để gắn một lớp với sự kiện xảy ra khi tải dự án hoặc khi bắt đầu chạy trò chơi. 
Trong trường hợp này, nó sẽ kích hoạt lớp StartupSceneLoader 
mỗi khi dự án được tải hoặc trò chơi được khởi động.


*/
public static class StartupSceneLoader
{
    static StartupSceneLoader()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            if (EditorSceneManager.GetActiveScene().buildIndex != 0)
            {
                EditorSceneManager.LoadScene(0);
            }
        }

    }
}
