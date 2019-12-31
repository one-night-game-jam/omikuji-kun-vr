using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClusterVR.InternalSDK.Core.Gimmick;
using UnityEngine;


namespace ClusterVRSDK.Editor.Preview
{
    public class VenueGimmickManager : IVenueGimmickManager
    {
        List<GimmickData> gimmickDataList = new List<GimmickData>();

        public List<GimmickData> GimmickDataList => gimmickDataList;

        public void Register(string id, IVenueGimmick gimmick)
        {
            gimmickDataList.Add(new GimmickData(id, gimmick));
        }

        public void RunFromEditor(string id, float diff)
        {
            foreach (var gimmickData in gimmickDataList.Where(x => x.Id == id))
            {
                gimmickData.Gimmick.Run(diff);
            }
        }

        public void RunFromTriggerSender(string id, float diff, PermissionType permissionType)
        {
            if (id[0] != '@' && permissionType == PermissionType.Audience)
            {
                return;
            }
            foreach (var gimmickData in gimmickDataList.Where(x => x.Id == id))
            {
                gimmickData.Gimmick.Run(diff);
            }
        }
    }

    public struct GimmickData
    {
        string id;
        IVenueGimmick gimmick;

        public string Id => id;
        public IVenueGimmick Gimmick => gimmick;

        public GimmickData(string id, IVenueGimmick gimmick)
        {
            this.id = id;
            this.gimmick = gimmick;
        }
    }
}
