using System.Threading.Tasks;
using ClusterVRSDK.Core.Editor;
using ClusterVRSDK.Core.Editor.Venue;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClusterVRSDK.Editor
{
    public class TokenAuthWidget
    {
        public readonly Reactive<UserInfo?> reactiveUserInfo = new Reactive<UserInfo?>();
        readonly Reactive<string> reactiveMessage = new Reactive<string>();

        bool isLoggingIn;

        public VisualElement CreateView()
        {
            var container = new VisualElement();
            container.Add(new IMGUIContainer(() => EditorGUILayout.LabelField("APIアクセストークン", EditorStyles.boldLabel)));

            var accessToken = new TextField();
            accessToken.RegisterValueChangedCallback(ev =>
            {
                ValidateAndLogin(ev.newValue);
            });
            container.Add(accessToken);

            container.Add(
                new Button(() => Application.OpenURL(Constants.WebBaseUrl + "/app/my/tokens"))
                {
                    text = "トークンを入手"
                });

            var messageLabel = new Label();
            container.Add(messageLabel);
            ReactiveBinder.Bind(reactiveMessage, msg => { messageLabel.text = msg; });

            // TODO: 他のwindowでloginしたときにも自動で同期する
            if (!string.IsNullOrEmpty(EditorPrefsUtils.SavedAccessToken))
            {
                accessToken.value = EditorPrefsUtils.SavedAccessToken;
            }

            // 初期状態 or 既存のトークンをvalidateして何かのメッセージを出すのに必要
            ValidateAndLogin(EditorPrefsUtils.SavedAccessToken);
            return container;
        }

        public void Logout()
        {
            reactiveUserInfo.Val = null;
            EditorPrefsUtils.SavedAccessToken = null;
        }

        async Task ValidateAndLogin(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                reactiveUserInfo.Val = null;
                reactiveMessage.Val = "アクセストークンが必要です";
                return;
            }

            if (token.Length != 64)
            {
                reactiveUserInfo.Val = null;
                reactiveMessage.Val = "不正なアクセストークンです";
                return;
            }

            // Call auth API
            if (isLoggingIn)
            {
                return;
            }
            try
            {
                isLoggingIn = true;
                var user = await APIServiceClient.GetMyUser.Call(Empty.Value, token);

                if (string.IsNullOrEmpty(user.Username))
                {
                    reactiveMessage.Val = "認証に失敗しました";
                    return;
                }
                reactiveUserInfo.Val = new UserInfo(user.Username, token);
                reactiveMessage.Val = "Logged in as " + "\"" + user.Username + "\"";

                EditorPrefsUtils.SavedAccessToken = token;
            }
            finally
            {
                isLoggingIn = false;
            }
        }
    }
}
