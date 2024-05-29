
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Ability;
using Unit.Gatherables;
using System.Linq;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.VisualScripting;
using Unit.Constructs;
using static UnityEditor.PlayerSettings;
using System;

namespace Unit.Entities
{
    public enum EntityPrimaryState
    {
        Move,
        Idle,
        Dead,
        Action,
    }

    public enum EntityActionState
    {
        None,
        Attack,
        Gather,
        Repair,
        Summon,
    }

    public class Player : Entity<PlayerSO>
    {
        [Header("- Player Specifics -")]

        [Header("Player Prefab Components")]
        [field: SerializeField] private Rigidbody rigidBody;
        private AbilityPrimary playerAbility;

        [Header("Camera Components")]
        //[SerializeField] private CameraFollow camFollow;

        [Header("Controller Components")]
        [SerializeField, Range(1, 20)] private float inputSmoothing = 8f;

        //Controller Variables
        private Vector3 raw_input;
        private Vector3 calculated_input;
        private Vector3 world_input;

        //States
        private EntityPrimaryState state;
        private EntityActionState action;

        //Action detect
        private LayerMask enemyLayerMask;
        private LayerMask miscLayerMask;
        private Collider[] enemyBuffer = new Collider[200];
        private List<Collider> reducedEnemyBuffer = new(200);
        private Collider[] miscBuffer = new Collider[200];
        private List<Gatherable> gatherableBuffer = new(200);

        //Action Targets
        private UnitIDInstance<Enemy, EnemySO> currentAttackTarget;
        private Construct currentSummonTarget;
        private List<UnitIDInstance<Gatherable, GatherableSO>> gatherableTargets = new(10);
        private List<Construct> currentRepairTargets = new(10);

        //Action Variables
        private float actionRemainingTime = 0;

        //Player Stats
        public float GatheringDamage { get; private set; }
        public float GatheringTime { get; private set; }
         public float GatherRadius { get; private set; } 
         public float ItemMagnetRadius { get; private set; } 
         public float CollectionRadius { get; private set; }
        public override float CurrentHealth { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        private void Awake()
        {
            enemyLayerMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Enemy);
            miscLayerMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Gatherable);
        }

        public override void AssignUnit(PlayerSO playerSO)
        {
            base.AssignUnit(playerSO);
            playerAbility = new(playerSO.DefaultAbility, this);

            GatheringTime = playerSO.BaseGatheringTime;
            GatherRadius = playerSO.BaseGatherRadius;
            ItemMagnetRadius = playerSO.BaseItemMagnetRadius;
            CollectionRadius = playerSO.BaseCollectionRadius;
        }

        private void GetInput()
        {
            raw_input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            raw_input.Normalize();

            calculated_input = Vector3.Lerp(raw_input, calculated_input, inputSmoothing * Time.deltaTime);
        }

        private bool Move()
        {
            Vector3 movementDirection = calculated_input.normalized;

            if (movementDirection != Vector3.zero)
            {
                transform.forward = movementDirection;

                rigidBody.MovePosition(rigidBody.position + MovementSpeed * Time.deltaTime * calculated_input);
                return true;
            }

            else
            {
                return false;
            }

        }

