using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;

namespace AllBets
{
    public class DiceController : StaticInstance<DiceController>
    {
        /* With support from the 'Wire' class,
         * this class controls all aspects regarding
         * the movement and interactions of the Die
        */

        #region Settings
        [Header("Settings")]
            public float angularVelocity = 1;
            public float lengthRate = 1;
        #endregion

        #region References
            
            #region FMOD SFX
            [Header("SFX")]
                [SerializeField] FMODUnity.EventReference impactSFX;
            #endregion

            public Transform pivot {get; private set;}
            public GameObject entangledEntity {get; private set;}
            private LineRenderer line;
            private List<Vector3> linePositions;
            
            private Rigidbody2D rb;
            public DistanceJoint2D joint {get; private set;}
            private new Collider2D collider;
            private new SpriteRenderer renderer;
            private Shadow shadow;
            
            private Sprite[] dieSprites;
            private ParticleSystem afterimage;
            private ParticleSystem.MainModule afterimageMain;
        #endregion

        #region Variables & Switches
            private int momentumFlipper = 1;
            private int dieNumber = 0;
            private bool isDeploying = false;
            private float originalDrag;
        #endregion

        #region Properties
            PlayerController Player {
                get => PlayerController.Instance;
            }
            public bool IsEntangled {
                get => entangledEntity != null;
            }
            public bool IsDeployed {
                get => this.enabled;
            }
            public int CurrentDieValue {
                get => dieNumber + 1;
            }
        #endregion
        
        #region Coroutine References
            Coroutine reelBack;
            Coroutine diceCycler;
        #endregion

        #region Coroutine Definitions
            IEnumerator ReelBackRoutine()
            {
                // Reel in until the die is picked up
                while (IsDeployed)
                {
                    joint.distance = Mathf.Max(0, joint.distance - lengthRate * 4 * Time.deltaTime);
                    if (rb.IsTouching(PlayerController.Instance.hitbox)) Pickup();

                    yield return null;
                }
            }

            IEnumerator DeployTimer()
            {
                isDeploying = true;
                yield return new WaitForSeconds(0.25f);
                isDeploying = false;
            }

            IEnumerator DiceCycler()
            {
                // Change the die's sprite every second
                while (true) 
                {
                    yield return new WaitForSeconds(1);

                    dieNumber = Random.Range(0, dieSprites.Length);
                    renderer.sprite = dieSprites[dieNumber];
                }
            }
        #endregion
        
        #region Monobehaviour Functions

            #region Events
                private void OnCollisionEnter2D(Collision2D other) 
                {
                    // If 'other' is the player, and the die is not currently being deployed or entangled
                    // if (other.collider.CompareTag("Player") && !isDeploying && !IsEntangled)
                    // {
                    //     Pickup();
                    // }
                    if (!other.collider.CompareTag("Player"))
                    {
                        // Make the die bounce away
                        // rb.velocity = other.relativeVelocity;

                        // If other is an enemy, knock a chip from the stack
                        Enemy enemy;
                        if (other.gameObject.TryGetComponent<Enemy>(out enemy))
                            enemy.GetDamaged(1);

                        // Play Impact SFX
                        FMODUnity.RuntimeManager.PlayOneShot(impactSFX);
                    }   
                }

                // private void OnCollisionStay2D(Collision2D other) 
                // {
                //     // If the die wasn't picked up on first contact, then it's picked up now on the next frame
                //     if (other.collider.CompareTag("Player") && !isDeploying && !IsEntangled)
                //     {
                //         Pickup();
                //     }
                // }

                private void OnDestroy() 
                {
                    if (diceCycler != null) StopCoroutine(diceCycler);
                    if (reelBack != null) StopCoroutine(reelBack);
                }
            #endregion

            protected override void Awake() 
            {
                base.Awake();

                line = GetComponent<LineRenderer>();
                linePositions = new List<Vector3>();
                dieSprites = Resources.LoadAll<Sprite>("Sprites/Dice");

                joint = GetComponentInParent<DistanceJoint2D>();
                rb = GetComponentInParent<Rigidbody2D>();
                collider = GetComponent<Collider2D>();
                renderer = GetComponent<SpriteRenderer>();
                afterimage = GetComponent<ParticleSystem>();
                afterimageMain = afterimage.main;

                shadow = transform.parent.GetComponentInChildren<Shadow>();
                shadow.caster = transform;

                originalDrag = rb.drag;
            }

            void Start()
            {
                diceCycler = StartCoroutine(DiceCycler());
                pivot = PlayerController.Instance.transform;
            }

