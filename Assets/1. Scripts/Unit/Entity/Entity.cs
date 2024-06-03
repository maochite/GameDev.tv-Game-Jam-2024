using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Animations;
using TMPro;
using System.Linq;

namespace Unit.Entities
{
    public enum EntityType
    {
        Player,
        Enemy,
        Construct,
        Gatherable,
    }


    public enum EntityStatType
    {
        Health,
        MovementSpeed,
    }

    public interface IUnit : IDamagable
    {
        public Guid ID { get; }
    }

    public interface IEntity : IUnit
    {
        public Transform Transform { get; }
        public EntitySO EntitySO { get; }
        public void DamageEntity(float amount);
    }

    public abstract class Entity<T> : Unit<T>, IEntity where T : EntitySO
    {
        [Header("- Entity Specifics -")]
        public EntitySO EntitySO => UnitSO;
        public virtual Transform Transform => gameObject.transform;

        [Header("- Entity Stats -")]
        [SerializeField, ReadOnly] protected float movementSpeed;
        [SerializeField, ReadOnly] protected float attackSpeed;
        [SerializeField, ReadOnly] protected float attackRadius;

        [field: SerializeField] public HealthBar HealthBar { get; private set; }
        public float MovementSpeed { get => movementSpeed; protected set => movementSpeed = value; }
        public float AttackSpeed { get => attackSpeed; protected set => attackSpeed = value; }
        public float AttackRadius { get => attackRadius; protected set => attackRadius = value; }

        protected const float RegenInterval = 1;
        protected float regenTimer = 0;
 
        public abstract void UpdateEntityStats();

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            if(HealthBar != null)
            {
                ConstraintSource source = new ConstraintSource
                {
                    sourceTransform = RotConstraint.Instance.transform,
                    weight = 1,
                };

                HealthBar.RotationConstraint.weight = 1;
                HealthBar.RotationConstraint.AddSource(source);
                HealthBar.RotationConstraint.constraintActive = true;
            }
        }

        public void DamageEntity(float amount)
        {
            CurrentHealth -= amount;
        }

        protected abstract void RegenEntity();

        public override void AssignUnit(T entitySO)
        {
            base.AssignUnit(entitySO);
            MovementSpeed = entitySO.BaseMovementSpeed;
            AttackSpeed = entitySO.BaseAttackTime;
            AttackRadius = entitySO.BaseAttackRange;

        }

    }
}
    