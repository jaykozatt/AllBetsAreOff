using System.Collections;
using UnityEngine;
using Cinemachine;

namespace AllBets
{
    public class PlayerController : StaticInstance<PlayerController>
    {
        #region Settings
        [Header("Settings")]
            public int lives = 3;
            public int invincibilityFrames = 10;
            public float movingSpeed = 90;
            public string tacklingLayerName = "Player Tackling";
        #endregion

        #region References
        [Header("References")]

            #region FMOD
                public FMODUnity.EventReference tackleSFX;
                private FMOD.Studio.EventInstance tackleInstance;
            #endregion

            [HideInInspector] public Rigidbody2D rb;
            public Collider2D hitbox {get; private set;}
            private CircleCollider2D hurtbox;
            public DistanceJoint2D joint {get; private set;}
            CinemachineFramingTransposer transposer;
            public Joystick joystick;
        #endregion

        #region Variables & Switches
            int framesUntilVulnerable = 0;
            Vector2 input;
            float lookaheadValue;
            bool isReeling = false;
            int normalLayer;
            int tacklingLayer;
        #endregion

        #region Coroutines References
            Coroutine reelCrash;
        #endregion

        #region Couroutine Definitions
            
            IEnumerator ReelCrash() // Sequence that makes the character tackle an entangled entity
            {
                // Get the target that will be tackled
                Transform target = Wire.Instance.entangledEntity.transform;
                
                transposer.m_LookaheadTime = 0;
                hurtbox.enabled = true;
                gameObject.layer = tacklingLayer;
                yield return new WaitForFixedUpdate();

                // While still entangled, move towards target
                Vector2 attackVector;
                while (Wire.Instance.IsEntangled)
                {
                    attackVector = (target.position - transform.position).normalized;
                    rb.AddForce(rb.mass * attackVector * movingSpeed/2, ForceMode2D.Impulse);
                    
                    yield return new WaitForFixedUpdate();
                }

                DiceController.Instance.Pickup();

                // Expand the hurtbox 1.5x its normal range
                // to mimic a shockwave after target impact
                float radius = hurtbox.radius;
                hurtbox.radius = 1.5f* radius;
                yield return null;

                // Until the character slows down past a threshold
                while (rb.velocity.magnitude > 10)
                {
                    yield return null;
                }

                // Set all values back to normal
                isReeling = false;
                transposer.m_LookaheadTime = lookaheadValue;
                hurtbox.radius = radius;
                hurtbox.enabled = false;
                gameObject.layer = normalLayer;

                // Give the player a grace period to get their bearings back
                framesUntilVulnerable = 20;
                
                yield return new WaitForFixedUpdate();
            }
        #endregion

        #region Monobehaviour Functions
            private void OnCollisionEnter2D(Collision2D other) 
            {
                if (isReeling)
                {
                    Wire.Instance.TryDetangle(other.gameObject);
                }
            }

            private void OnCollisionStay2D(Collision2D other) 
            {
                if (isReeling)
                {
                    Wire.Instance.TryDetangle(other.gameObject);
                }
            }
            private void OnTriggerEnter2D(Collider2D other) 
            {
                if (isReeling)
                {
                    tackleInstance.start(); // Play tackle SFX

                    Enemy enemy; // Damage the 'other' if it's an enemy
                    if (other.TryGetComponent<Enemy>(out enemy))
                        enemy.GetDamaged(enemy.numberOfChips);
                }
            }

            private void OnTriggerStay2D(Collider2D other) 
            {
                if (isReeling)
                {
                    tackleInstance.start(); // Play tackle SFX

                    Enemy enemy; // Damage the 'other' if it's an enemy
                    if (other.TryGetComponent<Enemy>(out enemy))
                        enemy.GetDamaged(enemy.numberOfChips);
                }
            }

            private void OnDestroy() 
            {
                if (reelCrash != null) StopCoroutine(reelCrash);
                tackleInstance.release();
            }

            protected override void Awake() 
            {
                base.Awake();

                rb = GetComponent<Rigidbody2D>();
                hitbox = GetComponent<Collider2D>();
                hurtbox = GetComponentsInChildren<CircleCollider2D>()[1];
                joint = GetComponent<DistanceJoint2D>();
            }

            void Start()
            {
                joint.distance = DiceController.Instance.maxWireLength;

                normalLayer = gameObject.layer;
                tacklingLayer = LayerMask.NameToLayer(tacklingLayerName);

                // Cinemachine References
                CinemachineVirtualCamera vcam1 = CameraController.Instance.frame1.vcam;
                transposer = vcam1.GetCinemachineComponent<CinemachineFramingTransposer>();
                lookaheadValue = transposer.m_LookaheadTime;

                // Create the FMOD SFX instance
                tackleInstance = FMODUnity.RuntimeManager.CreateInstance(tackleSFX);
            }

            void Update()
            {
                ProcessInput();

                // Tick invincibility freames        
                framesUntilVulnerable = Mathf.Max(0, framesUntilVulnerable - 1);
            }

            private void FixedUpdate() 
            {
                // Move the player character according to input
                rb.AddForce(rb.mass * input * movingSpeed);
            }
        #endregion

        #region Core Functions
            void ProcessInput()
            {
                if(Input.GetKeyDown(KeyCode.Escape))
                    switch (GameManager.Instance.gameState)
                    {
                        case GameState.Playing:
                            GameManager.Instance.PauseGame();
                            break;
                        case GameState.Paused:
                            GameManager.Instance.ResumeGame();
                            break;
                        default: break;
                    }

                if (GameManager.Instance.gameState == GameState.Playing)
                {
                    #if UNITY_STANDALONE || UNITY_EDITOR
                        input.x = Input.GetAxis("Horizontal");
                        input.y = Input.GetAxis("Vertical");
                        
                        if (Input.GetKey(KeyCode.Space)) 
                            OnLengthenKeyPress();
                        
                        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                            OnShortenKeyPress();
                    #endif

                    #if UNITY_ANDROID || UNITY_IOS
                        input = joystick.Direction;
                    #endif
                }
            }

            public void GetHurt()
            {
                // Unless currently invulnerable or reeling into a tackle,
                // decrease the lives' counter
                if (framesUntilVulnerable <= 0 && !isReeling)
                {
                    lives = Mathf.Max(0, lives - 1);
                    framesUntilVulnerable = invincibilityFrames;
                    
                    LivesInterface.Instance.UpdateDisplay(lives);
                    LivesInterface.Instance.HurtFlash();
                    tackleInstance.start();

                    // If lives reach 0, lose the game
                    if (lives <= 0) 
                    {
                        GameManager.Instance.LoseGame();
                        Destroy(transform.parent.gameObject);
                    }
                }
            }

            public void JoinTo(Collider2D other) 
            {
                joint.connectedBody = other.attachedRigidbody;
            }

            public void ResetJoint()
            {
                joint.connectedBody = DiceController.Instance.rb;
            }
        #endregion

        #region Event Functions
            public void OnShortenKeyPress() 
            {
                if (!Wire.Instance.IsEntangled)
                {
                    float wireLength = DiceController.Instance.joint.distance;
                    if (wireLength > 3) 
                        DiceController.Instance.ShortenWire();
                    else if (wireLength <= 3)
                        DiceController.Instance.ReelBack();
                }
                else
                {
                    if (!isReeling)
                    {
                        isReeling = true;
                        reelCrash = StartCoroutine(ReelCrash());
                    }
                }
            }

            public void OnLengthenKeyPress()
            {
                if (!Wire.Instance.IsEntangled)
                    DiceController.Instance.LengthenWire();
            }
        #endregion

    }
}