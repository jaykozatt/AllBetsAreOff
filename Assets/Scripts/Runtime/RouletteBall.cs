using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AllBets
{
    public class RouletteBall : MonoBehaviour
    {
        #region Settings
        [Header("Settings")]
            public float speedThreshold = 15;
            public string movingLayerName = "Moving";
        #endregion

        #region FMOD SFX
        [Header("SFX")]
            [SerializeField] FMODUnity.EventReference impactSFX;
            [SerializeField] FMODUnity.EventReference chimeSFX;
        #endregion

        #region References
            public Rigidbody2D rb {get; private set;}
            public static List<RouletteBall> ballList;
        #endregion

        #region Variables & Switches
            private float speedSqrThreshold;
            private int defaultLayer;
            private int movingLayer;
        #endregion

        #region Properties
            public static bool AnyIsBouncing {
                get => ballList.Any(ball => ball.rb.velocity.sqrMagnitude > ball.speedSqrThreshold);
            }
        #endregion

        #region Monobehaviour Functions
            #region Events
                private void OnCollisionEnter2D(Collision2D other) 
                {
                    int tacklingLayer = LayerMask.NameToLayer("Player Tackling");
                    if (other.gameObject.layer == tacklingLayer || other.gameObject.layer == movingLayer)
                        gameObject.layer = movingLayer;

                    Enemy enemy;
                    if (other.gameObject.TryGetComponent<Enemy>(out enemy) && rb.velocity.sqrMagnitude > speedSqrThreshold)
                    {
                        enemy.GetDamaged(enemy.numberOfChips);
                        FMODUnity.RuntimeManager.PlayOneShot(chimeSFX, transform.position);
                    }
                    else
                    {
                        FMODUnity.RuntimeManager.PlayOneShot(impactSFX, transform.position);
                    }
                }

                private void OnDestroy() 
                {
                    ballList?.Remove(this);
                }
            #endregion

            private void Awake() 
            {
                if (ballList == null) ballList = new List<RouletteBall>();
                ballList.Add(this);

                defaultLayer = gameObject.layer;
                movingLayer = LayerMask.NameToLayer(movingLayerName);

                speedSqrThreshold = speedThreshold * speedThreshold;
            }

            private void Start() 
            {
                rb = GetComponent<Rigidbody2D>();
            }

            private void Update() 
            {
                if (rb.velocity.sqrMagnitude < speedSqrThreshold)
                    gameObject.layer = defaultLayer;
            }
        #endregion
    }
}
