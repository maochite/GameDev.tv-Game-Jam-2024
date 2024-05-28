using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Entities
{
    public enum EntityActionAnimation
    {
        Chop,
        Mine,
        Attack,
        Summon,
    }


    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        public enum EntityActionAnimation
        {
            Chop,
            Mine,
            Attack,
            Summon,
        }


        public Transform EntityTransform;
        public AnimatorOverrideController UpAnimatorController;
        public AnimatorOverrideController DownAnimatorController;
        public AnimatorOverrideController LeftAnimatorController;
        public AnimatorOverrideController RightAnimatorController;

        protected SpriteRenderer spriteRenderer;
        protected Animator animator;
        protected Camera PlayerCamera;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            PlayerCamera = Camera.main;
        }

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0.0f, EntityTransform.rotation.z * -1.0f, 0.0f);
            UpdateSprite(Mathf.DeltaAngle(PlayerCamera.transform.eulerAngles.y + 180, EntityTransform.eulerAngles.y));
        }

        protected virtual void UpdateSprite(float angle)
        {
            float angleAbs = Mathf.Abs(angle);

            // Debug.Log(rotationAbs);

            if (angleAbs < 25)
            {
                animator.runtimeAnimatorController = DownAnimatorController;
            }
            else if (angleAbs < 145)
            {
                if (angle < 0)
                {
                    animator.runtimeAnimatorController = RightAnimatorController;
                }

                else animator.runtimeAnimatorController = LeftAnimatorController;
            }
            else
            {
                animator.runtimeAnimatorController = UpAnimatorController;
            }
        }

        public void ToggleIdleAnimation(bool toggle)
        {
            animator.SetBool("Idle", toggle);
        }

        public void ToggleWalkAnimation(bool toggle)
        {
            animator.SetBool("Walk", toggle);
        }

        public void TriggerAttackAnimation()
        {
            animator.SetTrigger("Attack");
        }

        public void TriggerMineAnimation()
        {
            animator.SetTrigger("Mine");
        }

        public void TriggerChopAnimation()
        {
            animator.SetTrigger("Chop");
        }

        public void TriggerSummonAnimation()
        {
            animator.SetTrigger("Summon");
        }


        public void ToggleDeathAnimation()
        {
            animator.SetBool("isAlive", false);
        }

        public void ChangeAnimationMultiplier(float animationSpeed)
        {
            animator.SetFloat("ActionSpeed", 1 / animationSpeed);
        }
    }
}