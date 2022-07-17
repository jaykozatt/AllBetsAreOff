using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int numberOfChips = 1;
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
        }
    }

    private void OnDestroy()
    {
        StopCoroutine(followAI);
    }

    [ExecuteInEditMode]
    private void Awake() 
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        GetComponentsInChildren<SpriteRenderer>(stack);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.mass = massPerChip * numberOfChips;
        rb.drag = 5;

        followAI = StartCoroutine(FollowAI(PlayerController.Instance.transform));
    }

    public void GetDamaged(int chipsOfDamage)
    {
        if (numberOfChips > 1)
        {
            numberOfChips = Mathf.Max(0, numberOfChips - chipsOfDamage);
            stack[numberOfChips].gameObject.SetActive(false);
            GameManager.Instance.AddScore(chipsOfDamage * scorePerChip);
        }
        else
        {
            GameManager.Instance.AddScore(1 * scorePerChip);
            Destroy(gameObject);
        }
        
        ParticleSystem.EmissionModule emission = toppleFX.emission;
        ParticleSystem.Burst burst = emission.GetBurst(0);
        burst.count = chipsOfDamage;

        emission.SetBurst(0, burst);
        
        toppleFX.transform.position = stack[numberOfChips].transform.position;
        toppleFX.Play();
    }

    IEnumerator FollowAI(Transform target)
    {
        Vector2 direction;

        yield return stunTimer;

        while (GameManager.Instance.gameState != GameState.Ended)
        {
            if (GameManager.Instance.gameState == GameState.Paused)
            {
                yield return null;
            }
            else
            {
                direction = (target.position - transform.position).normalized;

                rb.AddForce(rb.mass * direction * speed, ForceMode2D.Impulse);

                yield return new WaitForSeconds(Random.Range(3,5));
            }
        }
    }
}
