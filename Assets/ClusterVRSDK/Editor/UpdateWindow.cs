#if NET_4_6
using ClusterVRSDK.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace ClusterVRSDK.Editor
{
    [InitializeOnLoad]
    public class UpdateWindow : EditorWindow
    {
        static bool isProcessing;
        static bool isDone;

        static UpdateWindow()
        {
            EditorApplication.update += Tick;
        }

        [MenuItem("Test/UpdateCheck")]
        static void Test()
        {
            isProcessing = false;
            isDone = false;
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            if (isDone)
            {
                EditorApplication.update -= Tick;
                return;
            }

            if (isProcessing)
            {
                return;
            }

            var ckecker = new ForceUpdateChecker(needUpdate =>
            {
                isDone = true;
                if (needUpdate)
                {
                    var window = GetWindow<UpdateWindow>();
                    window.titleContent = new GUIContent("cluster.");
                }
                else
                {
                    GetWindow<UpdateWindow>().Close();
                }
            }, e =>
            {
                Debug.LogException(e);
                isDone = true;
            });
            ckecker.Run();
            isProcessing = true;
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox("A new version is available. Please update SDK.", MessageType.Warning);

            EditorGUILayout.Space();

            if (GUILayout.Button("Get Latest Version"))
            {
                Application.OpenURL(Constants.WebBaseUrl);
            }
        }
    }
}
#endif
