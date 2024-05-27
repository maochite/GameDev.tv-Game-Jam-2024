
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

namespace Unit.Entities
{
    public enum EntityPrimaryState
    {
        Move,
        Idle,
        Dead,
    }

    public enum EntityActionState
    {
        None,
        Attack,
        Gather,
        Repair,
        Summon,
    }

    public enum EntityActionAnimation
    {
        Chop,
        Mine,
        Attack,
        Summon,
    }


    public class Player : Entity<PlayerSO>
    {
        [Header("Player Prefab Components")]
        [field: SerializeField] private Rigidbody rigidBody;
        private PlayerSO playerSO;
        private AbilityPrimary playerAbility;

        [Header("Player Prefab Fields")]
        [SerializeField] private float enemySphereRadius = 20f;
        [SerializeField] private float miscSphereRadius = 1f;
        [SerializeField] private float baseGatheringSpeed = 1f;
        [SerializeField] private float baseAttackSpeed = 1;
        [SerializeField] private float baseMovementSpeed = 5;

        [Header("Camera Components")]
        //[SerializeField] private CameraFollow camFollow;

        [Header("Controller Components")]
        [SerializeField, Range(1, 20)] private float inputSmoothing = 8f;

        private Vector3 raw_input;
        private Vector3 calculated_input;
        private Vector3 world_input;

        private EntityPrimaryState state;
        private EntityActionState action;

        //Overlap detect
        private LayerMask enemyLayerMask;
        private LayerMask miscLayerMask;
        private Collider[] enemyBuffer = new Collider[200];
        private List<Collider> reducedEnemyBuffer = new(200);
        private Collider[] miscBuffer = new Collider[200];
        private List<Gatherable> gatherableBuffer = new(200);

        //private Tower[] towerBuffer = new Tower[200];

        private float actionRemainingTime = 0;
        private bool InAction = false;

        //Action Targets
        private UnitIDInstance<Enemy, EnemySO> currentAttackTarget;
        private Construct currentSummonTarget;
        private List<UnitIDInstance<Gatherable, GatherableSO>> gatherableTargets = new(10);
        private List<Construct> currentRepairTargets = new(10);


        private void Awake()
        {
            enemyLayerMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Enemy);
            miscLayerMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Gatherable);
        }

        public void AssignPlayer(PlayerSO playerSO)
        {
            AssignEntity(playerSO);

            this.playerSO = playerSO;
            playerAbility = new(playerSO.DefaultAbility, this);
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

                rigidBody.MovePosition(rigidBody.position + baseMovementSpeed * Time.deltaTime * calculated_input);
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
                if (action != EntityActionState.None)
                {
                    ResolveCurrentAction();
                }

                else
                {
                    GetActionFromProximity();
                }
            }

            if (Move())
            {
                ChangeState(EntityPrimaryState.Move);
            }

            else
            {
                ChangeState(EntityPrimaryState.Idle);
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
            }
        }

        public void GetActionFromProximity()
        {

            if (!playerAbility.IsCoolingDown())
            {
                int numEnemyColliders = Physics.OverlapSphereNonAlloc(transform.position, enemySphereRadius, enemyBuffer, enemyLayerMask);

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

                        ChangeAction(EntityActionState.Attack);
                    }

                    return;
                }
            }

            int numMiscColliders = Physics.OverlapSphereNonAlloc(transform.position, miscSphereRadius, miscBuffer, miscLayerMask);
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

        public void ChangeState(EntityPrimaryState entityState)
        {
            if (entityState == state) return;

            action = EntityActionState.None;

            Animator.ToggleWalkAnimation(false);
            //Animator.ToggleDeathAnimation(false);


            switch (entityState)
            {
                case EntityPrimaryState.Move:
                    Animator.ToggleWalkAnimation(true);
                    state = EntityPrimaryState.Move;
                    break;
                case EntityPrimaryState.Dead:
                    //Toggle Death Animation
                    break;
                case EntityPrimaryState.Idle:
                    state = EntityPrimaryState.Idle;
                    //Idle Toggle is automatic if everything else is off
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
                    EvaluateAnimation(playerSO.AttackAnimation, baseAttackSpeed);
                    actionRemainingTime = baseAttackSpeed;

                    action = EntityActionState.Attack;
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
                            EvaluateAnimation(gatherableUnit.Unit.UnitSO.GatheringAnimation, baseGatheringSpeed);
                            actionRemainingTime = baseGatheringSpeed;
                            action = EntityActionState.Gather;
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

        private void EvaluateAnimation(
            EntityActionAnimation entityActionAnimation, float actionSpeed)
        {

            switch (entityActionAnimation)
            {
                case EntityActionAnimation.Attack:
                    Animator.TriggerAttackAnimation(1 / actionSpeed);
                    break;
                case EntityActionAnimation.Chop:
                    Animator.TriggerChopAnimation(1 / actionSpeed);
                    break;
                case EntityActionAnimation.Mine:
                    Animator.TriggerMineAnimation(1 / actionSpeed);
                    break;
                case EntityActionAnimation.Summon:
                    Animator.TriggerSummonAnimation(1 / actionSpeed);
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
                    gatherableInstance.Unit.Gather();
                }
            }

        }
    }

}