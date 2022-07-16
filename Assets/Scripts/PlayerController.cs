using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : StaticInstance<PlayerController>
{

    public float speed;
    Vector2 input;
    Rigidbody2D rb;

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

        if (Input.GetKey(KeyCode.Space)) WireController.Instance.LengthenWire();
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) WireController.Instance.ShortenWire();
    }
}
