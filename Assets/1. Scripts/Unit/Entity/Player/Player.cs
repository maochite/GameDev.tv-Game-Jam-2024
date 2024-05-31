
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;
using Unit.Gatherables;
using Unit.Constructs;
using System;
using Storage;
using NaughtyAttributes;
using TMPro;

namespace Unit.Entities
{
    public enum EntityPrimaryState
    {
        Move,
        Idle,
        Dead,
        Action,
    }

    public enum EntityActionState
    {
        None,
        Attack,
        Gather,
        Repair,
        Summon,
    }

    public class Player : Entity<PlayerSO>
    {
        public static Player Instance { get; private set; }

        [Header("- Player Specifics -")]
        private const int maxAbilities = 4;

        [field: Header("Player Prefab Components")]
        [field: SerializeField] public PlayerSO PlayerSO { get; private set; }
        [field: SerializeField] public Inventory Inventory { get; private set; }
        [field: SerializeField] public SpellBook SpellBook { get; private set; }
        [field: SerializeField] public TMP_Text PlayerDialogue { get; private set; }
        [field: SerializeField] private Rigidbody rigidBody;
        private readonly AbilityPrimary[] playerAbilities = new AbilityPrimary[maxAbilities];

        [Header("Controller Components")]
        [SerializeField, Range(1, 20)] private float inputSmoothing = 8f;
        private KeyCode SpellBookKey = KeyCode.Q;
        private KeyCode BagToggleKey = KeyCode.E;

        //Controller Variables
        private Vector3 raw_input;
        private Vector3 calculated_input;
        private Vector3 world_input;

        //States
        private EntityPrimaryState state;
        private EntityActionState action;

        //Action detect
        private LayerMask enemyLayerMask;
        private LayerMask miscLayerMask;
        private Collider[] enemyBuffer = new Collider[200];
        private List<Collider> reducedEnemyBuffer = new(200);
        private Collider[] miscBuffer = new Collider[200];
        private List<Gatherable> gatherableBuffer = new(200);

        //Action Targets
        private UnitIDInstance<Enemy, EnemySO> currentAttackTarget;
        private Construct currentSummonTarget;
        private List<UnitIDInstance<Gatherable, GatherableSO>> gatherableTargets = new(10);
        private List<Construct> currentRepairTargets = new(10);

        //Action Variables
        private float actionRemainingTime = 0;

        //Player Stats
        [Header("Player Stats")]

        [SerializeField, ReadOnly] private float maxHealth = 1;
        [SerializeField, ReadOnly] private float currentHealth = 1;
        [SerializeField, ReadOnly] private float healthRegen = 1;
        [SerializeField, ReadOnly] private float gatheringDamage = 1;
        [SerializeField, ReadOnly] private float gatheringTime = 1;
        [SerializeField, ReadOnly] private float gatherRadius = 1;
        [SerializeField, ReadOnly] private float lightRadius = 1;
        [SerializeField, ReadOnly] private float repairTime = 1;
        [SerializeField, ReadOnly] private float itemMagnetRadius = 1;
        [SerializeField, ReadOnly] private float collectionRadius = 1;

        public float GatheringDamage { get => gatheringDamage; private set => gatheringDamage = value; }
        public float GatheringTime { get => gatheringTime; private set => gatheringTime = value; }
        public float GatherRadius { get => gatherRadius; private set => gatherRadius = value; }
        public float LightRadius { get => lightRadius; private set => lightRadius = value; }
        public float RepairTime { get => repairTime; private set => repairTime = value; }
        public float ItemMagnetRadius { get => itemMagnetRadius; private set => itemMagnetRadius = value; }
        public float CollectionRadius { get => collectionRadius; private set => collectionRadius = value; }

        protected virtual void OnApplicationQuit()
        {
            Destroy(gameObject);
            Instance = null;
        }

        public override float CurrentHealth 
        {
            get { return currentHealth; }

            protected set
            {
                if (!isActive) return;

                if (value < currentHealth)
                {
                    currentHealth = value;

                    if (currentHealth <= 0)
                    {
                        //OnDeath();
                        //Destroy(gameObject);
                        isActive = false;
                    }
                }

                else if (value > maxHealth)
                {
                    currentHealth = maxHealth;
                }

                else currentHealth = value;
            }
        }

        public override float MaxHealth
        {
            get => maxHealth;

            protected set
            {
                if (!isActive) return;

                maxHealth = value;

                if (maxHealth < 1)
                {
                    maxHealth = 1;
                }

                if (currentHealth >= maxHealth)
                {
                    currentHealth = maxHealth;

                    //healthBar.ToggleHealthBar(false);
                }

                //healthBar.SetHealthBarValue(_currentHealth, _maxHealth);
            }
        }

        public override float HealthRegen 
        { 
            get => HealthRegen; 

            protected set
            {
                if(healthRegen < 1)
                {
                    healthRegen = 1;
                }
            }
        }

