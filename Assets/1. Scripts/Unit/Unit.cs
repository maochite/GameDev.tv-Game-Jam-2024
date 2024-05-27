
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IDamagable
{
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
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
        public GUID InstanceID { get; private set; }
    }

    public abstract class Unit<T> : MonoBehaviour, IDamagable where T : UnitSO
    {
        public T UnitSO { get; protected set; }
        public GUID ID { get; private set; }
        public virtual int CurrentHealth { get; protected set; }
        public virtual int MaxHealth { get; protected set; }
    }
}