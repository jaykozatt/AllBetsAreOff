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
            [System.Serializable]
            public struct GracePeriod
            {
                public float timeBonus;
                public float remainingDuration;

            }
        #endregion

        #region Settings
        [Header("Settings")]
            public Vector2 intervalRange; // The max and min intervals between spawns
            public float gracePeriodDuration = 60; // 60 seconds

        #endregion

        #region References
        [Header("References")]
            public List<EnemyTemplate> templates;
            private List<GameObject> pool;
        #endregion
        
        #region Variables & Switches
            GracePeriod gracePeriod; // a bonus to the spawn interval that diminishes with time.
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
                    gracePeriod.timeBonus + 
                    (intervalRange.y - intervalRange.x) * 
                        Mathf.Pow(populationCount/maxPopulation, 2)
                ;
            }
            private bool CanSpawn {
                // Limit the population capacity to a third of the total
                // for the duration of the grace period
                get =>
                    gracePeriod.remainingDuration > 0 ?
                        populationCount < maxPopulation / 3 :
                        populationCount < maxPopulation
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
                
                gracePeriod.timeBonus = intervalRange.y - intervalRange.x;
                gracePeriod.remainingDuration = gracePeriodDuration;
            }

            private void Update() 
            {
                // Diminish the grace period bonus linearly over the course of {gracePeriodDuration} seconds
                gracePeriod.remainingDuration = Mathf.Max(0,gracePeriod.remainingDuration - Time.deltaTime);
                gracePeriod.timeBonus = Mathf.Lerp(0,
                    intervalRange.y - intervalRange.x,
                    gracePeriod.remainingDuration / gracePeriodDuration
                );

            }
        #endregion

        public bool TrySpawnAt(Vector3 pos, out GameObject instance)
        {
            if (CanSpawn)
            {
                instance = pool[Random.Range(0,pool.Count)];
                instance.SetActive(true);
                instance.GetComponent<Enemy>().Initialise(pos);
                
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