        protected override void Awake()
        {
            Instance = this;

            if(PlayerSO == null)
            {
                base.Awake();
            }

            enemyLayerMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Enemy);
            miscLayerMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Gatherable);
        }

        protected override void Start()
        {
            base.Start();
            AssignUnit(PlayerSO);
            Inventory.InitializeInventory();
        }

        public override void AssignUnit(PlayerSO playerSO)
        {
            base.AssignUnit(playerSO);
            playerAbilities[0] = new(playerSO.DefaultAbility, this);

            GatheringTime = playerSO.BaseGatheringTime;
            GatherRadius = playerSO.BaseGatherRadius;
            ItemMagnetRadius = playerSO.BaseItemMagnetRadius;
            CollectionRadius = playerSO.BaseCollectionRadius;
            GatheringDamage = playerSO.BaseGatheringDamage;
            LightRadius = playerSO.BaseLightRadius;
            RepairTime = playerSO.BaseRepairTime;
        }

        private void GetInput()
        {
            raw_input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            raw_input.Normalize();

            calculated_input = Vector3.Lerp(raw_input, calculated_input, inputSmoothing * Time.deltaTime);

            if (Input.GetKeyUp(BagToggleKey))
            {
                ToggleBag();
            }

            if (Input.GetKeyDown(SpellBookKey))
            {
                ToggleSpellBook();
            }
        }

        private bool Move()
        {
            Vector3 movementDirection = calculated_input.normalized;

            if (movementDirection != Vector3.zero)
            {
                transform.forward = movementDirection;

                rigidBody.MovePosition(rigidBody.position + MovementSpeed * Time.deltaTime * calculated_input);
                return true;
            }

            else
            {
                return false;
            }

        }

        public void Look(Vector3 pos)
        {
            Vector3 lookDir = Aim(pos);
            Quaternion rot = Quaternion.identity;

            if (lookDir != Vector3.zero)
            {
                rot = Quaternion.LookRotation(lookDir, Vector3.up);
            }

            if (rot != Quaternion.identity)
            {
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 3*Time.deltaTime);
                rigidBody.rotation = rot;
            }
        }

        private void Update()
        {
            GetInput();
        }

        private void FixedUpdate()
        {
            if (state == EntityPrimaryState.Dead) return;

            if (state == EntityPrimaryState.Idle)
            {
                 GetActionFromProximity();
            }

            if(state == EntityPrimaryState.Action)
            {
                if(action == EntityActionState.None)
                {
                    ChangePrimaryState(EntityPrimaryState.Idle);
                }

                else if (Move())
                {
                    action = EntityActionState.None;
                    ChangePrimaryState(EntityPrimaryState.Move);
                }

                else ResolveCurrentAction();
            }

            else
            {
                if (Move())
                {
                    ChangePrimaryState(EntityPrimaryState.Move);
                }

                else ChangePrimaryState(EntityPrimaryState.Idle);
            }

        }

        private void ResolveCurrentAction()
        {
            actionRemainingTime -= Time.deltaTime;

            if (actionRemainingTime > 0) return;

            else
            {
                if(action == EntityActionState.Attack)
                {
                    Attack();
                }

                else if (action == EntityActionState.Gather)
                {
                    Gather();
                }

                action = EntityActionState.None;
                ChangePrimaryState(EntityPrimaryState.Idle);
            }
        }

        public void GetActionFromProximity()
        {
            //TODO: Incorporate other abilities soon
            if (!playerAbilities[0].IsCoolingDown())
            {
                int numEnemyColliders = Physics.OverlapSphereNonAlloc(transform.position, AttackRadius, enemyBuffer, enemyLayerMask);

                if (numEnemyColliders > 0)
                {
                    reducedEnemyBuffer.Clear();
                    
                    for (int i = 0; i < numEnemyColliders; i++)
                    {
                        reducedEnemyBuffer.Add(enemyBuffer[i]);
                    }

                    Transform closestTransform = TransformUtility.FindClosestTransformSqr(transform.position, reducedEnemyBuffer);

                    if(closestTransform.TryGetComponent(out Enemy enemy))
                    {
                        currentAttackTarget = new(enemy);
                        Look(currentAttackTarget.Unit.transform.position);

                        ChangeAction(EntityActionState.Attack);
                    }

                    return;
                }
            }

            int numMiscColliders = Physics.OverlapSphereNonAlloc(transform.position, GatherRadius, miscBuffer, miscLayerMask);
            gatherableBuffer.Clear();

            for (int i = 0; i < numMiscColliders; i++)
            {

                if (miscBuffer[i].TryGetComponent(out Gatherable gatherable))
                {
                    gatherableBuffer.Add(gatherable);
                }
            }

            if (gatherableBuffer.Count != 0)
            {
                gatherableTargets.Clear();

                foreach (Gatherable gatherable in gatherableBuffer)
                {
                    gatherableTargets.Add(new(gatherable));
                }

                ChangeAction(EntityActionState.Gather);

                return;
            }
            

        }

        public void ChangePrimaryState(EntityPrimaryState entityState)
        {
            if (entityState == state) return;

            Animator.ToggleIdleAnimation(false);
            Animator.ToggleWalkAnimation(false);

            //Animator.ToggleDeathAnimation(false);


            switch (entityState)
            {
                case EntityPrimaryState.Move:
                    Animator.ChangeAnimationMultiplier(1);
                    Animator.ToggleWalkAnimation(true);
                    state = EntityPrimaryState.Move;
                    break;
                case EntityPrimaryState.Dead:
                    //Toggle Death Animation
                    break;
                case EntityPrimaryState.Idle:
                    Animator.ToggleIdleAnimation(true);
                    state = EntityPrimaryState.Idle;
                    //Idle Toggle is automatic if everything else is off
                    break;;
                case EntityPrimaryState.Action:
                    state = EntityPrimaryState.Action;
                    break;
                default:
                    break;
            }
        }

        public void ChangeAction(EntityActionState entityAction)
        {
            if (entityAction == action) return;

            switch (entityAction)
            {
                case EntityActionState.None:
                    action = EntityActionState.None;
                    break;
                case EntityActionState.Attack:
                    EvaluateActionAnimation(UnitSO.AttackAnimation, AttackSpeed);
                    actionRemainingTime = AttackSpeed;

                    action = EntityActionState.Attack;
                    ChangePrimaryState(EntityPrimaryState.Action);

                    break;
                case EntityActionState.Gather:

                    foreach (var gatherableUnit in gatherableTargets)
                    {
                        if (gatherableUnit.Unit == null || gatherableUnit.InstanceID != gatherableUnit.Unit.ID)
                        {
                            continue;
                        }

                        else
                        {
                            EvaluateActionAnimation(gatherableUnit.Unit.UnitSO.GatheringAnimation, GatheringTime);
                            actionRemainingTime = GatheringTime;
                            action = EntityActionState.Gather;

                            ChangePrimaryState(EntityPrimaryState.Action);

                            return;
                        }
                    }

                    //Shouldn't really get here
                    action = EntityActionState.None;

                    break;
                case EntityActionState.Summon:
                default:
                    break;
            }
        }

        public bool Attack()
        {
            if (currentAttackTarget.Unit != null
                    && currentAttackTarget.Unit.ID == currentAttackTarget.InstanceID)
            {
                //TODO more abilities
                if (playerAbilities[0].TryCast(currentAttackTarget.Unit.transform.position, out _))
                {
                    return true;
                }
            }

            return false;
        }

        public Vector3 Aim(Vector3 worldPos)
        {
            var direction = worldPos - transform.position;

            direction.y = 0;
            return direction;
        }

        public bool GetMouseWorldPosition(out Vector3 pos)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity))
            {
                // The Raycast hit something, return with the position.
                pos = hitInfo.point;
                return true;
            }
            else
            {
                pos = Vector3.zero;
                return false;
            }
        }

        private void EvaluateActionAnimation(
            EntityActionAnimation entityActionAnimation, float actionSpeed)
        {
            Animator.ChangeAnimationMultiplier(actionSpeed);

            switch (entityActionAnimation)
            {
                case EntityActionAnimation.Attack:
                    Animator.TriggerAttackAnimation();
                    break;
                case EntityActionAnimation.Chop:
                    Animator.TriggerChopAnimation();
                    break;
                case EntityActionAnimation.Mine:
                    Animator.TriggerMineAnimation();
                    break;
                case EntityActionAnimation.Summon:
                    Animator.TriggerSummonAnimation();
                    break;
                default:
                    Animator.TriggerSummonAnimation();
                    break;
            }
        }

        public override void UpdateEntityStats()
        {
            MaxHealth = EntityStatsManager.Instance.GetHealthModified(UnitSO);
            HealthRegen = EntityStatsManager.Instance.GetHealthRegenModified(UnitSO);
            MovementSpeed = EntityStatsManager.Instance.GetMovementModified(UnitSO);
            GatheringTime = EntityStatsManager.Instance.GetGatherSpeedModified(UnitSO);
            LightRadius = EntityStatsManager.Instance.GetGatherDamageModified(UnitSO);
            GatheringTime = EntityStatsManager.Instance.GetGatherSpeedModified(UnitSO);
            RepairTime = EntityStatsManager.Instance.GetRepairSpeedModified(UnitSO);
            ItemMagnetRadius = EntityStatsManager.Instance.GetItemMagnetRadius(UnitSO);

            //WeaponPrimary.Ability.UpdateAbilityStats();
        }

        public void LearnNewAbility(AbilitySO abilitySO, int abilitySlotNum)
        {
            if(abilitySlotNum < 0 || abilitySlotNum > 0)
            {
                Debug.LogError("Incorrect Ability Slot");
                return;
            }

            playerAbilities[abilitySlotNum] = new(abilitySO, this);
        }

        private void Gather()
        {

            foreach (var gatherableInstance in gatherableTargets)
            {
                if (gatherableInstance.Unit != null 
                    && gatherableInstance.Unit.ID == gatherableInstance.InstanceID)
                {
                    gatherableInstance.Unit.Gather(GatheringDamage);
                }
            }

        }

        private void ToggleBag()
        {
            Inventory.ToggleInventory();
        }

        private void ToggleSpellBook()
        {
            SpellBook.ToggleSpellBook();
        }
    }

}