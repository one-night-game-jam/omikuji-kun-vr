using ClusterVRSDK.Editor.Venue;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClusterVRSDK.Editor
{
    public class VenueUploadWindow : EditorWindow
    {
        [MenuItem("clusterSDK/VenueUpload")]
        public static void Open()
        {
            var window = GetWindow<VenueUploadWindow>();
            window.titleContent = new GUIContent("cluster UploadVenueWindow");
        }

        void OnEnable()
        {
            var tokenAuth = new TokenAuthWidget();
            var tokenAuthView = tokenAuth.CreateView(); // .Bindで作り直すとなぜかYogaNodeがStackoverflowするので使い回す
            rootVisualElement.Add(tokenAuthView);

            VisualElement venueUi = null;
            ReactiveBinder.Bind(tokenAuth.reactiveUserInfo, userInfo =>
            {
                if (venueUi != null)
                {
                    rootVisualElement.Remove(venueUi);
                    venueUi = null;
                }

                if (userInfo.HasValue)
                {
                    venueUi = CreateVenueUi(tokenAuth, userInfo.Value);
                    rootVisualElement.Add(venueUi);

                    tokenAuthView.style.display = DisplayStyle.None;
                }
                else
                {
                    tokenAuthView.style.display = DisplayStyle.Flex;
                }
            });
        }

        VisualElement CreateVenueUi(TokenAuthWidget tokenAuth, UserInfo userInfo)
        {
            var container = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                }
            };
            var sidePane = new VisualElement()
            {
                style =
                {
                    borderColor = new StyleColor(Color.gray),
                    borderRightWidth = 1,
                    paddingRight = 4,
                }
            };
            var mainPane = new VisualElement()
            {
                style = {flexGrow = 1}
            };
            container.Add(sidePane);
            container.Add(mainPane);

            // Side
            var sideMenu = new SideMenuVenueList(userInfo);
            sideMenu.AddView(sidePane);
            ReactiveBinder.Bind(sideMenu.reactiveForceLogout, forceLogout =>
            {
                if (forceLogout)
                {
                    tokenAuth.Logout();
                }
            });

            // Main
            ReactiveBinder.Bind(sideMenu.reactiveCurrentVenue, currentVenue =>
            {
                mainPane.Clear();
                if (currentVenue != null)
                {
                    var venueContent = new ScrollView(ScrollViewMode.Vertical) {style = {flexGrow = 1}};
                    new EditAndUploadVenueView(userInfo, currentVenue, () =>
                    {
                        sideMenu.RefetchVenueWithoutChangingSelection();
                    }).AddView(venueContent);
                    mainPane.Add(venueContent);
                }
            });

            return container;
        }
    }
}
