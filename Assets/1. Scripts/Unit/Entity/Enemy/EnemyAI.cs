using System;
using Unit.Entities;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [SerializeReference][HideInInspector] private NavMeshAgent nmAgent;
    [SerializeReference][HideInInspector] private Enemy enemy;
    [SerializeReference][HideInInspector] private float lastAttackTime;

    private void Awake()
    {
        nmAgent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        TimeManager.Instance.OnTick += TimeManager_OnTick;
    }

    private void OnDisable()
    {
        if (TimeManager.Instance)
        {
            TimeManager.Instance.OnTick -= TimeManager_OnTick;
        }
    }

    private void Start()
    {
        lastAttackTime = 0;
    }

    private void TimeManager_OnTick()
    {
        if (!nmAgent.isOnNavMesh)
        {
            return;
        }

        float curTime = Time.time;
        if (curTime < lastAttackTime + enemy.AttackSpeed)
        {
            // Don't move or attack if we just attacked
            // TODO: Might want to have a seperate attack delay if using the AttackSpeed is too much
            return;
        }

        float distance = Vector3.Distance(transform.position, Player.Instance.transform.position);
        
        if (distance > Mathf.Max(enemy.AttackRadius-0.5f, nmAgent.radius+0.1f))
        {
            nmAgent.SetDestination(Player.Instance.transform.position);
        } else if (curTime >= lastAttackTime + enemy.AttackSpeed) // Not needed if we exit above, but leaving it here anyways
        {
            nmAgent.SetDestination(transform.position); // Stop moving
            enemy.EnemyAbility.TryCast(Player.Instance.transform.position, out _); // TODO: Coroutine thing?
            lastAttackTime = curTime;
        }
    }
}
