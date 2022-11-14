using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllBets
{
    public class EnemyPool : StaticInstance<EnemyPool>
    {
        #region Structures
            [System.Serializable]
            public struct EnemyTemplate
            {
                public int maxCount;
                public Enemy enemy;
            }
        #endregion

        #region Settings
        [Header("Settings")]
            public Vector2 intervalRange; // The max and min intervals between spawns
        #endregion

        #region References
        [Header("References")]
            public List<EnemyTemplate> templates;
            private List<GameObject> pool;
        #endregion
        
        #region Variables & Switches
            float gracePeriodBonus; // a bonus to the spawn interval that diminishes with time.
            float gracePeriodDuration = 60; // 60 seconds
            int populationCount = 0;
            public int maxPopulation {get; private set;}
        #endregion

        #region Properties
            public float spawnInterval { 
                // An interval that remains short on low population counts
                // (unless the game's in its grace period), and rapidly increases
                // as it closes in towards the population cap. 
                get => 
                    intervalRange.x + 
                    gracePeriodBonus + 
                    (intervalRange.y - intervalRange.x) * 
                        Mathf.Pow(populationCount/maxPopulation, 2)
                ;
            }
        #endregion
        
        #region Monobehaviour Functions
            protected override void Awake() 
            {
                base.Awake();
                pool = new List<GameObject>();
                
                maxPopulation = 0; // Instantiate each enemy from the pool
                foreach (EnemyTemplate template in templates)
                {
                    GameObject enemy;
                    for(int i=0; i<template.maxCount; i++)
                    {
                        enemy = Instantiate(
                            template.enemy, 
                            transform.position, 
                            Quaternion.identity, 
                            transform
                        ).gameObject;
                        enemy.SetActive(false);
                        pool.Add(enemy);
                    }

                    maxPopulation += template.maxCount;
                }
                
                gracePeriodBonus = intervalRange.y - intervalRange.x;
            }

            private void Update() 
            {
                // Diminish the grace period bonus over the course of 10 x gracePeriodBonus seconds
                gracePeriodBonus = Mathf.Max(0, gracePeriodBonus - .1f * Time.deltaTime);
            }
        #endregion

        public bool TrySpawnAt(Vector3 pos, out GameObject instance)
        {
            if (populationCount < maxPopulation)
            {
                instance = pool[Random.Range(0,pool.Count)];
                instance.transform.position = pos;
                instance.SetActive(true);
                instance.GetComponent<Enemy>().Initialise();
                
                populationCount++;
                pool.Remove(instance);

                return true;
            }

            instance = null;
            return false;
        }

        public void Despawn(GameObject enemy)
        {
            enemy.SetActive(false);
            enemy.transform.position = transform.position;

            populationCount--;
            pool.Add(enemy);
        }
    }
}
