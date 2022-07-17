using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{

    Vector2 pivotPos;
    Vector2 diePos;
    List<Vector2> positions;
    new EdgeCollider2D  collider;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.CompareTag("Untagged"))
        {
            DiceController.Instance.TangleWireTo(other);
        }
        else
        {
            if (!other.CompareTag("Indestructible"))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                int damage = enemy.numberOfChips / 2;
                
                enemy.GetDamaged(damage);
            }

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<EdgeCollider2D>();
        positions = new List<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        pivotPos = DiceController.Instance.pivot.position - transform.position;
        diePos = DiceController.Instance.transform.position - transform.position;

        positions.Clear();
        positions.Add(diePos);
        positions.Add(pivotPos);

        collider.SetPoints(positions);
    }
}
