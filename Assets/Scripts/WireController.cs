using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireController : StaticInstance<WireController>
{

    public LineRenderer line;
    public List<Vector3> linePositions;
    
    private DistanceJoint2D joint;
    private Rigidbody2D rb;
    public float velocity = 1;
    public float lengthRate = 1;

    PlayerController Player {
        get => PlayerController.Instance;
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.collider.CompareTag("Player"))
        {
            gameObject.SetActive(false);
        }
    }

    protected override void Awake() 
    {
        base.Awake();

        line = GetComponent<LineRenderer>();
        linePositions = new List<Vector3>();

        joint = GetComponent<DistanceJoint2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // BeginSwing();
    }

    // Update is called once per frame
    void Update()
    {
        SwingDie();
        DrawWire();
    }

    // void BeginSwing()
    // {
    //     Vector2 radius = (Player.transform.position - transform.position);
    //     Vector2 direction = -radius.normalized;
    //     (direction.x, direction.y) = (direction.y, -direction.x);

    //     rb.velocity = direction * radius.magnitude * angularVelocity;
    // }

    void SwingDie()
    {
        Vector2 radius = (Player.transform.position - transform.position);
        Vector2 direction = -radius.normalized;
        (direction.x, direction.y) = (direction.y, -direction.x);

        
        // rb.velocity = (direction - radius).normalized * radius.magnitude * angularVelocity;

        rb.AddForce(rb.mass * (direction - radius).normalized / radius.magnitude * velocity * velocity);
    }

    void DrawWire()
    {
        linePositions.Clear();
        
        linePositions.Add(PlayerController.Instance.transform.position);
        linePositions.Add(transform.position);

        line.SetPositions(linePositions.ToArray());
    }

    public void LengthenWire()
    {
        joint.distance = Mathf.Min(8, joint.distance + lengthRate * Time.deltaTime) ;
    }

    public void ShortenWire() => joint.distance = Mathf.Max(0, joint.distance - lengthRate * Time.deltaTime);
}
