using Ability;
using JetBrains.Annotations;
using NaughtyAttributes;
using Storage;
using System.Collections;
using System.Collections.Generic;
using Unit.Gatherables;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

namespace Unit.Entities
{

    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : Entity<EnemySO>
    {
        [Header("- Enemy Specifics -")]
        public EnemySO EnemySO => UnitSO;

        [field: Header("Enemy Prefab Components")]
        [field: SerializeField] public SpriteAnimator Animator { get; private set; }
        [field: SerializeField] private Rigidbody rigidBody;
        public AbilityPrimary EnemyAbility { get;  private set; }

        [field: Header("AI")]
        public NavMeshAgent NMAgent { get; private set; }

        [Header("Enemy Stats")]
        [SerializeField, ReadOnly] private float maxHealth = 1;
        [SerializeField, ReadOnly] private float currentHealth = 1;
        [SerializeField, ReadOnly] private float healthRegen = 1;

        [Header("For Non-Pool Prefab Placement")]
        [SerializeField] private EnemySO nonPoolSO;

        //enemy fields
        private float lastAttackTime;
        private EntityPrimaryState primaryState;
        private EntityActionState actionState;

        public override float CurrentHealth
        {
            get { return currentHealth; }

            protected set
            {
                if (!isActive) return;

                if (value < currentHealth)
                {
                    currentHealth = value;

                    if (currentHealth <= 0)
                    {
                        //OnDeath();
                        ChangePrimaryState(EntityPrimaryState.Dead);
                        actionState = EntityActionState.None;
                        StartCoroutine(DeathTimer());
                        isActive = false;
                    }
                }

                if (value >= maxHealth)
                {
                    currentHealth = maxHealth;
                    HealthBar.ToggleHealthBar(false);
                    HealthBar.SetHealthBarValue(currentHealth, maxHealth);
                    return;
                }

                else currentHealth = value;

                HealthBar.ToggleHealthBar(true);
                HealthBar.SetHealthBarValue(currentHealth, maxHealth);
            }
        }

        public override float MaxHealth
        {
            get => maxHealth;

            protected set
            {
                if (!isActive) return;

                maxHealth = value;

                if (maxHealth < 1)
                {
                    maxHealth = 1;
                }

                if (currentHealth >= maxHealth)
                {
                    currentHealth = maxHealth;
                    HealthBar.ToggleHealthBar(false);
                }

                else
                {
                    HealthBar.ToggleHealthBar(true);
                }

                HealthBar.SetHealthBarValue(currentHealth, maxHealth);
            }
        }

        public override float HealthRegen
        {
            get => HealthRegen;

            protected set
            {
                if (healthRegen < 1)
                {
                    healthRegen = 1;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            NMAgent = GetComponent<NavMeshAgent>();
            if (nonPoolSO != null)
            {
                AssignUnit(nonPoolSO);
            }
        }

        public override void AssignUnit(EnemySO enemySO)
        {
            base.AssignUnit(enemySO);
            NMAgent.enabled = true;
            ChangePrimaryState(EntityPrimaryState.Idle);
            actionState = EntityActionState.None;
            lastAttackTime = 0;
            Animator.AssignAnimations(enemySO);
            EnemyAbility = new(enemySO.DemolishAbility, this);
            MaxHealth = UnitSO.BaseHealth;
            CurrentHealth = UnitSO.BaseHealth;
            UpdateEntityStats();
        }

        public override void UpdateEntityStats()
        {
            MaxHealth = EntityStatsManager.Instance.GetHealthModified(UnitSO);
            HealthRegen = EntityStatsManager.Instance.GetHealthRegenModified(UnitSO);
            MovementSpeed = EntityStatsManager.Instance.GetMovementModified(UnitSO);
            EnemyAbility.UpdateAbilityStats();
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

        private void TimeManager_OnTick()
        {
            if (primaryState == EntityPrimaryState.Dead) return;

            if (!NMAgent.isOnNavMesh)
            {
                return;
            }

            RegenEntity();
            float curTime = Time.time;
            if (actionState == EntityActionState.Attack)
            {
                if(curTime < lastAttackTime + AttackSpeed)
                {
                    return;
                }

                if (EnemyAbility.TryCast(Player.Instance.transform.position, out _))
                {
                    actionState = EntityActionState.None;
                    ChangePrimaryState(EntityPrimaryState.Idle);
                }

                else
                {
                    Debug.LogError("Shouldn't have failed the cast");
                }
            }

            float distance = Vector3.Distance(transform.position, Player.Instance.transform.position);

            if (actionState == EntityActionState.None
                && distance > Mathf.Max(AttackRadius - 0.5f, NMAgent.radius + 0.1f))
            {
                ChangePrimaryState(EntityPrimaryState.Move);

                NMAgent.SetDestination(Player.Instance.transform.position);
            }

            else if (curTime >= lastAttackTime + AttackSpeed) // Not needed if we exit above, but leaving it here anyways
            {
                NMAgent.SetDestination(transform.position); // Stop moving

                if (!EnemyAbility.IsCoolingDown())
                {
                    lastAttackTime = curTime;
                    ChangePrimaryState(EntityPrimaryState.Action);
                    actionState = EntityActionState.Attack;
                    Animator.TriggerAttackAnimation();
                }
                
            }  
        }

        private void ChangePrimaryState(EntityPrimaryState primaryState)
        {
            Animator.ToggleWalkAnimation(false);
            Animator.ToggleIdleAnimation(false);
            Animator.ToggleDeathAnimation(false);

            if (primaryState == EntityPrimaryState.Idle)
            {
                primaryState = EntityPrimaryState.Idle;
                Animator.ToggleIdleAnimation(true);
            }

            else if (primaryState == EntityPrimaryState.Move)
            {
                primaryState = EntityPrimaryState.Move;
                Animator.ToggleWalkAnimation(true);
            }

            else if(primaryState == EntityPrimaryState.Action)
            {
                primaryState = EntityPrimaryState.Action;
            }

            else
            {
                primaryState = EntityPrimaryState.Dead;
                Animator.ToggleDeathAnimation(true);
            }
        }

        protected override void RegenEntity()
        {
            regenTimer -= Time.deltaTime;

            if (regenTimer < 0)
            {
                CurrentHealth += healthRegen;
                regenTimer = RegenInterval;
            }
        }

        private IEnumerator DeathTimer()
        {
            yield return new WaitForSeconds(2);
            EnemyManager.Instance.ReturnEnemyToPool(this);
        }
    }
    
}
