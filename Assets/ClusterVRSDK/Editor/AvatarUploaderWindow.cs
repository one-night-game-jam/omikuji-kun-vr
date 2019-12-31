#if NET_4_6
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using ClusterVRSDK.Core.Editor;
using ClusterVRSDK.Core.Editor.Avatar;
using Ionic.Zip;
using UnityEditor;
using UnityEngine;
using VRM;
using VRMValidatorLibrary.ValidationErrors;

namespace ClusterVRSDK.Editor
{
    public class AvatarUploaderWindow : EditorWindow
    {
        struct Message
        {
            public Message(string body, MessageType messageType)
            {
                Body = body;
                MessageType = messageType;
            }

            public string Body { get; }

            public MessageType MessageType { get; }
        }
        readonly List<Message> messages = new List<Message>();

        TokenAuthWidget tokenAuth;

        // build avatar
        GameObject avatarGameObject;
        string avatarName;
        Camera thumbnailCamera;
        Texture thumbnailTexture;
        bool canBuildAvatar;
        string zipPath;
        VRMExportSettings exportSettings;

        // upload avatar
        bool isPolicyAccepted;
        bool canUploadAvatar;
        bool isUnlimited;

        bool isProcessing;

        // vrm validation
        readonly List<IValidationError> validationErrors = new List<IValidationError>();

        [MenuItem("clusterSDK/AvatarUpload")]
        static void Open()
        {
            var window = GetWindow<AvatarUploaderWindow>();
            window.titleContent = new GUIContent("cluster AvatarUpload");
        }

        void OnEnable()
        {
            tokenAuth = new TokenAuthWidget();
            rootVisualElement.Add(tokenAuth.CreateView());
            rootVisualElement.Add(UiUtils.Separator());
            rootVisualElement.Add(new IMGUIContainer(LegacyOnGUI));
        }

        void LegacyOnGUI()
        {
            ShowBuildAvatarUI();

            EditorGUILayout.Space();

            ShowPublishAvatarUI();

            EditorGUILayout.Space();

            ShowMessagesUI();
        }

        void ShowBuildAvatarUI()
        {
            EditorGUILayout.LabelField("Avatar", EditorStyles.boldLabel);

            avatarGameObject =
                EditorGUILayout.ObjectField("Avatar", avatarGameObject, typeof(GameObject), true) as GameObject;
            EditorGUILayout.Space();
            avatarName = EditorGUILayout.TextField("Avatar name", avatarName);
            thumbnailCamera =
                EditorGUILayout.ObjectField("Thumbnail camera", thumbnailCamera, typeof(Camera), true) as Camera;

            ShowThumbnail();

            var canExport = false;
            if (avatarGameObject != null && tokenAuth.reactiveUserInfo.Val.HasValue && !string.IsNullOrEmpty(avatarName))
            {
                var userInfo = tokenAuth.reactiveUserInfo.Val.Value;

                canExport = true;
                exportSettings = new VRMExportSettings();
                exportSettings.InitializeFrom(avatarGameObject);

                if (string.IsNullOrEmpty(exportSettings.Title))
                {
                    exportSettings.Title = avatarName;
                }

                if (string.IsNullOrEmpty(exportSettings.Author))
                {
                    exportSettings.Author = userInfo.Username;
                }

                foreach (var error in exportSettings.CanExport())
                {
                    canExport = false;
                    messages.Add(new Message(error, MessageType.Error));
                }
            }

            canBuildAvatar = tokenAuth.reactiveUserInfo.Val.HasValue &&
                             !string.IsNullOrEmpty(avatarName) &&
                             thumbnailCamera != null &&
                             canExport;

            EditorGUI.BeginDisabledGroup(!canBuildAvatar || isProcessing);
            if (GUILayout.Button("Build") && tokenAuth.reactiveUserInfo.Val.HasValue)
            {
                BuildVRM(tokenAuth.reactiveUserInfo.Val.Value);
            }

            EditorGUI.EndDisabledGroup();
        }

        void BuildVRM(UserInfo userInfo)
        {
            var thumbnailPath = ExportThumbnailFile();
            var vrmPath = ExportVRMFile(avatarGameObject);

            var checker = new ValidationRuleFetcher(userInfo.VerifiedToken, rule =>
            {
                validationErrors.Clear();
                isUnlimited = false;

                var errors = VRMValidatorLibrary.VrmValidator.Validate(vrmPath);
                foreach (var validationError in errors)
                {
                    Debug.Log(validationError.Meta);
                    validationErrors.Add(validationError);
                }

                if (validationErrors.Count != 0)
                {
                    Debug.Log(validationErrors);

                    if (rule.IsDefault())
                    {
                        canUploadAvatar = false;
                        return;
                    }

                    isUnlimited = true;
                }

                var zip = new ZipFile();
                zip.AddFile(thumbnailPath, "");
                zip.AddFile(vrmPath, "");
                zipPath = Application.temporaryCachePath + Path.DirectorySeparatorChar + "avatar.zip";
                zip.Save(zipPath);

                canUploadAvatar = true;
                EditorUtility.DisplayDialog("Success", "ビルド完了", "OK");
            }, e =>
            {
                canUploadAvatar = false;
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Failed", "ビルド失敗", "OK");
            });
            checker.Run();
        }

