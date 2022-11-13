using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllBets
{
    public class Wire : MonoBehaviour
    {
        #region References
            #region FMOD SFX
            [Header("SFX")]
                public FMODUnity.EventReference sliceSFX;
            #endregion
            EdgeCollider2D  hitbox;
        #endregion

        #region Variables & Switches
            Vector2 pivotPos;
            Vector2 diePos;
            Vector2 playerPos;
            List<Vector2> positions;
        #endregion
        
        #region Monobehaviour Functions
            private void OnTriggerEnter2D(Collider2D other) 
            {
                if (DiceController.Instance.IsDeployed)
                {
                    Enemy enemy; // Enemies tagged as 'Wrap Immune' are sliced instead of entangled
                    if (other.TryGetComponent<Enemy>(out enemy) && other.CompareTag("Wrap Immune"))
                    {
                        int damage = Mathf.Max(1, enemy.numberOfChips / 2);
                        enemy.GetDamaged(damage);
                        
                        FMODUnity.RuntimeManager.PlayOneShot(sliceSFX);
                    }
                    else if (!DiceController.Instance.IsEntangled)
                    {
                        DiceController.Instance.TangleWireTo(other);
                        PlayerController.Instance.TangleWireTo(other);
                    }
                }
            }

            // Start is called before the first frame update
            void Start()
            {
                hitbox = GetComponent<EdgeCollider2D>();
                positions = new List<Vector2>();
            }

            // Update is called once per frame
            void Update()
            {
                pivotPos = DiceController.Instance.pivot.position - transform.position;
                diePos = DiceController.Instance.transform.position - transform.position;
                playerPos = PlayerController.Instance.transform.position - transform.position;

                positions.Clear();
                positions.Add(diePos);
                positions.Add(pivotPos);
                if (DiceController.Instance.pivot != PlayerController.Instance.transform)
                    positions.Add(playerPos);

                hitbox.SetPoints(positions);
            }
        #endregion
    }
}
