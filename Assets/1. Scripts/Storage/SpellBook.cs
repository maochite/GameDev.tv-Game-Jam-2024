using Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellBook : MonoBehaviour
{
    [field: SerializeField] public UnityEngine.UI.Image SpellBookBoarder { get; private set; }
    [field: SerializeField] public HorizontalLayoutGroup ConstructGroup { get; private set; }
    [field: SerializeField] public Color ColorToggle { get; private set; }

    [field: Header("Construct Buttons")]

    [field: Space]
    [field: SerializeField] public Sprite Construct1Sprite { get; private set; }
    [field: SerializeField] public Sprite Construct2Sprite { get; private set; }
    [field: SerializeField] public Sprite Construct3Sprite { get; private set; }
    [field: SerializeField] public Sprite Construct4Sprite { get; private set; }

    [field: Space]
    [field: SerializeField] public Image ConstructImage1 { get; private set; }
    [field: SerializeField] public Image ConstructImage2 { get; private set; }
    [field: SerializeField] public Image ConstructImage3 { get; private set; }
    [field: SerializeField] public Image ConstructImage4 { get; private set; }

    [field: Space]
    [field: SerializeField] public Image ConstructBoarder1 { get; private set; }
    [field: SerializeField] public Image ConstructBoarder2 { get; private set; }
    [field: SerializeField] public Image ConstructBoarder3 { get; private set; }
    [field: SerializeField] public Image ConstructBoarder4 { get; private set; }

    private void Awake()
    {
        ToggleConstruct(0);
        ToggleSpellBook(false);
    }

    public void ToggleSpellBook(bool toggle)
    {
        if(toggle == false)
        {
            SpellBookBoarder.color = Color.white;
            ConstructGroup.gameObject.SetActive(false);
        }

        else
        {
            SpellBookBoarder.color = ColorToggle;
            ConstructGroup.gameObject.SetActive(true);
        }
    }

    public void ToggleConstruct(int numConstruct)
    {
        ConstructBoarder1.color = Color.white;
        ConstructBoarder2.color = Color.white;
        ConstructBoarder3.color = Color.white;
        ConstructBoarder4.color = Color.white;

        if (numConstruct == 0)
        {
            ConstructBoarder1.color = ColorToggle;
        }

        if (numConstruct == 1)
        {
            ConstructBoarder2.color = ColorToggle;
        }

        if (numConstruct == 2)
        {
            ConstructBoarder3.color = ColorToggle;
        }

        if (numConstruct == 3)
        {
            ConstructBoarder4.color = ColorToggle;
        }
    }

    public void RevealConstructImage(int numConstruct)
    {
        if (numConstruct == 0)
        {
            ConstructImage1.sprite = Construct1Sprite;
        }

        if (numConstruct == 1)
        {
            ConstructImage2.sprite = Construct2Sprite;
        }

        if (numConstruct == 2)
        {
            ConstructImage3.sprite = Construct3Sprite;
        }

        if (numConstruct == 3)
        {
            ConstructImage4.sprite = Construct4Sprite;
        }
    }
}
