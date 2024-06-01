using Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellBook : MonoBehaviour
{
    [field: SerializeField] public UnityEngine.UI.Image SpellBookBoarder { get; private set; }
    [field: SerializeField] public HorizontalLayoutGroup SpellGroup { get; private set; }
    [field: SerializeField] public Color ColorToggle { get; private set; }

    public void ToggleSpellBook(bool toggle)
    {
        if(toggle == false)
        {
            SpellBookBoarder.color = Color.white;
            SpellGroup.gameObject.SetActive(false);
        }

        else
        {
            SpellBookBoarder.color = ColorToggle;
            SpellGroup.gameObject.SetActive(true);
        }
    }
}
