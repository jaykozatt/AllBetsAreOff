using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Experimental.Rendering.Universal;

namespace AllBets
{
    public class Enemy : MonoBehaviour
    {

        #region Settings
        [Header("Settings")]
            public float impulseForce = 5; // The force applied to make it move on short bursts
            public int scorePerChip = 100; // Score value of each chip on the chipStack
            public Color lightColor = Color.yellow;
            [ColorUsage(false,true)] public Color alertColor; // A flash of color when about to attack

        #endregion

        #region References
            
            #region FMOD SFX
                [Header("SFX")]
                public FMODUnity.EventReference tackleSFX;
                private FMOD.Studio.EventInstance tackleInstance;

                public FMODUnity.EventReference chipsSFX;
                private FMOD.Studio.EventInstance chipsInstance;

                public FMODUnity.EventReference slideSFX;
                private FMOD.Studio.EventInstance slideInstance;
            #endregion

        [Header("References")]
            public TextPopup scorePopup;
            ParticleSystem toppleVFX;
            private Rigidbody2D rb;
            private SpriteRenderer lightSource;
            [SerializeField] List<SpriteRenderer> chipStack;
        #endregion

        #region Variables & Switches
            private int startingChips;
            public int numberOfChips {get; private set;}
            private bool isAttacking = false;
        #endregion

        #region Coroutine References
            Coroutine followAI;
        #endregion

        #region Coroutine Definitions
            IEnumerator FollowAI(Transform target)
            {
                Vector2 direction;

                WaitForSeconds warningTime = new WaitForSeconds(.6f);
                WaitForSeconds windupTime = new WaitForSeconds(.4f);
                while (GameManager.Instance.gameState < GameState.Ended)
                {
                    if (GameManager.Instance.gameState != GameState.Playing)
                    {
                        yield return null;
                    }
                    else
                    {
                        yield return new WaitForSeconds(Random.Range(2,5));

                        // Signal an incoming attack
                        lightSource.enabled = true;
                        foreach (SpriteRenderer sprite in chipStack)
                            sprite.material.SetInt("_EmissionEnabled", 1);
                        yield return warningTime;
                    
                        // Turn off signal, and prepare to attack
                        lightSource.enabled = false;
                        foreach (SpriteRenderer sprite in chipStack)
                            sprite.material.SetInt("_EmissionEnabled", 0);
                        yield return windupTime;

                        // if target is destroyed, check if game ended
                        if (target == null) continue;

                        // Halt movement for moment when a roulette ball starts bouncing
                        if (RouletteBall.AnyIsBouncing) 
                            yield return new WaitForSeconds(Random.Range(.1f,.3f));

                        direction = (target.position - transform.position).normalized;
                        rb.AddForce(rb.mass * direction * impulseForce, ForceMode2D.Impulse);
                        isAttacking = true;
                        slideInstance.start();
                    }
                }
            }
        #endregion

        #region Monobehaviour Functions
            private void OnCollisionEnter2D(Collision2D other) 
            {
                if (other.collider.CompareTag("Player") && isAttacking)
                {
                    PlayerController.Instance.GetHurt(-other.relativeVelocity);
                }
            }

            private void OnCollisionStay2D(Collision2D other) 
            {
                if (other.collider.CompareTag("Player") && isAttacking)
                {
                    PlayerController.Instance.GetHurt(-other.relativeVelocity);
                }
            }
            private void OnDisable() 
            {
                if (followAI != null) StopCoroutine(followAI);
                Wire.Instance?.TryDetangle(gameObject);
            }

            private void OnDestroy()
            {
                if (followAI != null) StopCoroutine(followAI);
                
                tackleInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                chipsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                slideInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

                tackleInstance.release();
                chipsInstance.release();
                slideInstance.release();

                if (DiceController.Instance != null)
                    Wire.Instance.TryDetangle(gameObject);
            }

            private void Awake() 
            {
                rb = GetComponentInChildren<Rigidbody2D>();
                lightSource = GetComponent<SpriteRenderer>();
                GetComponentsInChildren<SpriteRenderer>(chipStack);
                chipStack.Remove(lightSource);
                chipStack.RemoveAt(chipStack.Count-1);

                string particlesPath = "/Gameplay/Managers/ParticleSystems/";
                string topplePath = particlesPath + (CompareTag("Wrap Immune") ? "Blue Enemy" : "Purple Enemy");

                toppleVFX = GameObject.Find(topplePath).GetComponent<ParticleSystem>();
                startingChips = chipStack.Where(chip => chip.gameObject.activeInHierarchy).Count();
            }

            // Start is called before the first frame update
            void Start()
            {
                // Instance the FMOD SFXs
                tackleInstance = FMODUnity.RuntimeManager.CreateInstance(tackleSFX);
                chipsInstance = FMODUnity.RuntimeManager.CreateInstance(chipsSFX);
                slideInstance = FMODUnity.RuntimeManager.CreateInstance(slideSFX);
            }

            void Update() 
            {
                if (rb.velocity.sqrMagnitude <= 1) isAttacking = false;
                if (GameManager.Instance.gameState == GameState.Ended) StopCoroutine(followAI);
            }
        #endregion

        #region Core Functions
            public void Initialise(Vector3 position)
            {
                transform.position = position;

                // Enable the correct number of chips in the stack
                numberOfChips = startingChips;
                for (int i=0; i<numberOfChips; i++) {
                    chipStack[i].gameObject.SetActive(true);
                }

                // Set the emission color used before an attack
                lightSource.color = lightColor;
                foreach (SpriteRenderer sprite in chipStack) {
                    sprite.material.SetColor("_EmissionColor", alertColor);
                }

                // Restart its AI
                if (followAI != null) StopCoroutine(followAI); 
                followAI = StartCoroutine(FollowAI(PlayerController.Instance.transform));
            }

            public void GetDamaged(int chipsOfDamage)
            {
                // Instantiate a score popup
                TextPopup instance = Instantiate(scorePopup, transform.position, Quaternion.identity);
                
                // Get the VFX particle system used when toppling a stack
                ParticleSystem.EmissionModule emission = toppleVFX.emission;
                ParticleSystem.Burst burst = emission.GetBurst(0);

                // Set how many chips are gonna be lost visually
                burst.count = chipsOfDamage;
                emission.SetBurst(0, burst);
                
                // Compute leftover amount and set the origin position for the VFX, then play the effect
                int leftover = numberOfChips-chipsOfDamage;
                if (leftover < 0)
                    toppleVFX.transform.position = transform.position;
                else
                    toppleVFX.transform.position = chipStack[leftover].transform.position;
                toppleVFX.Play();

                // Clamp the leftover amount of chips 
                numberOfChips = Mathf.Max(0, leftover);
                chipStack[numberOfChips].gameObject.SetActive(false);
                
                // Add the score
                GameManager.Instance.AddScore(chipsOfDamage * scorePerChip, instance);

                // Play the toppling SFX according to the amount of chips toppled
                chipsInstance.setParameterByName("Number of chips", chipsOfDamage);
                chipsInstance.start();

                // If stack is defeated, increase score multiplier and return stack to the pool
                if (numberOfChips < 1) 
                {
                    GameManager.Instance.IncreaseCombo();
                    
                    StopCoroutine(followAI);
                    EnemyPool.Instance.Despawn(gameObject);

                    // Attempt to disentangle from wire if it was entangled.
                    Wire.Instance.TryDetangle(gameObject);
                }    
            }
        #endregion
        
    }
}
