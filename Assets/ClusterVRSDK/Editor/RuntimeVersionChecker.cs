#if !NET_4_6
using UnityEditor;
using System.Diagnostics;

[InitializeOnLoad]
public static class RuntimeVersionChecker
{
    static RuntimeVersionChecker()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.timeSinceStartup < 10) return;
        
        if (PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Legacy)
        {
            PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;

            if (EditorUtility.DisplayDialog(
                "再起動が必要です", 
                "スクリプティングランタイムのバージョンを.Net 4.xに変更しました。" +
                "\n変更を適用するためにはエディターを再起動する必要があります。" +
                "\n今すぐ再起動しますか?", 
                "再起動", "キャンセル"))
            {
                RestartUnity();
            }
        }
    }

    private static void RestartUnity()
    {
      #if UNITY_EDITOR_OSX
        var fileName = EditorApplication.applicationPath + "/Contents/MacOS/Unity";
      #else
        var fileName = EditorApplication.applicationPath;    
      #endif
        Process.Start(fileName);
        EditorApplication.Exit(0);
    }
}
#endif