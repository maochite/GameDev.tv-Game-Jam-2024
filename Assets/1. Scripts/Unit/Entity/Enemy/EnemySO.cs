using Ability;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Entities
{
    [CreateAssetMenu(menuName = "Unit/Entity/EnemySO")]
    public class EnemySO : EntitySO, ISpriteAnimated
    {
        [field: Header("Enemy Specifics")]
        [field: SerializeField] public AbilityPrimarySO AttackAbility { get; private set; }
        [field: SerializeField] public AbilityPrimarySO DemolishAbility { get; private set; }

        [field: Header("Sprite Stuff")]
        [field: SerializeField] public AnimatorOverrideController UpAnimatorController { get; private set; }
        [field: SerializeField] public AnimatorOverrideController DownAnimatorController { get; private set; }
        [field: SerializeField] public AnimatorOverrideController LeftAnimatorController { get; private set; }
        [field: SerializeField] public AnimatorOverrideController RightAnimatorController { get; private set; }
    }
}