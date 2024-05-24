using System.Collections;
using System.Collections.Generic;
using Unit.Entity;
using UnityEngine;

namespace Ability
{

    public class AbilityPrimary : Ability
    {
        public AbilityPrimary(AbilitySO abilitySO, IEntity entity) : base(abilitySO, entity) { }
    }
}