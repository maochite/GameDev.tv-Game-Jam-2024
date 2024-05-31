using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Dialogue
{
    public class DialogueManager : StaticInstance<DialogueManager>
    {
        [field: SerializeField] public TMP_Text TextBox { get; private set; }
        private readonly List<DialogueData> dialogueList = new(10);

        // Basic Typewriter Functionality
        private int _currentVisibleCharacterIndex;
        private Coroutine _typewriterCoroutine;

        private WaitForSeconds _simpleDelay;
        private WaitForSeconds _interpunctuationDelay;

        [Header("Typewriter Settings")]
        [SerializeField] private float charactersPerSecond = 20;
        [SerializeField] private float interpunctuationDelay = 0.5f;
        [SerializeField] private float textClearDelay = 5f;
        [SerializeField] private float fadeDelay = 1f;

        [field: Header("Debug Settings")]
        [field: SerializeField] public DialogueData TestDialogue { get; private set; }
        TMP_TextInfo textInfo;


        private bool hasActiveDialogue = false;

        protected void Start()
        {
            base.Awake();
        }

        public void QueueDialogue(DialogueData dialogueSO)
        {
            if (dialogueSO == null || dialogueSO.Text.Length <= 0) return;

            if (dialogueList.Count > 0)
            {
                if (dialogueList[0].Important)
                {
                    if (dialogueSO.Important)
                    {
                        dialogueList.Add(dialogueSO);
                    }

                    return;
                }

                else
                {
                    dialogueList.RemoveAt(0);
                }
            }

            hasActiveDialogue = false;
            dialogueList.Add(dialogueSO);
        }

        private IEnumerator Typewriter(DialogueData dialogue)
        {
            _currentVisibleCharacterIndex = 0;
            TextBox.maxVisibleCharacters = 0;
            TextBox.alpha = 1;

            TextBox.text = dialogue.Text;
            TextBox.ForceMeshUpdate();
            textInfo = TextBox.textInfo;

            while (_currentVisibleCharacterIndex < textInfo.characterCount + 1)
            {
                var lastCharacterIndex = textInfo.characterCount - 1;

                if (_currentVisibleCharacterIndex >= lastCharacterIndex)
                {
                    yield return new WaitForSeconds(textClearDelay);

                    float lerpTimer = 0f;
                    while(TextBox.alpha > 0)
                    {

                        lerpTimer += Time.deltaTime;
                        float t = Mathf.Clamp01(lerpTimer / fadeDelay);
                        TextBox.alpha = Mathf.Lerp(TextBox.alpha, 0, t);

                        yield return new WaitForEndOfFrame();
                    }

                    if(dialogueList.Count > 0 && dialogueList[0] == dialogue)
                    {
                        dialogueList.RemoveAt(0);
                        hasActiveDialogue = false;
                    }

                    yield break;
                }

                char character = textInfo.characterInfo[_currentVisibleCharacterIndex].character;
                TextBox.maxVisibleCharacters++;

                if (character == '?' || character == '.' || character == ',' || character == '!')
                {
                    yield return _interpunctuationDelay;
                }

                else
                {
                    yield return _simpleDelay;
                }

                _currentVisibleCharacterIndex++;
            }
        }

        private void StopCurrentDialogue()
        {
            if(dialogueList.Count > 0)
            {
                dialogueList.RemoveAt(0);
            }

            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }
        }

        public void Update()
        {
            if(!hasActiveDialogue && dialogueList.Count > 0)
            {
                if(_typewriterCoroutine != null)
                {
                    StopCoroutine(_typewriterCoroutine);
                }

#if UNITY_EDITOR
                _simpleDelay = new WaitForSeconds(1 / charactersPerSecond);
                _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);
#endif

                hasActiveDialogue = true;
                _typewriterCoroutine = StartCoroutine(Typewriter(dialogueList[0]));
            }
        }

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void InsertTestDialogue()
        {
            QueueDialogue(TestDialogue);
        }
    }
}