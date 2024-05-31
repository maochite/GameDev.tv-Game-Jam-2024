using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    [Serializable]
    public class DialogueData
    {
        [field: TextArea(15, 20)]
        [field: SerializeField] public string Text { get; private set; }
        [field: SerializeField] public bool Important { get; private set; }
    }
}