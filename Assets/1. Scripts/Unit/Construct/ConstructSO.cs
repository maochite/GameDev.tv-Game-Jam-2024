using Ability;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;

namespace Unit.Constructs
{
    [CreateAssetMenu(menuName = "Unit/ConstructSO")]
    public class ConstructSO : EntitySO
    {
        [field: Header("- Construct Specifics -")]
        [field: Header("Construct Ability")]
        [field: SerializeField] public AbilityPrimarySO AbilityPrimarySO { get; private set; }
        [field: SerializeField] public float TargetRange { get; private set; }

        [field: Header("Construct Rendering")]
        [field: SerializeField] public MeshFilter MeshFilter { get; private set; }
        [field: SerializeField] public Material Material { get; private set; }
    }

}