        public void Look(Vector3 pos)
        {
            Vector3 lookDir = Aim(pos);
            Quaternion rot = Quaternion.identity;

            if (lookDir != Vector3.zero)
            {
                rot = Quaternion.LookRotation(lookDir, Vector3.up);
            }

            if (rot != Quaternion.identity)
            {
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 3*Time.deltaTime);
                rigidBody.rotation = rot;
            }
        }

        private void Update()
        {
            GetInput();
        }

        private void FixedUpdate()
        {
            if (state == EntityPrimaryState.Dead) return;

            if (state == EntityPrimaryState.Idle)
            {
                 GetActionFromProximity();
            }

            if(state == EntityPrimaryState.Action)
            {
                if(action == EntityActionState.None)
                {
                    ChangePrimaryState(EntityPrimaryState.Idle);
                }

                else if (Move())
                {
                    action = EntityActionState.None;
                    ChangePrimaryState(EntityPrimaryState.Move);
                }

                else ResolveCurrentAction();
            }

            else
            {
                if (Move())
                {
                    ChangePrimaryState(EntityPrimaryState.Move);
                }

                else ChangePrimaryState(EntityPrimaryState.Idle);
            }

        }

        private void ResolveCurrentAction()
        {
            actionRemainingTime -= Time.deltaTime;

            if (actionRemainingTime > 0) return;

            else
            {
                if(action == EntityActionState.Attack)
                {
                    Attack();
                }

                else if (action == EntityActionState.Gather)
                {
                    Gather();
                }

                action = EntityActionState.None;
                ChangePrimaryState(EntityPrimaryState.Idle);
            }
        }

        public void GetActionFromProximity()
        {

            if (!playerAbility.IsCoolingDown())
            {
                int numEnemyColliders = Physics.OverlapSphereNonAlloc(transform.position, AttackRadius, enemyBuffer, enemyLayerMask);

                if (numEnemyColliders > 0)
                {
                    reducedEnemyBuffer.Clear();
                    
                    for (int i = 0; i < numEnemyColliders; i++)
                    {
                        reducedEnemyBuffer.Add(enemyBuffer[i]);
                    }

                    Transform closestTransform = TransformUtility.FindClosestTransformSqr(transform.position, reducedEnemyBuffer);

                    if(closestTransform.TryGetComponent(out Enemy enemy))
                    {
                        currentAttackTarget = new(enemy);
                        Look(currentAttackTarget.Unit.transform.position);

                        ChangeAction(EntityActionState.Attack);
                    }

                    return;
                }
            }

            int numMiscColliders = Physics.OverlapSphereNonAlloc(transform.position, GatherRadius, miscBuffer, miscLayerMask);
            gatherableBuffer.Clear();

            for (int i = 0; i < numMiscColliders; i++)
            {

                if (miscBuffer[i].TryGetComponent(out Gatherable gatherable))
                {
                    gatherableBuffer.Add(gatherable);
                }
            }

            if (gatherableBuffer.Count != 0)
            {
                gatherableTargets.Clear();

                foreach (Gatherable gatherable in gatherableBuffer)
                {
                    gatherableTargets.Add(new(gatherable));
                }

                ChangeAction(EntityActionState.Gather);

                return;
            }
            

        }

        public void ChangePrimaryState(EntityPrimaryState entityState)
        {
            if (entityState == state) return;

            Animator.ToggleIdleAnimation(false);
            Animator.ToggleWalkAnimation(false);

            //Animator.ToggleDeathAnimation(false);


            switch (entityState)
            {
                case EntityPrimaryState.Move:
                    Animator.ChangeAnimationMultiplier(1);
                    Animator.ToggleWalkAnimation(true);
                    state = EntityPrimaryState.Move;
                    break;
                case EntityPrimaryState.Dead:
                    //Toggle Death Animation
                    break;
                case EntityPrimaryState.Idle:
                    Animator.ToggleIdleAnimation(true);
                    state = EntityPrimaryState.Idle;
                    //Idle Toggle is automatic if everything else is off
                    break;;
                case EntityPrimaryState.Action:
                    state = EntityPrimaryState.Action;
                    break;
                default:
                    break;
            }
        }

        public void ChangeAction(EntityActionState entityAction)
        {
            if (entityAction == action) return;

            switch (entityAction)
            {
                case EntityActionState.None:
                    action = EntityActionState.None;
                    break;
                case EntityActionState.Attack:
                    EvaluateActionAnimation(UnitSO.AttackAnimation, AttackSpeed);
                    actionRemainingTime = AttackSpeed;

                    action = EntityActionState.Attack;
                    ChangePrimaryState(EntityPrimaryState.Action);

                    break;
                case EntityActionState.Gather:

                    foreach (var gatherableUnit in gatherableTargets)
                    {
                        if (gatherableUnit.Unit == null || gatherableUnit.InstanceID != gatherableUnit.Unit.ID)
                        {
                            continue;
                        }

                        else
                        {
                            EvaluateActionAnimation(gatherableUnit.Unit.UnitSO.GatheringAnimation, GatheringTime);
                            actionRemainingTime = GatheringTime;
                            action = EntityActionState.Gather;

                            ChangePrimaryState(EntityPrimaryState.Action);

                            return;
                        }
                    }

                    //Shouldn't really get here
                    action = EntityActionState.None;

                    break;
                case EntityActionState.Summon:
                default:
                    break;
            }
        }

        public bool Attack()
        {
            if (currentAttackTarget.Unit != null
                    && currentAttackTarget.Unit.ID == currentAttackTarget.InstanceID)
            {
                if (playerAbility.TryCast(currentAttackTarget.Unit.transform.position, out _))
                {
                    return true;
                }
            }

            return false;
        }

        public Vector3 Aim(Vector3 worldPos)
        {
            var direction = worldPos - transform.position;

            direction.y = 0;
            return direction;
        }

        public bool GetMouseWorldPosition(out Vector3 pos)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity))
            {
                // The Raycast hit something, return with the position.
                pos = hitInfo.point;
                return true;
            }
            else
            {
                pos = Vector3.zero;
                return false;
            }
        }

        private void EvaluateActionAnimation(
            EntityActionAnimation entityActionAnimation, float actionSpeed)
        {
            Animator.ChangeAnimationMultiplier(actionSpeed);

            switch (entityActionAnimation)
            {
                case EntityActionAnimation.Attack:
                    Animator.TriggerAttackAnimation();
                    break;
                case EntityActionAnimation.Chop:
                    Animator.TriggerChopAnimation();
                    break;
                case EntityActionAnimation.Mine:
                    Animator.TriggerMineAnimation();
                    break;
                case EntityActionAnimation.Summon:
                    Animator.TriggerSummonAnimation();
                    break;
                default:
                    Animator.TriggerSummonAnimation();
                    break;
            }
        }

        private void Gather()
        {

            foreach (var gatherableInstance in gatherableTargets)
            {
                if (gatherableInstance.Unit != null 
                    && gatherableInstance.Unit.ID == gatherableInstance.InstanceID)
                {
                    gatherableInstance.Unit.Gather(GatheringDamage);
                }
            }

        }
    }

}