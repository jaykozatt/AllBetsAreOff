using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllBets
{
    public class EnemyPool : StaticInstance<EnemyPool>
    {
        [System.Serializable]
        public struct EnemyTemplate
        {
            public int maxCount;
            public Enemy enemy;
        }

        public Vector2 intervalRange; // The max and min intervals between spawns
        float gracePeriod = 5;
        int populationCount = 0;
        public List<EnemyTemplate> templates;
        private List<GameObject> pool;

        public int maxPopulation {get; private set;}
        public float spawnInterval {
            get => 
                intervalRange.x + 
                gracePeriod + 
                (intervalRange.y - intervalRange.x) * 
                    Mathf.Pow(populationCount/maxPopulation, 2)
            ;
        }
        
        protected override void Awake() 
        {
            base.Awake();
            pool = new List<GameObject>();
            
            maxPopulation = 0;
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
            

            gracePeriod = intervalRange.y - intervalRange.x;
        }

        private void Update() 
        {
            // Diminish the grace period bonus over the course of 10 x gracePeriod seconds
            gracePeriod = Mathf.Max(0, gracePeriod - .1f * Time.deltaTime);
        }

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
