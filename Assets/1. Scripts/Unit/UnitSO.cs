using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit
{

    public class UnitSO : ScriptableObject
    {
        [field: SerializeField] public int DefaultHealth { get; private set; }
    }

}