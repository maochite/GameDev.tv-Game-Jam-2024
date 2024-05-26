using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
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

    public void ToggleWalkAnimation(bool walking)
    {
        animator.SetBool("Walk", walking);
    }

    public void TriggerAttackAnimation(float attackSpeed)
    {
        animator.SetFloat("AttackSpeed", attackSpeed);
        animator.SetTrigger("Attack");
        //Debug.Log(gameObject.GetComponentInParent<StatsManager>().gameObject.name + " has Attacked " + Time.time);
    }

    public void ToggleCastingAnimation(bool casting)
    {
        //send in and instantiate a particle system too, probably
        animator.SetBool("isCasting", casting);
    }

    //perhaps we should have multiple different cast animations depending on ability
    public void TriggerAfterCastAnimation()
    {
        animator.SetTrigger("hasCasted");
    }

    public void ToggleDeathAnimation()
    {
        animator.SetBool("isAlive", false);
    }
}
