using UnityEngine;

namespace ClusterVR.InternalSDK.Extensions
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class WorldSpaceCanvasScaler : MonoBehaviour, IWorldSpaceCanvasScaler
    {
        RectTransform rectTransform;

        RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                {
                    rectTransform = GetComponent<RectTransform>();
                }
                return rectTransform;
            }
        }

        public float PixelPerUnit
        {
            get { return 1 / RectTransform.localScale.x; }
            set
            {
                RectTransform.localScale = Vector3.one * Mathf.Max(1 / value, float.Epsilon);
            }
        }

        public Vector2 Size
        {
            get { return RectTransform.sizeDelta / PixelPerUnit; }
            set { RectTransform.sizeDelta = value * PixelPerUnit; }
        }
    }
}
