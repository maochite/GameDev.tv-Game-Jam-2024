using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class DialogueEvents : MonoBehaviour
    {
        public class DialogueEvent
        {
            public DialogueData Dialogue;
            public bool HasExecuted { get; set; } = false;
        }

        public DialogueData IntroDialogue;
        public DialogueData RejuvenatingSap;
        public DialogueData EnergizingSap;
        public DialogueData StimulatingSap;
        public DialogueData InvigoratingSap;
        public DialogueData RadiantRuby;
        public DialogueData DazzlingDiamond;
        public DialogueData EnchantedEmerald;
        public DialogueData AlluringAquamarine;
        public DialogueData BlazingScroll;
        public DialogueData ConjureScroll;
        public DialogueData HellfireScroll;
        public DialogueData SolarScroll;
        public DialogueData Torch;
        public DialogueData Hammer;
        public DialogueData Pickaxe;
        public DialogueData ChoppingAxe;
    }
}