            void Update()
            {
                CheckPivot();
                SwingDie();
                DrawWire();
                afterimageMain.startRotationZ = transform.localEulerAngles.z;
            }
        #endregion

        #region Core Functions
            void CheckPivot()
            {
                // Make sure that there's always a pivot to swing around of
                if (pivot == null || joint.connectedBody == null)
                {
                    pivot = PlayerController.Instance.transform;
                    joint.connectedBody = PlayerController.Instance.rb;
                }
            }

            void SwingDie() // Swings the die outwards
            {
                // Stop applying force if the die is in contact with something
                LayerMask mask = LayerMask.GetMask("Default");
                if (!collider.IsTouchingLayers(mask))
                {
                    // Set linear drag back to its initial value
                    rb.drag = originalDrag;

                    // While entangled, steadily reduce wire length to mimic a "wrap around" effect
                    if (IsEntangled) joint.distance = Mathf.Max(0, joint.distance - lengthRate * Time.deltaTime/2);
                    
                    // Compute radial vector (non-normalised and pointing inwards) 
                    // & centripetal acceleration magnitude
                    Vector2 radius = (pivot.position - transform.position);
                    float centripetalAccel = radius.magnitude * angularVelocity * angularVelocity;

                    // Get the direction that's tangential to the radius
                    Vector2 direction = -radius.normalized;
                    (direction.x, direction.y) = (direction.y, -direction.x);

                    // Figure out whether the die's moving clockwise or counter-clockwise
                    momentumFlipper = Vector3.Cross(rb.velocity,radius).z > 0 ? -1 : 1;

                    // Flip the direction according to current momentum. 
                    // Then add the outwards-pointing radial vector. 
                    // This is to cause the die to always move away from the centre
                    // as far as possible until the Joint component cancels it out. 
                    direction = (momentumFlipper * direction - radius).normalized;

                    // Finally add the resultant force
                    rb.AddForce(rb.mass * direction * centripetalAccel);
                }
                else
                {
                    rb.drag = 10; // Apply friction due to contact
                }
            }

            public void Pickup() 
            {
                // Hide everything related to the die and the wire
                this.enabled = false;
                renderer.enabled = false;
                collider.enabled = false;
                line.enabled = false;
                shadow.sprite.enabled = false;
                afterimage.Stop();

                joint.distance = 1;
                Detangle();
            } 

            void Deploy()
            {
                // Show everything related to the die and the wire
                this.enabled = true;
                renderer.enabled = true;
                collider.enabled = true;
                line.enabled = true;
                shadow.sprite.enabled = true;
                afterimage.Play();    

                // Set wire length to 3 units, and update its graphic
                joint.distance = 3;
                DrawWire();

                // Block further pickups for a set amount of time
                StartCoroutine(DeployTimer());
            }

            public void TangleWireTo(Collider2D other) 
            {
                // If no entity is entangled, entangle the 'other'
                if (entangledEntity == null)
                {
                    entangledEntity = other.gameObject;
                    pivot = entangledEntity.transform;
                    joint.connectedBody = other.attachedRigidbody;
                    joint.distance = ((Vector2)(transform.position - other.transform.position)).magnitude;
                }
            }

            public void Detangle()
            {
                // Reset pivot to player, and detangle die
                entangledEntity = null;
                pivot = Player.transform;
                joint.connectedBody = Player.rb;
            }

            public bool TryDetangle(GameObject entity)
            {
                // Try to detangle the given 'entity'
                if (entangledEntity == entity)
                {
                    entangledEntity = null;
                    pivot = Player.transform;
                    joint.connectedBody = Player.rb;
                    Pickup();

                    return true;
                }

                // If given 'entity' is not entangled, do nothing
                return false;
            }

            void DrawWire()
            {
                linePositions.Clear();
                
                linePositions.Add(PlayerController.Instance.transform.position);
                if (entangledEntity) linePositions.Add(entangledEntity.transform.position);
                linePositions.Add(transform.position);

                line.positionCount = linePositions.Count;
                line.SetPositions(linePositions.ToArray());
            }

            public void LengthenWire()
            {
                if (!IsDeployed) Deploy();
                else joint.distance = 
                    Mathf.Min(8, joint.distance + lengthRate * Time.deltaTime);
            }

            public void ShortenWire()
            {
                joint.distance = 
                    Mathf.Max(3, joint.distance - lengthRate * Time.deltaTime);
            }

            public void ReelBack()
            {
                reelBack = StartCoroutine(ReelBackRoutine());
            }
        #endregion

    }
}
