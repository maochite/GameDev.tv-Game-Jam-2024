using Dialogue;
using Items;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Storage
{


    public class ItemSlot : MonoBehaviour, IPointerEnterHandler
    {
        [field: SerializeField] public ItemSO ItemSO { get; private set; }
        [field: SerializeField] public int MaxItems { get; private set; }
        [field: SerializeField] public Image SpriteUI { get; private set; }
        [field: SerializeField] public TMP_Text NumTextUI { get; private set; }
        public int NumItems { get; set; } = 0;

        public void OnPointerEnter(PointerEventData eventData)
        {
            DialogueManager.Instance.QueueDialogue(ItemSO.DialogueTooltip);
        }

        public void UpdateItemSlotDisplay()
        {
            if (NumItems == 0)
            {
                NumTextUI.enabled = false;
                SpriteUI.enabled = false;
                return;
            }

            SpriteUI.enabled = true;
            NumTextUI.enabled = true;
            SpriteUI.sprite = ItemSO.UISprite;

            if (NumItems == MaxItems)
            {
                NumTextUI.text = "Max";
            }

            else
            {
                NumTextUI.text = NumItems.ToString();
            }
        }
    }
}