using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.VFX;
using NaughtyAttributes;
using Audio;



#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Ability
{

    public enum RelativeStartLocation
    {
        Source = 0,
        Target = 1,
    }

    public enum EntityHitType
    {
        Single = 0,
        Interval = 1,
    }

    public enum StartPositionOffsetType
    {
        Fixed = 0,
        RandomRange = 1,
    }

    public enum ModifiedPositionOffsetType
    {
        Fixed = 0,
        RandomRange = 1,
    }

    public enum TargetOffsetType
    {
        Fixed = 0,
        RandomRange = 1,
    }

    public enum RotationOffsetType
    {
        Fixed = 0,
        RandomRange = 1,
    }

    public enum HitResponseType
    {
        None,
        Chain,
        Ricochet,
    }

    public enum ChainFallback
    {
        DestroyAbility,
        Ricochet,
    }

    public abstract class AbilitySO : ScriptableObject
    {
        [field: SerializeField] public HitLayer HitMask { get; private set; }
        [field: SerializeField] public CollisionLayer ColliderMask { get; private set; }
        [field: SerializeField] public AudioSource AudioSource { get; private set; }
        [field: SerializeField] public VFX VFXData { get; private set; }
        [field: SerializeField] public Attribute AttributeData { get; private set; }
        [field: SerializeField] public Composition CompositionData { get; private set; }
        [field: SerializeField] public Incrementation IncrementData { get; private set; }
        [field: SerializeField] public Capacity CapacityData { get; private set; }
        [field: SerializeField] public Size SizeData { get; private set; }
        [field: SerializeField] public ActiveRotation ActiveRotationData { get; private set; }
        [field: SerializeField] public Hit HitData { get; private set; }
        [field: SerializeField] public Movement MovementData { get; private set; }
        [field: SerializeField] public bool Debuffs { get; private set; } = false;
        [field: SerializeField] public AudioClips AudioClips { get; private set; }

        [Serializable]
        public class Attribute
        {
            [field: SerializeField, Range(0.1f, 60)] public float Duration { get; private set; } = 1f;
            [field: SerializeField, Range(1, 50)] public int Damage { get; private set; } = 1;
            [field: SerializeField, Range(0.1f, 60)] public float Cooldown { get; private set; } = 1;
        }

        [Serializable]
        public class VFX
        {
            [field: SerializeField] public Particle.ParticleSO ParticlePrimary { get; private set; }
            //[field: SerializeField] public ParticleSO ParticleCast { get; private set; }
            [field: SerializeField] public Particle.ParticleSO ParticleHit { get; private set; }
            [field: SerializeField] public Particle.ParticleSO ParticleProp { get; private set; }
        }

        [Serializable]
        public class Composition
        {
            [SerializeField]
            private RelativeStartLocation relativeStartLocation;
            public RelativeStartLocation RelativeStartLocation => relativeStartLocation;

            [field: SerializeField] public bool LocalCoordinates { get; private set; } = true;

            [field: SerializeField] public bool RemoveYDirection { get; private set; } = true;

            [field: SerializeField] public Offset OffsetData { get; private set; }


            [Serializable]
            public class Offset
            {
                [field: SerializeField, Header("Let's use this for the project")] public bool Y_StartPositionIsZero { get; private set; } = true;

                //Start Position Offst
                [field: SerializeField, Header("Start Position Offset Data")]
                public StartPositionOffsetType StartPositionOffsetType { get; private set; }

                [AllowNesting]
                [SerializeField, ShowIf("StartPositionOffsetType", StartPositionOffsetType.Fixed)]
                private Vector3 startPositionOffset = Vector3.zero;
                public Vector3 StartPositionOffset => startPositionOffset;

                [AllowNesting]
                [SerializeField, ShowIf("StartPositionOffsetType", StartPositionOffsetType.RandomRange)]
                private Vector3 startPositionRandomRangeA = Vector3.zero;
                public Vector3 StartPositionRandomRangeA => startPositionRandomRangeA;

                [AllowNesting]
                [SerializeField, ShowIf("StartPositionOffsetType", StartPositionOffsetType.RandomRange)]
                private Vector3 startPositionRandomRangeB = Vector3.zero;
                public Vector3 StartPositionRandomRangeB => startPositionRandomRangeB;

                //Target Position Offset
                [field: SerializeField, Header("Target Position Offset Data")]
                public TargetOffsetType TargetPositionOffsetType { get; private set; }

                [AllowNesting]
                [SerializeField, ShowIf("TargetPositionOffsetType", TargetOffsetType.Fixed)]
                private Vector3 targetPositionOffset = Vector3.zero;
                public Vector3 TargetPositionOffset => targetPositionOffset;

                [AllowNesting]
                [SerializeField, ShowIf("TargetPositionOffsetType", TargetOffsetType.RandomRange)]
                private Vector3 targetPositionRandomRangeA = Vector3.zero;
                public Vector3 TargetPositionRandomRangeA => targetPositionRandomRangeA;

                [AllowNesting]
                [SerializeField, ShowIf("TargetPositionOffsetType", TargetOffsetType.RandomRange)]
                private Vector3 targetPositionRandomRangeB = Vector3.zero;
                public Vector3 TargetPositionRandomRangeB => targetPositionRandomRangeB;

                //Rotation Offset
                [field: SerializeField, Header("Rotational Angle Offset Data")]
                public RotationOffsetType RotationOffsetType { get; private set; }

                [AllowNesting]
                [SerializeField, ShowIf("RotationOffsetType", RotationOffsetType.Fixed)]
                private Vector3 rotationOffset = Vector3.zero;
                public Vector3 RotationOffset => rotationOffset;

                [AllowNesting]
                [SerializeField, ShowIf("RotationOffsetType", RotationOffsetType.RandomRange)]
                private Vector3 rotationRandomRangeA = Vector3.zero;
                public Vector3 RotationRandomRangeA => rotationRandomRangeA;

                [AllowNesting]
                [SerializeField, ShowIf("RotationOffsetType", RotationOffsetType.RandomRange)]
                private Vector3 rotationRandomRangeB = Vector3.zero;
                public Vector3 RotationRandomRangeB => rotationRandomRangeB;

                //Modified Position Offset
                [field: SerializeField, Header("Modified Position Offset Data")]
                public ModifiedPositionOffsetType ModifiedPositionOffsetType { get; private set; }

                [AllowNesting]
                [SerializeField, ShowIf("ModifiedPositionOffsetType", ModifiedPositionOffsetType.Fixed)]
                private Vector3 modifiedPositionOffset = Vector3.zero;
                public Vector3 ModifiedPositionOffset => modifiedPositionOffset;

                [AllowNesting]
                [SerializeField, ShowIf("ModifiedPositionOffsetType", ModifiedPositionOffsetType.RandomRange)]
                private Vector3 modifiedRandomRangeA = Vector3.zero;
                public Vector3 ModifiedRandomRangeA => modifiedRandomRangeA;

                [AllowNesting]
                [SerializeField, ShowIf("ModifiedPositionOffsetType", ModifiedPositionOffsetType.RandomRange)]
                private Vector3 modifiedRandomRangeB = Vector3.zero;
                public Vector3 ModifiedRandomRangeB => modifiedRandomRangeB;
            }

        }

        [Serializable]
        public class Incrementation
        {
            [field: SerializeField, Range(1, 1000)] public int NumIncrements { get; private set; } = 1;
            [field: SerializeField, Range(0.1f, 60)] public float TimeBetweenIncrements { get; private set; } = 0.5f;
            [field: SerializeField, Range(0, 100)] public float GapTime { get; private set; } = 1;
            [field: SerializeField, Range(1, 100)] public int IncrementsPerGap { get; private set; } = 1;
            public bool NewStartPosPerIncrement { get; private set; } = false;


            [SerializeField]
            private AnimationCurve angleCurveY = new(
            new Keyframe(0, 0), new Keyframe(1, 0));
            public AnimationCurve AngleCurveY => angleCurveY;
        }

        [Serializable]
        public class Capacity
        {
            [field: SerializeField, Range(1, 50)] public int AbilityAmount { get; private set; } = 1;
            [field: SerializeField, Range(0, 360)] public float SpreadAngle { get; private set; } = 0;
            [field: SerializeField, Range(0, 360)] public float MaximumSpreadAngle { get; private set; } = 0;
        }

        [Serializable]
        public class Size
        {

            [SerializeField, Range(0.1f, 100)] private float baseSize = 0.5f;
            public float BaseSize => baseSize;

            [SerializeField]
            private AnimationCurve sizeCurveMultiplier = new(new Keyframe(0, 1), new Keyframe(1, 1));
            public AnimationCurve SizeCurveMultiplier => sizeCurveMultiplier;
        }

        [Serializable]
        public class ActiveRotation
        {
            [SerializeField, InfoBox("Rotational Angle values range from 360 to -360 are normalized from 1 to -1", EInfoBoxType.Normal)]
            private AnimationCurve angleCurveX = new(
                new Keyframe(0, 0), new Keyframe(1, 0));
            public AnimationCurve AngleCurveX => angleCurveX;

            [SerializeField]
            private AnimationCurve angleCurveY = new(
            new Keyframe(0, 0), new Keyframe(1, 0));
            public AnimationCurve AngleCurveY => angleCurveY;

            [SerializeField]
            private AnimationCurve angleCurveZ = new(
            new Keyframe(0, 0), new Keyframe(1, 0));
            public AnimationCurve AngleCurveZ => angleCurveZ;
        }

        [Serializable]
        public class Hit
        {
            [field: SerializeField] public EntityHitType EntityHitType { get; private set; } = EntityHitType.Single;

            [AllowNesting]
            [SerializeField, ShowIf("EntityHitType", EntityHitType.Interval), Range(0.25f, 10f)]
            private float entityHitCooldown = 0.25f;
            public float TargetHitCooldown => entityHitCooldown;

            [field: SerializeField] public HitResponseType HitResponseType { get; private set; } = HitResponseType.None;

            [AllowNesting]
            [field: SerializeField, HideIf("HitResponseType", HitResponseType.None), Range(1, 1000)]
            private int maxHits = 1;
            public int MaxHits => maxHits;

            [AllowNesting]
            [field: SerializeField, HideIf("HitResponseType", HitResponseType.None)]
            private bool hitResponseYRotOnly = true;
            public bool HitReponseYRotOnly => hitResponseYRotOnly;

            [AllowNesting]
            [SerializeField, ShowIf("HitResponseType", HitResponseType.Chain), Range(0.1f, 100)]
            private float chainRange = 1;
            public float ChainRange => chainRange;

            [AllowNesting]
            [SerializeField, ShowIf("HitResponseType", HitResponseType.Chain)]
            private ChainFallback chainFallback = ChainFallback.DestroyAbility;
            public ChainFallback ChainFallback => chainFallback;

        }


        [Serializable]
        public class Movement
        {


            [AllowNesting]
            [SerializeField, Range(0f, 100)]
            private float baseSpeed = 1f;
            public float BaseSpeed => baseSpeed;

            [SerializeField]
            private AnimationCurve speedMultiplier = new(new Keyframe(0, 1), new Keyframe(1, 1));
            public AnimationCurve SpeedCurveMultiplier => speedMultiplier;

            [SerializeField]
            private Homing homingData;
            public Homing HomingData => homingData;

            [Serializable]
            public class Homing
            {
                [field: SerializeField] public bool Enabled { get; private set; } = false;

                [AllowNesting]
                [SerializeField, ShowIf("Enabled"), Range(0.1f, 1000)]
                private float rotationSpeed = 0.1f;
                public float BaseRotSpeed => rotationSpeed;

                [AllowNesting]
                [SerializeField, ShowIf("Enabled")]
                private AnimationCurve rotationSpeedMultiplier = new(new Keyframe(0, 1), new Keyframe(1, 1));
                public AnimationCurve RotSpeedMultiplier => rotationSpeedMultiplier;

                [AllowNesting]
                [SerializeField, ShowIf("Enabled"), Range(0.1f, 100)]
                private float range = 0.1f;
                public float Range => range;

                [AllowNesting]
                [SerializeField, ShowIf("Enabled")]
                private bool rotateYAxisOnly = true;
                public bool RotateYAxisOnly => rotateYAxisOnly;

                [AllowNesting]
                [SerializeField, ShowIf("Enabled")]
                private bool disableHomingOnHit = true;
                public bool DisableHomingOnHit => disableHomingOnHit;
            }
        }
    }
}

