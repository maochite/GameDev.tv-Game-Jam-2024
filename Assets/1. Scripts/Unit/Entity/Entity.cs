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
        public Guid TargetID { get; }
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
        public EntitySO EntitySO => UnitSO;

        public virtual Transform Transform => gameObject.transform;
        public virtual Guid TargetID { get; protected set; }
        public bool IsActive { get; protected set; } = false;

        public float MovementSpeed { get; protected set; }

        [field: SerializeField] public SpriteAnimator Animator { get; protected set; }

        protected virtual void Start()
        {

        }

        protected virtual void AssignEntity(T entitySO)
        {
            UnitSO = entitySO;
            TargetID = Guid.NewGuid();
        }

    }
}
    