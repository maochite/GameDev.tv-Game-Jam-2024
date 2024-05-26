using Particle;
using System;
using System.Collections;
using System.Collections.Generic;
using Unit.Entity;
using UnityEngine;
using UnityEngine.VFX;

/*
 * This class handles ability gameobjects after being instantiated by their unique scriptableobject
 */

namespace Ability
{

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class AbilityObject : MonoBehaviour
    {

        //readonly HashSet<IBaseTrigger> setTriggerCooldowns = new();

        //private AbilitySubsystemData abilitySubsystem;

        private enum TargetCastRequirements
        {
            NearestTarget,
            FurthestTarget,
            RandomTarget,
        }

        public struct CurveData
        {
            public AnimationCurve curve;
            public float baseValue;
        }

        // Constants
        private const float FullRotation = 360.0f;

        // Fields
        private Ability ability;
        public Ability Ability => ability;

        private AbilitySO abilitySO;
        public AbilitySO AbilitySO => abilitySO;
        //[field: SerializeField] public MeshRenderer ShadowMesh { get; private set; }

        private AbilityCastingData castingData;
        private AbilityInitializeData initializeData;
        private ParticleWrapper primaryParticles;
        private ParticleWrapper castingParticles;
        private ParticleWrapper hitParticles;

        //Unit History Structures
        private readonly Dictionary<Guid, float> previousTargetHits = new(500);
        private readonly HashSet<Collider> previousColliderHits = new(500);
        private HashSet<Guid> excludedTargets = new();

        private readonly Collider[] totalColliders = new Collider[500];
        private int remainingHits = 0;
        private int remainingTriggerHits = 0;

        //State Data
        private float currentAbilityTime;
        private float abilityTimeElapsed;
        private float abilityTimeNormalized;

        private float currentSpeed;
        private float currentSize;
        private Vector3 currentAngles;
        private Quaternion currentRotation;
        private float currentHomingSpeed;

        private CurveData speedCurveData;
        private CurveData sizeCurveData;
        private CurveData angleXCurveData;
        private CurveData angleYCurveData;
        private CurveData angleZCurveData;
        private CurveData homingSpeedCurveData;
        private Quaternion currentAngleRotation;
        private float currentDeltaSpeed;

        //Vectors and Rotation
        private Quaternion homingRotation;

        //Ability Object Flags
        private bool returnAbilityToPool = false;
        private bool hasHomingEnabled = false;
        private bool hasHomingTarget = false;

        //Mask
        LayerMask hitmask;
        LayerMask collisionMask;

        //FindTargetsInRange()
        private readonly List<IEntity> enemiesInRange = new(200);

        private Vector3 closestTarget;
        private float closestDistance;

        private SphereCollider abilityCollider;
        private Rigidbody rb;

        private Quaternion hitRotation = Quaternion.identity;

        private Collider currentHitTarget = null;

        public Quaternion PreviousRotation { get; private set; }

        // Properties
        public float CurrentUpdateSize => currentSize;
        public float CurrentUpdateSpeed => currentSpeed;
        public Vector3 CurrentObjectAngles => currentAngles;

        private void SetAnglesFromCurve()
        {
            float evaluationTime = abilityTimeNormalized;

            float baseValue = angleXCurveData.baseValue;

            currentAngles.x = angleXCurveData.curve.Evaluate(evaluationTime)
                * baseValue * Time.fixedDeltaTime;

            currentAngles.y = angleYCurveData.curve.Evaluate(evaluationTime)
                * baseValue * Time.fixedDeltaTime;

            currentAngles.z = angleZCurveData.curve.Evaluate(evaluationTime)
                * baseValue * Time.fixedDeltaTime;

            currentAngleRotation = AbilityTools.AnglesToRotation(currentAngles);
        }

        private void SetSizeFromCurve()
        {
            float evaluationTime = abilityTimeNormalized;


            float size = sizeCurveData.curve.Evaluate(evaluationTime)
                * sizeCurveData.baseValue;

            if (size <= 0)
            {
                size = 0.01f;
            }

            this.currentSize = size;
            abilityCollider.radius = currentSize / 2;
            //ShadowMesh.transform.localScale = new Vector3(size, size, size);

            if (primaryParticles != null) primaryParticles.ChangeParticalSize(size);
        }

        private void SetSpeedFromCurve()
        {
            float evaluationTime = abilityTimeNormalized;

            float speed = speedCurveData.curve.Evaluate(evaluationTime)
                * speedCurveData.baseValue;

            this.currentSpeed = speed;
            currentDeltaSpeed = speed * Time.fixedDeltaTime;
        }


        private void SetHomingSpeedFromCurve()
        {
            if (!abilitySO.MovementData.HomingData.Enabled) return;

            float evaluationTime = abilityTimeNormalized;

            float rotSpeed = homingSpeedCurveData.curve.Evaluate(evaluationTime)
                * homingSpeedCurveData.baseValue;

            currentHomingSpeed = rotSpeed;
        }

        private void Awake()
        {
            abilityCollider = GetComponent<SphereCollider>();
            rb = GetComponent<Rigidbody>();
        }

        public void AssignAbility(AbilityCastingData castingData, AbilityInitializeData initializeData)
        {
            ability = castingData.Ability;
            abilitySO = castingData.Ability.AbilitySO;

            transform.SetPositionAndRotation(initializeData.ModifiedPosition, initializeData.LookRotation);

            homingRotation = transform.rotation;

            this.castingData = castingData;
            this.initializeData = initializeData;

            returnAbilityToPool = false;
            currentHitTarget = null;
            hasHomingEnabled = abilitySO.MovementData.HomingData.Enabled;
            remainingHits = abilitySO.HitData.MaxHits;

            previousColliderHits.Clear();
            previousTargetHits.Clear();
            excludedTargets = castingData.ExcludedTargets;

            abilityTimeNormalized = 0;
            abilityTimeElapsed = 0;
            currentAbilityTime = abilitySO.AttributeData.Duration;

            speedCurveData.curve = abilitySO.MovementData.SpeedCurveMultiplier;
            sizeCurveData.curve = abilitySO.SizeData.SizeCurveMultiplier;

            angleXCurveData.curve = abilitySO.ActiveRotationData.AngleCurveX;
            angleYCurveData.curve = abilitySO.ActiveRotationData.AngleCurveY;
            angleZCurveData.curve = abilitySO.ActiveRotationData.AngleCurveZ;

            speedCurveData.baseValue = ability.AbilitySpeed;
            sizeCurveData.baseValue = ability.AbilitySize;
            angleXCurveData.baseValue = FullRotation;

            homingSpeedCurveData.curve = abilitySO.MovementData.HomingData.RotSpeedMultiplier;
            homingSpeedCurveData.baseValue = abilitySO.MovementData.HomingData.BaseRotSpeed;

            abilityCollider.radius = ability.AbilitySize / 2;

            //ShadowMesh.transform.localScale = 
            //    new Vector3(ability.AbilitySize, ability.AbilitySize, ability.AbilitySize);

            UpdateCurves();

            hitmask = LayerUtility.LayerMaskByLayerEnumType((LayerEnum)abilitySO.HitMask);
            collisionMask = LayerUtility.LayerMaskByLayerEnumType((LayerEnum)abilitySO.ColliderMask);

            InitializeParticleData();

            gameObject.SetActive(true);
            StartCoroutine(HomingAbility());
        }

        private void InitializeParticleData()
        {
            if(abilitySO.VFXData.ParticlePrimary != null)
            {
                primaryParticles = ParticleManager.Instance.RequestParticleSystem(abilitySO.VFXData.ParticlePrimary);
                primaryParticles.AttachParticleSystem(transform);
                primaryParticles.ChangeParticalSize(currentSize);
                primaryParticles.Activate();
            }
        }

        private void FixedUpdate()
        {

            UpdateTime();
            UpdateCurves();
            UpdateMovement();

            currentHitTarget = null;

            if (abilityTimeElapsed >= currentAbilityTime || returnAbilityToPool)
            {
                ExpireAbility();
                return;
            }
        }

        public void UpdateTime()
        {
            abilityTimeElapsed += Time.fixedDeltaTime;
            abilityTimeNormalized =
                Mathf.Clamp01(abilityTimeElapsed / currentAbilityTime);

        }

        void UpdateCurves()
        {
            SetSizeFromCurve();
            SetSpeedFromCurve();
            SetAnglesFromCurve();
            SetHomingSpeedFromCurve();
        }

        void UpdateMovement()
        {
            PreviousRotation = transform.rotation;

            if (currentHitTarget != null)
            {
                rb.MoveRotation(hitRotation);

                rb.MovePosition(
                    transform.position
                    + transform.forward
                    * currentDeltaSpeed);

                hitRotation = Quaternion.identity;
            }

            else if (hasHomingEnabled && hasHomingTarget)
            {

                rb.MoveRotation(homingRotation * currentAngleRotation);
                rb.MovePosition(
                    transform.position
                    + transform.forward
                    * currentDeltaSpeed);

                hasHomingTarget = false;
            }

            else
            {

                rb.MoveRotation(transform.rotation * currentAngleRotation);
                rb.MovePosition(transform.position
                    + transform.forward
                    * currentDeltaSpeed);
            }
        }

        public void HitResponse(Collider currentHitCollider)
        {

            if (abilitySO.HitData.HitResponseType == HitResponseType.None) return;

            if (abilitySO.HitData.HitResponseType == HitResponseType.Chain)
            {
                ChainAbility(currentHitCollider);
            }

            else if (abilitySO.HitData.HitResponseType == HitResponseType.Ricochet)
            {
                RicochetAbility(currentHitCollider);
            }

            currentHitTarget = currentHitCollider;
            remainingHits--;

            if (remainingHits <= 0)
            {
                returnAbilityToPool = true;
            }
        }

        private bool FindCompatibleEntityInRange(Vector3 pos, float radius, out IEntity newTarget, TargetCastRequirements targetRequirement)
        {
            int numTotalColliders = Physics.OverlapSphereNonAlloc(transform.position, radius, totalColliders, hitmask);

            (IEntity target, float distance) nearestTarget = new(null, radius);
            (IEntity target, float distance) furthestTarget = new(null, 0);

            enemiesInRange.Clear();
            newTarget = null;

            for (int i = 0; i < numTotalColliders; i++)
            {
                var collider = totalColliders[i];

                if (collider.gameObject.TryGetComponent(out IEntity target)
                    && TargetHitEligibility(target))
                {
                    // Check line of sight
                    if (IsInLineOfSight(target, out RaycastHit hit))
                    {
                        if (hit.distance < nearestTarget.distance)
                        {
                            nearestTarget.target = target;
                            nearestTarget.distance = hit.distance;
                        }

                        if (hit.distance > furthestTarget.distance)
                        {
                            furthestTarget.target = target;
                            furthestTarget.distance = hit.distance;
                        }

                        enemiesInRange.Add(target);
                    }
                }
            }

            if (targetRequirement == TargetCastRequirements.FurthestTarget
                && furthestTarget.target != null)
            {
                newTarget = furthestTarget.target;
                return true;
            }

            else if (targetRequirement == TargetCastRequirements.NearestTarget
               && nearestTarget.target != null)
            {
                newTarget = nearestTarget.target;
                return true;
            }

            else if (targetRequirement == TargetCastRequirements.RandomTarget
                && enemiesInRange.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, enemiesInRange.Count);

                // Return the item at the random index
                newTarget = enemiesInRange[randomIndex];
                return true;
            }

            return false;
        }

