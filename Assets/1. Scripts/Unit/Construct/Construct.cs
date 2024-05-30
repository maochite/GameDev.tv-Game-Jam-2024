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

        private GameManager gameManager;

        public ConstructSO ConstructSO => UnitSO;

        public Transform Transform => transform;

        public override float CurrentHealth { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public override float MaxHealth { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public override float HealthRegen { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        private float delayTimer = 0f;

        private void OnValidate()
        {
            Awake();
        }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            //rangeIndicator = GetComponent<ConstructRangeIndicator>();
            //decalProjector = GetComponentInChildren<DecalProjector>();
            gameManager = GameManager.Instance;
        }

        public void Start()
        {
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


            constructAbility = new AbilityPrimary(ConstructSO.AbilityPrimarySO, this);
       

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
                    ConstructSO.TargetRange / 2,
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

        public void FireAttack(Vector3 targetPos)
        {
            constructAbility.TryCast(targetPos, out _);
        }

        public override void UpdateEntityStats()
        {
            throw new NotImplementedException();
        }
    }
}