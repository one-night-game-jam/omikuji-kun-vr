using System.Collections;
using System.Collections.Generic;
using ClusterVR.InternalSDK.Core;
using UnityEngine;
using System.Threading.Tasks;

namespace ClusterVRSDK.Editor.Preview
{
    public class AvatarRespawner
    {
        readonly float despawnHeight;
        readonly PlayerPresenter playerPresenter;

        public AvatarRespawner(float despawnHeight, PlayerPresenter playerPresenter)
        {
            this.despawnHeight = despawnHeight;
            this.playerPresenter = playerPresenter;
            CheckHeight();
        }

        async void CheckHeight()
        {
            while (playerPresenter.DesktopPlayerController != null)
            {
                if (playerPresenter.playerTransform.position.y < despawnHeight)
                {
                    Bootstrap.SpawnPointManager.Respawn(playerPresenter.PermissionType, playerPresenter.playerTransform);
                }
                await Task.Delay(300);
            }
        }

    }

}
