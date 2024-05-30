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
        [field: SerializeField] public Sprite UISprite { get; private set; }
        [field: SerializeField] public Sprite WorldSprite { get; private set; }
    }
}