        bool ValidateAvatar(GameObject target)
        {
            var animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                messages.Add(new Message("You should set Animator to model.", MessageType.Error));
                return false;
            }

            var avatar = animator.avatar;
            if (avatar == null)
            {
                messages.Add(new Message("You should set Avatar to Animator.", MessageType.Error));
                return false;
            }

            if (!avatar.isValid)
            {
                messages.Add(new Message("Animator Avatar is invalid.", MessageType.Error));
                return false;
            }

            if (!avatar.isHuman)
            {
                messages.Add(new Message("Animator Avatar should be Humanoid.", MessageType.Error));
                return false;
            }

            return true;
        }

        void ShowPublishAvatarUI()
        {
            if (!canUploadAvatar || !tokenAuth.reactiveUserInfo.Val.HasValue)
            {
                return;
            }
            var userInfo = tokenAuth.reactiveUserInfo.Val.Value;

            EditorGUILayout.LabelField("Upload", EditorStyles.boldLabel);

            if (GUILayout.Button("Show policy"))
            {
                Application.OpenURL(Constants.WebBaseUrl + "/terms");
            }

            isPolicyAccepted = EditorGUILayout.Toggle("Accept policy", isPolicyAccepted);
            EditorGUI.BeginDisabledGroup(!isPolicyAccepted || isProcessing);
            if (GUILayout.Button("Upload"))
            {
                isProcessing = true;
                EditorUtility.DisplayProgressBar("Uploading...", "アップロード中", 0.5F);
                var service = new AvatarUploadService(userInfo.VerifiedToken, zipPath, avatarName,
                    () =>
                    {
                        EditorUtility.ClearProgressBar();
                        EditorUtility.DisplayDialog("Success", "アップロード完了", "OK");
                        isProcessing = false;
                    }, e =>
                    {
                        EditorUtility.ClearProgressBar();
                        EditorUtility.DisplayDialog("Failed", "アップロードに失敗しました", "OK");
                        isProcessing = false;
                    });
                service.Run();
            }

            EditorGUI.EndDisabledGroup();
        }

        void ShowMessagesUI()
        {
            EditorGUILayout.LabelField("System Messages");

            if (canBuildAvatar)
            {
                messages.Add(new Message("You can build an avatar.", MessageType.Info));
            }
            else
            {
                canUploadAvatar = false;
            }

            ShowMessages();
        }

        void ShowThumbnail()
        {
            if (thumbnailTexture == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayoutOption[] thumbnailSize =
            {
                GUILayout.Width(256),
                GUILayout.Height(256)
            };
            GUILayout.FlexibleSpace();
            GUILayout.Label(thumbnailTexture, thumbnailSize);
            GUILayout.Label("", GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
        }

        void ShowMessages()
        {
            foreach (var message in messages)
            {
                EditorGUILayout.HelpBox(message.Body, message.MessageType);
            }

            var messageType = MessageType.Error;
            if (isUnlimited)
            {
                messageType = MessageType.Warning;
            }

            foreach (var validationError in validationErrors)
            {
                EditorGUILayout.HelpBox(BuildErrorMessage(validationError), messageType);
            }

            messages.Clear();
        }

        static string BuildErrorMessage(IValidationError validationError)
        {
            switch (validationError.Code)
            {
                case ErrorCodes.UnallowedImageMimeType:
                    return $"{validationError.Resource}の画像は使用できません。";
                case ErrorCodes.UnallowedShader:
                    return $"{validationError.Resource}は使用できないシェーダーです。";
                case ErrorCodes.UnallowedVrmVersion:
                    return "サポートされていないバージョンのVRMで作成されたデータです。";
                case ErrorCodes.TooLargeImageResolution:
                    return $"{validationError.Resource}のサイズが大きすぎます。";
                case ErrorCodes.InvalidValue:
                    return $"{validationError.Resource}は{validationError.Meta["expected"]}である必要があります。";
                case ErrorCodes.NullMaterial:
                    return $"{validationError.Resource}にnullなマテリアルが含まれています。";
                case ErrorCodes.TooLargeCount:
                    return $"{validationError.Resource}の数が多すぎます。" +
                           $"{validationError.Resource}の数は{validationError.Meta["max"]}以下にする必要があります。";
                case ErrorCodes.Unknown:
                default:
                    return "不明なエラー";
            }
        }

        string ExportThumbnailFile()
        {
            var original = RenderTexture.active;
            var renderTexture = new RenderTexture(512, 512, 24);
            thumbnailCamera.targetTexture = renderTexture;
            thumbnailCamera.Render();
            RenderTexture.active = renderTexture;
            var newTexture = new Texture2D(renderTexture.width, renderTexture.height);
            newTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            newTexture.Apply();
            thumbnailTexture = newTexture;
            thumbnailCamera.targetTexture = null;
            RenderTexture.active = original;

            var pngPath = Application.temporaryCachePath + Path.DirectorySeparatorChar + "thumbnail.png";
            var pngData = newTexture.EncodeToPNG();
            File.WriteAllBytes(pngPath, pngData);

            return pngPath;
        }

        string ExportVRMFile(GameObject target)
        {
            var vrmPath = Application.temporaryCachePath + Path.DirectorySeparatorChar + "avatar.vrm";
            exportSettings.Source = target;
            exportSettings.Export(vrmPath);

            return vrmPath;
        }
    }
}
#endif
