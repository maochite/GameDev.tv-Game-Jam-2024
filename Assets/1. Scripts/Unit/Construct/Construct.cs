using Ability;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Unit.Construct
{
    public class Construct : Unit<ConstructSO>, ICaster
    {
        [SerializeField, ReadOnly] private AbilityPrimary constructAbility;
        //[SerializeField, ReadOnly] private ConstructRangeIndicator rangeIndicator;
        //[SerializeField, ReadOnly] private DecalProjector decalProjector;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private readonly Collider[] hitColliders = new Collider[200];

        private GameManager gameManager;

        public ConstructSO ConstructSO => UnitSO;

        public Transform Transform => transform;

        public Guid TargetID => throw new NotImplementedException();

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

        public void AssignConstruct(ConstructSO constructSO, Vector3 pos, Quaternion rot)
        {
            //Flush the previous material
            Destroy(meshRenderer.material);

            Mesh newMesh = constructSO.MeshFilter.sharedMesh;
            meshFilter.mesh = newMesh;
            meshRenderer.material = constructSO.Material;
            transform.SetPositionAndRotation(pos, rot);


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

    }
}