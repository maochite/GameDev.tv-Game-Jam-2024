using NaughtyAttributes;
using System;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;

namespace WaveSpawns
{
    [Serializable]
    public class OscillatingWaveSpawn : WaveSpawn
    {
        [SerializeField, MinValue(1), MaxValue(20), AllowNesting] private float spawnRange;
        [SerializeField, MinValue(0), MaxValue(20), AllowNesting] private float Oscillations;

        const float halfRotations = 2;

        public override List<Enemy> Spawn(EnemySO enemySO, int enemiesToSpawn)
        {
            List<Enemy> spawns = new(enemiesToSpawn);

            Vector3 newPos;
            float xPos;
            float zPos;

            float center = (EnemyManager.Instance.SpawnBufferRadius + 0.5f) + spawnRange / 2f;

            float step = halfRotations / enemiesToSpawn;

            int rand = UnityEngine.Random.Range(0, enemiesToSpawn);
            //Vector3 randAngle = new(0,UnityEngine.Random.Range(0, 360), 0);

            Vector3 playerPos = GetRandomPlayer().Transform.position;

            for (int i = rand; i < enemiesToSpawn + rand; i++)
            {
                xPos = (center + (spawnRange / 2f) * MathF.Sin(i * step * Mathf.PI * Oscillations)) * MathF.Cos(Mathf.PI * step * i);
                zPos = (center + (spawnRange / 2f) * MathF.Sin(i * step * Mathf.PI * Oscillations)) * MathF.Sin(Mathf.PI * step * i);

                newPos = new Vector3(xPos, 0, zPos) + playerPos;

                //Fix this later
                //var rotatedPos = RotatePointAroundPivot(newPos, Vector3.zero, randAngle);

                Quaternion rotation = UnityEngine.Random.rotation; // TODO
                spawns.Add(SpawnEnemy(enemySO, newPos, rotation));
            }

            return spawns;
        }

        public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }
    }
}
