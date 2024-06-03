using Ability;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using Unit.Gatherables;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

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
        [field: SerializeField] public SkinnedMeshRenderer Mesh { get; private set; }

        [Header("Construct Stats")]
        [SerializeField, ReadOnly] private float maxHealth = 1;
        [SerializeField, ReadOnly] private float currentHealth = 1;
        [SerializeField, ReadOnly] private float healthRegen = 1;

        private float towerDegen = 1;

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
                        ConstructManager.Instance.ReturnConstructToPool(this);
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
                return;
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
            gameObject.SetActive(true);

            foreach(Material material in Mesh.materials)
            {
                Destroy(material);
            }

            Material[] mats = new Material[] { UnitSO.Material, UnitSO.Material, UnitSO.RockMaterial };
            Mesh.materials = mats;

            MaxHealth = UnitSO.BaseHealth;
            CurrentHealth = UnitSO.BaseHealth;
            constructAbility = new AbilityPrimary(UnitSO.AbilityPrimarySO, this);
       
        }


        private void FixedUpdate()
        {
            delayTimer = -Time.deltaTime;

            RegenEntity();

            if (delayTimer > 0) return;

            if (constructAbility != null && !constructAbility.IsCoolingDown())
            {

                int colliderAmount = Physics.OverlapSphereNonAlloc(transform.position,
                    UnitSO.BaseAttackRange / 2,
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

        protected override void RegenEntity()
        {
            regenTimer -= Time.deltaTime; 

            if (regenTimer < 0)
            {
                CurrentHealth -= towerDegen;
                regenTimer = RegenInterval;
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