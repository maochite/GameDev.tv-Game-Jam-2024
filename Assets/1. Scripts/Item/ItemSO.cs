using System;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;

namespace Items
{
    public enum ItemDropType
    {
        Basic,
        Scatter,
    }

    public abstract class ItemSO : ScriptableObject
    {
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public ItemDrop ItemDropBehaviour { get; private set; }

        [Serializable]
        public class ItemDrop
        {

            [field: SerializeField] public ItemDropType ItemDropType { get; private set; }
            [field: SerializeField, Range(0, 50)] public float ScatterForce { get; private set; } = 1f;
            [field: SerializeField, Range(0, 1)] public float SpawnOffset { get; private set; } = 0.25f;
            [field: SerializeField] public bool CanHome { get; private set; }
            [field: SerializeField, Range(0, 50)] public float HomingSpeed { get; private set; } = 5f;
            [field: SerializeField, Range(0, 5)] public float HomingDelay { get; private set; } = 1.25f;
            [field: SerializeField, Range(1, 50)] public float HomingRange { get; private set; } = 2f;
        }
    }
}