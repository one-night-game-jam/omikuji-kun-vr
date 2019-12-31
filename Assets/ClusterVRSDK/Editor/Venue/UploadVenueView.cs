using System;
using System.IO;
using System.Linq;
using ClusterVRSDK.Core.Editor;
using ClusterVRSDK.Core.Editor.Venue;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace ClusterVRSDK.Editor.Venue
{
    public class UploadVenueView
    {
        readonly UserInfo userInfo;
        readonly Core.Editor.Venue.Json.Venue venue;

        bool executeUpload;
        string errorMessage;
        UploadVenueService currentUploadService;

        public UploadVenueView(UserInfo userInfo, Core.Editor.Venue.Json.Venue venue)
        {
            Assert.IsNotNull(venue);
            this.userInfo = userInfo;
            this.venue = venue;
        }

        public VisualElement CreateView()
        {
            return new IMGUIContainer(() => {Process(); DrawUI();});
        }

        void Process()
        {
            if (executeUpload)
            {
                executeUpload = false;
                currentUploadService = null;

                if (!VenueSdkTools.ValidateVenue(out errorMessage))
                {
                    Debug.LogError(errorMessage);
                    EditorUtility.DisplayDialog("ClusterVRSDK", errorMessage, "閉じる");
                    return;
                }

                try
                {
                    AssetExporter.ExportCurrentSceneResource(venue.VenueId.Value, false); //Notice UnityPackage が大きくなりすぎてあげれないので一旦やめる
                }
                catch (Exception e)
                {
                    errorMessage = $"現在のSceneのUnityPackage作成時にエラーが発生しました。 {e.Message}";
                    return;
                }

                currentUploadService = new UploadVenueService(
                    userInfo.VerifiedToken,
                    venue,
                    () => errorMessage = "",
                    exception =>
                    {
                        errorMessage = $"会場データのアップロードに失敗しました。リトライしてみてください。 {exception.Message}";
                        EditorWindow.GetWindow<VenueUploadWindow>().Repaint();
                    });
                currentUploadService.Run();
                errorMessage = null;
            }
        }

        void DrawUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("アップロードするシーンを開いておいてください。", MessageType.Info);


            if (GUILayout.Button($"'{venue.Name}'としてアップロードする"))
            {
                executeUpload = EditorUtility.DisplayDialog(
                    "会場をアップロードする",
                    $"'{venue.Name}'としてアップロードします。よろしいですか？",
                    "アップロード",
                    "キャンセル"
                );
            }

            EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }

            if (currentUploadService == null)
            {
                return;
            }

            if (!currentUploadService.IsProcessing)
            {
                EditorUtility.ClearProgressBar();
                foreach (var status in currentUploadService.UploadStatus)
                {
                    var text = status.Value ? "Success" : "Failed";
                    EditorGUILayout.LabelField(status.Key.ToString(), text);
                }
            }
            else
            {
                var statesValue = currentUploadService.UploadStatus.Values.ToList();
                var finishedProcessCount = statesValue.Count(x => x);
                var allProcessCount = statesValue.Count;
                EditorUtility.DisplayProgressBar(
                    "Venue Upload",
                    $"upload processing {finishedProcessCount} of {allProcessCount}",
                    (float) finishedProcessCount / allProcessCount
                );
            }

            if (!currentUploadService.IsProcessing
                && currentUploadService.UploadStatus.Values.Any(x => !x))
            {
                if (GUILayout.Button("アップロードリトライ"))
                {
                    currentUploadService.Run();
                    errorMessage = null;
                }
            }

            if (File.Exists(EditorPrefsUtils.LastBuildPath))
            {
                var fileInfo = new FileInfo(EditorPrefsUtils.LastBuildPath);
                EditorGUILayout.LabelField($"日時：{fileInfo.LastWriteTime}");
                EditorGUILayout.LabelField($"サイズ：{(double) fileInfo.Length / (1024 * 1024):F2} MB"); // Byte => MByte
            }
        }
    }
}
