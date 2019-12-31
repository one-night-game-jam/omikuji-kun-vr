using System;
using System.Collections;
using System.Collections.Generic;
using ClusterVRSDK.Preview;
using UnityEditor;
using UnityEngine;

namespace ClusterVRSDK.Editor.Preview
{
    public class PlayerPresenter
    {
        const string NonVRPrefabPath = "Assets/ClusterVRSDK/Editor/Preview/Prefabs/PreviewOnly.prefab";
        GameObject previewOnly;
        DesktopPlayerController desktopPlayerController;

        Transform cameraTransform;
        public Transform playerTransform => desktopPlayerController.transform;

        public Transform CameraTransform => cameraTransform;

        public DesktopPlayerController DesktopPlayerController => desktopPlayerController;

        PermissionType permissionType;
        EnterDeviceType enterDeviceType;
        public PermissionType PermissionType => permissionType;

        public PlayerPresenter(PermissionType permissionType, EnterDeviceType enterDeviceType)
        {
            //TODO 引数のenumをPlayerのGameObjectに渡しつつシーンに生成
            this.permissionType = permissionType;
            this.enterDeviceType = enterDeviceType;
            var previewOnlyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NonVRPrefabPath);
            previewOnly = PrefabUtility.InstantiatePrefab(previewOnlyPrefab) as GameObject;
            desktopPlayerController = previewOnly.GetComponentInChildren<DesktopPlayerController>();
            cameraTransform = previewOnly.GetComponentInChildren<Camera>().transform;

            //Permissionに応じた初期位置にスポーンする
            Bootstrap.SpawnPointManager.Respawn(permissionType, desktopPlayerController.transform);
        }

        public void ChangePermissionType(PermissionType permissionType)
        {
            this.permissionType = permissionType;
            ChangeLayer(permissionType);
        }

        void ChangeLayer(PermissionType permissionType)
        {
            switch (permissionType)
            {
                case PermissionType.Performer:
                    desktopPlayerController.gameObject.layer = LayerMask.NameToLayer("Performer");
                    break;
                case PermissionType.Audience:
                    desktopPlayerController.gameObject.layer = LayerMask.NameToLayer("Audience");
                    break;
            }

        }

    }

    public enum PermissionType
    {
        //TODO 必要な役職を列挙
        Performer,
        Audience
    }

    public enum EnterDeviceType
    {
        Desktop,
        VR
    }
}
