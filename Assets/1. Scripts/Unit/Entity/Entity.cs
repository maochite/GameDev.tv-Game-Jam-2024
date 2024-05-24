using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Animations;
using TMPro;
using System.Linq;

namespace Unit.Entity
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


    public interface IEntity : IDamagable
    {
        public Transform Transform { get; }
        public EntitySO EntitySO { get; }
        public Guid TargetID { get; }
    }

    public abstract class Entity<T> : Unit<T> where T : EntitySO
    {

        public virtual Transform Transform => gameObject.transform;
        public virtual EntitySO EntitySO { get; protected set; }
        public virtual Guid TargetID { get; protected set; }
        public bool IsActive { get; protected set; } = false;

        public float MovementSpeed { get; protected set; }

        protected virtual void Start()
        {

        }

        public virtual void AssignEntity(T entitySO)
        {
            EntitySO = entitySO;
            TargetID = Guid.NewGuid();
        }

    }
}
    