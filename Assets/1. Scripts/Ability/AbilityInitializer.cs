
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Audio;
using static AbilityTools;


namespace Ability
{

    public struct AbilityInitializeData
    {
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
        public Vector3 ModifiedPosition;
        public Quaternion LookRotation;

        public AbilityInitializeData(
            Vector3 startPosition,
            Vector3 targetPosition,
            Vector3 modifiedPosition,
            Quaternion lookRotation)
        {
            StartPosition = startPosition;
            ModifiedPosition = modifiedPosition;
            TargetPosition = targetPosition;
            LookRotation = lookRotation;
        }
    }

    public class AbilityCastingData
    {
        public Ability Ability { get; set; }
        public Transform SourceTransform { get; set; }
        public Vector3 StartPoint { get; set; }
        public Vector3 TargetPoint { get; set; }
        public HashSet<Guid> ExcludedTargets { get; set; }

        /// <summary>
        /// Targeting struct where all information is provided on instantiation
        /// </summary>
        public AbilityCastingData(
            Ability ability,
            Transform sourceTransform,
            Vector3 targetPoint,
            HashSet<Guid> excludedTargets = null)
        {
            Ability = ability;
            SourceTransform = sourceTransform;
            StartPoint = sourceTransform.position;
            TargetPoint = targetPoint;

            if (excludedTargets == null)
            {
                ExcludedTargets = new();
            }

            else ExcludedTargets = excludedTargets;
        }
    }


    public class AbilityInitializer : StaticInstance<AbilityInitializer>
    {
        private struct InitialCompositionData
        {
            public bool updateStartDirection;

            public Vector3 forwardDir;

            public Vector3 startDirection;
            public Vector3 startPos;
            public Vector3 targetPos;

            public Matrix4x4 startPosMatrix;
            public Matrix4x4 targetPosMatrix;

            public Vector3 startPosOffset;
            public Vector3 targetPosOffset;
            public Vector3 modifiedPosOffset;

            public Quaternion rotation;
            public Vector3 offsetPosition;
        }

        public struct InitialSpreadData
        {
            public int abilityAmount;
            public float spreadDegrees;
            public float maxSpreadDegrees;
            public float rotOffset;
        }

        private struct InitialIncrementData
        {
            public int numIncrements;
            public int incrementsPerGap;
            public float gapTime;
            public float timeBetweenIncrements;
            public float fullDuration;
        }

        [SerializeField, Header("Prefab")] AbilityObject AbilityObjectPrefab;

        [SerializeField, Header("Pool")] int initalPoolSize = 500;
        [SerializeField] int poolExtension = 25;

        //Debug, dont delete
        [SerializeField, ReadOnly] int poolSize = 0;
        [SerializeField, ReadOnly] int currentActive = 0;

        readonly Queue<AbilityObject> abilityPool = new();
        private HashSet<AbilityObject> currentDequeuedObjects = new(50);

        public Coroutine Initialize(AbilityCastingData castingData)
        {
            return StartCoroutine(Routine(castingData));
        }

        public IEnumerator Routine(AbilityCastingData castingData)
        {
            Ability ability = castingData.Ability;
            float timeElapsed = 0;
            float timeNormalized = 0;

            InitialIncrementData incrementData = SetIncrementData(ability);

            for (int i = 0; i < incrementData.numIncrements; i++)
            {
                InitialCompositionData compositionData = SetCompositionData(ability, ref castingData);
                InitialSpreadData spreadData = SetAbilitySpreadData(ability);

                UpdateStartAndTargetPositionOffset(ability, ref compositionData);

                timeNormalized =
                     Mathf.Clamp01(timeElapsed / incrementData.fullDuration);

                if (incrementData.incrementsPerGap != 0
                    && i != 0
                    && i % incrementData.incrementsPerGap == 0)
                {
                    timeElapsed += incrementData.gapTime;
                    yield return new WaitForSeconds(incrementData.gapTime);
                    timeNormalized =
                         Mathf.Clamp01(timeElapsed / incrementData.fullDuration);
                }

                UpdateRotationOffset(ability, ref compositionData, timeNormalized);
                UpdateModifiedPositionOffset(ability, ref compositionData);

                UpdateStartPositionAndDirection(ability, ref compositionData);
                ApplyRotationWithForwardOffset(ref compositionData);
                ApplySpreadRotationsThenAssign(ref compositionData, ref spreadData, ref castingData);


                if(ability.AbilitySO.AudioSource != null)
                {
                    var wrapper = AudioManager.Instance.RequestAudioSource(ability.AbilitySO.AudioSource);
                    wrapper.Play(castingData.SourceTransform.position + castingData.SourceTransform.forward);

                }

                timeElapsed += incrementData.timeBetweenIncrements;
                yield return new WaitForSeconds(incrementData.timeBetweenIncrements);
            }
        }

