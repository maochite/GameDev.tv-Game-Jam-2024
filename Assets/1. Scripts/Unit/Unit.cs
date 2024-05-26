
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
}

namespace Unit
{

    public abstract class Unit<T> : MonoBehaviour, IDamagable where T : UnitSO
    {
        public T UnitSO { get; protected set; }
        public virtual int CurrentHealth { get; protected set; }
        public virtual int MaxHealth { get; protected set; }
    }
}