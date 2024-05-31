using Dialogue;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class SceneTransitions : MonoBehaviour
{
    [field: SerializeField] public Canvas Canvas { get; private set; }
    [field: SerializeField] public TMP_Text TextBox { get; private set; }
    [field: SerializeField] public Image FadeImage { get; private set; }
    [field: SerializeField] public float FadeDuration { get; private set; }


    [field: Header("Intro To Game Transition")]
    [field: SerializeField] public DialogueData IntroDialogue { get; private set; }
    [field: SerializeField] public float IntroDuration { get; private set; }
}
