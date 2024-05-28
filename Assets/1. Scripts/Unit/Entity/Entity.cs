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

    public interface ICaster : IUnit
    {
        public Transform Transform { get; }
    }


    public interface IEntity : IUnit, ICaster
    {
        public EntitySO EntitySO { get; }
    }

    public abstract class Entity<T> : Unit<T>, IEntity where T : EntitySO
    {
        [Header("- Entity Specifics -")]
        public EntitySO EntitySO => UnitSO;

        public virtual Transform Transform => gameObject.transform;
        public virtual Guid ID { get; protected set; }
        public bool IsActive { get; protected set; } = false;

        public float MovementSpeed { get; protected set; }
        public float AttackSpeed { get; protected set; }
        public float AttackRadius { get; protected set; }

        [field: SerializeField] public SpriteAnimator Animator { get; protected set; }

        protected virtual void Start()
        {

        }

        public override void AssignUnit(T entitySO)
        {
            base.AssignUnit(entitySO);
            MovementSpeed = entitySO.BaseMovementSpeed;
            AttackSpeed = entitySO.BaseAttackTime;
            AttackRadius = entitySO.BaseAttackRadius;

        }

    }
}
    