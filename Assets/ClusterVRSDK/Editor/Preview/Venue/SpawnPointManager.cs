using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClusterVR.InternalSDK.Core;
using UnityEngine;
using Random = System.Random;

namespace ClusterVRSDK.Editor.Preview
{
    public class SpawnPointManager
    {
        private List<ISpawnPoint> spawnPointList;

        public SpawnPointManager(List<ISpawnPoint> spawnPointList)
        {
            this.spawnPointList = spawnPointList;
        }

        public void Respawn(PermissionType permissionType,Transform targetTransform)
        {
            var rnd = new Random();
            ISpawnPoint[] spawnCandidates;
            ISpawnPoint targetSpawnPoint;
            if (permissionType == PermissionType.Performer)
            {
                spawnCandidates = spawnPointList.Where(x => x.SpawnType == SpawnType.OnStage1 || x.SpawnType == SpawnType.OnStage2).ToArray();
                if (spawnCandidates.Length != 0)
                {
                    targetSpawnPoint = spawnCandidates[rnd.Next(spawnCandidates.Length)];
                    targetTransform.position = targetSpawnPoint.Position;
                    return;
                }
            }
            spawnCandidates = spawnPointList.Where(x => x.SpawnType == SpawnType.Entrance).ToArray();
            targetSpawnPoint = spawnCandidates[rnd.Next(spawnCandidates.Length)];
            targetTransform.position = targetSpawnPoint.Position;
        }
    }
}
