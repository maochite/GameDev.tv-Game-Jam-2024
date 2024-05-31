using Ability;
using Items;
using Particle;
using System.Collections;
using System.Collections.Generic;
using Unit;
using Unit.Entities;
using Unit.Gatherables;
using UnityEngine;

public class SubManager : MonoBehaviour 
{ 
    [SerializeField] private AbilityInitializer abilityInitializer;
    [SerializeField] private ParticleManager particleManager;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private EntityStatsManager entityStatsManager;
    [SerializeField] private ConstructManager constructManager;
    [SerializeField] private ItemManager itemManager;
    [SerializeField] private GatherableManager gatherableManager;
}
