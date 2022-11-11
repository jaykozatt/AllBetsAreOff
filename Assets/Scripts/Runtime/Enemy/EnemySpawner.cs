using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllBets
{
    public class EnemySpawner : MonoBehaviour
    {
        #region Settings
        [Header("Settings")]
            [SerializeField] int spawnInterval = 12;
        #endregion
        
        #region References
            private Camera cam;
            private GameObject actors;
            private GameObject[] enemies;
        #endregion

        #region Coroutine References
            Coroutine spawner;
        #endregion

        #region Coroutine Definitions
            IEnumerator Spawner(int interval)
            {
                Vector2 point;
                while (GameManager.Instance.gameState < GameState.Ended)
                {
                    if (GameManager.Instance.gameState == GameState.Playing)
                    {
                        // If the spawner is outside of the player's field of view
                        point = cam.WorldToViewportPoint(transform.position);
                        if (point.x < 0 || point.x > 1 || point.y < 0 && point.y > 1)
                        {
                            GameObject instance; // Wait for a time interval after spawning a new enemy
                            if (EnemyPool.Instance.TrySpawnAt(transform.position, out instance))
                                yield return new WaitForSeconds(EnemyPool.Instance.spawnInterval);
                            else
                                yield return null;
                        }
                        else 
                        {
                            // Skip this frame if the spawner is in view of the player
                            yield return null;
                        }
                    }
                    else
                    {
                        // Skip while not currently playing
                        yield return null;
                    }

                }
            }
        #endregion

        #region Monobehaviour Functions
            private void OnDestroy() {
                if (spawner != null) StopCoroutine(spawner);
            }

            private void Awake() {
                cam = Camera.main;
                enemies = Resources.LoadAll<GameObject>("Prefab/Enemies");
                actors = GameObject.Find("/Actors");
            }

            private void Start() {
                spawner = StartCoroutine(Spawner(spawnInterval));
            }
        #endregion

    }
}
