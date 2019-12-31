using System.Collections;
using System.Collections.Generic;
using ClusterVR.InternalSDK.Core;
using UnityEngine;

namespace ClusterVRSDK.Editor.Preview
{
    public class MainScreenPresenter
    {
        List<IMainScreenView> mainScreenViews;

        public MainScreenPresenter(List<IMainScreenView> mainScreenViews)
        {
            this.mainScreenViews = mainScreenViews;
        }

        public void SetImage(Texture targetTexture)
        {
            foreach (var mainScreenView in mainScreenViews)
            {
                mainScreenView.UpdateContent(targetTexture);
            }
        }
    }
}
