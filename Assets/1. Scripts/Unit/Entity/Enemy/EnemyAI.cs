using Unit.Entities;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [SerializeReference][HideInInspector] private NavMeshAgent nmAgent;

    private void Awake()
    {
        nmAgent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        TimeManager.Instance.OnTick += TimeManager_OnTick;
    }

    private void OnDisable()
    {
        TimeManager.Instance.OnTick -= TimeManager_OnTick;
    }

    private void TimeManager_OnTick()
    {
        nmAgent.SetDestination(Player.Instance.transform.position);
    }
}
