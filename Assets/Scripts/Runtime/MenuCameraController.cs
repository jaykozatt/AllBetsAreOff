using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace AllBets
{
    public class MenuCameraController : StaticInstance<MenuCameraController> 
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
            public List<Frame> frames;
        #endregion   

        #region References
            Camera cam;
        #endregion

        public bool IsBlending 
        {
            get => cam.GetComponent<CinemachineBrain>().IsBlending;
        }

        public float BlendingTime
        {
            get => cam.GetComponent<CinemachineBrain>().m_DefaultBlend.BlendTime;
        }

        protected override void Awake()
        {
            base.Awake();
            cam = Camera.main;

            foreach (Frame frame in frames)
            {
                frame.SetActive(false);
            }

            frames[0].SetActive(true);
        }
    }
}