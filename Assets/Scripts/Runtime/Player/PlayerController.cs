using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

namespace AllBets
{
    public class PlayerController : StaticInstance<PlayerController>
    {
        #region Settings
        [Header("Settings")]
            public int lives = 3;
            public int invincibilityFrames = 10;
            public float movingSpeed = 90;
        #endregion

        #region References
        [Header("References")]

            #region FMOD
                public FMODUnity.EventReference tackleSFX;
                private FMOD.Studio.EventInstance tackleInstance;
            #endregion

            [HideInInspector] public Rigidbody2D rb;
            private CircleCollider2D hurtbox;
            CinemachineFramingTransposer transposer;
            public Joystick joystick;
        #endregion

        #region Variables & Switches
            private int framesUntilVulnerable = 0;
            Vector2 input;
            float lookaheadValue;
            bool isReeling = false;
        #endregion

        #region Coroutines References
            Coroutine reelCrash;
        #endregion

        #region Couroutine Definitions
            
            IEnumerator ReelCrash() // Sequence that makes the character tackle an entangled entity
            {
                // Get the target that will be tackled
                Transform target = DiceController.Instance.entangledEntity.transform;
                
                transposer.m_LookaheadTime = 0;
                hurtbox.enabled = true;

                // While still entangled, move towards target
                Vector2 attackVector;
                while (DiceController.Instance.IsEntangled)
                {
                    attackVector = (target.position - transform.position).normalized;
                    rb.velocity = attackVector * movingSpeed * 2;
                    
                    yield return null;
                }

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

                // Give the player a grace period to get their bearings back
                framesUntilVulnerable = 20;
            }
        #endregion

        #region Monobehaviour Functions
            private void OnTriggerEnter2D(Collider2D other) 
            {
                if (isReeling)
                {
                    tackleInstance.start(); // Play tackle SFX
                    
                    // If the 'other' is entangled, then detangle.
                    DiceController.Instance.TryDetangle(other.gameObject);

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

            void Start()
            {
                rb = GetComponent<Rigidbody2D>();
                hurtbox = GetComponentsInChildren<CircleCollider2D>()[1];
                tackleInstance = FMODUnity.RuntimeManager.CreateInstance(tackleSFX);

                // Cinemachine References
                CinemachineVirtualCamera vcam1 = CameraController.Instance.frame1.vcam;
                transposer = vcam1.GetCinemachineComponent<CinemachineFramingTransposer>();
                lookaheadValue = transposer.m_LookaheadTime;
            }

            void Update()
            {
                ProcessInput();

                // Tick invincibility freames        
                framesUntilVulnerable = Mathf.Max(0, framesUntilVulnerable - 1);

                // Switch to wider frame of view when entangled
                CameraController.Instance.frame2.SetActive(DiceController.Instance.IsEntangled);
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
        #endregion

        #region Event Functions
            public void OnShortenKeyPress() 
            {
                if (!DiceController.Instance.IsEntangled)
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
                if (!DiceController.Instance.IsEntangled)
                    DiceController.Instance.LengthenWire();
            }
        #endregion

    }
}