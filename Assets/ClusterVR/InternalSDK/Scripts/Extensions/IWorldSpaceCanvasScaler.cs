using UnityEngine;

namespace ClusterVR.InternalSDK.Extensions
{
    public interface IWorldSpaceCanvasScaler
    {
        float PixelPerUnit { get; set; }
        Vector2 Size { get; set; }
    }
}
