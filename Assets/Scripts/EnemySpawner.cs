using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int interval = 10;
    public Camera cam;
    public GameObject[] enemies;
    private Transform actors;

    Coroutine spawnerRoutine;

    private void OnDestroy() {
        if (spawnerRoutine != null) StopCoroutine(spawnerRoutine);
    }

    private void Awake() {
        cam = Camera.main;
        enemies = Resources.LoadAll<GameObject>("Prefab");
        actors = GameObject.Find("/Actors").transform;
    }

    private void Start() {
        spawnerRoutine = StartCoroutine(Spawner(interval));
    }

    IEnumerator Spawner(int interval)
    {
        Vector2 point;
        while (GameManager.Instance.gameState < GameState.Ended)
        {
            if (GameManager.Instance.gameState == GameState.Playing)
            {
                point = cam.WorldToViewportPoint(transform.position);
                if (point.x < 0 || point.x > 1 || point.y < 0 && point.y > 1)
                {
                    Instantiate(
                        enemies[Random.Range(0,enemies.Length)],
                        transform.position,
                        Quaternion.identity,
                        actors
                    );
                }
                yield return new WaitForSeconds(interval);
            }
            else
            {
                yield return null;
            }

        }
    }
}
