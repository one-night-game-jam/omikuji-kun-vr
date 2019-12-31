using System;
using System.Collections.Generic;
using System.Linq;
using ClusterVR.InternalSDK.Core;
using ClusterVR.InternalSDK.Core.Gimmick;
using UnityEditor;
using UnityEngine;


namespace ClusterVRSDK.Editor.Preview
{
    [InitializeOnLoad]
    public static class Bootstrap
    {
        static RankingScreenPresenter rankingScreenPresenter;

        public static RankingScreenPresenter RankingScreenPresenter => rankingScreenPresenter;
        static CommentScreenPresenter commentScreenPresenter;
        public static CommentScreenPresenter CommentScreenPresenter => commentScreenPresenter;

        static VenueGimmickManager venueGimmickManager;

        public static VenueGimmickManager VenueGimmickManager => venueGimmickManager;

        static MainScreenPresenter mainScreenPresenter;

        public static MainScreenPresenter MainScreenPresenter => mainScreenPresenter;

        static SpawnPointManager spawnPointManager;

        public static SpawnPointManager SpawnPointManager => spawnPointManager;

        static PlayerPresenter playerPresenter;

        public static PlayerPresenter PlayerPresenter => playerPresenter;
        public static Action<List<string>> OnUpdateTriggerIdList;
        static AvatarRespawner avatarRespawner;

        static Bootstrap()
        {
            EditorApplication.playModeStateChanged += OnChangePlayMode;
        }

        static void OnChangePlayMode(PlayModeStateChange playMode)
        {
            switch (playMode)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    PreviewControlWindow.SetIsInGameMode(false);
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    PreviewControlWindow.SetIsInGameMode(true);
                    var commentScreenViews = new List<ICommentScreenView>();
                    var mainScreenViews = new List<IMainScreenView>();
                    var rankingScreenViews = new List<IRankingScreenView>();
                    var spawnPoints = new List<ISpawnPoint>();
                    var despawnHeight = float.MinValue;
                    var triggerSenders = new List<ITriggerSender>();

                    foreach (var binding in Resources.FindObjectsOfTypeAll<SdkBindingBase>()
                        .Where(x => x.gameObject.scene.isLoaded))
                    {
                        switch (binding)
                        {
                            case ICommentScreenView commentScreenView:
                                commentScreenViews.Add(commentScreenView);
                                break;
                            case IRankingScreenView rankingScreenView:
                                rankingScreenViews.Add(rankingScreenView);
                                break;
                            case IMainScreenView mainScreenView:
                                mainScreenViews.Add(mainScreenView);
                                break;
                            case ISpawnPoint spawnPoint:
                                spawnPoints.Add(spawnPoint);
                                break;
                            case IWarpPortal warpPortal:
                                warpPortal.OnEnterWarpPortalEvent += (sender, e) =>
                                {
                                    if (!e.KeepPosition)
                                    {
                                        playerPresenter.DesktopPlayerController.transform.position = e.ToPosition;
                                    }

                                    if (!e.KeepRotation)
                                    {
                                        playerPresenter.CameraTransform.rotation = e.ToRotation;
                                    }
                                };
                                break;
                            case IDespawnHeight _despawnHeight:
                                despawnHeight = _despawnHeight.Height;
                                break;
                            case ITriggerSender triggerSender:
                                triggerSender.TriggerEvent += (sender, args) => venueGimmickManager.RunFromTriggerSender(args.Id,0,playerPresenter.PermissionType);
                                break;
                        }
                    }


                    venueGimmickManager = new VenueGimmickManager();

                    foreach (var venueGimmick in Resources.FindObjectsOfTypeAll<VenueGimmickBase>()
                        .Where(x => x.gameObject.scene.isLoaded))
                    {
                        venueGimmick.Initialize(venueGimmickManager);
                    }

                    rankingScreenPresenter = new RankingScreenPresenter(rankingScreenViews);
                    commentScreenPresenter = new CommentScreenPresenter(commentScreenViews);
                    mainScreenPresenter = new MainScreenPresenter(mainScreenViews);
                    spawnPointManager = new SpawnPointManager(spawnPoints);
                    //疑似Playerの設定

                    playerPresenter = new PlayerPresenter(PermissionType.Audience, EnterDeviceType.Desktop);
                    avatarRespawner = new AvatarRespawner(despawnHeight, playerPresenter);

                    rankingScreenPresenter.SetRanking(10);
                    OnUpdateTriggerIdList?.Invoke(venueGimmickManager.GimmickDataList.Select(x => x.Id).ToList());
                    break;
            }
        }
    }
}
