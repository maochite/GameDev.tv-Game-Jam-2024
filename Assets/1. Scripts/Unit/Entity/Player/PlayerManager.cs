using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Entities
{
    public class PlayerManager : PersistentSingleton<PlayerManager>
    {
        [field: SerializeField] public Player Player;
        [field: SerializeField] public PlayerSO PlayerSO;

        public void AssignPlayer()
        {
            Player.AssignPlayer(PlayerSO);
        }
    }
}
