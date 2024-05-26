
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

namespace Unit.Entities
{
    public enum EntityState
    {
        Move,
        Attack,
        Chop,
        Mine,
        Idle,
        Dead,
        Summon,
    }

    public class Player : Entity<PlayerSO>
    {
        [Header("Player Prefab Components")]
        [field: SerializeField] private Rigidbody rigidBody;
        private PlayerSO playerSO;
        private AbilityPrimary playerAbility;

        [Header("Player Prefab Fields")]
        [field: SerializeField] public float enemySphereRadius = 20f;
        [field: SerializeField] public float miscSphereRadius = 1f;
        [field: SerializeField] public float gatheringCooldown = 1f;

        [Header("Camera Components")]
        //[SerializeField] private CameraFollow camFollow;

        [Header("Controller Components")]
        [SerializeField, Range(1, 20)] private float inputSmoothing = 8f;

        private float lastGatheringCooldownTime;

        private float movementSpeed = 5;
        private Vector3 raw_input;
        private Vector3 calculated_input;
        private Vector3 world_input;

        private EntityState state;

        //Overlap detect
        private LayerMask enemyLayerMask;
        private LayerMask miscLayerMask;
        private Collider[] enemyBuffer = new Collider[200];
        private Collider[] miscBuffer = new Collider[200];
        private List<Gatherable> gatherableBuffer = new(200);

        //private Tower[] towerBuffer = new Tower[200];

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

                rigidBody.MovePosition(rigidBody.position + movementSpeed * Time.deltaTime * calculated_input);
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
            if(state == EntityState.Idle)
            {
                ProximityAction();
            }

            if (Move())
            {
                ChangeState(EntityState.Move);
            }

            else
            {
                ChangeState(EntityState.Idle);
            }
            
        }

        public void ProximityAction()
        {
            int numEnemyColliders = Physics.OverlapSphereNonAlloc(transform.position, enemySphereRadius, enemyBuffer, enemyLayerMask);

            if (numEnemyColliders > 0)
            {
                var closestTransform = TransformUtility.FindClosestTransformSqr(transform.position, enemyBuffer);

                //if (Attack())
                //{
                //    ChangeState(EntityState.Attack);
                //}

                return;
            }

            int numMiscColliders = Physics.OverlapSphereNonAlloc(transform.position, miscSphereRadius, miscBuffer, miscLayerMask);
            gatherableBuffer.Clear();

            if(!IsGathering())
            {
                for (int i = 0; i < numMiscColliders; i++)
                {

                    if (miscBuffer[i].TryGetComponent(out Gatherable gatherable))
                    {
                        gatherableBuffer.Add(gatherable);
                    }
                }

                if (gatherableBuffer.Count != 0)
                {
                    GatherResources(gatherableBuffer);

                    return;
                }
            }

        }

        public void ChangeState(EntityState entityState)
        {
            if(state == EntityState.Move)
            {
                Animator.ToggleWalkAnimation(false);
            }

            switch (entityState)
            {
                case EntityState.Move:
                    Animator.ToggleWalkAnimation(true);
                    state = EntityState.Move;
                    break;
                case EntityState.Attack:
                    Animator.TriggerAttackAnimation(1 / playerAbility.AbilitySO.AttributeData.Cooldown);
                    state = EntityState.Attack;
                    break;
                case EntityState.Summon:
                    break;
                case EntityState.Chop:
                    Animator.TriggerChopAnimation();
                    state = EntityState.Chop;
                    break;
                case EntityState.Mine:
                    Animator.TriggerMineAnimation();
                    state = EntityState.Mine;
                    break;
                case EntityState.Dead:
                    break;
                case EntityState.Idle:
                    state = EntityState.Idle;
                    break;
                default:
                    break;
            }
        }

        public bool IsAttacking()
        {
            if (playerAbility.IsCoolingDown())
            {
                return true;
            }

            else return false;
        }

        public bool IsGathering()
        {
            if (Time.time < lastGatheringCooldownTime)
            {
                return true;
            }

            else return false;
        }


        public bool Attack()
        {
            if (Input.GetMouseButton(0))
            {
                if(GetMouseWorldPosition(out Vector3 pos) 
                    && playerAbility.TryCast(pos, out _))
                {
                    Look(pos);
                    ChangeState(EntityState.Attack);
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

        private void GatherResources(List<Gatherable> gatherables)
        {
            if (gatherables == null || gatherables.Count == 0) return;

            if (gatherables[0].UnitSO.ResourceType == ResourceType.Wood)
            {
                ChangeState(EntityState.Chop);
            }

            else if (gatherables[0].UnitSO.ResourceType == ResourceType.Stone
                || gatherables[0].UnitSO.ResourceType == ResourceType.Gold)
            {
                ChangeState(EntityState.Mine);
            }

            foreach (Gatherable gatherable in gatherables)
            {
                gatherable.Gather();
            }

            lastGatheringCooldownTime = Time.time + gatheringCooldown;
        }
    }

    public static class Helpers
    {
        //private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
        //public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
    }
}