        private bool IsInLineOfSight(IEntity target, out RaycastHit hit)
        {
            int originalLayer = target.Transform.gameObject.layer;

            target.Transform.gameObject.layer = LayerUtility.LayerByLayerEnumType(LayerEnum.LineOfSight);
            //entity.Transform.gameObject.layer = LayerMask.NameToLayer("LineOfSight");

            bool isInLineOfSight = Physics.Linecast(
                transform.position,
                target.Transform.position,
                out hit,
                LayerUtility.LayerMaskByLayerEnumType(LayerEnum.LineOfSight));

            target.Transform.gameObject.layer = originalLayer;

            return isInLineOfSight;
        }

        IEnumerator HomingAbility()
        {
            while (true)
            {
                do
                {
                    int randomFrames = UnityEngine.Random.Range(1, 5);

                    for (int i = 0; i < randomFrames; i++)
                    {
                        yield return new WaitForFixedUpdate();
                    }

                } while (!hasHomingEnabled);

                if (FindCompatibleEntityInRange(
                    transform.position,
                    abilitySO.MovementData.HomingData.Range / 2,
                    out IEntity newTarget,
                    TargetCastRequirements.NearestTarget))
                {

                    Vector3 targetDirection = (newTarget.Transform.position - transform.position).normalized;


                    if (abilitySO.MovementData.HomingData.RotateYAxisOnly)
                    {
                        targetDirection.y = 0;
                    }

                    Quaternion targetRotation;

                    targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                    homingRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, currentHomingSpeed * Time.fixedDeltaTime);

                    hasHomingTarget = true;
                };
            }

        }

        //Hit Evalutations Method
        private void OnTriggerStay(Collider collider)
        {
            LayerMask colliderMask = LayerUtility.LayerToLayerMask(collider.gameObject.layer);

            ///Collider is part of targets able to be 'hit'
            if (LayerUtility.CheckMaskOverlap(hitmask, colliderMask))
            {

                //Check if collider is an Entity, and if so we apply associated methods
                if (collider.TryGetComponent(out IEntity target))
                {
                    //Check if IEntity is eligible to be hit, otherwise return
                    if (!TargetHitEligibility(target))
                    {
                        return;
                    }

                    //If IEntity can be hit then call the next associated methods
                    else
                    {
                        //Enemy was hit; add to dictionary
                        previousTargetHits[target.TargetID] = Time.time;
                        EntityHitEvent(target);
                        //VFXHitEvent();

                        //TriggerAbilityOnHit(target);
                        //TriggerEffectOnHit(target);
                    }
                }

                //If collider is also part of the collisional mask, we flag it and return
                if (LayerUtility.CheckMaskOverlap(collisionMask, colliderMask))
                {
                    VFXHitEvent();

                    returnAbilityToPool = true;

                    return;
                }

                //Otherwise, the ability continues the process by responding to the hit
                else if (!previousColliderHits.Contains(collider))
                {

                    if (abilitySO.MovementData.HomingData.DisableHomingOnHit)
                    {
                        hasHomingEnabled = false;
                    }

                    previousColliderHits.Add(collider);

                    HitResponse(collider);
                }
            }

            //Collider is part of the collisional mask, we flag it and return
            else if (LayerUtility.CheckMaskOverlap(collisionMask, colliderMask))
            {
                VFXHitEvent();
                returnAbilityToPool = true;
                return;
            }

            //Collider is not part of the hit or collisional mask, so ignore it and return
            else
            {
                return;
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            previousColliderHits.Remove(collider);
        }

        bool TargetHitEligibility(IEntity target)
        {
            if (excludedTargets.Contains(target.TargetID))
                return false;

            if (!previousTargetHits.ContainsKey(target.TargetID))
            {
                return true; // Entity not in dictionary, so it's eligible
            }

            else if (abilitySO.HitData.EntityHitType == EntityHitType.Interval)
            {
                float lastHitTime = previousTargetHits[target.TargetID];

                return Time.time - lastHitTime >= abilitySO.HitData.TargetHitCooldown;

            }

            else return false;
        }

        bool TargetHitEligibility(Guid guid)
        {
            if (excludedTargets.Contains(guid))
                return false;

            if (!previousTargetHits.ContainsKey(guid))
            {
                return true; // Entity not in dictionary, so it's eligible
            }

            else
            {
                float lastHitTime = previousTargetHits[guid];

                return Time.time - lastHitTime >= abilitySO.HitData.TargetHitCooldown;

            }

        }

        void ChainAbility(Collider currentHitCollider)
        {
            Vector3 collisionPoint = currentHitCollider.ClosestPoint(transform.position);

            if (FindCompatibleEntityInRange(
                collisionPoint,
                abilitySO.HitData.ChainRange / 2,
                out IEntity newTarget,
                TargetCastRequirements.RandomTarget))
            {
                Vector3 targetDirection = (newTarget.Transform.position - transform.position).normalized;

                if (abilitySO.HitData.HitReponseYRotOnly)
                {
                    targetDirection.y = 0;
                }

                Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

                //Force Rotation Change when chaining
                hitRotation = targetRotation;
            }

            else if (abilitySO.HitData.ChainFallback == ChainFallback.DestroyAbility)
            {
                returnAbilityToPool = true;
            }

            else //ChainFallback.Ricochet
            {
                if(!RicochetAbility(currentHitCollider))
                {
                    returnAbilityToPool = true;
                }
            }
        }

        //Sucks, we need raycasts instead
        bool RicochetAbility(Collider currentHitCollider)
        {
            Vector3 adjustedPos = transform.position + (-transform.forward * 2);
            LayerMask layerMask = LayerUtility.LayerToLayerMask(currentHitCollider.gameObject.layer);

            RaycastHit hit;
            if (Physics.Raycast(adjustedPos, transform.forward, out hit, 5, layerMask))
            {

                // Get the collision normal
                Vector3 normal = hit.normal;

                // Reflect the velocity
                Vector3 reflectedVelocity = Vector3.Reflect(transform.forward, normal);

                hitRotation = Quaternion.LookRotation(reflectedVelocity, Vector3.up);

                return true;
            }

            return false;
        }

        void EntityHitEvent(IEntity entity)
        {
            //Debug.Log("Hit entity "+entity + " " + abilitySO.AttributeData.Damage);

            entity.CurrentHealth -= ability.Damage;

        }

        void VFXHitEvent()
        {
            if (abilitySO.VFXData.ParticleHit != null)
            {
                hitParticles = ParticleManager.Instance.RequestParticleSystem(abilitySO.VFXData.ParticleHit);
                hitParticles.SetOrientation(transform.position + -transform.forward * 0.5f, Quaternion.identity);
                hitParticles.ChangeParticalSize(currentSize);
                hitParticles.Activate();
            }
        }

        //HashSet<Guid> TriggerExclusions(PrimaryAbilitySO castableAbility)
        //{
        //    HashSet<Guid> id_set = null;

        //    if (castableAbility.TriggerData.ExcludePreviousEntities)
        //    {
        //        id_set = new();
        //        foreach (Guid guid in excludedTargets)
        //        {
        //            id_set.Add(guid);
        //        }

        //        foreach (var excluded in previousTargetHits)
        //        {
        //            if (!TargetHitEligibility(excluded.Key))
        //            {
        //                id_set.Add(excluded.Key);
        //            }
        //        }

        //        if (id_set.Count == 0) id_set = null;
        //    }

        //    return id_set;
        //}

        //void TriggerEffectOnHit(IEntity hitTarget)
        //{
        //    foreach (EffectTriggerOnHit trigger in ability.EffectTriggersOnHit)
        //    {
        //        trigger.EffectSO.InstantiateEffect(ability.Caster).Apply(hitTarget);
        //    }
        //}

        //void TriggerAbilityOnHit(IEntity hitTarget)
        //{
        //    if (ability is not PrimaryAbility castableAbility || remainingTriggerHits < 1) return;

        //    HashSet<Guid> id_set = TriggerExclusions(castableAbility.PrimaryAbilitySO);

        //    foreach (AbilityTriggerOnHitMod trigger in castableAbility.AbilityTriggersOnHit)
        //    {
        //        GameManager.Instance.AbilityInitializer.Initialize(new(
        //            new AuxiliaryAbility(trigger.AbilitySO, ability.Caster),
        //            transform.position,
        //            hitTarget.Transform.position,
        //            id_set));

        //        remainingTriggerHits--;
        //    }
        //}

        //void TriggerAbilityOnExpiration()
        //{
        //    if (ability is not PrimaryAbility castableAbility) return;

        //    HashSet<Guid> id_set = TriggerExclusions(castableAbility.PrimaryAbilitySO);

        //    foreach (AbilityTriggerOnExpiration trigger in castableAbility.AbilityTriggersOnExpiration)
        //    {
        //        GameManager.Instance.AbilityInitializer.Initialize(new(
        //            new AuxiliaryAbility(trigger.AbilitySO, ability.Caster),
        //            transform.position,
        //            transform.position,
        //            id_set));

        //        remainingTriggerHits--;
        //    }
        //}

        public void ExpireAbility()
        {
            //TriggerAbilityOnExpiration();
            primaryParticles.ReturnParticles(PreviousRotation);
            AbilityInitializer.Instance.ReturnAbilityToPool(this);
        }



        void OnDrawGizmos()
        {
            //// Draw Gizmos for visualization
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, abilityCollider.radius);


            Gizmos.color = Color.grey;
            Gizmos.DrawSphere(initializeData.ModifiedPosition, 0.2f);
        }
    }
}