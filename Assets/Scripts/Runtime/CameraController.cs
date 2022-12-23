using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

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
            public float camShakeIntensity=2f;
            public float freezeDuration=1f;
            public Frame frame1;
            public Frame frame2;
            public Frame frame3;
            public Frame frame4;
        #endregion

        Camera cam;
        CinemachineImpulseSource impulseSource;
        EdgeCollider2D boundingCollider;
        List<Vector2> boundingPositions;
        Vector3[] viewportCorners;

        private void Start() 
        {
            cam = Camera.main;
            boundingCollider = cam.GetComponent<EdgeCollider2D>();
            impulseSource = GetComponent<CinemachineImpulseSource>();
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
            frame4.vcam = vcams[3];

            frame1.SetActive(true);
            frame2.SetActive(false);
            frame3.SetActive(false);
            frame4.SetActive(false);
            
        }

        private void Update() 
        {
            if (Wire.Instance != null)
                frame2.SetActive(
                    Wire.Instance.IsEntangled
                );

            // Enable frame3 whenever a roulette ball surpasses its speed threshold
            if (RouletteBall.ballList.Count > 0)
                frame3.SetActive(
                    RouletteBall.AnyIsBouncing
                );

            // Update the viewport bounds box collider
            for (int i = 0; i< boundingPositions.Count; i++)
            {
                boundingPositions[i] = cam.ViewportToWorldPoint(viewportCorners[i]);
            }
            boundingCollider.SetPoints(boundingPositions);
        }

        public bool IsWatching(Vector3 position)
        {
            Vector3 point = cam.WorldToViewportPoint(position);

            return point.x < 0 || point.x > 1 || point.y < 0 && point.y > 1;
        }

        public void Shake()
        {
            impulseSource.GenerateImpulse(camShakeIntensity*5f);
        }

        public void ImpactFrom(Vector3 position, Vector3 velocity)
        {
            impulseSource.GenerateImpulseAt(position, camShakeIntensity*velocity);
        }

        public void FreezeShake()
        {
            Time.timeScale = 0;

            frame4.SetActive();
            Shake();

            DOTween.To(
                ()=>Time.timeScale,
                (value)=>Time.timeScale=value,
                1, freezeDuration
            )
                .SetEase(Ease.InExpo)
                .SetUpdate(true)
                .OnComplete(()=>frame4.SetActive(false))
            ;
        }
    }
}
