using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueEvents : MonoBehaviour
{
    public class DialogueEvent
    {
        public GameObject Dialogue;
        public bool HasExecuted { get; set; } = false;
    }
}
