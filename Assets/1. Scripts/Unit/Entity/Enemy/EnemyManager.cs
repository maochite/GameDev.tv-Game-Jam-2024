
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Entities
{

    public class EnemyManager : StaticInstance<EnemyManager>
    {
        [field: SerializeField] public Enemy EnemyPrefab { get; private set; }

        [Header("Enemy Pooling")]
        [SerializeField] int initalPoolSize = 25;
        [SerializeField] int poolExtension = 25;
        [SerializeField] public float SpawnBufferRadius = 8f;

        [SerializeField, ReadOnly] int currentActive = 0;

        private HashSet<Enemy> activeEnemies = new();
        private Queue<Enemy> enemySystemPool = new();

        [Header("Enemy Test SO")]
        [SerializeField] EnemySO enemyTestSO;
        [SerializeField] Vector3 testLocation = Vector3.zero;

        private void Start()
        {
            ExtendEnemyPool(initalPoolSize);
        }

        private void ExtendEnemyPool(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Enemy enemy = Instantiate(EnemyPrefab, Vector3.zero, Quaternion.identity, transform);
                enemy.gameObject.SetActive(false);
                enemySystemPool.Enqueue(enemy);
            }
        }

        public Enemy RequestEnemy(EnemySO enemySO, Vector3 pos, Quaternion rot)
        {
            if (!enemySystemPool.TryDequeue(out Enemy enemy))
            {
                ExtendEnemyPool(poolExtension);
                enemy = enemySystemPool.Dequeue();
            }


            enemy.transform.SetPositionAndRotation(pos, rot);
            enemy.AssignUnit(enemySO);
            enemy.gameObject.SetActive(true);
            activeEnemies.Add(enemy);
            
            currentActive = activeEnemies.Count;


            return enemy;
        }


        public void ReturnEnemyToPool(Enemy enemy)
        {
            if(!activeEnemies.Contains(enemy))
            {
                Debug.LogWarning("Invalid Enemy returned to Enemy Pool");
                return;
            }

            activeEnemies.Remove(enemy);

            enemy.gameObject.SetActive(false);
            enemySystemPool.Enqueue(enemy);

            currentActive = activeEnemies.Count;
        }

        public void UpdateActiveEnemyModifiedStats()
        {
            foreach (Enemy enemy in activeEnemies)
            {
                //enemy.UpdateEntityStats();
            }
        }

        void Update()
        {

        }

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void SpawnEnemyTest()
        {
            if(enemyTestSO != null)
            {
                RequestEnemy(enemyTestSO, testLocation, Quaternion.identity);
            }
        }
    }
}