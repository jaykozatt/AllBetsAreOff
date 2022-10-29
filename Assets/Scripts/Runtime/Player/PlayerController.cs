using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : StaticInstance<PlayerController>
{

    public FMODUnity.EventReference tackleSFX;
    private FMOD.Studio.EventInstance tackleInstance;

    public int lives = 3;
    public int invincibilityFrames = 10;
    private int framesUntilVulnerable = 0;

    public float speed;
    Vector2 input;
    public Rigidbody2D rb;
    private CircleCollider2D hurtbox;
    bool isReeling = false;
    float lookaheadValue;
    CinemachineVirtualCamera vcam1;
    CinemachineFramingTransposer transposer;
    List<Transform> targets;
    // bool targetWasReached = false;

    public Joystick joystick;

    Coroutine reelCrash;
    private Transform targetGroup;

    private void OnTriggerEnter2D(Collider2D other) {

        int defaultLayer = LayerMask.NameToLayer("Default");
        if (isReeling && other.gameObject.layer == defaultLayer)
        {
            targets = DiceController.Instance.GetTangledPoints();
            bool otherIsEntangled = targets.Count > 0 && targets[0] == other.transform;
            if (!otherIsEntangled && !other.gameObject.CompareTag("Indestructible"))
            {
                Enemy enemy = other.gameObject.GetComponent<Enemy>();
                enemy.GetDamaged(enemy.numberOfChips);
                tackleInstance.start();
            }
            else if (otherIsEntangled)
            {
                targets = DiceController.Instance.GetTangledPoints();
                Transform target = targets[0];

                DiceController.Instance.Detangle();
                if (!target.CompareTag("Indestructible"))
                {
                    Enemy enemy = target.GetComponent<Enemy>();
                    enemy.GetDamaged(enemy.numberOfChips);
                }
                
                tackleInstance.start();
            }
        }
    }

    private void OnCollisionStay2D(Collision2D other) 
    {
        int defaultLayer = LayerMask.NameToLayer("Default");
        if (other.collider.gameObject.layer == defaultLayer)
        {
            targets = DiceController.Instance.GetTangledPoints();
            if (targets.Count > 0 && targets[0] == other.transform)
            {
                DiceController.Instance.Detangle();   
            }
        }
    }

    private void OnDestroy() {
        if (reelCrash != null) StopCoroutine(reelCrash);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hurtbox = GetComponentsInChildren<CircleCollider2D>()[1];
        tackleInstance = FMODUnity.RuntimeManager.CreateInstance(tackleSFX);
        targets = new List<Transform>();
        vcam1 = GameObject.Find("/Setup/CM vcam1").GetComponent<CinemachineVirtualCamera>();
        transposer = vcam1.GetCinemachineComponent<CinemachineFramingTransposer>();
        targetGroup = GameObject.Find("/Actors/Player/TargetGroup").transform;

        lookaheadValue = transposer.m_LookaheadTime;
        
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();        
        framesUntilVulnerable = Mathf.Max(0, framesUntilVulnerable - 1);

        if (DiceController.Instance.IsEntangled)
            vcam1.Priority = 0;
        else
            vcam1.Priority = 2;
    }

    private void FixedUpdate() 
    {
        rb.AddForce(rb.mass * input * speed);
        
    }

    void ProcessInput()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            switch (GameManager.Instance.gameState)
            {
                case GameState.Playing:
                    GameManager.Instance.PauseGame();
                    break;
                case GameState.Paused:
                    print("Resuming!");
                    GameManager.Instance.ResumeGame();
                    break;
                default: break;
            }

        if (GameManager.Instance.gameState == GameState.Playing)
        {
            #if UNITY_STANDALONE || UNITY_EDITOR
                input.x = Input.GetAxis("Horizontal");
                input.y = Input.GetAxis("Vertical");
                
                if (Input.GetKey(KeyCode.Space)) 
                    OnLengthenKeyPress();
                
                if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    OnShortenKeyPress();
            #endif

            #if UNITY_ANDROID || UNITY_IOS
                input = joystick.Direction;
            #endif
        }
    }

    public void OnShortenKeyPress() 
    {
        if (!DiceController.Instance.IsEntangled)
        {
            float wireLength = DiceController.Instance.joint.distance;
            if (wireLength > 3) 
                DiceController.Instance.ShortenWire();
            else if (wireLength <= 3)
                DiceController.Instance.ReelBack();
        }
        else
        {
            if (!isReeling)
            {
                isReeling = true;
                reelCrash = StartCoroutine(ReelCrash());
            }
        }
    }

    public void OnLengthenKeyPress()
    {
        DiceController.Instance.LengthenWire();
    }

    IEnumerator ReelCrash()
    {
        targets = DiceController.Instance.GetTangledPoints();
        transposer.m_LookaheadTime = 0;
        hurtbox.enabled = true;

        Vector2 attackVector;
        Transform target;
        while (DiceController.Instance.IsEntangled)
        {
            target = targets[0];
            attackVector = (target.position - transform.position).normalized;

            rb.velocity = attackVector * speed * 2;
            
            yield return null;
        }

        float radius = hurtbox.radius;
        hurtbox.radius = 1.5f* radius;
        yield return null;

        while (rb.velocity.magnitude > 10)
        {
            yield return null;
        }

        isReeling = false;
        transposer.m_LookaheadTime = lookaheadValue;
        hurtbox.radius = radius;
        hurtbox.enabled = false;
    }

    public void GetHurt()
    {
        if (framesUntilVulnerable <= 0 && !isReeling)
        {
            lives = Mathf.Max(0, lives - 1);
            framesUntilVulnerable = invincibilityFrames;
            LivesInterface.Instance.UpdateDisplay(lives);
            LivesInterface.Instance.HurtFlash();
            tackleInstance.start();

            if (lives <= 0) 
            {
                GameManager.Instance.LoseGame();
                Destroy(transform.parent.gameObject);
            }
        }

    }

}
