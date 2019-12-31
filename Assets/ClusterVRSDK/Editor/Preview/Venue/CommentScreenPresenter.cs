using System;
using System.Collections;
using System.Collections.Generic;
using ClusterVR.InternalSDK.Core;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace ClusterVRSDK.Editor.Preview
{
    public class CommentScreenPresenter
    {
        List<ICommentScreenView> commentScreenViews;
        public CommentScreenPresenter(List<ICommentScreenView> commentScreenViews)
        {
            this.commentScreenViews = commentScreenViews;
        }

        public void SendComment(Comment comment)
        {
            foreach (var commentScreenView in commentScreenViews)
            {
                commentScreenView.AddComment(comment);
            }
        }

        public void SendCommentFromEditorUI(string displayName, string userName, string content)
        {
            var user = new User(displayName,userName,x => {});
            var comment = new Comment(user,content,false);
            SendComment(comment);
        }
    }
}


