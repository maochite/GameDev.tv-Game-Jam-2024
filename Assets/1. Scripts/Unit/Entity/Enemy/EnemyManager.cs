
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Entity
{

    public class EnemyManager : StaticInstance<EnemyManager>
    {
        [SerializeField] public Enemy EnemyPrefab { get; private set; }

        [SerializeField, Header("Enemy Pooling")] List<EnemySO> EnemySOList;
        [SerializeField] int initalPoolSize = 25;
        [SerializeField] int poolExtension = 25;

        [SerializeField, Header("Testing")] EnemySO testEnemySO;

        [SerializeField, ReadOnly] int currentActive = 0;

        private List<Enemy> activeEnemies = new();

        private Dictionary<EnemySO, Queue<Enemy>> enemySystemPools = new();

        private void Start()
        {
            return; 
            foreach (EnemySO enemySO in EnemySOList)
            {
                CreateNewEnemyPool(enemySO);
            }

            CreateNewEnemyPool(testEnemySO);
        }

        private void CreateNewEnemyPool(EnemySO enemySO)
        {
            enemySystemPools.Add(enemySO, new());
            ExtendEnemyPool(enemySO, initalPoolSize);
        }

        private void ExtendEnemyPool(EnemySO enemySO, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Enemy enemy = Instantiate(EnemyPrefab, Vector3.zero, Quaternion.identity, transform);
                enemy.gameObject.SetActive(false);
                enemySystemPools[enemySO].Enqueue(enemy);
            }
        }

        public Enemy RequestEnemy(EnemySO enemySO, Vector3 pos, Quaternion rot)
        {
            if (!enemySystemPools.ContainsKey(enemySO))
            {
                CreateNewEnemyPool(enemySO);
            }

            if (!enemySystemPools[enemySO].TryDequeue(out Enemy enemy))
            {
                ExtendEnemyPool(enemySO, poolExtension);
                enemy = enemySystemPools[enemySO].Dequeue();
            }


            enemy.transform.SetPositionAndRotation(pos, rot);
            enemy.AssignEnemy(enemySO);
            enemy.gameObject.SetActive(true);
            activeEnemies.Add(enemy);
            currentActive++;


            return enemy;
        }


        public void ReturnEnemyToPool(Enemy enemy)
        {
            if (!enemySystemPools.ContainsKey(enemy.EnemySO))
            {
                return;
            }


            enemy.gameObject.SetActive(false);
            enemySystemPools[enemy.EnemySO].Enqueue(enemy);
            activeEnemies.Remove(enemy);
            currentActive--;
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

        }
    }
}