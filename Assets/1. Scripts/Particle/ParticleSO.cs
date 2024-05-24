using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Particle
{
    [CreateAssetMenu(menuName = "ParticleSO")]
    public class ParticleSO : ScriptableObject
    {
        [field: SerializeField] public ParticleSystem ParticleSystemPrefab { get; private set; }
        [field: SerializeField, Range(0, 5), Header("Size Options")] public float SizeModifier { get; private set; } = 1f;
        [field: SerializeField, Header("Lingering Options")] public bool ClearPrimaryParticles { get; private set; } = false;
        [field: SerializeField] public bool ClearSecondaryParticles { get; private set; } = false;
        [field: SerializeField, Range(0, 5)] public float LingerTime { get; private set; } = 0f;
    }
}