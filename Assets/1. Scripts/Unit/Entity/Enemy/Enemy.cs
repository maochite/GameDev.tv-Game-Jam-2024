using Ability;
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

    public class Enemy : Entity<EnemySO>
    {
        [Header("- Enemy Specifics -")]
        public EnemySO EnemySO => UnitSO;

        [field: Header("Player Prefab Components")]
        [field: SerializeField] private Rigidbody rigidBody;
        public AbilityPrimary EnemyAbility { get;  private set; }

        [Header("Enemy Stats")]
        [SerializeField, ReadOnly] private float maxHealth = 1;
        [SerializeField, ReadOnly] private float currentHealth = 1;
        [SerializeField, ReadOnly] private float healthRegen = 1;

        [Header("For Non-Pool Prefab Placement")]
        [SerializeField] private EnemySO nonPoolSO;
        [SerializeField] private NavMeshAgent nmAgent;

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
                        EnemyManager.Instance.ReturnEnemyToPool(this);
                        isActive = false;
                    }
                }

                else if (value > maxHealth)
                {
                    currentHealth = maxHealth;
                }

                else currentHealth = value;
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

                    //healthBar.ToggleHealthBar(false);
                }

                //healthBar.SetHealthBarValue(_currentHealth, _maxHealth);
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

            if (nonPoolSO != null)
            {
                AssignUnit(nonPoolSO);
            }
        }

        public override void AssignUnit(EnemySO enemySO)
        {
            base.AssignUnit(enemySO);
            EnemyAbility = new(enemySO.DefaultAbility, this);
            MaxHealth = UnitSO.BaseHealth;
            CurrentHealth = UnitSO.BaseHealth;
            UpdateEntityStats();
        }

        public override void UpdateEntityStats()
        {
            MaxHealth = EntityStatsManager.Instance.GetHealthModified(UnitSO);
            HealthRegen = EntityStatsManager.Instance.GetHealthRegenModified(UnitSO);
            MovementSpeed = EntityStatsManager.Instance.GetMovementModified(UnitSO);
            //DefaultAbility.UpdateAbilityStats();
        }


    }
    
}
