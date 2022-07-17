using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int numberOfChips = 1;
    public int scorePerChip = 100;

    public float speed = 5;
    public int minimumDistance = 1;

    public Rigidbody2D rb;
    // private WaitForSeconds moveTimer = new WaitForSeconds(5);
    private WaitForSeconds stunTimer = new WaitForSeconds(5);

    Coroutine followAI;

    private void OnDestroy() {

        if (numberOfChips > 1)
        {
            // Split in half

            GameManager.Instance.AddScore(scorePerChip * numberOfChips / 2);
        }
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(scorePerChip);
        }

        StopCoroutine(followAI);

    }

    private void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.mass = 50 * numberOfChips;
        rb.drag = 5;

        followAI = StartCoroutine(FollowAI(PlayerController.Instance.transform));
    }


    IEnumerator FollowAI(Transform target)
    {
        Vector2 direction;

        yield return stunTimer;

        while (true)
        {
            direction = (target.position - transform.position).normalized;

            rb.AddForce(rb.mass * direction * speed, ForceMode2D.Impulse);

            yield return new WaitForSeconds(Random.Range(3,5));
        }
    }
}
