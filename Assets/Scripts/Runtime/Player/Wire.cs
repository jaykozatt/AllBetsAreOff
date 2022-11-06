using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllBets
{
    public class Wire : MonoBehaviour
    {
        public FMODUnity.EventReference sliceSFX;
        private FMOD.Studio.EventInstance sliceInstance;

        Vector2 pivotPos;
        Vector2 diePos;
        List<Vector2> positions;
        new EdgeCollider2D  collider;

        private void OnTriggerEnter2D(Collider2D other) 
        {
            if (DiceController.Instance.IsDeployed)
            {
                if (!other.CompareTag("Untagged"))
                {
                    DiceController.Instance.TangleWireTo(other);
                }
                else
                {
                    Enemy enemy;
                    if (other.TryGetComponent<Enemy>(out enemy))
                    {
                        int damage = Mathf.Max(1, enemy.numberOfChips / 2);
                        enemy.GetDamaged(damage);
                        
                        sliceInstance.start();
                    }
                }
            }
        }

        private void OnDestroy() 
        {
            sliceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            sliceInstance.release();
        }

        // Start is called before the first frame update
        void Start()
        {
            collider = GetComponent<EdgeCollider2D>();
            positions = new List<Vector2>();

            sliceInstance = FMODUnity.RuntimeManager.CreateInstance(sliceSFX);
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
}
