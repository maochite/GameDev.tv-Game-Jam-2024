using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WaveSpawns
{
    [Serializable]
    public class RandomWaveSpawn : WaveSpawn
    {
        [SerializeField, MinValue(1), MaxValue(20), AllowNesting] private float spawnRangeMin;
        [SerializeField, MinValue(2), MaxValue(20), AllowNesting] private float spawnRangeMax;

        public override List<Enemy> Spawn(EnemySO enemySO, int enemiesToSpawn)
        {
            List<Enemy> spawns = new(enemiesToSpawn);

            if (spawnRangeMin > spawnRangeMax)
            {
                (spawnRangeMax, spawnRangeMin) = (spawnRangeMin, spawnRangeMax);
            }

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                Vector2 Circle2D = Random.insideUnitCircle;
                Vector3 Circle3D = new Vector3(Circle2D.x, 0, Circle2D.y).normalized;
                float minRadius2 = Mathf.Pow(EnemyManager.Instance.SpawnBufferRadius + spawnRangeMin, 2);
                float maxRadius2 = Mathf.Pow(EnemyManager.Instance.SpawnBufferRadius + spawnRangeMax, 2);
                float randomDistance = Mathf.Sqrt(Random.value * (maxRadius2 - minRadius2) + minRadius2);

                Vector3 newPos = GetRandomPlayer().Transform.position + Circle3D * randomDistance;

                Quaternion rotation = Random.rotation; // TODO

                spawns.Add(SpawnEnemy(enemySO, newPos, rotation));
            }

            return spawns;
        }
    }
}
