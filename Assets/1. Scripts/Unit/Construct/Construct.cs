using Ability;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using Unit.Gatherables;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Unit.Constructs
{
    public class Construct : Entity<ConstructSO>
    {

        [Header("For Non-Pool Prefab Placement")]
        [SerializeField] private ConstructSO nonPoolSO;

        [SerializeField, ReadOnly] private AbilityPrimary constructAbility;
        //[SerializeField, ReadOnly] private ConstructRangeIndicator rangeIndicator;
        //[SerializeField, ReadOnly] private DecalProjector decalProjector;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private readonly Collider[] hitColliders = new Collider[200];

        [Header("Construct Stats")]
        [SerializeField, ReadOnly] private float maxHealth = 1;
        [SerializeField, ReadOnly] private float currentHealth = 1;
        [SerializeField, ReadOnly] private float healthRegen = 1;

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
                        //Destroy(gameObject);
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


        private float delayTimer = 0f;

        protected override void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            //rangeIndicator = GetComponent<ConstructRangeIndicator>();
            //decalProjector = GetComponentInChildren<DecalProjector>();

            base.Awake();

            if (nonPoolSO != null)
            {
                AssignUnit(nonPoolSO);
            }
        }
   

        public override void AssignUnit(ConstructSO constructSO)
        {
            base.AssignUnit(constructSO);

            //Flush the previous material
            //Destroy(meshRenderer.material);
            //
            //Mesh newMesh = constructSO.MeshFilter.sharedMesh;
            //meshFilter.mesh = newMesh;
            //meshRenderer.material = constructSO.Material;


            constructAbility = new AbilityPrimary(UnitSO.AbilityPrimarySO, this);
       

            gameObject.SetActive(true);
        }

        public void ReturnConstruct()
        {
            //gameManager.ConstructManager.AbilityModHandler.RemoveListener(AbilityModHandler);
        }


        private void FixedUpdate()
        {
            delayTimer = -Time.deltaTime;

            if (delayTimer > 0) return;

            if (constructAbility != null && !constructAbility.IsCoolingDown())
            {

                int colliderAmount = Physics.OverlapSphereNonAlloc(transform.position,
                    UnitSO.TargetRange / 2,
                    hitColliders,
                    LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Enemy));

                if (colliderAmount > 0)
                {
                    //eventually need some options for what to target besides first collider
                    FireAttack(hitColliders[0].transform.position);
                }

                else
                {
                    //Let's spread some overlap checks throughout the frames
                    delayTimer = 0.2f;
                }
                
            }
        }


        public override void UpdateEntityStats()
        {
            MaxHealth = EntityStatsManager.Instance.GetHealthModified(UnitSO);
            HealthRegen = EntityStatsManager.Instance.GetHealthRegenModified(UnitSO);
            MovementSpeed = EntityStatsManager.Instance.GetMovementModified(UnitSO);
            //DefaultAbility.UpdateAbilityStats();
        }

        public void FireAttack(Vector3 targetPos)
        {
            constructAbility.TryCast(targetPos, out _);
        }

    }
}