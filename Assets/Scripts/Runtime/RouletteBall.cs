using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AllBets
{
    public class RouletteBall : MonoBehaviour
    {
        #region FMOD SFX
        [Header("SFX")]
            [SerializeField] FMODUnity.EventReference impactSFX;
            [SerializeField] FMODUnity.EventReference chimeSFX;
        #endregion

        public float speedSqrThreshold = 100;
        public string defaultLayerName = "Default";
        private int defaultLayer;
        public string movingLayerName = "Moving";
        private int movingLayer;
        public Rigidbody2D rb;

        public static List<RouletteBall> balls;

        public static bool AnyIsBouncing {
            get => balls.Any(ball => ball.rb.velocity.sqrMagnitude > ball.speedSqrThreshold);
        }

        private void OnCollisionEnter2D(Collision2D other) 
        {
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
            balls?.Remove(this);
        }

        private void Awake() 
        {
            if (balls == null) balls = new List<RouletteBall>();
            balls.Add(this);

            defaultLayer = LayerMask.NameToLayer(defaultLayerName);
            movingLayer = LayerMask.NameToLayer(movingLayerName);
        }

        private void Start() 
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update() 
        {
            if (rb.velocity.sqrMagnitude > speedSqrThreshold)
                gameObject.layer = movingLayer;
            else
                gameObject.layer = defaultLayer;
        }
    }
}
