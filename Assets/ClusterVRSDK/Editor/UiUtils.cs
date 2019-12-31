using UnityEngine;
using UnityEngine.UIElements;

namespace ClusterVRSDK.Editor
{
    public class UiUtils
    {
        public static VisualElement Separator()
        {
            return new VisualElement()
            {
                style =
                {
                    borderBottomWidth = 2,
                    borderColor = new StyleColor(Color.gray),
                    marginTop = 8,
                    marginBottom = 8,
                }
            };
        }
    }
}
