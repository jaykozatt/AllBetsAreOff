using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;

public class DiceController : StaticInstance<DiceController>
{
    public FMODUnity.EventReference impactSFX;
    private FMOD.Studio.EventInstance impactInstance;

    public CinemachineVirtualCamera cam;
    private Shadow shadow;

    public LineRenderer line;
    public List<Vector3> linePositions;
    
    public DistanceJoint2D joint;
    private Rigidbody2D rb;
    private new Collider2D collider;
    private new SpriteRenderer renderer;
    public float velocity = 1;
    public float lengthRate = 1;
    public Transform pivot;
    private bool isDeploying = false;
    private int flipper = 1;
    private int diceNumber = 0;

    public Sprite[] dice;

    private List<Transform> pointsTangled;
    Coroutine reelBack;
    Coroutine diceCycler;

    PlayerController Player {
        get => PlayerController.Instance;
    }
    public bool IsEntangled {
        get => pointsTangled.Count > 0;
    }
    public bool IsDeployed {
        get => this.enabled;
    }
    public int CurrentDice {
        get => diceNumber + 1;
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.collider.CompareTag("Player") && !isDeploying && !IsEntangled)
        {
            this.enabled = false;
        }
        else if (!other.collider.CompareTag("Player"))
        {
            // flipper = flipper == 1 ? -1 : 1;
            rb.velocity = other.relativeVelocity;

            Enemy enemy;
            if (other.gameObject.TryGetComponent<Enemy>(out enemy))
                enemy.GetDamaged(1);

            impactInstance.start();
        }
        
    }

    private void OnCollisionStay2D(Collision2D other) 
    {
        if (other.collider.CompareTag("Player") && !isDeploying && !IsEntangled)
        {
            this.enabled = false;
        }
    }
    private void OnEnable() {
        renderer.enabled = true;
        collider.enabled = true;
        line.enabled = true;
        shadow.sprite.enabled = true;    
        DrawWire();
    }

    private void OnDisable() {
        renderer.enabled = false;
        collider.enabled = false;
        line.enabled = false;
        shadow.sprite.enabled = false;
    }

    private void OnDestroy() 
    {
        if (diceCycler != null) StopCoroutine(diceCycler);
        if (reelBack != null) StopCoroutine(reelBack);
        impactInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    protected override void Awake() 
    {
        base.Awake();

        line = GetComponent<LineRenderer>();
        linePositions = new List<Vector3>();
        pointsTangled = new List<Transform>();

        joint = GetComponentInParent<DistanceJoint2D>();
        rb = GetComponentInParent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        renderer = GetComponent<SpriteRenderer>();

        shadow = transform.parent.GetComponentInChildren<Shadow>();
        shadow.caster = transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        diceCycler = StartCoroutine(DiceCycler());
        pivot = PlayerController.Instance.transform;
        impactInstance = FMODUnity.RuntimeManager.CreateInstance(impactSFX);
    }

    // Update is called once per frame
    void Update()
    {
        CheckPivot();
        SwingDie();
        DrawWire();
    }

    void CheckPivot()
    {
        if (pivot == null || joint.connectedBody == null)
        {
            pivot = PlayerController.Instance.transform;
            joint.connectedBody = PlayerController.Instance.rb;
        }
    }
    void SwingDie()
    {
        LayerMask mask = LayerMask.GetMask("Default");
        if (!collider.IsTouchingLayers(mask))
        {
            if (IsEntangled) joint.distance = Mathf.Max(0, joint.distance - lengthRate * Time.deltaTime/2);
            
            Vector2 radius = (pivot.position - transform.position);
            Vector2 direction = -radius.normalized;
            (direction.x, direction.y) = (direction.y, -direction.x);

            flipper = Vector3.Cross(rb.velocity,radius).z > 0 ? -1 : 1;

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
        }
    }

    public void Detangle()
    {
        pointsTangled.RemoveAt(0);
        if (pointsTangled.Count <= 0) 
        {
            pivot = Player.transform;
            joint.connectedBody = Player.rb;
        }
    }

    public bool TryDetangle(Transform point)
    {
        if (pointsTangled.Contains(point))
        {
            pointsTangled.Remove(point);
            
            if (pointsTangled.Count <= 0) 
            {
                pivot = Player.transform;
                joint.connectedBody = Player.rb;
            }

            return true;
        }

        return false;
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
        joint.distance = Mathf.Max(3, joint.distance - lengthRate * Time.deltaTime);

        // if (cam.m_Lens.OrthographicSize > 5 && cam.m_Lens.OrthographicSize > joint.distance)
        //     cam.m_Lens.OrthographicSize = Mathf.Max(5, joint.distance);
    }

    public void ReelBack()
    {
        reelBack = StartCoroutine(ReelBackRoutine());
    }

    public List<Transform> GetTangledPoints()
    {
        return pointsTangled;
    }

    IEnumerator ReelBackRoutine()
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

    IEnumerator DiceCycler()
    {
        while (true) 
        {
            yield return new WaitForSeconds(1);

            diceNumber = Random.Range(0, dice.Length);
            renderer.sprite = dice[diceNumber];
        }
    }

}
