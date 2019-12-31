using System;
using System.Collections.Generic;
using ClusterVRSDK.Editor;
using ClusterVRSDK.Editor.Preview;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class PreviewControlWindow : EditorWindow
{
    static bool isInGameMode;
    const string messageWhenNotPlayMode = "プレビューオプションは実行中のみ使用可能です";

    static Dictionary<LabelType, int> dictLabelAndFontSize = new Dictionary<LabelType, int>
    {
        {LabelType.h1,18},
        {LabelType.h2,14}
    };

    [MenuItem("Window/PreviewControlWindow")]
    public static void Show()
    {
        PreviewControlWindow wnd = GetWindow<PreviewControlWindow>();
        wnd.titleContent = new GUIContent("PreviewControlWindow");
    }

    public static void SetIsInGameMode(bool value)
    {
        isInGameMode = value;
    }

    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        Bootstrap.OnUpdateTriggerIdList += triggerIdList => root.Add(GenerateTriggerSection(triggerIdList));
        root.Add(GenerateCommentSection());
        root.Add(UiUtils.Separator());
        root.Add(GenerateMainScreenSection());
        root.Add(UiUtils.Separator());
        root.Add(GenerateUserDataSection());
        root.Add(UiUtils.Separator());
        //root.Add(GenerateTriggerSection());

    }

    void SendComment(string displayName, string userName, string content)
    {
        if (!isInGameMode)
        {
            Debug.LogWarning(messageWhenNotPlayMode);
            return;
        }

        if (String.IsNullOrEmpty(displayName))
            displayName = "DisplayName";
        if (String.IsNullOrEmpty(userName))
            userName = "UserName";
        Bootstrap.CommentScreenPresenter.SendCommentFromEditorUI(displayName, userName, content);
    }

    void ShowMainScreenPicture()
    {
        if (!isInGameMode)
        {
            Debug.LogWarning(messageWhenNotPlayMode);
            return;
        }
        Bootstrap.MainScreenPresenter.SetImage(AssetDatabase.LoadAssetAtPath<Texture>("Assets/ClusterVRSDK/Editor/Preview/SampleTexture/cluster_logo.png"));
    }

    void SendTrigger(string id, float diff)
    {
        Bootstrap.VenueGimmickManager.RunFromEditor(id, diff);
    }

    Label GenerateLabel(LabelType labelType, string content)
    {
        Label label = new Label(content);
        label.style.fontSize = dictLabelAndFontSize[labelType];
        return label;
    }

    VisualElement GenerateSection()
    {
        return new VisualElement
        {
            style =
            {
                paddingTop = 10,
                paddingLeft = 10,
                paddingRight = 10,
                paddingBottom = 10,
                flexShrink = 0
            }
        };
    }

    VisualElement GenerateCommentSection()
    {
        var commentSection = GenerateSection();
        commentSection.Add(GenerateLabel(LabelType.h1, "コメント"));
        commentSection.Add(GenerateLabel(LabelType.h2, "表示名"));
        TextField displayNameField = new TextField();
        commentSection.Add(displayNameField);

        commentSection.Add(GenerateLabel(LabelType.h2, "ユーザー名"));
        TextField userNameField = new TextField();
        commentSection.Add(userNameField);

        commentSection.Add(GenerateLabel(LabelType.h2, "コメント内容"));

        TextField commentContentField = new TextField();
        commentContentField.style.unityTextAlign = TextAnchor.UpperLeft;
        commentContentField.multiline = true;
        commentContentField.style.height = 50;
        foreach (var child in commentContentField.Children())
        {
            child.style.unityTextAlign = TextAnchor.UpperLeft;
        }

        commentSection.Add(commentContentField);

        Button commentSendButton = new Button(() =>
        {
            SendComment(displayNameField.value, userNameField.value, commentContentField.value);
            displayNameField.value = "";
            userNameField.value = "";
            commentContentField.value = "";
        });
        commentSendButton.text = "コメントを送信";
        commentSection.Add(commentSendButton);
        return commentSection;
    }

    VisualElement GenerateMainScreenSection()
    {
        var mainScreenSection = GenerateSection();
        mainScreenSection.Add(GenerateLabel(LabelType.h1, "メインスクリーン"));
        Button sampleImageSendButton = new Button(ShowMainScreenPicture);
        sampleImageSendButton.text = "サンプル画像を投影";
        mainScreenSection.Add(sampleImageSendButton);
        return mainScreenSection;
    }

    VisualElement GenerateTriggerSection(List<string> triggerIdList)
    {
        var triggerSection = GenerateSection();
        triggerSection.Add(GenerateLabel(LabelType.h1, "トリガー"));
        triggerSection.Add(GenerateLabel(LabelType.h2, "トリガー内容"));

        var triggerPopupField = new PopupField<string>(triggerIdList,0);
        triggerSection.Add(triggerPopupField);

        Button triggerSendButton = new Button(() =>
        {
            SendTrigger(triggerPopupField.value, 0f);
        });
        triggerSendButton.text = "トリガーを送信";
        triggerSection.Add(triggerSendButton);
        return triggerSection;
    }

    VisualElement GenerateUserDataSection()
    {
        var userDataSection = GenerateSection();
        userDataSection.Add(GenerateLabel(LabelType.h1, "プレイヤー情報"));
        userDataSection.Add(GenerateLabel(LabelType.h2, "権限"));
        var currentPermission = GenerateLabel(LabelType.h2, "現在の権限:参加者");
        Button permissionChangeButton = new Button(() =>
        {
            if (!isInGameMode)
            {
                Debug.LogWarning(messageWhenNotPlayMode);
                return;
            }
            if (Bootstrap.PlayerPresenter.PermissionType == PermissionType.Audience)
            {
                Bootstrap.PlayerPresenter.ChangePermissionType(PermissionType.Performer);
                currentPermission.text = "現在の権限:パフォーマー";
            }
            else
            {
                Bootstrap.PlayerPresenter.ChangePermissionType(PermissionType.Audience);
                currentPermission.text = "現在の権限:参加者";
            }
        });
        permissionChangeButton.text = "権限変更";
        userDataSection.Add(currentPermission);
        userDataSection.Add(permissionChangeButton);

        userDataSection.Add(GenerateLabel(LabelType.h2, "リスポーン"));
        Button respawnButton = new Button(() =>
        {
            Bootstrap.SpawnPointManager.Respawn(Bootstrap.PlayerPresenter.PermissionType, Bootstrap.PlayerPresenter.DesktopPlayerController.transform);
        });
        respawnButton.text = "リスポーンする";
        userDataSection.Add(respawnButton);
        return userDataSection;
    }


    enum LabelType
    {
        h1,
        h2
    }
}
