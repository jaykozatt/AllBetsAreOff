using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace AllBets
{
    public class CameraController : StaticInstance<CameraController>
    {
        [System.Serializable]
        public struct Frame 
        {
            [SerializeField] int priority;
            public CinemachineVirtualCamera vcam;

            public void SetActive(bool enabled = true)
            {
                vcam.Priority =  enabled? priority : 0;
            }
        }

        #region Settings
        [Header("Settings")]
            public Frame frame1;
            public Frame frame2;
            public Frame frame3;
        #endregion

        Camera cam;
        EdgeCollider2D boundingCollider;
        List<Vector2> boundingPositions;
        Vector3[] viewportCorners;

        protected override void Awake() 
        {
            base.Awake();
            
            cam = Camera.main;
            boundingCollider = GetComponentInChildren<EdgeCollider2D>();
            CinemachineVirtualCamera[] vcams = GetComponentsInChildren<CinemachineVirtualCamera>();
            
            boundingPositions = new List<Vector2>(5);
            viewportCorners = new Vector3[5];
            viewportCorners[0] = new Vector3(1,1, cam.nearClipPlane);
            viewportCorners[1] = new Vector3(1,0, cam.nearClipPlane);
            viewportCorners[2] = new Vector3(0,0, cam.nearClipPlane);
            viewportCorners[3] = new Vector3(0,1, cam.nearClipPlane);
            viewportCorners[4] = new Vector3(1,1, cam.nearClipPlane);

            frame1.vcam = vcams[0];
            frame2.vcam = vcams[1];
            frame3.vcam = vcams[2];

            frame1.SetActive();
            frame2.SetActive(false);
            frame3.SetActive(false);
        }

        private void Update() 
        {
            // Enable frame3 whenever a roulette ball surpasses its speed threshold
            frame3.SetActive(
                RouletteBall.AnyIsBouncing
            );

            // Update the bounding box collider
            for (int i = 0; i< boundingPositions.Count; i++)
            {
                boundingPositions[i] = cam.ViewportToWorldPoint(viewportCorners[i]);
            }
            boundingCollider.SetPoints(boundingPositions);
        }

        public bool IsSeeing(Vector3 position)
        {
            Vector3 point = cam.WorldToViewportPoint(position);

            return point.x < 0 || point.x > 1 || point.y < 0 && point.y > 1;
        }
    }
}
