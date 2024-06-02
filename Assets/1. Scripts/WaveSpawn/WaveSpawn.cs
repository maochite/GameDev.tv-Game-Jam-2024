using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using Unit.Entities;

namespace WaveSpawns
{
    public abstract class WaveSpawn {
        public abstract List<Enemy> Spawn(EnemySO enemySO, int enemiesToSpawn);

        public Enemy SpawnEnemy(EnemySO enemySO, Vector3 spawnPos, Quaternion rotation)
        {
            if (!SpawnPosOk(spawnPos, out Player overlapPlayer))
            {
                Vector3 newSpawnPos = PushAwayFrom(spawnPos, overlapPlayer.transform.position, EnemyManager.Instance.SpawnBufferRadius + 0.5f);
                if (SpawnPosOk(newSpawnPos, out _))
                {
                    spawnPos = newSpawnPos;
                }
            }

            if (NavMeshUtils.NearestPointOnNavmesh(spawnPos, out Vector3 nmPos))
            {
                spawnPos = nmPos;
            }

            Enemy enemy = EnemyManager.Instance.RequestEnemy(enemySO, spawnPos, rotation);

            //NetworkObject enemyNetworkObj = InstanceFinder.NetworkManager.GetPooledInstantiated(
            //    prefab.gameObject,
            //    InstanceFinder.NetworkManager.IsServer
            //);

            //enemyNetworkObj.SetParent(ObjectPools.Instance.EnemyContainer);
            //enemyNetworkObj.transform.SetPositionAndRotation(spawnPos, Quaternion.Euler(rotation));

            //EnemyAI enemyAI = enemyNetworkObj.GetComponent<EnemyAI>();
            //enemyAI.EnemyType = EnemyTable.Instance.GetRandomEnemy().EnemyType;
            //InstanceFinder.ServerManager.Spawn(enemyNetworkObj.gameObject);

            return enemy;
        }

        public Player GetRandomPlayer()
        {
            // TODO
            return Player.Instance;
        }

        private Vector3 PushAwayFrom(Vector3 pos, Vector3 targetPos, float radius)
        {
            Vector3 direction = new(pos.x - targetPos.x, 0, pos.z - targetPos.z);
            Vector3 push = direction.normalized * radius;
            return targetPos + push;
        }

        private bool SpawnPosOk(Vector3 spawnPos, [NotNullWhen(false)] out Player overlapPlayer)
        {
            // TODO
            overlapPlayer = null;
            Player player = Player.Instance;
            //foreach (Player player in PlayerManager.ServerPlayers)
            //{
                Vector2 posVec2 = new(spawnPos.x, spawnPos.z);
                Vector2 playerPosVec2 = new(player.transform.position.x, player.transform.position.z);

                if (Vector2.Distance(posVec2, playerPosVec2) < EnemyManager.Instance.SpawnBufferRadius)
                {
                    // Store the player that was overlapping the spawn point
                    overlapPlayer = player;
                    return false;
                }
            //}

            return true;
        }
    }
}
