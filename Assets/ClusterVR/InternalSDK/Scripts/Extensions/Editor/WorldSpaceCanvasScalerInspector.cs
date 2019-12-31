using UnityEditor;

namespace ClusterVR.InternalSDK.Extensions.Editor
{
    [CustomEditor(typeof(WorldSpaceCanvasScaler))]
    public class WorldSpaceCanvasScalerInspector : UnityEditor.Editor
    {
        WorldSpaceCanvasScaler canvasScaler;

        void OnEnable()
        {
            canvasScaler = (WorldSpaceCanvasScaler)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            var pixelPerUnit = EditorGUILayout.FloatField("Pixel Per Unit", canvasScaler.PixelPerUnit);
            var size = EditorGUILayout.Vector2Field("Size", canvasScaler.Size);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(canvasScaler, "Change world space canvas scaler");

                canvasScaler.PixelPerUnit = pixelPerUnit;
                canvasScaler.Size = size;
            }
        }
    }
}
