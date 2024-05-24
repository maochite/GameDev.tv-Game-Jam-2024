
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; }
}

namespace Unit
{

    public abstract class Unit<T> : MonoBehaviour, IDamagable where T : UnitSO
    {
        public T UnitSO { get; private set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; }
    }
}