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
    private new Collider2D collider;
    private new SpriteRenderer renderer;
    public float velocity = 1;
    public float lengthRate = 1;
    private Transform pivot;
    private bool isTangled = false;
    private bool isDeploying = false;
    private int flipper = 1;

    private List<Transform> pointsTangled;
    Coroutine reelBack;

    PlayerController Player {
        get => PlayerController.Instance;
    }
    public bool IsTangled {
        get => isTangled;
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.collider.CompareTag("Player") && !isDeploying && !isTangled)
        {
            this.enabled = false;
        }
        else if (!other.collider.CompareTag("Player"))
        {
            flipper = flipper == 1 ? -1 : 1;
            rb.velocity = other.relativeVelocity;
        }
        
    }

    private void OnCollisionStay2D(Collision2D other) 
    {
        if (other.collider.CompareTag("Player") && !isDeploying && !isTangled)
        {
            this.enabled = false;
        }
    }
    private void OnEnable() {
        renderer.enabled = true;
        collider.enabled = true;
        line.enabled = true;    
    }

    private void OnDisable() {
        renderer.enabled = false;
        collider.enabled = false;
        line.enabled = false;
    }

    protected override void Awake() 
    {
        base.Awake();

        line = GetComponent<LineRenderer>();
        linePositions = new List<Vector3>();
        pointsTangled = new List<Transform>();

        joint = GetComponent<DistanceJoint2D>();
        rb = GetComponentInParent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        renderer = GetComponent<SpriteRenderer>();
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
        LayerMask mask = LayerMask.GetMask("Default");
        if (!collider.IsTouchingLayers(mask))
        {
            if (IsTangled) joint.distance = Mathf.Max(0, joint.distance - lengthRate * Time.deltaTime/2);
            
            Vector2 radius = (pivot.position - transform.position);
            Vector2 direction = -radius.normalized;
            (direction.x, direction.y) = (direction.y, -direction.x);

            rb.AddForce(rb.mass * (flipper*direction - radius).normalized / radius.magnitude * velocity * velocity);
        }
    }

    public void TangleWireTo(Collider2D other) 
    {
        if (!pointsTangled.Contains(other.transform))
        {
            pivot = other.transform;
            joint.connectedBody = other.attachedRigidbody;
            joint.distance = ((Vector2)(transform.position - other.transform.position)).magnitude;
            pointsTangled.Add(other.transform);
            isTangled = true;
        }
    }

    public void Detangle()
    {
        pointsTangled.RemoveAt(0);
        if (pointsTangled.Count <= 0) 
        {
            pivot = Player.transform;
            joint.connectedBody = Player.rb;
            isTangled = false;
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
        if (!this.enabled)
        {
            joint.distance = 3;
            this.enabled = true;
            isDeploying = true;
            StartCoroutine(DeployTimer());
        }
        else
        {
            joint.distance = Mathf.Min(8, joint.distance + lengthRate * Time.deltaTime);
        }

        
        // if (cam.m_Lens.OrthographicSize < joint.distance) 
        //     cam.m_Lens.OrthographicSize = joint.distance;
    }

    public void ShortenWire()
    {
        joint.distance = Mathf.Max(0, joint.distance - lengthRate * Time.deltaTime);

        reelBack = StartCoroutine(ReelBack());

        // if (cam.m_Lens.OrthographicSize > 5 && cam.m_Lens.OrthographicSize > joint.distance)
        //     cam.m_Lens.OrthographicSize = Mathf.Max(5, joint.distance);
    }

    public List<Transform> GetTangledPoints()
    {
        return pointsTangled;
    }

    IEnumerator ReelBack()
    {
        while (this.enabled)
        {
            joint.distance = Mathf.Max(0, joint.distance - lengthRate * 4 * Time.deltaTime);

            yield return null;
        }
    }

    IEnumerator DeployTimer()
    {
        yield return new WaitForSeconds(0.25f);
        isDeploying = false;
    }

}
