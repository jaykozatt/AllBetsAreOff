using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    public Transform caster;
    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = caster.localPosition + offset;
    }
}