        private InitialCompositionData SetCompositionData(Ability ability, ref AbilityCastingData castingData)
        {
            InitialCompositionData compositionData = new()
            {
                updateStartDirection = true,
            };

            Vector3 relativePos;

            if (ability.AbilitySO.IncrementData.NewStartPosPerIncrement)
            {
                relativePos = castingData.SourceTransform.position;
            }

            else relativePos = castingData.StartPoint;

            if(ability.AbilitySO.CompositionData.OffsetData.Y_StartPositionIsZero)
            {
                relativePos = new(relativePos.x, 0, relativePos.z);
            }

             compositionData.forwardDir = (castingData.TargetPoint - relativePos).normalized;

            if (compositionData.forwardDir == Vector3.zero) compositionData.forwardDir = Vector3.forward;

            if (ability.AbilitySO.CompositionData.RelativeStartLocation == RelativeStartLocation.Source)
            {
                compositionData.startPosMatrix = Matrix4x4.TRS(
                    relativePos, Quaternion.LookRotation(compositionData.forwardDir), Vector3.one);

                compositionData.targetPosMatrix = Matrix4x4.TRS(
                    castingData.TargetPoint, Quaternion.LookRotation(compositionData.forwardDir), Vector3.one);
            }

            else
            {
                compositionData.startPosMatrix = Matrix4x4.TRS(
                    castingData.TargetPoint, Quaternion.LookRotation(compositionData.forwardDir), Vector3.one);

                compositionData.targetPosMatrix = compositionData.startPosMatrix;
            }

            return compositionData;
        }

        private InitialSpreadData SetAbilitySpreadData(Ability ability)
        {

            InitialSpreadData abilitySpreadData = new()
            {
                rotOffset = 0,
                abilityAmount = Mathf.RoundToInt(ability.AbilitySO.CapacityData.AbilityAmount),
                maxSpreadDegrees = ability.AbilitySO.CapacityData.MaximumSpreadAngle
            };

            abilitySpreadData.spreadDegrees = ability.AbilitySO.CapacityData.SpreadAngle
                / abilitySpreadData.abilityAmount;

            return abilitySpreadData;
        }

        private InitialIncrementData SetIncrementData(Ability ability)
        {
            AbilitySO abilitySO = ability.AbilitySO;

            InitialIncrementData incrementInfo = new();

            incrementInfo.numIncrements = abilitySO.IncrementData.NumIncrements;

            incrementInfo.incrementsPerGap = abilitySO.IncrementData.IncrementsPerGap;

            incrementInfo.gapTime = abilitySO.IncrementData.GapTime;

            incrementInfo.timeBetweenIncrements = ability.AbilitySO.IncrementData.TimeBetweenIncrements;

            int numGaps = (abilitySO.IncrementData.NumIncrements - 1) / incrementInfo.incrementsPerGap;
            incrementInfo.fullDuration = (numGaps * incrementInfo.gapTime) + (incrementInfo.timeBetweenIncrements * abilitySO.IncrementData.NumIncrements);

            return incrementInfo;
        }

