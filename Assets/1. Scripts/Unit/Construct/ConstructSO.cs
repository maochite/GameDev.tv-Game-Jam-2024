using Ability;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Constructs
{
    public class ConstructSO : UnitSO
    {
        [field: SerializeField] public MeshFilter MeshFilter { get; private set; }
        [field: SerializeField] public Material Material { get; private set; }
        [field: SerializeField] public AbilityPrimarySO AbilityPrimarySO { get; private set; }
        [field: SerializeField] public float TargetRange { get; private set; }
    }

}