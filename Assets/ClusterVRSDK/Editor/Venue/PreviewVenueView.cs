using System.IO;
using ClusterVRSDK.Core.Editor;
using ClusterVRSDK.Core.Editor.Venue;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClusterVRSDK.Editor.Venue
{
    public class PreviewVenueView
    {
        readonly Reactive<Core.Editor.Venue.Json.Venue> reactiveCurrentVenue;

        public  PreviewVenueView(Reactive<Core.Editor.Venue.Json.Venue> reactiveCurrentVenue)
        {
            this.reactiveCurrentVenue = reactiveCurrentVenue;
        }

        public void AddView(VisualElement parent)
        {
            var view = new IMGUIContainer(() =>
            {
                Process();
                DrawUI();
            });
            parent.Add(view);
        }

        bool executeBuild;
        string errorMessage;
        bool executePreview;
        bool executePreviousPreview;

        void Process()
        {
            if (executePreview || executePreviousPreview)
            {
                var previous = executePreviousPreview;
                executePreview = false;
                executePreviousPreview = false;

                if (!VenueSdkTools.ValidateVenue(out errorMessage))
                {
                    Debug.LogError(errorMessage);
                    EditorUtility.DisplayDialog("ClusterVRSDK", errorMessage, "閉じる");
                    return;
                }

                if (!previous)
                {
                    EditorPrefsUtils.PreviousBuildSceneName = reactiveCurrentVenue.Val?.VenueId?.Value ?? "sceneName";
                    AssetExporter.PreparePreview(EditorPrefsUtils.PreviousBuildSceneName);
                }

                VenueSdkTools.PreviewVenue(EditorPrefsUtils.LastBuildPath, EditorPrefsUtils.PreviousBuildSceneName);

                errorMessage = "";
            }
        }

        void DrawUI()
        {
            {
                EditorGUILayout.Space();

                var assetBundleName = $"{EditorPrefsUtils.PreviousBuildSceneName}";
                var previousBuildPath = $"{Application.temporaryCachePath}/{EditorUserBuildSettings.activeBuildTarget}/{assetBundleName}";

                if (File.Exists(previousBuildPath))
                {
                    executePreviousPreview = GUILayout.Button("以前のビルドをプレビュー");
                }

                executePreview = GUILayout.Button("　今開いているシーンをプレビュー　");
            }

            EditorGUILayout.Space();

            if (File.Exists(EditorPrefsUtils.LastBuildPath))
            {
                var fileInfo = new FileInfo(EditorPrefsUtils.LastBuildPath);
                EditorGUILayout.LabelField($"日時：{fileInfo.LastWriteTime}");
                EditorGUILayout.LabelField($"サイズ：{(double) fileInfo.Length / (1024 * 1024):F2} MB"); // Byte => MByte
            }
        }
    }
}
