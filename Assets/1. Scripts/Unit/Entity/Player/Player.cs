
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Ability;

namespace Unit.Entity
{
    public enum EntityState
    {
        Move,
        Attack,
        Idle,
    }

    public class Player : Entity<PlayerSO>
    {
        [Header("Player Components")]
        [field: SerializeField] private Rigidbody rigidBody;
        private PlayerSO playerSO;
        private AbilityPrimary playerAbility;

        [Header("Camera Components")]
        //[SerializeField] private CameraFollow camFollow;

        [Header("Controller Components")]
        [SerializeField, Range(1, 20)] private float inputSmoothing = 8f;

        private float movementSpeed = 5;
        private Vector3 raw_input;
        private Vector3 calculated_input;
        private Vector3 world_input;

        private EntityState state;


        public void AssignPlayer(PlayerSO playerSO)
        {
            AssignEntity(playerSO);

            this.playerSO = playerSO;
            playerAbility = new(playerSO.DefaultAbility, this);
        }

        private void GatherInput()
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
            GatherInput();


        }

        private void FixedUpdate()
        {
            if (!IsAttacking())
            {
                if (Attack())
                {
                    ChangeState(EntityState.Attack);
                }

                else if (Move())
                {
                    ChangeState(EntityState.Move);
                }

                else
                {
                    ChangeState(EntityState.Idle);
                }
            }

            else
            {
                ChangeState(EntityState.Idle);
            }
            
        }

        public void ChangeState(EntityState entityState)
        {
            if(state == EntityState.Move)
            {
                Animator.ToggleWalkAnimation(false);
            }

            if(entityState == EntityState.Move)
            {
                Animator.ToggleWalkAnimation(true);
                state = EntityState.Move;
            }

            if (entityState == EntityState.Attack)
            {
                Animator.TriggerAttackAnimation(1 / playerAbility.AbilitySO.AttributeData.Cooldown);
                state = EntityState.Attack;
            }
        }

        public bool IsAttacking()
        {
            if (state == EntityState.Attack)
            {
                if (playerAbility.IsCoolingDown())
                {
                    Debug.Log("CHECK");
                    return true;
                }

                else
                {
                    ChangeState(EntityState.Idle);
                    return false;
                }
            }

            return false;
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

    }

    public static class Helpers
    {
        //private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
        //public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
    }
}