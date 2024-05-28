
using System;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEditor;
using UnityEngine;

public interface IDamagable
{
    public int CurrentHealth { get; }
}

namespace Unit
{
    public struct UnitIDInstance<T, S>
        where T : Unit<S>
        where S : UnitSO 
    {
        public UnitIDInstance(T unit)
        {
            Unit = unit;
            InstanceID = unit.ID;
        }

        public T Unit { get; private set; }
        public Guid InstanceID { get; private set; }
    }

    public abstract class Unit<T> : MonoBehaviour, IDamagable where T : UnitSO
    {
        [Header("Unity Specifics")]
        public T UnitSO { get; protected set; }
        public Guid ID { get; private set; }
        public abstract int CurrentHealth { get; protected set; }
        
        public virtual void AssignUnit(T unitSO)
        {
            UnitSO = unitSO;
            ID = Guid.NewGuid();
        }
    }
}