using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireController : StaticInstance<WireController>
{

    public LineRenderer line;
    public List<Vector3> linePositions;
    
    private DistanceJoint2D joint;
    private Rigidbody2D rb;
    public float angularVelocity = 1;

    PlayerController Player {
        get => PlayerController.Instance;
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
        BeginSwing();
    }

    // Update is called once per frame
    void Update()
    {
        SwingDie();
        DrawWire();
    }

    void BeginSwing()
    {
        Vector2 radius = (Player.transform.position - transform.position);
        Vector2 direction = -radius.normalized;
        (direction.x, direction.y) = (direction.y, -direction.x);

        rb.velocity = direction * radius.magnitude * angularVelocity;
    }

    void SwingDie()
    {
        Vector2 radius = (Player.transform.position - transform.position);
        Vector2 direction = -radius.normalized;
        (direction.x, direction.y) = (direction.y, -direction.x);

        
        rb.velocity = direction * radius.magnitude * angularVelocity;

        // rb.AddForce(direction * radius.magnitude * angularVelocity * angularVelocity);
    }

    void DrawWire()
    {
        linePositions.Clear();
        
        linePositions.Add(PlayerController.Instance.transform.position);
        linePositions.Add(transform.position);

        line.SetPositions(linePositions.ToArray());
    }
}
