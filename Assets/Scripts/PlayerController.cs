using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : StaticInstance<PlayerController>
{

    public float speed;
    Vector2 input;
    public Rigidbody2D rb;

    bool isReeling = false;
    List<Transform> targets;
    bool targetWasReached = false;

    Coroutine reelCrash;

    private void OnCollisionEnter2D(Collision2D other) 
    {
        int defaultLayer = LayerMask.NameToLayer("Default");
        if (isReeling && other.collider.gameObject.layer == defaultLayer)
        {
            targetWasReached = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        rb.AddForce(rb.mass * input * speed);

        if (!WireController.Instance.IsTangled)
        {
            if (Input.GetKey(KeyCode.Space)) WireController.Instance.LengthenWire();
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) WireController.Instance.ShortenWire();
        }
        else
        {
            if (!isReeling && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
            {
                targets = WireController.Instance.GetTangledPoints();
                isReeling = true;
                reelCrash = StartCoroutine(ReelCrash());
            }
        }
    }

    IEnumerator ReelCrash()
    {
        Vector2 attackVector;
        Transform target;
        while (targets.Count > 0)
        {
            target = targets[0];
            attackVector = (target.position - transform.position).normalized;

            rb.velocity = attackVector * speed * 2;
            
            yield return null;
            
            if (targetWasReached)
            {
                target = targets[0];

                // targets.RemoveAt(0);
                WireController.Instance.Detangle();
                Destroy(target.gameObject);
                
                targetWasReached = false;
            }
        }

        isReeling = false;
    }
}
