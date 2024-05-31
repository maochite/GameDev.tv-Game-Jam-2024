using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

namespace Unit.Entities
{
    //public class PlayerManager : StaticInstance<PlayerManager>
    //{
    //    [field: SerializeField] private Player player;
    //    [field: SerializeField] private PlayerSO playerSO;
    //    [field: SerializeField] private TMP_Text playerDialogue;

    //    private bool isActive = false;

    //    public void AssignPlayer()
    //    {
    //        player.AssignUnit(playerSO);
    //        isActive = true;
    //    }

    //    public bool TryGetPlayer(out Player player)
    //    {
    //        player = null;

    //        if (!isActive) return false;

    //        else
    //        {
    //            player = this.player;
    //            return true;
    //        }
    //    }

    //    public bool TryGetPlayerPosition(out Vector3 pos)
    //    {
    //        pos = Vector3.zero;

    //        if (!isActive) return false;

    //        else
    //        {
    //            pos = player.transform.position;
    //            return true;
    //        }
    //    }

    //    public bool TryGetPlayerTextBox(out TMP_Text textBox)
    //    {
    //        textBox = null;

    //        if (!isActive) return false;

    //        else
    //        {
    //            textBox = playerDialogue;
    //            return true;
    //        }
    //    }
    //}
}
