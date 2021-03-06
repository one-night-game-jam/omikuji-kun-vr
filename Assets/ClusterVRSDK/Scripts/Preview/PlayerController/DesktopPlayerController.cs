﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClusterVRSDK.Preview
{
    //プレビュー時にインスタンス生成されるプレイヤーキャラを操作するためのComponentです。PreviewOnly内PlayerControllerにアタッチされています。
    public class DesktopPlayerController : MonoBehaviour
    {
        [SerializeField] Transform cameraTransform;
        [SerializeField] CharacterController characterController;
        [SerializeField] DesktopMouseDragListener desktopMouseDragListener;
        [SerializeField] float moveSpeed;
        float fallingSpeed;

        private void Start()
        {
            desktopMouseDragListener.OnMouseDrag += Rotate;
        }

        private void Update()
        {
            var x = Input.GetAxisRaw("Horizontal");
            var z = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(x, 0, z);
            direction.Normalize();
            direction = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * direction;
            var velocity = direction * moveSpeed;
            if (characterController.isGrounded)
            {
                fallingSpeed = 0;
            }
            else
            {
                fallingSpeed += Time.deltaTime * 9.81f;
            }

            velocity.y = -fallingSpeed;
            characterController.Move(velocity * Time.deltaTime);
        }

        public void Rotate(Vector2 delta)
        {
            var euler = cameraTransform.eulerAngles;
            delta *= 120;
            euler = new Vector3(ClampAngle(euler.x - delta.y, -80, 80), euler.y + delta.x, 0);
            cameraTransform.rotation = Quaternion.Euler(euler);
        }

        static float ClampAngle(float angle, float min, float max)
        {
            angle += 180;
            angle = Mathf.Repeat(angle, 360);
            angle -= 180;
            angle = Mathf.Clamp(angle, min, max);

            return angle;
        }
    }
}
