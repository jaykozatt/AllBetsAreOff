using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Enemy : MonoBehaviour
{
    public FMODUnity.EventReference tackleSFX;
    private FMOD.Studio.EventInstance tackleInstance;

    public FMODUnity.EventReference chipsSFX;
    private FMOD.Studio.EventInstance chipsInstance;

    public FMODUnity.EventReference slideSFX;
    private FMOD.Studio.EventInstance slideInstance;
    

    [HideInInspector] public int numberOfChips = 1;
    public int scorePerChip = 100;
    public int massPerChip = 2;
    public ParticleSystem toppleFX;
    public List<SpriteRenderer> stack;

    public float speed = 5;
    public int minimumDistance = 1;

    public Rigidbody2D rb;
    // private WaitForSeconds moveTimer = new WaitForSeconds(5);
    private WaitForSeconds stunTimer = new WaitForSeconds(5);

    Coroutine followAI;


    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.collider.CompareTag("Player"))
        {
            PlayerController.Instance.GetHurt();
            tackleInstance.start();
        }
    }

    private void OnDestroy()
    {
        StopCoroutine(followAI);
        tackleInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        chipsInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        slideInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void Awake() 
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        GetComponentsInChildren<SpriteRenderer>(stack);

        string particlesPath = "/Managers/ParticleSystems/";
        string topplePath = particlesPath + (CompareTag("Tough") ? "Purple Enemy" : "Blue Enemy");

        toppleFX = GameObject.Find(topplePath).GetComponent<ParticleSystem>();
    }

    // Start is called before the first frame update
    void Start()
    {
        numberOfChips = stack.Where(c => c.gameObject.activeInHierarchy).Count();
        rb.mass = massPerChip * numberOfChips;
        rb.drag = 5;

        tackleInstance = FMODUnity.RuntimeManager.CreateInstance(tackleSFX);
        chipsInstance = FMODUnity.RuntimeManager.CreateInstance(chipsSFX);
        slideInstance = FMODUnity.RuntimeManager.CreateInstance(slideSFX);

        followAI = StartCoroutine(FollowAI(PlayerController.Instance.transform));
    }

    public void GetDamaged(int chipsOfDamage)
    {
        ParticleSystem.EmissionModule emission = toppleFX.emission;
        ParticleSystem.Burst burst = emission.GetBurst(0);
        burst.count = chipsOfDamage;

        emission.SetBurst(0, burst);
        
        toppleFX.transform.position = stack[numberOfChips-chipsOfDamage].transform.position;
        toppleFX.Play();

        numberOfChips = Mathf.Max(0, numberOfChips - chipsOfDamage);
        stack[numberOfChips].gameObject.SetActive(false);
        GameManager.Instance.AddScore(chipsOfDamage * scorePerChip);

        chipsInstance.setParameterByName("Number of chips", chipsOfDamage);
        chipsInstance.start();
        
        if (numberOfChips < 1) Destroy(gameObject);
    }

    IEnumerator FollowAI(Transform target)
    {
        Vector2 direction;

        yield return stunTimer;

        while (GameManager.Instance.gameState < GameState.Ended)
        {
            if (GameManager.Instance.gameState == GameState.Paused)
            {
                yield return null;
            }
            else
            {
                direction = (target.position - transform.position).normalized;

                rb.AddForce(rb.mass * direction * speed, ForceMode2D.Impulse);
                slideInstance.start();

                yield return new WaitForSeconds(Random.Range(3,5));
            }
        }
    }
}
