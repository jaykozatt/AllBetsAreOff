using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;

public class WireController : StaticInstance<WireController>
{
    public CinemachineVirtualCamera cam;

    public LineRenderer line;
    public List<Vector3> linePositions;
    
    public DistanceJoint2D joint;
    private Rigidbody2D rb;
    public float velocity = 1;
    public float lengthRate = 1;
    private Transform pivot;

    private List<Transform> pointsTangled;

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
        pointsTangled = new List<Transform>();

        joint = GetComponent<DistanceJoint2D>();
        rb = GetComponentInParent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // BeginSwing();
        pivot = PlayerController.Instance.transform;
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
        Vector2 radius = (pivot.position - transform.position);
        Vector2 direction = -radius.normalized;
        (direction.x, direction.y) = (direction.y, -direction.x);

        
        // rb.velocity = (direction - radius).normalized * radius.magnitude * angularVelocity;

        rb.AddForce(rb.mass * (direction - radius).normalized / radius.magnitude * velocity * velocity);
    }

    public void TangleWireTo(Collider2D other) 
    {
        if (!pointsTangled.Contains(other.transform))
        {
            pivot = other.transform;
            joint.connectedBody = other.attachedRigidbody;
            joint.distance = ((Vector2)(transform.position - other.transform.position)).magnitude;
            pointsTangled.Add(other.transform);
        }
    }

    void DrawWire()
    {
        linePositions.Clear();
        
        linePositions.Add(PlayerController.Instance.transform.position);
        linePositions.AddRange(pointsTangled.Select(p => p.position));
        linePositions.Add(transform.position);

        line.positionCount = linePositions.Count;
        line.SetPositions(linePositions.ToArray());
    }

    public void LengthenWire()
    {
        joint.distance = Mathf.Min(8, joint.distance + lengthRate * Time.deltaTime);
        
        if (cam.m_Lens.OrthographicSize < joint.distance) 
            cam.m_Lens.OrthographicSize = joint.distance;
    }

    public void ShortenWire()
    {
        joint.distance = Mathf.Max(0, joint.distance - lengthRate * Time.deltaTime);

        if (cam.m_Lens.OrthographicSize > 5 && cam.m_Lens.OrthographicSize > joint.distance)
            cam.m_Lens.OrthographicSize = Mathf.Max(5, joint.distance);
    } 
}
