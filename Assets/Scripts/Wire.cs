using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{

    Vector2 playerPos;
    Vector2 diePos;
    List<Vector2> positions;
    new EdgeCollider2D  collider;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<EdgeCollider2D>();
        positions = new List<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        playerPos = PlayerController.Instance.transform.position - transform.position;
        diePos = WireController.Instance.transform.position - transform.position;

        positions.Clear();
        positions.Add(diePos);
        positions.Add(playerPos);

        collider.SetPoints(positions);
    }
}
