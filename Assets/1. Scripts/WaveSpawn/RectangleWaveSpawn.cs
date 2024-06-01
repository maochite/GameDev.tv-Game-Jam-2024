using NaughtyAttributes;
using System;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;

namespace WaveSpawns
{
    [Serializable]
    public class RectangleWaveSpawn : WaveSpawn
    {
        [SerializeField, MinValue(3), MaxValue(20), AllowNesting] private float SizeX;
        [SerializeField, MinValue(3), MaxValue(20), AllowNesting] private float SizeZ;

        public override List<Enemy> Spawn(EnemySO enemySO, int enemiesToSpawn)
        {
            List<Enemy> spawns = new(enemiesToSpawn);

            Vector3 center = GetRandomPlayer().Transform.position;

            float totalSize = (SizeX * 2) + (SizeZ * 2);
            float step = totalSize / (enemiesToSpawn + 1);

            Vector3 startPos;
            Vector3 curPos;

            int total = 0;

            Quaternion rotation = UnityEngine.Random.rotation; // TODO

            startPos = center + new Vector3(-(SizeX/2), 0, -(SizeZ/2));
            curPos=startPos;
            for (; curPos.x<=startPos.x+SizeX; curPos.x+=step, ++total)
            {
                spawns.Add(SpawnEnemy(enemySO, curPos, rotation));
            }

            startPos = center + new Vector3((SizeX/2), 0, -(SizeZ/2));
            curPos = new(startPos.x, startPos.y, startPos.z+(step-(curPos.x-startPos.x)/2));
            for (; curPos.z<=startPos.z+SizeZ; curPos.z+=step, ++total)
            {
                spawns.Add(SpawnEnemy(enemySO, curPos, rotation));
            }

            startPos = center + new Vector3((SizeX/2), 0, (SizeZ/2));
            curPos = new(startPos.x-(step-(curPos.z-startPos.z)/2), startPos.y, startPos.z);
            for (; curPos.x>=startPos.x-SizeX; curPos.x-=step, ++total)
            {
                spawns.Add(SpawnEnemy(enemySO, curPos, rotation));
            }

            startPos = center + new Vector3(-(SizeX/2), 0, (SizeZ/2));
            curPos = new(startPos.x, startPos.y, startPos.z-(step-(curPos.x-startPos.x)/2));
            for (; curPos.z>=startPos.z-SizeZ && total<enemiesToSpawn; curPos.z-=step, ++total)
            {
                spawns.Add(SpawnEnemy(enemySO, curPos, rotation));
            }

            return spawns;
        }
    }
}
