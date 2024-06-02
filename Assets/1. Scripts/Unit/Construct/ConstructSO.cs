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

        [field: Header("Construct Rendering")]
        [field: SerializeField] public MeshFilter MeshFilter { get; private set; }
        [field: SerializeField] public Material Material { get; private set; }


        [field: Header("Resource Requirements")]
        [field: SerializeField, Min(1)] public int Wood { get; private set; }
        [field: SerializeField, Min(1)] public int Stone { get; private set; }
        [field: SerializeField, Min(1)] public int Gold { get; private set; }
    }

}