        private void UpdateStartAndTargetPositionOffset(Ability ability, ref InitialCompositionData compositionData)
        {
            //Position offset values
            if (ability.AbilitySO.CompositionData.OffsetData.StartPositionOffsetType == StartPositionOffsetType.Fixed)
            {
                compositionData.startPosOffset = ability.AbilitySO.CompositionData.OffsetData.StartPositionOffset;
            }

            else //(ability.StorableSO.CompositionData.PositionOffsetType == PositionOffsetType.RandomRange)
            {
                compositionData.startPosOffset = RandomRangeVector(
                    ability.AbilitySO.CompositionData.OffsetData.StartPositionRandomRangeA,
                    ability.AbilitySO.CompositionData.OffsetData.StartPositionRandomRangeB);
            }

            //Target offset values
            if (ability.AbilitySO.CompositionData.OffsetData.TargetPositionOffsetType == TargetOffsetType.Fixed)
            {
                compositionData.targetPosOffset = ability.AbilitySO.CompositionData.OffsetData.TargetPositionOffset;
            }

            else //(ability.StorableSO.CompositionData.TargetOffsetType == TargetOffsetType.RandomRange)
            {
                compositionData.targetPosOffset = RandomRangeVector(
                    ability.AbilitySO.CompositionData.OffsetData.TargetPositionRandomRangeA,
                    ability.AbilitySO.CompositionData.OffsetData.TargetPositionRandomRangeB);
            }
        }


        private void UpdateStartPositionAndDirection(Ability ability, ref InitialCompositionData compositionData)
        {

            //Resolve positional values after being modified
            if (ability.AbilitySO.CompositionData.LocalCoordinates)
            {
                compositionData.startPos = compositionData.startPosMatrix.MultiplyPoint3x4(compositionData.startPosOffset);
                compositionData.targetPos = compositionData.targetPosMatrix.MultiplyPoint3x4(compositionData.targetPosOffset);
            }

            else
            {
                compositionData.startPos = compositionData.startPosMatrix.GetPosition() + compositionData.startPosOffset;
                compositionData.targetPos = compositionData.targetPosMatrix.GetPosition() + compositionData.targetPosOffset;
            }

            //Get Direction from resolved positional values
            if (compositionData.updateStartDirection)
            {
                compositionData.startDirection = (compositionData.targetPos - compositionData.startPos).normalized;
            }

            //Fallback if a direction vector becomes zeroed. This can happen if position and target location are the same. 
            //We should respect direction even if the ability has no movement as forward offset position and rotation can still apply
            if (compositionData.startDirection == Vector3.zero)
            {
                if (ability.AbilitySO.CompositionData.LocalCoordinates)
                {
                    compositionData.startDirection = compositionData.targetPosMatrix.GetColumn(2);
                }

                else compositionData.startDirection = Vector3.forward;
            }
        }

        private void UpdateRotationOffset(Ability ability, ref InitialCompositionData compositionData, float normalizedTime)
        {
            Vector3 rotationalOffset;

            //Rotation offset values
            if (ability.AbilitySO.CompositionData.OffsetData.RotationOffsetType == RotationOffsetType.Fixed)
            {
                rotationalOffset = ability.AbilitySO.CompositionData.OffsetData.RotationOffset;

                compositionData.rotation = AnglesToRotation(rotationalOffset);
            }

            else //(ability.StorableSO.CompositionData.RotationOffsetType == RotationOffsetType.RandomRange)
            {
                Vector3 randomRotOffset = RandomRangeVector(ability.AbilitySO.CompositionData.OffsetData.RotationRandomRangeA,
                    ability.AbilitySO.CompositionData.OffsetData.RotationRandomRangeB);

                rotationalOffset = randomRotOffset;

                compositionData.rotation = AnglesToRotation(rotationalOffset);
            }

            float y = ability.AbilitySO.IncrementData.AngleCurveY.Evaluate(normalizedTime) * 360.0f;

            compositionData.rotation *= AnglesToRotation(new Vector3(0, y, 0));
        }

