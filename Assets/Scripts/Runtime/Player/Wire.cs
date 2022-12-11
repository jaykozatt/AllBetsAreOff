using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllBets
{
    public class Wire : StaticInstance<Wire>
    {
        /* This class controls how other objects interact 
         * with the wire. And serves as the supporting structure
         * that links the Player character with the Die.
        */
        
        #region References

            #region FMOD SFX
            [Header("SFX")]
                public FMODUnity.EventReference sliceSFX;
            #endregion
           
            EdgeCollider2D  hitbox;
        #endregion

        #region Variables & Switches
            Vector2 pivotPos;
            Vector2 diePos;
            Vector2 playerPos;
            public GameObject entangledEntity {get; private set;}
            public LineRenderer line {get; private set;}
            private List<Vector3> linePositions;
            List<Vector2> positions;
        #endregion

        #region Properties
            public bool IsEntangled {
                get => entangledEntity != null;
            }
        #endregion
        
        #region Monobehaviour Functions
            private void OnTriggerEnter2D(Collider2D other) 
            {
                if (DiceController.Instance.IsDeployed)
                {
                    Enemy enemy; // Enemies tagged as 'Wrap Immune' are sliced instead of entangled
                    if (other.TryGetComponent<Enemy>(out enemy) && (other.CompareTag("Wrap Immune") || IsEntangled))
                    {
                        int damage = Mathf.Max(1, enemy.numberOfChips / 2);
                        enemy.GetDamaged(damage);
                        
                        FMODUnity.RuntimeManager.PlayOneShot(sliceSFX);
                    }
                    else if (!IsEntangled)
                    {
                        TryEntangle(other);
                    }
                }
            }

            // Start is called before the first frame update
            void Start()
            {
                line = GetComponent<LineRenderer>();
                hitbox = GetComponent<EdgeCollider2D>();
                positions = new List<Vector2>();
                linePositions = new List<Vector3>();
            }

            // Update is called once per frame
            void Update()
            {
                pivotPos = DiceController.Instance.pivot.position - transform.position;
                diePos = DiceController.Instance.transform.position - transform.position;
                playerPos = PlayerController.Instance.transform.position - transform.position;

                positions.Clear();
                positions.Add(diePos);
                positions.Add(pivotPos);
                if (entangledEntity != null)
                    positions.Add(playerPos);

                hitbox.SetPoints(positions);
                
                DrawWire();
            }

        #endregion

        #region Core Functions
            public bool TryEntangle(Collider2D entity)
            {
                if (entangledEntity == null)
                {
                    entangledEntity = entity.gameObject;

                    DiceController.Instance.JoinTo(entity);
                    PlayerController.Instance.JoinTo(entity);

                    return true;
                }

                return false;
            }

            public bool TryDetangle(GameObject entity)
            {
                if (entangledEntity == entity)
                {
                    entangledEntity = null;

                    DiceController.Instance.ResetJoint();
                    PlayerController.Instance.ResetJoint();

                    return true;
                }

                return false;
            }

            public void DrawWire()
            {
                linePositions.Clear();
                
                linePositions.Add(PlayerController.Instance.transform.position);
                if (entangledEntity != null) linePositions.Add(entangledEntity.transform.position);
                linePositions.Add(DiceController.Instance.transform.position);

                line.positionCount = linePositions.Count;
                line.SetPositions(linePositions.ToArray());
            }
        #endregion
    }
}
