
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
using static ConstructManager;
using static Ability.AbilitySO.Composition;

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
        Build,
    }

    public enum BuildState
    {
        Obstructed,
        Buildable,
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
        private readonly ConstructSO[] playerConstructs = new ConstructSO[4];

        [Header("Controller Components")]
        [SerializeField, Range(1, 20)] private float inputSmoothing = 8f;
        private KeyCode SpellBookKey = KeyCode.Q;
        private KeyCode BagToggleKey = KeyCode.E;
        private KeyCode ConstructKey1 = KeyCode.Alpha1;
        private KeyCode ConstructKey2 = KeyCode.Alpha2;
        private KeyCode ConstructKey3 = KeyCode.Alpha3;
        private KeyCode ConstructKey4 = KeyCode.Alpha4;


        [Header("Construct Building")]
        private BuildState buildState;
        private bool preparingBuild = false;
        private Vector3 currentBuildLocation;
        private LayerMask buildingArea;
        private LayerMask excludedArea;
        private int currentSelectedConstruct;
        private Collider[] constructAreaColliders = new Collider[200];
        [SerializeField, Range(1, 10)] private float targetSafeZoneSize = 5;

        [Header("Construct Indicator")]
        [SerializeField] private GameObject constructPreview;
        [SerializeField] private GameObject arrowIndicatorPrefab;
        [SerializeField] private float indicatorForwardOffset;
        [SerializeField] private float indicatorHeightOffset;
        [SerializeField] private AnimationCurve trajectoryCurve;
        [SerializeField] private int resolution = 10;
        private List<GameObject> arrowIndicators;

        //Controller Variables
        private Vector3 raw_input;
        private Vector3 calculated_input;
        private Vector3 world_input;

        //States
        private EntityPrimaryState state;
        private EntityActionState action;

        //Action detect
        private LayerMask enemyLayerMask;
        private LayerMask gatherLayerMask;
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
        [SerializeField, ReadOnly] private float buildTime = 1;
        [SerializeField, ReadOnly] private float lightRadius = 1;
        [SerializeField, ReadOnly] private float repairTime = 1;
        [SerializeField, ReadOnly] private float itemMagnetRadius = 1;
        [SerializeField, ReadOnly] private float collectionRadius = 1;

        public float GatheringDamage { get => gatheringDamage; private set => gatheringDamage = value; }
        public float GatheringTime { get => gatheringTime; private set => gatheringTime = value; }
        public float GatherRadius { get => gatherRadius; private set => gatherRadius = value; }
        public float BuildTime { get => buildTime; private set => buildTime = value; }
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

            LayerMask constructMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Construct);
            LayerMask gatherMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Gatherable);
            LayerMask enemyMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Enemy);
            LayerMask playerMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Player);
            LayerMask indestructibleMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Indestructible);
            LayerMask terrainMask = LayerUtility.LayerMaskByLayerEnumType(LayerEnum.Terrain);

            enemyLayerMask = enemyMask;
            gatherLayerMask = gatherMask;

            buildingArea = terrainMask;
            excludedArea = LayerUtility.CombineMasks(constructMask, gatherMask, enemyMask, playerMask, indestructibleMask);

            constructPreview = Instantiate(constructPreview);
            constructPreview.SetActive(false);
            arrowIndicators = new(resolution);

            for (int i = 0; i < resolution; i++)
            {
                arrowIndicators.Add(Instantiate(arrowIndicatorPrefab, transform));
                arrowIndicators[i].SetActive(false);
            }
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
            playerConstructs[0] = playerSO.DefaultConstruct;

            MaxHealth = playerSO.BaseHealth;
            CurrentHealth = playerSO.BaseHealth;
            GatheringTime = playerSO.BaseGatheringTime;
            GatherRadius = playerSO.BaseGatherRadius;
            ItemMagnetRadius = playerSO.BaseItemMagnetRadius;
            CollectionRadius = playerSO.BaseCollectionRadius;
            GatheringDamage = playerSO.BaseGatheringDamage;
            LightRadius = playerSO.BaseLightRadius;
            RepairTime = playerSO.BaseRepairTime;

            BuildTime = playerSO.BaseBuildingTime;

            UpdateEntityStats();
        }

        private void GetInput()
        {
            raw_input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            raw_input.Normalize();

            calculated_input = Vector3.Lerp(raw_input, calculated_input, inputSmoothing * Time.deltaTime);

            if (Input.GetKeyDown(BagToggleKey))
            {
                ToggleBag();
            }

            if (Input.GetKeyDown(SpellBookKey))
            {
                //ToggleSpellBook();
            }

            if(preparingBuild)
            {
                SelectConstruct();

                if (TryGetTargetedLocation(buildingArea, out RaycastHit raycastHit))
                {
                    EvaluateBuild(raycastHit);
                }   
            }
        }

        private bool Move()
        {
            Vector3 movementDirection = calculated_input.normalized;

            if (movementDirection != Vector3.zero)
            {
                transform.forward = movementDirection;


                rigidBody.velocity = movementDirection * MovementSpeed;
                return true;
            }
            else
            {
   
                rigidBody.velocity = Vector3.zero;
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

            if(currentBuildLocation != Vector3.zero)
            {
                ChangeAction(EntityActionState.Build);
            }

            if (state == EntityPrimaryState.Idle)
            {
                 GetActionFromProximity();
            }

            if (state == EntityPrimaryState.Action)
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
                if (action == EntityActionState.Build)
                {
                    Build();
                }

                else if (action == EntityActionState.Attack)
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

            int numMiscColliders = Physics.OverlapSphereNonAlloc(transform.position, GatherRadius, miscBuffer, gatherLayerMask);
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

            if(state != EntityPrimaryState.Action)
            {
                currentBuildLocation = Vector3.zero;
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

                case EntityActionState.Build:

                    EvaluateActionAnimation(UnitSO.BuildAnimation, BuildTime);
                    action = EntityActionState.Build;
                    ChangePrimaryState(EntityPrimaryState.Action);
                    ToggleSpellBook(false);
                    Look(currentBuildLocation);
                    actionRemainingTime = BuildTime;
                    break;

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

        public void EvaluateBuild(RaycastHit raycastHit)
        {
            if (raycastHit.collider == null) return;

            ShowArrowIndicators(transform.position, raycastHit.point);
            constructPreview.transform.position = raycastHit.point;
            constructPreview.SetActive(true);

            if (IsNormalUpwards(raycastHit))
            {
                Vector3 towerHalfSquare = targetSafeZoneSize * 0.5f * Vector3.one;

                var colliders = Physics.OverlapBoxNonAlloc(
                    raycastHit.point, towerHalfSquare,
                    constructAreaColliders,
                    Quaternion.identity,
                    excludedArea);

                if (colliders > 0)
                {
                    buildState = BuildState.Obstructed;
                }

                else
                {
                    buildState = BuildState.Buildable;

                    if (Input.GetMouseButtonUp(0))
                    {
                        currentBuildLocation = raycastHit.point;
                    }
                }
            }

            else
            {
                buildState = BuildState.Obstructed;
            }
        }

        private void SelectConstruct()
        {
            if (Input.GetKeyDown(ConstructKey1))
            {
                currentSelectedConstruct = 0;
            }

            else if (Input.GetKeyDown(ConstructKey2))
            {
                currentSelectedConstruct = 1;
            }

            else if (Input.GetKeyDown(ConstructKey3))
            {
                currentSelectedConstruct = 2;
            }

            else if (Input.GetKeyDown(ConstructKey4))
            {
                currentSelectedConstruct = 3;
            }
        }
        
        private void Build()
        {
            //check resources

            ConstructSO selectedConstructSO = playerConstructs[currentSelectedConstruct];

            ConstructManager.Instance.PlaceConstruct(selectedConstructSO, currentBuildLocation);
            currentBuildLocation = Vector3.zero;
        }
        
        private void ShowArrowIndicators(Vector3 fromLocation, Vector3 toLocation)
        {
            Vector3 direction = (toLocation - fromLocation).normalized;
            Vector3 offsetVector = direction * indicatorForwardOffset;
            fromLocation += offsetVector;

            for (int i = 0; i < arrowIndicators.Count; i++)
            {
                float t = (float)i / resolution;
                Vector3 point = Vector3.Lerp(fromLocation, toLocation, t);
                point.y += trajectoryCurve.Evaluate(t);
                arrowIndicators[i].transform.position = point;
                arrowIndicators[i].SetActive(true);
            }
        }

        private void HideArrowIndicators()
        {
            foreach(GameObject gameObj in arrowIndicators)
            {
                gameObj.SetActive(false);
            }
        }

        private bool IsNormalUpwards(RaycastHit hit)
        {
            return Vector3.Dot(hit.normal, Vector3.up) > 0.9f;
        }

        private bool TryGetTargetedLocation(LayerMask mask, out RaycastHit raycastHit)
        {

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, mask))
            {
                return true;
            }

            return false;
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

        private void ToggleSpellBook(bool toggle)
        {
            //We're currently building
            if (action == EntityActionState.Build && actionRemainingTime > 0)
            {
                return;
            }


            if (toggle)
            {
                preparingBuild = true;
                currentSelectedConstruct = 0;
                SpellBook.ToggleSpellBook(true);
            }

            else
            {
                HideArrowIndicators();
                constructPreview.SetActive(false);
                SpellBook.ToggleSpellBook(false);
            }
            
        }
    }

}