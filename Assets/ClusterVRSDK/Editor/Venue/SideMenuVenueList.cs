using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClusterVRSDK.Core.Editor.Venue;
using ClusterVRSDK.Core.Editor.Venue.Json;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClusterVRSDK.Editor.Venue
{
    public class SideMenuVenueList
    {
        public readonly Reactive<bool> reactiveForceLogout = new Reactive<bool>();
        public readonly Reactive<Core.Editor.Venue.Json.Venue> reactiveCurrentVenue = new Reactive<Core.Editor.Venue.Json.Venue>();

        readonly UserInfo userInfo;

        readonly Dictionary<GroupID, Venues> allVenues = new Dictionary<GroupID, Venues>();

        VisualElement selector;

        public SideMenuVenueList(UserInfo userInfo)
        {
            this.userInfo = userInfo;
        }

        public void AddView(VisualElement parent)
        {
            selector = new VisualElement() {style = {flexGrow = 1}};
            parent.Add(selector);
            RefreshVenueSelector();

            new PreviewVenueView(reactiveCurrentVenue).AddView(parent);
        }

        public void RefetchVenueWithoutChangingSelection()
        {
            var currentVenue = reactiveCurrentVenue.Val;
            if (currentVenue != null)
            {
                RefreshVenueSelector(currentVenue.Group.Id, currentVenue.VenueId);
            }
            else
            {
                RefreshVenueSelector();
            }
        }

        async Task RefreshVenueSelector(GroupID groupIdToSelect = null, VenueID venueIdToSelect = null)
        {
            selector.Clear();
            selector.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox("会場情報を取得しています...", MessageType.None)));

            VisualElement venuePicker = null;
            void RecreateVenuePicker(GroupID groupId)
            {
                if (venuePicker != null)
                {
                    selector.Remove(venuePicker);
                }

                venuePicker = CreateVenuePicker(groupId, allVenues[groupId], venueIdToSelect);
                selector.Add(venuePicker);
            }

            try
            {
                var groups = await APIServiceClient.GetGroups.Call(Empty.Value, userInfo.VerifiedToken, 3);
                foreach (var group in groups.List)
                {
                    allVenues[group.Id] = await APIServiceClient.GetGroupVenues.Call(group.Id, userInfo.VerifiedToken, 3);
                }

                selector.Clear();

                selector.Add(new Label("ユーザー"));
                var userSelector = new VisualElement(){style = {flexDirection = FlexDirection.Row, flexShrink = 0}};
                userSelector.Add(new Label(userInfo.Username));
                userSelector.Add(new Button(() => reactiveForceLogout.Val = true) {text = "切替"});
                selector.Add(userSelector);

                if (groups.List.Count == 0)
                {
                    selector.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox("clusterにてチーム登録をお願いいたします", MessageType.Warning)));
                }
                else
                {
                    selector.Add(new Label("所属チーム"));
                    var teamMenu = new PopupField<Group>(groups.List, 0, group => group.Name, group => group.Name);
                    teamMenu.RegisterValueChangedCallback(ev => RecreateVenuePicker(ev.newValue.Id));
                    selector.Add(teamMenu);

                    var groupToSelect = groups.List.Find(group => group.Id == groupIdToSelect) ?? groups.List[0];
                    teamMenu.SetValueWithoutNotify(groupToSelect);

                    selector.Add(UiUtils.Separator());
                    RecreateVenuePicker(groupToSelect.Id);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                selector.Clear();
                selector.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox($"会場情報の取得に失敗しました {e.Message}", MessageType.Error)));
            }
        }

        VisualElement CreateVenuePicker(GroupID groupId, Venues venues, VenueID venueIdToSelect = null)
        {
            var venueList = new ScrollView(ScrollViewMode.Vertical)
            {
                style = {marginTop = 8}
            };
            venueList.Add(new Button(() => CreateNewVenue(groupId))
            {
                text = "新規会場",
                style = {color = new StyleColor(new Color(0.5f, 1, 0.5f))}
            });

            foreach (var venue in venues.List.OrderBy(venue => venue.Name))
            {
                var venueButton = new Button(() => { reactiveCurrentVenue.Val = venue; })
                {
                    text = venue.Name,
                    style = {unityTextAlign = TextAnchor.MiddleLeft},
                };
                venueList.Add(venueButton);
            }

            reactiveCurrentVenue.Val = venues.List.Find(venue => venue.VenueId == venueIdToSelect);

            return venueList;
        }

        void CreateNewVenue(GroupID groupId)
        {
            var newVenuePayload = new PostNewVenuePayload
            {
                name = "NewVenue",
                description = "説明未設定",
                groupId = groupId.Value,
            };

            var postVenueService =
                new PostRegisterNewVenueService(
                    userInfo.VerifiedToken,
                    newVenuePayload,
                    venue =>
                    {
                        RefreshVenueSelector(groupId, venue.VenueId);
                        reactiveCurrentVenue.Val = venue;
                    },
                    exception =>
                    {
                        Debug.LogException(exception);
                        selector.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox($"新規会場の登録ができませんでした。{exception.Message}", MessageType.Error)));
                    });
            postVenueService.Run();
        }
    }
}