        private void UpdateModifiedPositionOffset(Ability ability, ref InitialCompositionData compositionData)
        {
            //Forward Offset values
            if (ability.AbilitySO.CompositionData.OffsetData.ModifiedPositionOffsetType == ModifiedPositionOffsetType.Fixed)
            {
                compositionData.modifiedPosOffset = ability.AbilitySO.CompositionData.OffsetData.ModifiedPositionOffset;
            }

            else //(ability.StorableSO.CompositionData.ForwardOffsetType == ForwardOffsetType.RandomRange)
            {
                compositionData.modifiedPosOffset = RandomRangeVector(
                    ability.AbilitySO.CompositionData.OffsetData.ModifiedRandomRangeA,
                    ability.AbilitySO.CompositionData.OffsetData.ModifiedRandomRangeB);
            }
        }

        private void ApplyRotationWithForwardOffset(ref InitialCompositionData compositionData)
        {
            compositionData.rotation = Quaternion.LookRotation(compositionData.startDirection) * compositionData.rotation;
            //compositionData.offsetPosition = compositionData.rotation * Vector3.forward * compositionData.modifiedPosOffset.z;

            Vector3 rightDir = compositionData.rotation * Vector3.right * compositionData.modifiedPosOffset.x;
            Vector3 upDir = compositionData.rotation * Vector3.up * compositionData.modifiedPosOffset.y;
            Vector3 forwardDir = compositionData.rotation * Vector3.forward * compositionData.modifiedPosOffset.z;

            compositionData.offsetPosition = rightDir + upDir + forwardDir;

            if (compositionData.offsetPosition == Vector3.zero)
            {
                compositionData.offsetPosition = compositionData.rotation * Vector3.forward * 0.01f;
            }
        }

        private void ApplySpreadRotationsThenAssign(
            ref InitialCompositionData compositionData,
            ref InitialSpreadData spreadData,
            ref AbilityCastingData castingData)
        {
            for (int i = 0; i < spreadData.abilityAmount; ++i)
            {

                spreadData.rotOffset += (i & 1) == 1 ? Math.Min(spreadData.spreadDegrees, spreadData.maxSpreadDegrees) : 0;
                spreadData.rotOffset *= -1;

                Quaternion spreadLookRotation = Quaternion.AngleAxis(spreadData.rotOffset, compositionData.rotation * Vector3.up);
                Vector3 spreadPosition = compositionData.startPos + spreadLookRotation * compositionData.offsetPosition;

                Vector3 direction = spreadPosition - compositionData.startPos;

                if (castingData.Ability.AbilitySO.CompositionData.RemoveYDirection)
                {
                    direction.y = 0;
                }


                spreadLookRotation = (direction != Vector3.zero) ? Quaternion.LookRotation(direction) : Quaternion.identity;

                AbilityInitializeData initializerInfo = new(
                    compositionData.startPos,
                    compositionData.targetPos,
                    spreadPosition,
                    spreadLookRotation);

                RequestAbility(castingData, initializerInfo);

            }
        }


        private void OnValidate()
        {
            poolSize = initalPoolSize;
        }

        private void Start()
        {
            ExtendAbilityPool(initalPoolSize);
        }

        private void RequestAbility(AbilityCastingData castingData, AbilityInitializeData initializeData)
        {
            if (abilityPool.Count == 0)
            {
                ExtendAbilityPool(poolExtension);
            }

            AbilityObject ability = abilityPool.Dequeue();
            currentDequeuedObjects.Add(ability);
            currentActive++;

            ability.AssignAbility(castingData, initializeData);
        }

        public void ReturnAbilityToPool(AbilityObject ability)
        {
            ability.gameObject.SetActive(false);
            abilityPool.Enqueue(ability);
            currentDequeuedObjects.Remove(ability);
            currentActive--;
        }


        private void ExtendAbilityPool(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var newAbility = Instantiate(AbilityObjectPrefab, transform);
                newAbility.gameObject.SetActive(false);
                abilityPool.Enqueue(newAbility);
                poolSize++;
            }
        }

        public void ReturnAllAbilityObjects()
        {
            List<AbilityObject> objectsToExpire = new(currentDequeuedObjects);

            // Iterate over the copy
            foreach (AbilityObject abilityObject in objectsToExpire)
            {
                abilityObject.ExpireAbility();
            }
        }
    }

}