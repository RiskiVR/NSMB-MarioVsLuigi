using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

using Fusion;
using NSMB.Extensions;
using NSMB.Utils;

public class PlayerController : FreezableEntity, IPlayerInteractable {

    #region Variables

    //---Networked Variables
    //-Player State
    [Networked] public Enums.PowerupState State { get; set; } = Enums.PowerupState.Small;
    [Networked] public byte Stars { get; set; }
    [Networked] public byte Coins { get; set; }
    [Networked] public sbyte Lives { get; set; } = -1;
    [Networked] public Enums.PowerupState StoredPowerup { get; set; }

    //-Player Movement
    //Generic
    [Networked] public NetworkBool IsOnGround { get; set; }
    [Networked] public NetworkBool IsCrouching { get; set; }
    //Knockback
    [Networked] public NetworkBool IsInKnockback { get; set; }
    [Networked] public TickTimer KnockbackTimer { get; set; }
    //Groundpound
    [Networked] public NetworkBool IsGroundpounding { get; set; }
    [Networked] public TickTimer GroundpoundStartTimer { get; set; }
    //Spinner
    [Networked] public NetworkBool IsSpinnerFlying { get; set; }
    [Networked] public NetworkBool IsDrilling { get; set; }
    //Walljump
    [Networked] public TickTimer WallJumpTimer { get; set; }
    [Networked] public TickTimer WallSlideTimer { get; set; }

    //-Death & Respawning
    [Networked] public NetworkBool IsDead { get; set; }
    [Networked] public TickTimer RespawnTimer { get; set; }
    [Networked] public TickTimer DeathTimer { get; set; }

    //-Entity Interactions
    [Networked] public HoldableEntity HeldEntity { get; set; }
    [Networked] public TickTimer ShellSlowdownTimer { get; set; }
    [Networked] public TickTimer DamageInvincibilityTimer { get; set; }

    //-Powerup Stuffs
    [Networked] public TickTimer FireballShootTimer { get; set; }
    [Networked] public NetworkBool CanShootFireball { get; set; }
    [Networked] public TickTimer StarmanTimer { get; set; }
    [Networked] public NetworkBool IsPropellerFlying { get; set; }
    [Networked] public TickTimer PropellerLaunchTimer { get; set; }
    [Networked] public TickTimer PropellerSpinTimer { get; set; }
    [Networked] public TickTimer GiantStartTimer { get; set; }
    [Networked] public TickTimer GiantTimer { get; set; }
    [Networked] public TickTimer GiantEndTimer { get; set; }
    [Networked] public NetworkBool IsInShell { get; set; }

    //---Properties
    public bool IsStarmanInvincible { get => !StarmanTimer.ExpiredOrNotRunning(Runner); }
    public bool IsDamageable { get => !(IsStarmanInvincible || DamageInvincibilityTimer.ExpiredOrNotRunning(Runner)); }
    public override bool IsCarryable => true;
    public override bool IsFlying => IsSpinnerFlying || IsPropellerFlying; //doesn't work consistently?

    //---Components
    private BoxCollider2D[] hitboxes;

    public Enums.PowerupState previousState;
    public FadeOutManager fadeOut;
    public AudioSource sfxBrick;
    private Animator animator;
    private NetworkRigidbody2D networkRigidbody;
    public CameraController cameraController;
    public PlayerAnimationController animationController;

    public int playerId = -1;

    [SerializeField] public float slowriseGravity = 0.85f, normalGravity = 2.5f, flyingGravity = 0.8f, flyingTerminalVelocity = 1.25f, drillVelocity = 7f, groundpoundTime = 0.25f, groundpoundVelocity = 10, blinkingSpeed = 0.25f, terminalVelocity = -7f, jumpVelocity = 6.25f, megaJumpVelocity = 16f, launchVelocity = 12f, wallslideSpeed = -4.25f, giantStartTime = 1.5f, soundRange = 10f, slopeSlidingAngle = 12.5f, pickupTime = 0.5f;
    [SerializeField] public float propellerLaunchVelocity = 6, propellerFallSpeed = 2, propellerSpinFallSpeed = 1.5f, propellerSpinTime = 0.75f, propellerDrillBuffer, heightSmallModel = 0.42f, heightLargeModel = 0.82f;
    [SerializeField] private GameObject models;
    [SerializeField] public CharacterData character;




    public bool previousOnGround, crushGround, doGroundSnap, jumping, properJump, hitRoof, skidding, turnaround, singlejump, doublejump, triplejump, bounce, groundpoundLastFrame, sliding, hitBlock, functionallyRunning, hitLeft, hitRight, stuckInBlock, alreadyStuckInBlock, usedPropellerThisJump, stationaryGiantEnd, fireballKnockback, startedSliding;
    public float jumpLandingTimer, landing, koyoteTime, groundpoundStartTimer, pickupTimer, powerupFlash, jumpBuffer;
    public float floorAngle, pipeTimer;

    //MOVEMENT STAGES
    private static readonly int WALK_STAGE = 1, RUN_STAGE = 3, STAR_STAGE = 4;
    private static readonly float[] SPEED_STAGE_MAX = { 0.9375f, 2.8125f, 4.21875f, 5.625f, 8.4375f };
    private static readonly float SPEED_SLIDE_MAX = 7.5f;
    private static readonly float[] SPEED_STAGE_ACC = { 7.9101585f, 3.955081725f, 3.515625f, 2.63671875f, 84.375f };
    private static readonly float[] WALK_TURNAROUND_ACC = { 39.74853510198f, 8.7890625f, 14.062536f };
    private static readonly float BUTTON_RELEASE_DEC = 3.9550781196f;
    private static readonly float SKIDDING_THRESHOLD = 4.6875f;
    private static readonly float SKIDDING_DEC = 10.54687536f;
    private static readonly float SKIDDING_STAR_DEC = SPEED_STAGE_ACC[^1];

    private static readonly float WALLJUMP_HSPEED = 4.21874f;
    private static readonly float WALLJUMP_VSPEED = 6.4453125f;

    private static readonly float KNOCKBACK_DEC = 7.9101585f;

    private static readonly float[] SPEED_STAGE_SPINNER_MAX = { 1.12060546875f, 2.8125f };
    private static readonly float[] SPEED_STAGE_SPINNER_ACC = { 7.91015625f, 3.955078125f };

    private static readonly float[] SPEED_STAGE_MEGA_ACC = { 28.125f, 4.83398433f, 4.83398433f, 4.83398433f, 4.83398433f };
    private static readonly float[] WALK_TURNAROUND_MEGA_ACC = { 4.614257808f, 10.546875f, 21.09375f };

    private static readonly float TURNAROUND_THRESHOLD = 2.8125f;
    private static readonly float TURNAROUND_ACC = 28.125f;
    private float turnaroundFrames;
    private int turnaroundBoostFrames;

    private static readonly float[] BUTTON_RELEASE_ICE_DEC = { 0.439453125f, 1.483154296875f, 1.483154296875f, 1.483154296875f, 1.483154296875f };
    private static readonly float SKIDDING_ICE_DEC = 3.955078125f;
    private static readonly float WALK_TURNAROUND_ICE_ACC = 2.63671875f;

    private static readonly float SLIDING_45_ACC = 13.1835975f;
    private static readonly float SLIDING_22_ACC = 5.2734375f;

    public float RunningMaxSpeed => SPEED_STAGE_MAX[RUN_STAGE];
    public float WalkingMaxSpeed => SPEED_STAGE_MAX[WALK_STAGE];

    private int MovementStage {
        get {
            float xVel = Mathf.Abs(currentVelocity.x);
            float[] arr = (IsSpinnerFlying || IsPropellerFlying) && State != Enums.PowerupState.MegaMushroom ? SPEED_STAGE_SPINNER_MAX : SPEED_STAGE_MAX;
            for (int i = 0; i < arr.Length; i++) {
                if (xVel <= arr[i])
                    return i;
            }
            return arr.Length - 1;
        }
    }

    //Walljumping variables
    public bool wallSlideLeft, wallSlideRight;

    private int _starCombo;
    public int StarCombo {
        get => IsStarmanInvincible ? _starCombo : 0;
        set => _starCombo = IsStarmanInvincible ? value : 0;
    }

    public Vector2 pipeDirection;

    public FrozenCube frozenObject;

    public Vector2 giantSavedVelocity, currentVelocity, previousFrameVelocity, previousFramePosition;

    public GameObject onSpinner;
    public PipeManager pipeEntering;
    public bool step, alreadyGroundpounded;

    //Tile data
    private Enums.Sounds footstepSound = Enums.Sounds.Player_Walk_Grass;
    public bool onIce;
    private readonly List<Vector3Int> tilesStandingOn = new(), tilesJumpedInto = new(), tilesHitSide = new();

    private GameObject trackIcon;

    private bool initialKnockbackFacingRight = false;

    // == FREEZING VARIABLES ==


    public BoxCollider2D MainHitbox => hitboxes[0];
    public Vector2 WorldHitboxSize => MainHitbox.size * transform.lossyScale;

    public PlayerNetworkInput currentInputs, previousInputs;


    public PlayerData data;

    #endregion

    #region Unity Methods
    public override void Awake() {
        base.Awake();

        cameraController = GetComponent<CameraController>();

        animator = GetComponentInChildren<Animator>();
        sfxBrick = GetComponents<AudioSource>()[1];
        //hitboxManager = GetComponent<WrappingHitbox>();
        animationController = GetComponent<PlayerAnimationController>();
        networkRigidbody = GetComponent<NetworkRigidbody2D>();
        fadeOut = GameObject.FindGameObjectWithTag("FadeUI").GetComponent<FadeOutManager>();

        body.position = transform.position = GameManager.Instance.GetSpawnpoint(playerId);

        //TODO
        //int count = 0;
        //foreach (var player in PhotonNetwork.PlayerList) {

        //    Utils.GetCustomProperty(Enums.NetPlayerProperties.Spectator, out bool spectating, photonView.Owner.CustomProperties);
        //    if (spectating)
        //        continue;

        //    if (player == photonView.Owner)
        //        break;
        //    count++;
        //}
        //playerId = count;

        //Utils.GetCustomProperty(Enums.NetRoomProperties.Lives, out lives);


        GameManager.Instance.players.Add(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        GameManager.Instance.players.Remove(this);
    }

    public void OnEnable() {
        InputSystem.controls.Player.ReserveItem.performed += OnReserveItem;
    }

    public void OnDisable() {
        InputSystem.controls.Player.ReserveItem.performed -= OnReserveItem;
    }

    public override void Spawned() {
        hitboxes = GetComponentsInChildren<BoxCollider2D>();
        trackIcon = UIUpdater.Instance.CreatePlayerIcon(this);
        transform.position = body.position = GameManager.Instance.spawnpoint;

        cameraController.IsControllingCamera = Object.HasInputAuthority;
        cameraController.Recenter();

        body = networkRigidbody.Rigidbody;

        data = Object.InputAuthority.GetPlayerData(Runner);
        if (Object.HasInputAuthority) {
            GameManager.Instance.localPlayer = this;
            GameManager.Instance.spectationManager.Spectating = false;
        }
    }

    private void UpdateInputs() {
        previousInputs = currentInputs;
        currentInputs = GetInput<PlayerNetworkInput>() ?? new();
    }

    public override void FixedUpdateNetwork() {
        //game ended, freeze.

        if (!GameManager.Instance.musicEnabled) {
            models.SetActive(false);
            return;
        }
        if (GameManager.Instance.gameover) {
            body.velocity = Vector2.zero;
            animator.enabled = false;
            body.isKinematic = true;
            return;
        }

        UpdateInputs();

        if (IsDead) {

            if (DeathTimer.Expired(Runner)) {
                PreRespawn();
                DeathTimer = TickTimer.None;
            }

            if (RespawnTimer.Expired(Runner)) {
                Respawn();
            }

        } else {
            currentVelocity = body.velocity;

            CheckForPowerupActions();

            groundpoundLastFrame = IsGroundpounding;
            previousOnGround = IsOnGround;
            HandleBlockSnapping();
            bool snapped = GroundSnapCheck();

            CheckForEntityCollision();

            HandleGroundCollision();
            IsOnGround |= snapped;
            doGroundSnap = IsOnGround;
            HandleTileProperties();
            TickCounters(Runner.DeltaTime);
            HandleMovement(Runner.DeltaTime);
            HandleGiantTiles(true);
            UpdateHitbox();

            body.velocity = currentVelocity;
        }

        animationController.UpdateAnimatorStates();
        animationController.HandleMiscStates();
        HandleLayerState();
        previousFrameVelocity = body.velocity;
        previousFramePosition = body.position;
    }
    #endregion

    private void CheckForPowerupActions() {
        //fireball shoot on sprint check
        if (Settings.Instance.fireballFromSprint && currentInputs.buttons.WasPressed(previousInputs.buttons, PlayerControls.Sprint)) {
            if (State == Enums.PowerupState.FireFlower || State == Enums.PowerupState.IceFlower) {
                ActivatePowerupAction();
            }
        }

        //powerup action button check
        if (currentInputs.buttons.WasPressed(previousInputs.buttons, PlayerControls.PowerupAction)) {
            ActivatePowerupAction();
        }
    }

    #region -- COLLISIONS --
    private void HandleGroundCollision() {
        tilesJumpedInto.Clear();
        tilesStandingOn.Clear();
        tilesHitSide.Clear();

        bool ignoreRoof = false;
        int down = 0, left = 0, right = 0, up = 0;

        crushGround = false;
        foreach (BoxCollider2D hitbox in hitboxes) {
            ContactPoint2D[] contacts = new ContactPoint2D[20];
            int collisionCount = hitbox.GetContacts(contacts);

            for (int i = 0; i < collisionCount; i++) {
                ContactPoint2D contact = contacts[i];
                GameObject go = contact.collider.gameObject;
                Vector2 n = contact.normal;
                Vector2 p = contact.point + (contact.normal * -0.15f);
                if (n == Vector2.up && contact.point.y > body.position.y)
                    continue;

                Vector3Int vec = Utils.WorldToTilemapPosition(p);
                if (!contact.collider || contact.collider.CompareTag("Player"))
                    continue;

                if (Vector2.Dot(n, Vector2.up) > .05f) {
                    if (Vector2.Dot(currentVelocity.normalized, n) > 0.1f && !IsOnGround) {
                        if (!contact.rigidbody || contact.rigidbody.velocity.y < currentVelocity.y)
                            //invalid flooring
                            continue;
                    }

                    crushGround |= !go.CompareTag("platform") && !go.CompareTag("frozencube");
                    down++;
                    tilesStandingOn.Add(vec);
                } else if (contact.collider.gameObject.layer == Layers.LayerGround) {
                    if (Vector2.Dot(n, Vector2.down) > .9f) {
                        up++;
                        tilesJumpedInto.Add(vec);
                    } else {
                        if (n.x < 0) {
                            right++;
                        } else {
                            left++;
                        }
                        tilesHitSide.Add(vec);
                    }
                }
            }
        }

        IsOnGround = down >= 1;
        hitLeft = left >= 1;
        hitRight = right >= 1;
        hitRoof = !ignoreRoof && up > 1;
    }
    private void HandleTileProperties() {
        onIce = false;
        footstepSound = Enums.Sounds.Player_Walk_Grass;
        foreach (Vector3Int pos in tilesStandingOn) {
            TileBase tile = Utils.GetTileAtTileLocation(pos);
            if (tile == null)
                continue;
            if (tile is TileWithProperties propTile) {
                footstepSound = propTile.footstepSound;
                onIce = propTile.iceSkidding;
            }
        }
    }

    private readonly Collider2D[] results = new Collider2D[64];
    private readonly Collider2D[] tempResults = new Collider2D[64];
    private static ContactFilter2D filter;

    private void CheckForEntityCollision() {
        //Don't check for collisions if we're dead, frozen, in a pipe, etc.
        if (IsDead || IsFrozen || pipeEntering)
            return;

        if (!filter.useLayerMask) {
            filter.SetLayerMask((int) (((uint) (/*(1 << Layers.LayerPlayer) | */(1 << Layers.LayerGround))) ^ 0xFFFFFFFF));
        }

        int collisions = 0;
        foreach (BoxCollider2D hitbox in hitboxes) {
            int count = Runner.GetPhysicsScene2D().OverlapBox(body.position + hitbox.offset * transform.localScale, hitbox.size * transform.localScale, 0, filter, tempResults);
            Array.Copy(tempResults, 0, results, collisions, count);
            collisions += count;
        }

        for (int i = 0; i < collisions; i++) {
            GameObject collidedObject = results[i].gameObject;

            //don't interact with ourselves.
            if (results[i].attachedRigidbody == body)
                continue;

            //don't interact with objects we're holding.
            if (HeldEntity && HeldEntity.gameObject == collidedObject)
                continue;

            if (collidedObject.GetComponentInParent<IPlayerInteractable>() is IPlayerInteractable interactable)
                interactable.InteractWithPlayer(this);
        }
    }

    public void OnCollisionStay2D(Collision2D collision) {
        if ((IsInKnockback && !fireballKnockback) || IsFrozen)
            return;

        GameObject obj = collision.gameObject;

        switch (collision.gameObject.tag) {
        case "MarioBrosPlatform": {
            List<Vector2> points = new();
            foreach (ContactPoint2D c in collision.contacts) {
                if (c.normal != Vector2.down)
                    continue;

                points.Add(c.point);
            }
            if (points.Count == 0)
                return;

            Vector2 avg = new();
            foreach (Vector2 point in points)
                avg += point;
            avg /= points.Count;

            MarioBrosPlatform platform = obj.GetComponent<MarioBrosPlatform>();
            platform.Bump(this, avg);
            break;
        }
        }
    }


    public void InteractWithPlayer(PlayerController other) {

        //hit players

        if (other.IsStarmanInvincible) {
            //They are invincible. let them decide if they've hit us.
            if (IsStarmanInvincible) {
                //oh, we both are. bonk.
                DoKnockback(other.body.position.x > body.position.x, 1, true, 0);
                DoKnockback(other.body.position.x < body.position.x, 1, true, 0);
            }
            return;
        }

        if (IsStarmanInvincible) {
            //we are invincible. murder time :)
            if (other.State == Enums.PowerupState.MegaMushroom) {
                //wait fuck-
                DoKnockback(other.body.position.x > body.position.x, 1, true, 0);
                return;
            }

            Powerdown(false);
            body.velocity = previousFrameVelocity;
            return;
        }

        float dot = Vector2.Dot((body.position - other.body.position).normalized, Vector2.up);
        bool above = dot > 0.7f;
        bool otherAbove = dot < -0.7f;

        //mega mushroom cases
        if (State == Enums.PowerupState.MegaMushroom || other.State == Enums.PowerupState.MegaMushroom) {
            if (State == Enums.PowerupState.MegaMushroom && other.State == Enums.PowerupState.MegaMushroom) {
                //both giant
                if (above) {
                    bounce = true;
                    IsGroundpounding = false;
                    IsDrilling = false;
                    PlaySound(Enums.Sounds.Enemy_Generic_Stomp);
                } else if (!otherAbove) {
                    DoKnockback(other.body.position.x < body.position.x, 0, true, 0);
                    DoKnockback(other.body.position.x > body.position.x, 0, true, 0);
                }
            } else if (State == Enums.PowerupState.MegaMushroom) {
                //only we are giant
                Powerdown(false);
                body.velocity = previousFrameVelocity;
            }
            return;
        }

        //blue shell cases
        if (IsInShell) {
            //we are blue shell
            if (!otherAbove) {
                //hit them. powerdown them
                if (other.IsInShell) {
                    //collide with both
                    DoKnockback(other.body.position.x < body.position.x, 1, true, 0);
                    DoKnockback(other.body.position.x > body.position.x, 1, true, 0);
                } else {
                    Powerdown(false);
                }
                float dotRight = Vector2.Dot((body.position - other.body.position).normalized, Vector2.right);
                FacingRight = dotRight > 0;
                return;
            }
        }
        if (State == Enums.PowerupState.BlueShell && otherAbove && (!other.IsGroundpounding && !other.IsDrilling) && (IsCrouching || IsGroundpounding)) {
            body.velocity = new(SPEED_STAGE_MAX[RUN_STAGE] * 0.9f * (other.body.position.x < body.position.x ? 1 : -1), body.velocity.y);
        }
        if (other.IsInShell && !above)
            return;

        if (!above && other.State == Enums.PowerupState.BlueShell && !other.IsInShell && other.IsCrouching && !IsGroundpounding && !IsDrilling) {
            //they are blue shell
            bounce = true;
            PlaySound(Enums.Sounds.Enemy_Generic_Stomp);
            return;
        }

        if (above) {
            //hit them from above
            bounce = !IsGroundpounding && !IsDrilling;
            bool groundpounded = IsGroundpounding || IsDrilling;

            if (State == Enums.PowerupState.MiniMushroom && other.State != Enums.PowerupState.MiniMushroom) {
                //we are mini, they arent. special rules.
                if (groundpounded) {
                    DoKnockback(other.body.position.x < body.position.x, 1, false, 0);
                    IsGroundpounding = false;
                    bounce = true;
                } else {
                    PlaySound(Enums.Sounds.Enemy_Generic_Stomp);
                }
            } else if (other.State == Enums.PowerupState.MiniMushroom && groundpounded) {
                //we are big, groundpounding a mini opponent. squish.
                DoKnockback(other.body.position.x > body.position.x, 3, false, 0);
                bounce = false;
            } else {
                if (other.State == Enums.PowerupState.MiniMushroom && groundpounded) {
                    Powerdown(false);
                } else {
                    DoKnockback(other.body.position.x < body.position.x, groundpounded ? 3 : 1, false, 0);
                }
            }
            body.velocity = new Vector2(previousFrameVelocity.x, body.velocity.y);

            return;
        } else if (!IsInKnockback && !other.IsInKnockback && !otherAbove && IsOnGround && other.IsOnGround && (Mathf.Abs(previousFrameVelocity.x) > WalkingMaxSpeed || Mathf.Abs(other.previousFrameVelocity.x) > WalkingMaxSpeed)) {
            //bump

            DoKnockback(other.body.transform.position.x < body.position.x, 1, true, 0);
            DoKnockback(other.body.transform.position.x > body.position.x, 1, true, 0);
        }
    }

    public void OnTriggerEnter2D(Collider2D collider) {
        if (IsDead || IsFrozen || pipeEntering || !MainHitbox.IsTouching(collider))
            return;


        OnTriggerStay2D(collider);
    }

    protected void OnTriggerStay2D(Collider2D collider) {
        GameObject obj = collider.gameObject;
        if (obj.CompareTag("spinner")) {
            onSpinner = obj;
            return;
        }
    }

    protected void OnTriggerExit2D(Collider2D collider) {
        if (collider.CompareTag("spinner"))
            onSpinner = null;
    }
    #endregion

    #region -- CONTROLLER FUNCTIONS --
    private void ActivatePowerupAction() {
        if (IsDead || IsFrozen || IsInKnockback || pipeEntering || GameManager.Instance.gameover || HeldEntity)
            return;

        switch (State) {
        case Enums.PowerupState.IceFlower:
        case Enums.PowerupState.FireFlower: {
            if (wallSlideLeft || wallSlideRight || IsGroundpounding || triplejump || IsSpinnerFlying || IsDrilling || IsCrouching || sliding)
                return;

            int count = 0;
            foreach (FireballMover existingFire in FindObjectsOfType<FireballMover>()) {
                if (existingFire.Object?.InputAuthority == Object.InputAuthority && ++count >= 6)
                    return;
            }

            if (count <= 1) {
                FireballShootTimer = TickTimer.CreateFromSeconds(Runner, 1.25f);
                CanShootFireball = count == 0;
            } else if (FireballShootTimer.ExpiredOrNotRunning(Runner)) {
                FireballShootTimer = TickTimer.CreateFromSeconds(Runner, 1.25f);
                CanShootFireball = true;
            } else if (CanShootFireball) {
                CanShootFireball = false;
            } else {
                return;
            }

            bool ice = State == Enums.PowerupState.IceFlower;
            NetworkPrefabRef prefab = ice ? PrefabList.Instance.Obj_Iceball : PrefabList.Instance.Obj_Fireball;

            Vector2 pos = body.position + new Vector2(FacingRight ^ animator.GetCurrentAnimatorStateInfo(0).IsName("turnaround") ? 0.5f : -0.5f, 0.3f);
            if (Utils.IsTileSolidAtWorldLocation(pos)) {
                if (Runner.IsForward) {
                    GameObject wallPrefab = ice ? PrefabList.Instance.Particle_IceballWall : PrefabList.Instance.Particle_FireballWall;
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }
            } else {
                Runner.Spawn(prefab, pos, inputAuthority: Object.InputAuthority, onBeforeSpawned: (runner, obj) => {
                    FireballMover mover = obj.GetComponent<FireballMover>();
                    mover.OnBeforeSpawned(this, FacingRight ^ animator.GetCurrentAnimatorStateInfo(0).IsName("turnaround"));
                });
            }

            if (Runner.IsForward) {
                Enums.Sounds sound = ice ? Enums.Sounds.Powerup_Iceball_Shoot : Enums.Sounds.Powerup_Fireball_Shoot;
                PlaySound(sound);
                animator.SetTrigger("fireball");
            }

            WallJumpTimer = TickTimer.None;
            break;
        }
        case Enums.PowerupState.PropellerMushroom: {
            if (IsGroundpounding || (IsSpinnerFlying && IsDrilling) || IsPropellerFlying || IsCrouching || sliding || !WallJumpTimer.ExpiredOrNotRunning(Runner))
                return;

            StartPropeller();
            break;
        }
        }
    }

    protected void StartPropeller() {
        if (usedPropellerThisJump)
            return;

        currentVelocity.y = propellerLaunchVelocity;
        PropellerLaunchTimer = TickTimer.CreateFromSeconds(Runner, 1f);
        PlaySound(Enums.Sounds.Powerup_PropellerMushroom_Start);

        animator.SetTrigger("propeller_start");
        IsPropellerFlying = true;
        IsSpinnerFlying = false;
        IsCrouching = false;

        singlejump = false;
        doublejump = false;
        triplejump = false;

        wallSlideLeft = false;
        wallSlideRight = false;

        if (IsOnGround) {
            IsOnGround = false;
            doGroundSnap = false;
            body.position += Vector2.up * 0.15f;
        }
        usedPropellerThisJump = true;
    }

    public void OnReserveItem(InputAction.CallbackContext context) {
        if (!Object.HasInputAuthority || GameManager.Instance.paused || GameManager.Instance.gameover)
            return;

        if (StoredPowerup == Enums.PowerupState.None || IsDead) {
            PlaySound(Enums.Sounds.UI_Error);
            return;
        }

        RPC_SpawnReserveItem();
    }
    #endregion

    #region -- POWERUP / POWERDOWN --

    public void Powerdown(bool ignoreInvincible) {
        if (ignoreInvincible || !IsDamageable)
            return;

        previousState = State;
        bool nowDead = false;

        switch (State) {
        case Enums.PowerupState.MiniMushroom:
        case Enums.PowerupState.Small: {
            Death(false, false);
            nowDead = true;
            break;
        }
        case Enums.PowerupState.Mushroom: {
            State = Enums.PowerupState.Small;
            powerupFlash = 2f;
            SpawnStars(1, false);
            break;
        }
        case Enums.PowerupState.FireFlower:
        case Enums.PowerupState.IceFlower:
        case Enums.PowerupState.PropellerMushroom:
        case Enums.PowerupState.BlueShell: {
            State = Enums.PowerupState.Mushroom;
            powerupFlash = 2f;
            SpawnStars(1, false);
            break;
        }
        }
        IsPropellerFlying = false;
        PropellerLaunchTimer = TickTimer.None;
        PropellerSpinTimer = TickTimer.None;
        usedPropellerThisJump = false;

        if (!nowDead) {
            DamageInvincibilityTimer = TickTimer.CreateFromSeconds(Runner, 3f);
            PlaySound(Enums.Sounds.Player_Sound_Powerdown);
        }
    }
    #endregion

    #region -- FREEZING --

    public override void Freeze(FrozenCube cube) {
        if (IsInKnockback || !IsDamageable || IsFrozen || State == Enums.PowerupState.MegaMushroom)
            return;

        PlaySound(Enums.Sounds.Enemy_Generic_Freeze);
        frozenObject = cube;
        IsFrozen = true;
        frozenObject.AutoBreakTimer = TickTimer.CreateFromSeconds(Runner, 1.75f);
        animator.enabled = false;
        body.isKinematic = true;
        body.simulated = false;
        IsInKnockback = false;
        skidding = false;
        IsDrilling = false;
        wallSlideLeft = false;
        wallSlideRight = false;
        IsPropellerFlying = false;

        PropellerLaunchTimer = TickTimer.None;
        skidding = false;
    }

    public override void Unfreeze(UnfreezeReason reason) {
        if (!IsFrozen)
            return;

        IsFrozen = false;
        animator.enabled = true;
        body.simulated = true;
        body.isKinematic = false;

        int knockbackStars = reason switch {
            UnfreezeReason.Timer => 0,
            UnfreezeReason.Groundpounded => 2,
            _ => 1
        };

        if (frozenObject && frozenObject.Object.HasStateAuthority) {
            frozenObject.Holder?.DoKnockback(frozenObject.Holder.FacingRight, 1, true, 0);
            frozenObject.Kill();
        }

        if (knockbackStars > 0)
            DoKnockback(FacingRight, knockbackStars, true, -1);
        else
            DamageInvincibilityTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);
    }
    #endregion

    public override void Bump(BasicEntity bumper, Vector3Int tile, InteractableTile.InteractionDirection direction) {
        if (IsInKnockback)
            return;

        DoKnockback(bumper.body.position.x < body.position.x, 1, false, 0);
    }

    #region -- COIN / STAR COLLECTION --



    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_SpawnCoinEffects(Vector3 position, byte coins) {
        PlaySound(Enums.Sounds.World_Coin_Collect);
        NumberParticle num = Instantiate(PrefabList.Instance.Particle_CoinNumber, position, Quaternion.identity).GetComponentInChildren<NumberParticle>();
        num.ApplyColorAndText(Utils.GetSymbolString(coins.ToString(), Utils.numberSymbols), animationController.GlowColor);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, InvokeResim = true)]
    public void RPC_SpawnReserveItem() {
        if (StoredPowerup == Enums.PowerupState.None)
            return;

        SpawnItem(Enums.PowerupFromState[StoredPowerup].prefab);
        StoredPowerup = Enums.PowerupState.None;
    }

    public void SpawnItem(NetworkPrefabRef prefab) {

        if (prefab == NetworkPrefabRef.Empty)
            prefab = Utils.GetRandomItem(Runner, this).prefab;

        Runner.Spawn(prefab, new(body.position.x, cameraController.currentPosition.y + 1.68f, 0), onBeforeSpawned: (runner, obj) => {
            obj.GetComponent<MovingPowerup>().OnBeforeSpawned(this, 0f);
        });
        PlaySound(Enums.Sounds.Player_Sound_PowerupReserveUse);
    }

    private void SpawnStars(int amount, bool deathplane) {

        bool fastStars = amount > 2 && Stars > 2;
        int starDirection = FacingRight ? 1 : 2;

        while (amount > 0) {
            if (Stars <= 0)
                break;

            if (!fastStars) {
                if (starDirection == 0)
                    starDirection = 2;
                if (starDirection == 3)
                    starDirection = 1;
            }

            Runner.Spawn(PrefabList.Instance.Obj_BigStar, body.position + Vector2.up * WorldHitboxSize.y, onBeforeSpawned: (runner, obj) => {
                StarBouncer bouncer = obj.GetComponent<StarBouncer>();
                bouncer.OnBeforeSpawned((byte) starDirection, false, deathplane);
            });

            starDirection = (starDirection + 1) % 4;
            Stars--;
            amount--;
        }
        GameManager.Instance.CheckForWinner();
    }
    #endregion

    #region -- DEATH / RESPAWNING --
    public void Death(bool deathplane, bool fire) {
        if (IsDead)
            return;

        animator.Play("deadstart");
        if (--Lives == 0) {
            GameManager.Instance.CheckForWinner();
        }

        IsDead = true;
        DeathTimer = TickTimer.CreateFromSeconds(Runner, 3f);

        onSpinner = null;
        pipeEntering = null;
        IsInShell = false;
        IsPropellerFlying = false;
        PropellerLaunchTimer = TickTimer.None;
        PropellerSpinTimer = TickTimer.None;
        IsSpinnerFlying = false;
        IsDrilling = false;
        sliding = false;
        IsCrouching = false;
        skidding = false;
        turnaround = false;
        IsGroundpounding = false;
        IsInKnockback = false;
        wallSlideLeft = false;
        wallSlideRight = false;
        animator.SetBool("knockback", false);
        animator.SetBool("flying", false);
        animator.SetBool("firedeath", fire);

        PlaySound(cameraController.IsControllingCamera ? Enums.Sounds.Player_Sound_Death : Enums.Sounds.Player_Sound_DeathOthers);

        SpawnStars(1, deathplane);
        body.isKinematic = false;
        AttemptThrowHeldItem();

        if (Object.HasInputAuthority)
            ScoreboardUpdater.instance.OnDeathToggle();
    }

    private void AttemptThrowHeldItem(bool? right = null, bool crouch = false) {
        if (right == null)
            right = FacingRight;

        if (HeldEntity)
            HeldEntity.Throw(right.Value, crouch);
        HeldEntity = null;
    }

    public void PreRespawn() {

        RespawnTimer = TickTimer.CreateFromSeconds(Runner, 1.3f);

        sfx.enabled = true;
        if (Lives == 0) {
            GameManager.Instance.CheckForWinner();

            if (Object.HasInputAuthority)
                GameManager.Instance.spectationManager.Spectating = true;

            Runner.Despawn(Object);
            Destroy(trackIcon);
            return;
        }
        FacingRight = true;
        transform.localScale = Vector2.one;
        transform.position = body.position = GameManager.Instance.GetSpawnpoint(playerId);
        IsDead = false;
        previousState = State = Enums.PowerupState.Small;
        animationController.DisableAllModels();
        animator.SetTrigger("respawn");
        StarmanTimer = TickTimer.None;
        GiantTimer = TickTimer.None;
        GiantEndTimer = TickTimer.None;
        GiantStartTimer = TickTimer.None;
        IsGroundpounding = false;
        body.isKinematic = false;

        Instantiate(PrefabList.Instance.Particle_Respawn, body.position, Quaternion.identity);

        cameraController.Recenter();
    }

    public void Respawn() {

        //gameObject.SetActive(true);
        IsDead = false;
        State = Enums.PowerupState.Small;
        previousState = Enums.PowerupState.Small;
        currentVelocity = Vector2.zero;
        wallSlideLeft = false;
        wallSlideRight = false;
        WallSlideTimer = TickTimer.None;
        WallJumpTimer = TickTimer.None;
        IsSpinnerFlying = false;
        FacingRight = true;

        IsPropellerFlying = false;
        usedPropellerThisJump = false;
        PropellerLaunchTimer = TickTimer.None;
        PropellerSpinTimer = TickTimer.None;

        IsCrouching = false;
        IsOnGround = false;
        sliding = false;
        koyoteTime = 1f;
        jumpBuffer = 0;
        StarmanTimer = TickTimer.None;
        GiantStartTimer = TickTimer.None;
        GiantEndTimer = TickTimer.None;
        GiantTimer = TickTimer.None;
        singlejump = false;
        doublejump = false;
        turnaround = false;
        triplejump = false;
        IsInKnockback = false;
        bounce = false;
        skidding = false;
        IsGroundpounding = false;
        IsInShell = false;
        landing = 0f;
        ResetKnockback();
        Instantiate(Resources.Load("Prefabs/Particle/Puff"), transform.position, Quaternion.identity);
        models.transform.rotation = Quaternion.Euler(0, 180, 0);

        if (Object.HasInputAuthority)
            ScoreboardUpdater.instance.OnRespawnToggle();

    }
    #endregion

    #region -- SOUNDS / PARTICLES --
    public void PlaySoundEverywhere(Enums.Sounds sound) {
        GameManager.Instance.sfx.PlayOneShot(sound.GetClip(character));
    }
    public void PlaySound(Enums.Sounds sound, byte variant = 0, float volume = 1) {
        if (sound == Enums.Sounds.Powerup_MegaMushroom_Break_Block) {
            sfxBrick.Stop();
            sfxBrick.clip = sound.GetClip(character, variant);
            sfxBrick.Play();
        } else {
            PlaySound(sound, character, variant, volume);
        }
    }
    protected void SpawnParticle(string particle, Vector2 worldPos, Quaternion? rot = null) {
        Instantiate(Resources.Load(particle), worldPos, rot ?? Quaternion.identity);
    }
    protected void SpawnParticle(GameObject particle, Vector2 worldPos, Quaternion? rot = null) {
        Instantiate(particle, worldPos, rot ?? Quaternion.identity);
    }

    protected void GiantFootstep() {
        CameraController.ScreenShake = 0.15f;
        SpawnParticle(PrefabList.Instance.Particle_Groundpound, body.position + new Vector2(FacingRight ? 0.5f : -0.5f, 0));
        PlaySound(Enums.Sounds.Powerup_MegaMushroom_Walk, (byte) (step ? 1 : 2));
        step = !step;
    }

    protected void Footstep() {
        if (State == Enums.PowerupState.MegaMushroom)
            return;

        bool left = currentInputs.buttons.IsSet(PlayerControls.Left);
        bool right = currentInputs.buttons.IsSet(PlayerControls.Right);

        bool reverse = currentVelocity.x != 0 && ((left ? 1 : -1) == Mathf.Sign(currentVelocity.x));
        if (onIce && (left ^ right) && reverse) {
            PlaySound(Enums.Sounds.World_Ice_Skidding);
            return;
        }
        if (IsPropellerFlying) {
            PlaySound(Enums.Sounds.Powerup_PropellerMushroom_Kick);
            return;
        }
        if (Mathf.Abs(currentVelocity.x) < WalkingMaxSpeed)
            return;

        PlaySound(footstepSound, (byte) (step ? 1 : 2), Mathf.Abs(currentVelocity.x) / (RunningMaxSpeed + 4));
        step = !step;
    }
    #endregion

    #region -- TILE COLLISIONS --
    private void HandleGiantTiles(bool pipes) {
        //TODO?
        if (State != Enums.PowerupState.MegaMushroom || !GiantStartTimer.ExpiredOrNotRunning(Runner))
            return;

        Vector2 checkSize = WorldHitboxSize * 1.1f;

        bool grounded = previousFrameVelocity.y < -8f && IsOnGround;
        Vector2 offset = Vector2.zero;
        if (grounded)
            offset = Vector2.down / 2f;

        Vector2 checkPosition = body.position + (Vector2.up * checkSize * 0.5f) + (2 * Time.fixedDeltaTime * currentVelocity) + offset;

        Vector3Int minPos = Utils.WorldToTilemapPosition(checkPosition - (checkSize * 0.5f), wrap: false);
        Vector3Int size = Utils.WorldToTilemapPosition(checkPosition + (checkSize * 0.5f), wrap: false) - minPos;

        for (int x = 0; x <= size.x; x++) {
            for (int y = 0; y <= size.y; y++) {
                Vector3Int tileLocation = new(minPos.x + x, minPos.y + y, 0);
                Vector2 worldPosCenter = Utils.TilemapToWorldPosition(tileLocation) + Vector3.one * 0.25f;
                Utils.WrapTileLocation(ref tileLocation);

                InteractableTile.InteractionDirection dir = InteractableTile.InteractionDirection.Up;
                if (worldPosCenter.y - 0.25f + Physics2D.defaultContactOffset * 2f <= body.position.y) {
                    if (!grounded && !IsGroundpounding)
                        continue;

                    dir = InteractableTile.InteractionDirection.Down;
                } else if (worldPosCenter.y + Physics2D.defaultContactOffset * 2f >= body.position.y + size.y) {
                    dir = InteractableTile.InteractionDirection.Up;
                } else if (worldPosCenter.x <= body.position.x) {
                    dir = InteractableTile.InteractionDirection.Left;
                } else if (worldPosCenter.x >= body.position.x) {
                    dir = InteractableTile.InteractionDirection.Right;
                }

                BreakablePipeTile pipe = GameManager.Instance.tilemap.GetTile<BreakablePipeTile>(tileLocation);
                if (pipe && (pipe.upsideDownPipe || !pipes || IsGroundpounding))
                    continue;

                InteractWithTile(tileLocation, dir);
            }
        }
        if (pipes) {
            for (int x = 0; x <= size.x; x++) {
                for (int y = size.y; y >= 0; y--) {
                    Vector3Int tileLocation = new(minPos.x + x, minPos.y + y, 0);
                    Vector2 worldPosCenter = Utils.TilemapToWorldPosition(tileLocation) + Vector3.one * 0.25f;
                    Utils.WrapTileLocation(ref tileLocation);

                    InteractableTile.InteractionDirection dir = InteractableTile.InteractionDirection.Up;
                    if (worldPosCenter.y - 0.25f + Physics2D.defaultContactOffset * 2f <= body.position.y) {
                        if (!grounded && !IsGroundpounding)
                            continue;

                        dir = InteractableTile.InteractionDirection.Down;
                    } else if (worldPosCenter.x - 0.25f < checkPosition.x - checkSize.x * 0.5f) {
                        dir = InteractableTile.InteractionDirection.Left;
                    } else if (worldPosCenter.x + 0.25f > checkPosition.x + checkSize.x * 0.5f) {
                        dir = InteractableTile.InteractionDirection.Right;
                    }

                    BreakablePipeTile pipe = GameManager.Instance.tilemap.GetTile<BreakablePipeTile>(tileLocation);
                    if (!pipe || !pipe.upsideDownPipe || dir == InteractableTile.InteractionDirection.Up)
                        continue;

                    InteractWithTile(tileLocation, dir);
                }
            }
        }
    }

    private int InteractWithTile(Vector3Int tilePos, InteractableTile.InteractionDirection direction) {
        TileBase tile = GameManager.Instance.tilemap.GetTile(tilePos);
        if (!tile)
            return 0;
        if (tile is InteractableTile it)
            return it.Interact(this, direction, Utils.TilemapToWorldPosition(tilePos)) ? 1 : 0;

        return 0;
    }
    #endregion

    #region -- KNOCKBACK --

    public void DoKnockback(bool fromRight, int starsToDrop, bool fireball, int attackerView) {
        if (fireball && fireballKnockback && IsInKnockback)
            return;
        if (IsInKnockback && !fireballKnockback)
            return;

        if (GameManager.Instance.GameStartTick == -1 || !DamageInvincibilityTimer.ExpiredOrNotRunning(Runner) || pipeEntering || IsFrozen || IsDead || !GiantStartTimer.ExpiredOrNotRunning(Runner) || !GiantEndTimer.ExpiredOrNotRunning(Runner))
            return;

        if (State == Enums.PowerupState.MiniMushroom && starsToDrop > 1) {
            SpawnStars(2, false);
            Powerdown(false);
            return;
        }

        if (IsInKnockback || fireballKnockback)
            starsToDrop = Mathf.Min(1, starsToDrop);

        IsInKnockback = true;
        KnockbackTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
        fireballKnockback = fireball;
        initialKnockbackFacingRight = FacingRight;

        //TODO:
        //PhotonView attacker = PhotonNetwork.GetPhotonView(attackerView);
        //if (attackerView >= 0) {
        //    if (attacker)
        //        SpawnParticle("Prefabs/Particle/PlayerBounce", attacker.transform.position);
        //
        //    if (fireballKnockback)
        //        PlaySound(Enums.Sounds.Player_Sound_Collision_Fireball, 0, 3);
        //    else
        //        PlaySound(Enums.Sounds.Player_Sound_Collision, 0, 3);
        //}
        animator.SetBool("fireballKnockback", fireball);
        animator.SetBool("knockforwards", FacingRight != fromRight);

        float megaVelo = State == Enums.PowerupState.MegaMushroom ? 3 : 1;
        currentVelocity = new Vector2(
            (fromRight ? -1 : 1) *
            ((starsToDrop + 1) / 2f) *
            4f *
            megaVelo *
            (fireball ? 0.5f : 1f),

            fireball ? 0 : 4.5f
        );

        if (IsOnGround && !fireball)
            body.position += Vector2.up * 0.15f;

        IsOnGround = false;
        doGroundSnap = false;
        IsInShell = false;
        IsGroundpounding = false;
        IsSpinnerFlying = false;
        IsPropellerFlying = false;
        PropellerLaunchTimer = TickTimer.None;
        PropellerSpinTimer = TickTimer.None;
        sliding = false;
        IsDrilling = false;
        body.gravityScale = normalGravity;
        wallSlideLeft = wallSlideRight = false;

        SpawnStars(starsToDrop, false);
        HandleLayerState();
    }

    public void ResetKnockbackFromAnim() {
        ResetKnockback();
    }

    protected void ResetKnockback() {
        DamageInvincibilityTimer = TickTimer.CreateFromSeconds(Runner, 2f);
        KnockbackTimer = TickTimer.None;
        bounce = false;
        IsInKnockback = false;
        currentVelocity.x = 0;
        FacingRight = initialKnockbackFacingRight;
    }
    #endregion

    #region -- ENTITY HOLDING --
    public void HoldingWakeup() {
        HeldEntity = null;
        //holdingOld = null;
        //throwInvincibility = 0;
        Powerdown(false);
    }

    public void SetHolding(HoldableEntity entity) {
        if (HeldEntity) {
            HeldEntity.Holder = null;
            HeldEntity.PreviousHolder = this;
        }

        HeldEntity = entity;

        if (HeldEntity != null) {
            HeldEntity.Holder = this;
            HeldEntity.PreviousHolder = null;

            if (HeldEntity is FrozenCube) {
                animator.Play("head-pickup");
                animator.ResetTrigger("fireball");
                PlaySound(Enums.Sounds.Player_Voice_DoubleJump, 2);
                pickupTimer = 0;
            } else {
                pickupTimer = pickupTime;
            }
            animator.ResetTrigger("throw");
            animator.SetBool("holding", true);

            SetHoldingOffset();
        }
    }
    #endregion

    private void HandleSliding(bool up, bool down, bool left, bool right) {
        startedSliding = false;
        if (IsGroundpounding) {
            if (IsOnGround) {
                if (State == Enums.PowerupState.MegaMushroom) {
                    IsGroundpounding = false;
                    GroundpoundStartTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    return;
                }
                if (!IsInShell && Mathf.Abs(floorAngle) >= slopeSlidingAngle) {
                    IsGroundpounding = false;
                    sliding = true;
                    alreadyGroundpounded = true;
                    currentVelocity = new Vector2(-Mathf.Sign(floorAngle) * SPEED_SLIDE_MAX, 0);
                    startedSliding = true;
                } else {
                    currentVelocity = Vector2.zero;
                    if (!down || State == Enums.PowerupState.MegaMushroom) {
                        IsGroundpounding = false;
                        GroundpoundStartTimer = TickTimer.CreateFromSeconds(Runner, 0.25f);
                    }
                }
            }
            if (up && (GroundpoundStartTimer.RemainingTime(Runner) ?? 0f) <= 0.05f) {
                IsGroundpounding = false;
                currentVelocity = Vector2.down * groundpoundVelocity;
            }
        }
        if (!((FacingRight && hitRight) || (!FacingRight && hitLeft)) && IsCrouching && Mathf.Abs(floorAngle) >= slopeSlidingAngle && !IsInShell && State != Enums.PowerupState.MegaMushroom) {
            sliding = true;
            IsCrouching = false;
            alreadyGroundpounded = true;
        }
        if (sliding && IsOnGround && Mathf.Abs(floorAngle) > slopeSlidingAngle) {
            float angleDeg = floorAngle * Mathf.Deg2Rad;

            bool uphill = Mathf.Sign(floorAngle) == Mathf.Sign(currentVelocity.x);
            float speed = Time.fixedDeltaTime * 5f * (uphill ? Mathf.Clamp01(1f - (Mathf.Abs(currentVelocity.x) / RunningMaxSpeed)) : 4f);

            float newX = Mathf.Clamp(currentVelocity.x - (Mathf.Sin(angleDeg) * speed), -(RunningMaxSpeed * 1.3f), RunningMaxSpeed * 1.3f);
            float newY = Mathf.Sin(angleDeg) * newX + 0.4f;
            currentVelocity = new Vector2(newX, newY);

        }

        if (sliding && (up || ((left ^ right) && !down) || (Mathf.Abs(floorAngle) < slopeSlidingAngle && IsOnGround && currentVelocity.x == 0 && !down) || (FacingRight && hitRight) || (!FacingRight && hitLeft))) {
            sliding = false;
            if (currentVelocity.x == 0 && IsOnGround)
                PlaySound(Enums.Sounds.Player_Sound_SlideEnd);

            //alreadyGroundpounded = false;
        }
    }

    private void HandleSlopes() {
        if (!IsOnGround) {
            floorAngle = 0;
            return;
        }

        RaycastHit2D hit = Physics2D.BoxCast(body.position + (Vector2.up * 0.05f), new Vector2((MainHitbox.size.x - Physics2D.defaultContactOffset * 2f) * transform.lossyScale.x, 0.1f), 0, body.velocity.normalized, (body.velocity * Time.fixedDeltaTime).magnitude, Layers.MaskAnyGround);
        if (hit) {
            //hit ground
            float angle = Vector2.SignedAngle(Vector2.up, hit.normal);
            if (Mathf.Abs(angle) > 89)
                return;

            float x = floorAngle != angle ? previousFrameVelocity.x : currentVelocity.x;

            floorAngle = angle;

            float change = Mathf.Sin(angle * Mathf.Deg2Rad) * x * 1.25f;
            currentVelocity = new Vector2(x, change);
            IsOnGround = true;
            doGroundSnap = true;
        } else if (IsOnGround) {
            hit = Physics2D.BoxCast(body.position + (Vector2.up * 0.05f), new Vector2((MainHitbox.size.x + Physics2D.defaultContactOffset * 3f) * transform.lossyScale.x, 0.1f), 0, Vector2.down, 0.3f, Layers.MaskAnyGround);
            if (hit) {
                float angle = Vector2.SignedAngle(Vector2.up, hit.normal);
                if (Mathf.Abs(angle) > 89)
                    return;

                float x = floorAngle != angle ? previousFrameVelocity.x : currentVelocity.x;
                floorAngle = angle;

                float change = Mathf.Sin(angle * Mathf.Deg2Rad) * x * 1.25f;
                currentVelocity = new Vector2(x, change);
                IsOnGround = true;
                doGroundSnap = true;
            } else {
                floorAngle = 0;
            }
        }
    }

    void HandleLayerState() {
        bool hitsNothing = animator.GetBool("pipe") || IsDead || stuckInBlock || !GiantStartTimer.ExpiredOrNotRunning(Runner) || (!GiantEndTimer.ExpiredOrNotRunning(Runner) && stationaryGiantEnd);
        //bool shouldntCollide = (DamageInvincibilityTimer > 0 && !IsStarmanInvincible) || (knockback && !fireballKnockback);

        int layer = Layers.LayerPlayer;
        if (hitsNothing) {
            layer = Layers.LayerHitsNothing;
        }
        //else if (shouldntCollide) {
        //    layer = Layers.LayerPassthrough;
        //}

        gameObject.layer = layer;
    }

    private bool GroundSnapCheck() {
        if (IsDead || (currentVelocity.y > 0 && !IsOnGround) || !doGroundSnap || pipeEntering || gameObject.layer == Layers.LayerHitsNothing)
            return false;

        bool prev = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = false;
        RaycastHit2D hit = Physics2D.BoxCast(body.position + Vector2.up * 0.1f, new Vector2(WorldHitboxSize.x, 0.05f), 0, Vector2.down, 0.4f, Layers.MaskAnyGround);
        Physics2D.queriesStartInColliders = prev;
        if (hit) {
            body.position = new(body.position.x, hit.point.y + Physics2D.defaultContactOffset);
            return true;
        }
        return false;
    }

    #region -- PIPES --

    private void DownwardsPipeCheck(bool down) {
        if (!down || State == Enums.PowerupState.MegaMushroom || !IsOnGround || IsInKnockback || IsInShell)
            return;

        foreach (RaycastHit2D hit in Physics2D.RaycastAll(body.position, Vector2.down, 0.1f)) {
            GameObject obj = hit.transform.gameObject;
            if (!obj.CompareTag("pipe"))
                continue;
            PipeManager pipe = obj.GetComponent<PipeManager>();
            if (pipe.miniOnly && State != Enums.PowerupState.MiniMushroom)
                continue;
            if (!pipe.entryAllowed)
                continue;

            //Enter pipe
            pipeEntering = pipe;
            pipeDirection = Vector2.down;

            currentVelocity = Vector2.down;
            transform.position = body.position = new Vector2(obj.transform.position.x, transform.position.y);

            PlaySound(Enums.Sounds.Player_Sound_Powerdown);
            IsCrouching = false;
            sliding = false;
            IsPropellerFlying = false;
            IsDrilling = false;
            usedPropellerThisJump = false;
            IsGroundpounding = false;
            IsInShell = false;
            break;
        }
    }

    private void UpwardsPipeCheck(bool up) {
        if (!up || IsGroundpounding || !hitRoof || State == Enums.PowerupState.MegaMushroom)
            return;

        //todo: change to nonalloc?
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(body.position, Vector2.up, 1f)) {
            GameObject obj = hit.transform.gameObject;
            if (!obj.CompareTag("pipe"))
                continue;
            PipeManager pipe = obj.GetComponent<PipeManager>();
            if (pipe.miniOnly && State != Enums.PowerupState.MiniMushroom)
                continue;
            if (!pipe.entryAllowed)
                continue;

            //pipe found
            pipeEntering = pipe;
            pipeDirection = Vector2.up;

            currentVelocity = Vector2.up;
            transform.position = body.position = new Vector2(obj.transform.position.x, transform.position.y);

            PlaySound(Enums.Sounds.Player_Sound_Powerdown);
            IsCrouching = false;
            sliding = false;
            IsPropellerFlying = false;
            usedPropellerThisJump = false;
            IsSpinnerFlying = false;
            IsInShell = false;
            break;
        }
    }
    #endregion

    private void HandleCrouching(bool crouchInput) {
        if (sliding || IsPropellerFlying || IsInKnockback)
            return;

        if (State == Enums.PowerupState.MegaMushroom) {
            IsCrouching = false;
            return;
        }
        bool prevCrouchState = IsCrouching || IsGroundpounding;
        IsCrouching = ((IsOnGround && crouchInput && !IsGroundpounding) || (!IsOnGround && crouchInput && IsCrouching) || (IsCrouching && ForceCrouchCheck())) && !HeldEntity;
        if (IsCrouching && !prevCrouchState) {
            //crouch start sound
            PlaySound(State == Enums.PowerupState.BlueShell ? Enums.Sounds.Powerup_BlueShell_Enter : Enums.Sounds.Player_Sound_Crouch);
        }
    }

    public bool ForceCrouchCheck() {
        //janky fortress ceilingn check, m8
        if (State == Enums.PowerupState.BlueShell && IsOnGround && SceneManager.GetActiveScene().buildIndex != 4)
            return false;
        if (State <= Enums.PowerupState.MiniMushroom)
            return false;

        float width = MainHitbox.bounds.extents.x;
        float uncrouchHeight = GetHitboxSize(false).y * transform.lossyScale.y;

        bool ret = Runner.GetPhysicsScene2D().BoxCast(body.position + Vector2.up * 0.1f, new(width - 0.05f, 0.05f), 0, Vector2.up, uncrouchHeight - 0.1f, Layers.MaskOnlyGround);
        return ret;
    }

    private void HandleWallslide(bool holdingLeft, bool holdingRight, bool jump) {

        Vector2 currentWallDirection;
        if (holdingLeft) {
            currentWallDirection = Vector2.left;
        } else if (holdingRight) {
            currentWallDirection = Vector2.right;
        } else if (wallSlideLeft) {
            currentWallDirection = Vector2.left;
        } else if (wallSlideRight) {
            currentWallDirection = Vector2.right;
        } else {
            return;
        }

        HandleWallSlideStopChecks(currentWallDirection, holdingRight, holdingLeft);

        wallSlideRight &= !WallSlideTimer.Expired(Runner) && hitRight && currentVelocity.y < -0.1f;
        wallSlideLeft &= !WallSlideTimer.Expired(Runner) && hitLeft && currentVelocity.y < -0.1f;

        if (wallSlideLeft || wallSlideRight) {
            //walljump check
            FacingRight = wallSlideLeft;
            if (jump && WallJumpTimer.ExpiredOrNotRunning(Runner)) {
                //perform walljump

                hitRight = false;
                hitLeft = false;
                currentVelocity = new Vector2(WALLJUMP_HSPEED * (wallSlideLeft ? 1 : -1), WALLJUMP_VSPEED);
                singlejump = false;
                doublejump = false;
                triplejump = false;
                IsOnGround = false;
                jumpBuffer = 0f;
                bounce = false;
                PlaySound(Enums.Sounds.Player_Sound_WallJump);
                PlaySound(Enums.Sounds.Player_Voice_WallJump, (byte) GameManager.Instance.Random.RangeExclusive(1, 3));

                Vector2 offset = MainHitbox.size * 0.5f;
                offset.x *= wallSlideLeft ? -1 : 1;

                if (Runner.IsForward)
                    SpawnParticle(PrefabList.Instance.Particle_Walljump, body.position + offset, wallSlideLeft ? Quaternion.identity : Quaternion.Euler(0, 180, 0));

                WallJumpTimer = TickTimer.CreateFromSeconds(Runner, 16f / 60f);
                animator.SetTrigger("walljump");
                WallSlideTimer = TickTimer.None;
            }
        } else {
            //walljump starting check
            bool canWallslide = !IsInShell && currentVelocity.y < -0.1f && !IsGroundpounding && !IsOnGround && !HeldEntity && State != Enums.PowerupState.MegaMushroom && !IsSpinnerFlying && !IsDrilling && !IsCrouching && !sliding && !IsInKnockback;
            if (!canWallslide)
                return;

            //Check 1
            if (!WallJumpTimer.ExpiredOrNotRunning(Runner))
                return;

            //Check 2
            if (!WallSlideTimer.ExpiredOrNotRunning(Runner))
                return;

            //Check 4: already handled
            //Check 5.2: already handled

            //Check 6
            if (IsCrouching)
                return;

            //Check 8
            if (!((currentWallDirection == Vector2.right && FacingRight) || (currentWallDirection == Vector2.left && !FacingRight)))
                return;

            //Start wallslide
            wallSlideRight = currentWallDirection == Vector2.right;
            wallSlideLeft = currentWallDirection == Vector2.left;
            IsPropellerFlying = false;
        }

        wallSlideRight &= !WallSlideTimer.Expired(Runner) && hitRight;
        wallSlideLeft &= !WallSlideTimer.Expired(Runner) && hitLeft;
    }

    private void HandleWallSlideStopChecks(Vector2 wallDirection, bool right, bool left) {
        bool floorCheck = !Runner.GetPhysicsScene2D().Raycast(body.position, Vector2.down, 0.3f, Layers.MaskAnyGround);
        if (!floorCheck) {
            WallSlideTimer = TickTimer.None;
            wallSlideRight = false;
            wallSlideLeft = false;
            return;
        }

        bool moveDownCheck = currentVelocity.y < 0;
        if (!moveDownCheck)
            return;

        bool wallCollisionCheck = wallDirection == Vector2.left ? hitLeft : hitRight;
        if (!wallCollisionCheck)
            return;

        bool heightLowerCheck = Runner.GetPhysicsScene2D().Raycast(body.position + new Vector2(0, 0.2f), wallDirection, MainHitbox.size.x * 2, Layers.MaskOnlyGround);
        if (!heightLowerCheck)
            return;

        if ((wallDirection == Vector2.left && !left) || (wallDirection == Vector2.right && !right)) {
            if (WallSlideTimer.ExpiredOrNotRunning(Runner))
                WallSlideTimer = TickTimer.CreateFromSeconds(Runner, 16 / 60f);
        }
    }

    private void HandleJumping(bool jumpPressed, bool forceJump) {
        if (IsInKnockback || IsDrilling || (State == Enums.PowerupState.MegaMushroom && singlejump))
            return;

        bool topSpeed = Mathf.Abs(currentVelocity.x) >= RunningMaxSpeed;
        if (bounce || (forceJump && (IsOnGround || (koyoteTime < 0.07f && !IsPropellerFlying)) && !startedSliding)) {

            bool canSpecialJump = (forceJump || (bounce && jumpPressed)) && properJump && !IsSpinnerFlying && !IsPropellerFlying && topSpeed && landing < 0.45f && !HeldEntity && !triplejump && !IsCrouching && !IsInShell && ((currentVelocity.x < 0 && !FacingRight) || (currentVelocity.x > 0 && FacingRight)) && !Runner.GetPhysicsScene2D().Raycast(body.position + new Vector2(0, 0.1f), Vector2.up, 1f, Layers.MaskOnlyGround);
            float jumpBoost = 0;

            koyoteTime = 1;
            jumpBuffer = 0;
            skidding = false;
            turnaround = false;
            sliding = false;
            wallSlideLeft = false;
            wallSlideRight = false;
            WallSlideTimer = TickTimer.None;
            //alreadyGroundpounded = false;
            IsGroundpounding = false;
            GroundpoundStartTimer = TickTimer.None;
            IsDrilling = false;
            IsSpinnerFlying &= bounce;
            IsPropellerFlying &= bounce;

            if (!bounce && onSpinner && !HeldEntity) {
                PlaySound(Enums.Sounds.Player_Voice_SpinnerLaunch);
                PlaySound(Enums.Sounds.World_Spinner_Launch);
                currentVelocity.y = launchVelocity;
                IsSpinnerFlying = true;
                IsOnGround = false;
                body.position += Vector2.up * 0.075f;
                doGroundSnap = false;
                previousOnGround = false;
                IsCrouching = false;
                IsInShell = false;
                return;
            }

            float vel = State switch {
                Enums.PowerupState.MegaMushroom => megaJumpVelocity,
                _ => jumpVelocity + Mathf.Abs(currentVelocity.x) / RunningMaxSpeed * 1.05f,
            };


            if (canSpecialJump && singlejump) {
                //Double jump
                singlejump = false;
                doublejump = true;
                triplejump = false;
                PlaySound(Enums.Sounds.Player_Voice_DoubleJump, (byte) GameManager.Instance.Random.RangeExclusive(1, 3));
            } else if (canSpecialJump && doublejump) {
                //Triple Jump
                singlejump = false;
                doublejump = false;
                triplejump = true;
                jumpBoost = 0.5f;
                PlaySound(Enums.Sounds.Player_Voice_TripleJump);
            } else {
                //Normal jump
                singlejump = true;
                doublejump = false;
                triplejump = false;
            }
            currentVelocity.y = vel + jumpBoost;
            IsOnGround = false;
            doGroundSnap = false;
            body.position += Vector2.up * 0.075f;
            properJump = true;
            jumping = true;

            if (!bounce) {
                //play jump sound
                Enums.Sounds sound = State switch {
                    Enums.PowerupState.MiniMushroom => Enums.Sounds.Powerup_MiniMushroom_Jump,
                    Enums.PowerupState.MegaMushroom => Enums.Sounds.Powerup_MegaMushroom_Jump,
                    _ => Enums.Sounds.Player_Sound_Jump,
                };
                PlaySound(sound);
            }
            bounce = false;
        }
    }


    public void UpdateHitbox() {
        bool crouchHitbox = State != Enums.PowerupState.MiniMushroom && pipeEntering == null && ((IsCrouching && !IsGroundpounding) || IsInShell || sliding);
        Vector2 hitbox = GetHitboxSize(crouchHitbox);

        MainHitbox.size = hitbox;
        MainHitbox.offset = Vector2.up * 0.5f * hitbox;
    }

    public Vector2 GetHitboxSize(bool crouching) {
        float height;

        if (State <= Enums.PowerupState.Small || (IsStarmanInvincible && !IsOnGround && !crouching && !sliding && !IsSpinnerFlying && !IsPropellerFlying) || IsGroundpounding) {
            height = heightSmallModel;
        } else {
            height = heightLargeModel;
        }

        if (crouching)
            height *= State <= Enums.PowerupState.Small ? 0.7f : 0.5f;

        return new(MainHitbox.size.x, height);
    }

    private void HandleWalkingRunning(bool left, bool right) {

        if (!WallJumpTimer.ExpiredOrNotRunning(Runner)) {
            if ((WallJumpTimer.RemainingTime(Runner) ?? 0f) < 0.2f && (hitLeft || hitRight)) {
                WallJumpTimer = TickTimer.None;
            } else {
                currentVelocity.x = WALLJUMP_HSPEED * (FacingRight ? 1 : -1);
                return;
            }
        }

        if (IsGroundpounding || !GroundpoundStartTimer.ExpiredOrNotRunning(Runner) || IsInKnockback || pipeEntering || jumpLandingTimer > 0 || !(WallJumpTimer.ExpiredOrNotRunning(Runner) || IsOnGround || currentVelocity.y < 0))
            return;

        if (!IsOnGround)
            skidding = false;

        if (IsInShell) {
            currentVelocity.x = SPEED_STAGE_MAX[RUN_STAGE] * 0.9f * (FacingRight ? 1 : -1) * (1f - (ShellSlowdownTimer.RemainingTime(Runner) ?? 0f));
            return;
        }

        bool run = functionallyRunning && (!IsSpinnerFlying || State == Enums.PowerupState.MegaMushroom);

        int maxStage;
        if (IsStarmanInvincible && run && IsOnGround)
            maxStage = STAR_STAGE;
        else if (run)
            maxStage = RUN_STAGE;
        else
            maxStage = WALK_STAGE;

        int stage = MovementStage;
        float acc = State == Enums.PowerupState.MegaMushroom ? SPEED_STAGE_MEGA_ACC[stage] : SPEED_STAGE_ACC[stage];
        float sign = Mathf.Sign(currentVelocity.x);

        if ((left ^ right) && (!IsCrouching || (IsCrouching && !IsOnGround && State != Enums.PowerupState.BlueShell)) && !IsInKnockback && !sliding) {
            //we can walk here

            float speed = Mathf.Abs(currentVelocity.x);
            bool reverse = currentVelocity.x != 0 && ((left ? 1 : -1) == sign);

            //check that we're not going above our limit
            float max = SPEED_STAGE_MAX[maxStage];
            if (speed > max) {
                acc = -acc;
            }

            if (reverse) {
                turnaround = false;
                if (IsOnGround) {
                    if (speed >= SKIDDING_THRESHOLD && !HeldEntity && State != Enums.PowerupState.MegaMushroom) {
                        skidding = true;
                        FacingRight = sign == 1;
                    }

                    if (skidding) {
                        if (onIce) {
                            acc = SKIDDING_ICE_DEC;
                        } else if (speed > SPEED_STAGE_MAX[RUN_STAGE]) {
                            acc = SKIDDING_STAR_DEC;
                        }  else {
                            acc = SKIDDING_DEC;
                        }
                        turnaroundFrames = 0;
                    } else {
                        if (onIce) {
                            acc = WALK_TURNAROUND_ICE_ACC;
                        } else {
                            turnaroundFrames = Mathf.Min(turnaroundFrames + 0.2f, WALK_TURNAROUND_ACC.Length - 1);
                            acc = State == Enums.PowerupState.MegaMushroom ? WALK_TURNAROUND_MEGA_ACC[(int) turnaroundFrames] : WALK_TURNAROUND_ACC[(int) turnaroundFrames];
                        }
                    }
                } else {
                    acc = SPEED_STAGE_ACC[0];
                }
            } else {

                if (skidding && !turnaround) {
                    skidding = false;
                }

                if (turnaround && turnaroundBoostFrames > 0 && speed != 0) {
                    turnaround = false;
                    skidding = false;
                }

                if (turnaround && speed < TURNAROUND_THRESHOLD) {
                    if (--turnaroundBoostFrames <= 0) {
                        acc = TURNAROUND_ACC;
                        skidding = false;
                    } else {
                        acc = 0;
                    }
                } else {
                    turnaround = false;
                }
            }

            int direction = left ? -1 : 1;
            float newX = currentVelocity.x + (acc / Runner.Simulation.Config.TickRate) * direction;

            if (Mathf.Abs(newX) - speed > 0) {
                //clamp only if accelerating
                newX = Mathf.Clamp(newX, -max, max);
            }

            if (skidding && !turnaround && Mathf.Sign(newX) != sign) {
                //turnaround
                turnaround = true;
                turnaroundBoostFrames = 5;
                newX = 0;
            }

            currentVelocity.x = newX;

        } else if (IsOnGround) {
            //not holding anything, sliding, or holding both directions. decelerate

            skidding = false;
            turnaround = false;

            if (currentVelocity.x == 0)
                return;

            if (sliding) {
                float angle = Mathf.Abs(floorAngle);
                if (angle > slopeSlidingAngle) {
                    //uphill / downhill
                    acc = (angle > 30 ? SLIDING_45_ACC : SLIDING_22_ACC) * ((Mathf.Sign(floorAngle) == sign) ? -1 : 1);
                } else {
                    //flat ground
                    acc = -SPEED_STAGE_ACC[0];
                }
            } else if (onIce)
                acc = -BUTTON_RELEASE_ICE_DEC[stage];
            else if (IsInKnockback)
                acc = -KNOCKBACK_DEC;
            else
                acc = -BUTTON_RELEASE_DEC;

            int direction = (int) Mathf.Sign(currentVelocity.x);
            float newX = currentVelocity.x + (acc / Runner.Simulation.Config.TickRate) * direction;

            if ((direction == -1) ^ (newX <= 0))
                newX = 0;

            if (sliding) {
                newX = Mathf.Clamp(newX, -SPEED_SLIDE_MAX, SPEED_SLIDE_MAX);
            }

            currentVelocity.x = newX;

            if (newX != 0)
                FacingRight = newX > 0;
        }

        IsInShell |= State == Enums.PowerupState.BlueShell && !sliding && IsOnGround && functionallyRunning && !HeldEntity && Mathf.Abs(currentVelocity.x) >= SPEED_STAGE_MAX[RUN_STAGE] * 0.9f;
        if (IsOnGround || previousOnGround)
            currentVelocity.y = 0;

    }

    bool HandleStuckInBlock() {
        if (!body || State == Enums.PowerupState.MegaMushroom)
            return false;

        Vector2 checkSize = WorldHitboxSize * new Vector2(1, 0.75f);
        Vector2 checkPos = transform.position + (Vector3) (Vector2.up * checkSize / 2f);

        if (!Utils.IsAnyTileSolidBetweenWorldBox(checkPos, checkSize * 0.9f, false)) {
            alreadyStuckInBlock = stuckInBlock = false;
            return false;
        }
        stuckInBlock = true;
        body.gravityScale = 0;
        currentVelocity = Vector2.zero;
        IsGroundpounding = false;
        IsPropellerFlying = false;
        IsDrilling = false;
        IsSpinnerFlying = false;
        IsOnGround = true;

        if (!alreadyStuckInBlock) {
            // Code for mario to instantly teleport to the closest free position when he gets stuck

            //prevent mario from clipping to the floor if we got pushed in via our hitbox changing (shell on ice, for example)
            transform.position = body.position = previousFramePosition;
            checkPos = transform.position + (Vector3) (Vector2.up * checkSize / 2f);

            float distanceInterval = 0.025f;
            float minimDistance = 0.95f; // if the minimum actual distance is anything above this value this code will have no effect
            float travelDistance = 0;
            float targetInd = -1; // Basically represents the index of the interval that'll be chosen for mario to be popped out
            int angleInterval = 45;

            for (float i = 0; i < 360 / angleInterval; i ++) { // Test for every angle in the given interval
                float ang = i * angleInterval;
                float testDistance = 0;

                float radAngle = Mathf.PI * ang / 180;
                Vector2 testPos;

                // Calculate the distance mario would have to be moved on a certain angle to stop collisioning
                do {
                    testPos = checkPos + new Vector2(Mathf.Cos(radAngle) * testDistance, Mathf.Sin(radAngle) * testDistance);
                    testDistance += distanceInterval;
                }
                while (Utils.IsAnyTileSolidBetweenWorldBox(testPos, checkSize * 0.975f));

                // This is to give right angles more priority over others when deciding
                float adjustedDistance = testDistance * (1 + Mathf.Abs(Mathf.Sin(radAngle * 2) / 2));

                // Set the new minimum only if the new position is inside of the visible level
                if (testPos.y > GameManager.Instance.cameraMinY && testPos.x > GameManager.Instance.cameraMinX && testPos.x < GameManager.Instance.cameraMaxX){
                    if (adjustedDistance < minimDistance) {
                        minimDistance = adjustedDistance;
                        travelDistance = testDistance;
                        targetInd = i;
                    }
                }
            }

            // Move him
            if (targetInd != -1) {
                float radAngle = Mathf.PI * (targetInd * angleInterval) / 180;
                Vector2 lastPos = checkPos;
                checkPos += new Vector2(Mathf.Cos(radAngle) * travelDistance, Mathf.Sin(radAngle) * travelDistance);
                transform.position = body.position = new(checkPos.x, body.position.y + (checkPos.y - lastPos.y));
                stuckInBlock = false;
                return false; // Freed
            }
        }

        alreadyStuckInBlock = true;
        currentVelocity = Vector2.right * 2f;
        return true;
    }

    void TickCounters(float delta) {
        //if (!pipeEntering)
        //    Utils.TickTimer(ref invincible, 0, delta);

        Utils.TickTimer(ref jumpBuffer, 0, delta);
        Utils.TickTimer(ref pipeTimer, 0, delta);
        Utils.TickTimer(ref jumpLandingTimer, 0, delta);
        Utils.TickTimer(ref pickupTimer, 0, -delta, pickupTime);

        if (IsOnGround)
            Utils.TickTimer(ref landing, 0, -delta);
    }

    public void FinishMegaMario(bool success) {
        if (success) {
            PlaySoundEverywhere(Enums.Sounds.Player_Voice_MegaMushroom);
        } else {
            //hit a ceiling, cancel
            giantSavedVelocity = Vector2.zero;
            State = Enums.PowerupState.Mushroom;
            GiantEndTimer = TickTimer.CreateFromSeconds(Runner, giantStartTime - GiantStartTimer.RemainingTime(Runner) ?? 0f);
            animator.enabled = true;
            animator.Play("mega-cancel", 0, 1f - (GiantEndTimer.RemainingTime(Runner) ?? 0f / giantStartTime));
            GiantStartTimer = TickTimer.None;
            stationaryGiantEnd = true;
            StoredPowerup = Enums.PowerupState.MegaMushroom;
            GiantTimer = TickTimer.None;
            PlaySound(Enums.Sounds.Player_Sound_PowerupReserveStore);
        }
        body.isKinematic = false;
    }

    private void HandleFacingDirection() {
        if (IsGroundpounding && !IsOnGround)
            return;

        //Facing direction
        bool right = currentInputs.buttons.IsSet(PlayerControls.Right);
        bool left = currentInputs.buttons.IsSet(PlayerControls.Left);

        if (!WallJumpTimer.ExpiredOrNotRunning(Runner)) {
            FacingRight = currentVelocity.x > 0;
        } else if (!IsInShell && !sliding && !skidding && !IsInKnockback && !(animator.GetCurrentAnimatorStateInfo(0).IsName("turnaround") || turnaround)) {
            if (right ^ left)
                FacingRight = right;
        } else if (GiantStartTimer.ExpiredOrNotRunning(Runner) && GiantEndTimer.ExpiredOrNotRunning(Runner) && !skidding && !(animator.GetCurrentAnimatorStateInfo(0).IsName("turnaround") || turnaround)) {
            if (IsInKnockback || (IsOnGround && State != Enums.PowerupState.MegaMushroom && Mathf.Abs(currentVelocity.x) > 0.05f)) {
                FacingRight = currentVelocity.x > 0;
            } else if ((!IsInShell || !GiantStartTimer.ExpiredOrNotRunning(Runner)) && (right || left)) {
                FacingRight = right;
            }
            if (!IsInShell && ((Mathf.Abs(currentVelocity.x) < 0.5f && IsCrouching) || onIce) && (right || left))
                FacingRight = right;
        }
    }

    public void EndMega() {
        if (State != Enums.PowerupState.MegaMushroom)
            return;

        State = Enums.PowerupState.Mushroom;
        GiantEndTimer = TickTimer.CreateFromSeconds(Runner, giantStartTime / 2f);
        stationaryGiantEnd = false;
        DamageInvincibilityTimer = TickTimer.CreateFromSeconds(Runner, 3f);
        PlaySoundEverywhere(Enums.Sounds.Powerup_MegaMushroom_End);

        if (currentVelocity.y > 0)
            currentVelocity.y *= 0.33f;
    }

    public void HandleBlockSnapping() {
        if (pipeEntering || IsDrilling)
            return;

        //if we're about to be in the top 2 pixels of a block, snap up to it, (if we can fit)

        if (currentVelocity.y > 0)
            return;

        Vector2 nextPos = body.position + Time.fixedDeltaTime * 2f * currentVelocity;

        if (!Utils.IsAnyTileSolidBetweenWorldBox(nextPos + WorldHitboxSize.y * 0.5f * Vector2.up, WorldHitboxSize))
            //we are not going to be inside a block next fixed update
            return;

        //we ARE inside a block. figure out the height of the contact
        // 32 pixels per unit
        bool orig = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = true;
        RaycastHit2D contact = Physics2D.BoxCast(nextPos + 3f / 32f * Vector2.up, new(WorldHitboxSize.y, 1f / 32f), 0, Vector2.down, 3f / 32f, Layers.MaskAnyGround);
        Physics2D.queriesStartInColliders = orig;

        if (!contact || contact.normal.y < 0.1f) {
            //we didn't hit the ground, we must've hit a ceiling or something.
            return;
        }

        float point = contact.point.y + Physics2D.defaultContactOffset;
        if (body.position.y > point + Physics2D.defaultContactOffset) {
            //dont snap when we're above the block
            return;
        }

        Vector2 newPosition = new(body.position.x, point);

        if (Utils.IsAnyTileSolidBetweenWorldBox(newPosition + WorldHitboxSize.y * 0.5f * Vector2.up, WorldHitboxSize)) {
            //it's an invalid position anyway, we'd be inside something.
            return;
        }

        //valid position, snap upwards
        body.position = newPosition;
    }

    private void HandleMovement(float delta) {
        functionallyRunning = currentInputs.buttons.IsSet(PlayerControls.Sprint) || State == Enums.PowerupState.MegaMushroom || IsPropellerFlying;

        if (IsDead)
            return;

        if (body.position.y + transform.lossyScale.y < GameManager.Instance.GetLevelMinY()) {
            //death via pit
            Death(true, false);
            return;
        }

        if (IsFrozen) {
            if (!frozenObject) {
                Unfreeze(UnfreezeReason.Other);
            } else {
                currentVelocity = Vector2.zero;
                return;
            }
        }

        if (HeldEntity && (HeldEntity.Dead || IsFrozen || HeldEntity.IsFrozen))
            SetHolding(null);

        //FrozenCube holdingCube;
        //if (((holdingCube = HeldEntity as FrozenCube) && holdingCube) || ((holdingCube = holdingOld as FrozenCube) && holdingCube)) {
        //    foreach (BoxCollider2D hitbox in hitboxes) {
        //        Physics2D.IgnoreCollision(hitbox, holdingCube.hitbox, throwInvincibility > 0);
        //    }
        //}

        if (GiantStartTimer.IsRunning) {
            currentVelocity = Vector2.zero;
            transform.position = body.position = previousFramePosition;
            if (GiantStartTimer.Expired(Runner)) {
                FinishMegaMario(true);
                GiantStartTimer = TickTimer.None;
            } else {
                body.isKinematic = true;
                if (animator.GetCurrentAnimatorClipInfo(0).Length <= 0 || animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "mega-scale")
                    animator.Play("mega-scale");


                Vector2 checkSize = WorldHitboxSize * new Vector2(0.75f, 1.1f);
                Vector2 normalizedVelocity = currentVelocity;
                if (!IsGroundpounding)
                    normalizedVelocity.y = Mathf.Max(0, currentVelocity.y);

                Vector2 offset = Vector2.zero;
                if (singlejump && IsOnGround)
                    offset = Vector2.down / 2f;

                Vector2 checkPosition = body.position + Vector2.up * checkSize / 2f + offset;

                Vector3Int minPos = Utils.WorldToTilemapPosition(checkPosition - (checkSize / 2), wrap: false);
                Vector3Int size = Utils.WorldToTilemapPosition(checkPosition + (checkSize / 2), wrap: false) - minPos;

                for (int x = 0; x <= size.x; x++) {
                    Vector3Int tileLocation = new(minPos.x + x, minPos.y + size.y, 0);
                    Utils.WrapTileLocation(ref tileLocation);
                    TileBase tile = Utils.GetTileAtTileLocation(tileLocation);

                    bool cancelMega;
                    if (tile is BreakableBrickTile bbt)
                        cancelMega = !bbt.breakableByGiantMario;
                    else
                        cancelMega = Utils.IsTileSolidAtTileLocation(tileLocation);

                    if (cancelMega) {
                        FinishMegaMario(false);
                        return;
                    }
                }
            }
            return;
        }
        if (GiantEndTimer.IsRunning && stationaryGiantEnd) {
            currentVelocity = Vector2.zero;
            body.isKinematic = true;
            transform.position = body.position = previousFramePosition;

            if (GiantEndTimer.Expired(Runner)) {
                DamageInvincibilityTimer = TickTimer.CreateFromSeconds(Runner, 2f);
                currentVelocity = giantSavedVelocity;
                animator.enabled = true;
                body.isKinematic = false;
                State = previousState;
                GiantEndTimer = TickTimer.None;
            }
            return;
        }

        if (State == Enums.PowerupState.MegaMushroom) {
            HandleGiantTiles(true);
            if (IsOnGround && singlejump) {
                SpawnParticle("Prefabs/Particle/GroundpoundDust", body.position);
                CameraController.ScreenShake = 0.15f;
                singlejump = false;
            }
            StarmanTimer = TickTimer.None;
        }

        //pipes > stuck in block, else the animation gets janked.
        if (pipeEntering || !GiantStartTimer.ExpiredOrNotRunning(Runner) || (!GiantEndTimer.ExpiredOrNotRunning(Runner) && stationaryGiantEnd) || animator.GetBool("pipe"))
            return;
        if (HandleStuckInBlock())
            return;

        bool right = currentInputs.buttons.IsSet(PlayerControls.Right);
        bool left = currentInputs.buttons.IsSet(PlayerControls.Left);
        bool down = currentInputs.buttons.IsSet(PlayerControls.Down);
        bool up = currentInputs.buttons.IsSet(PlayerControls.Up);
        bool jump = currentInputs.buttons.IsSet(PlayerControls.Jump);

        if (currentInputs.buttons.WasPressed(previousInputs.buttons, PlayerControls.Jump))
            jumpBuffer = 0.15f;

        bool doJump = (jumpBuffer > 0) && (IsOnGround || koyoteTime < 0.07f || wallSlideLeft || wallSlideRight);

        //Pipes
        if (pipeTimer <= 0) {
            DownwardsPipeCheck(down);
            UpwardsPipeCheck(up);
        }

        if (IsInKnockback) {
            if (bounce)
                ResetKnockback();

            wallSlideLeft = false;
            wallSlideRight = false;
            IsCrouching = false;
            IsInShell = false;
            currentVelocity -= currentVelocity * (delta * 2f);
            if (IsOnGround && Mathf.Abs(currentVelocity.x) < 0.35f && KnockbackTimer.Expired(Runner))
                ResetKnockback();

            AttemptThrowHeldItem();
        }

        //activate blocks jumped into
        if (hitRoof) {
            currentVelocity.y = Mathf.Min(currentVelocity.y, -0.1f);
            bool tempHitBlock = false;
            foreach (Vector3Int tile in tilesJumpedInto) {
                int temp = InteractWithTile(tile, InteractableTile.InteractionDirection.Up);
                if (temp != -1)
                    tempHitBlock |= temp == 1;
            }
            if (tempHitBlock && State == Enums.PowerupState.MegaMushroom) {
                CameraController.ScreenShake = 0.15f;
                PlaySound(Enums.Sounds.World_Block_Bump);
            }
        }

        alreadyGroundpounded &= down;

        if (IsDrilling) {
            PropellerSpinTimer = TickTimer.None;
            if (IsPropellerFlying) {
                if (!down) {
                    Utils.TickTimer(ref propellerDrillBuffer, 0, Time.deltaTime);
                    if (propellerDrillBuffer <= 0)
                        IsDrilling = false;
                } else {
                    propellerDrillBuffer = 0.15f;
                }
            }
        }

        if (PropellerLaunchTimer.IsRunning) {
            if (PropellerLaunchTimer.Expired(Runner)) {
                PropellerLaunchTimer = TickTimer.None;
            } else {
                float remainingTime = PropellerLaunchTimer.RemainingTime(Runner) ?? 0f;
                currentVelocity.y = propellerLaunchVelocity - (remainingTime < 0.4f ? (1 - (remainingTime / .4f)) * propellerLaunchVelocity : 0);
            }
        }

        if (currentInputs.buttons.IsSet(PlayerControls.PowerupAction) && !WallJumpTimer.ExpiredOrNotRunning(Runner) && (IsPropellerFlying || !usedPropellerThisJump)) {
            if (currentVelocity.y < -0.1f && IsPropellerFlying && !IsDrilling && !wallSlideLeft && !wallSlideRight && (PropellerSpinTimer.RemainingTime(Runner) ?? 0f) < propellerSpinTime / 4f) {
                PropellerSpinTimer = TickTimer.CreateFromSeconds(Runner, propellerSpinTime);
                PlaySound(Enums.Sounds.Powerup_PropellerMushroom_Spin);
            }
        }

        if (HeldEntity) {
            wallSlideLeft = false;
            wallSlideRight = false;
            SetHoldingOffset();
        }

        //throwing held item
        ThrowHeldItem(left, right, down);

        //blue shell enter/exit
        if (State != Enums.PowerupState.BlueShell || !functionallyRunning)
            IsInShell = false;

        if (IsInShell) {
            down = true;
            if (hitLeft || hitRight) {
                foreach (var tile in tilesHitSide)
                    InteractWithTile(tile, InteractableTile.InteractionDirection.Up);
                FacingRight = hitLeft;
                PlaySound(Enums.Sounds.World_Block_Bump);
            }
        }

        //Ground
        if (IsOnGround) {
            if (hitRoof && crushGround && currentVelocity.y <= 0.1 && State != Enums.PowerupState.MegaMushroom) {
                //Crushed.
                Powerdown(true);
            }

            koyoteTime = 0;
            usedPropellerThisJump = false;
            wallSlideLeft = false;
            wallSlideRight = false;
            jumping = false;
            if (IsDrilling)
                SpawnParticle("Prefabs/Particle/GroundpoundDust", body.position);

            if (onSpinner && Mathf.Abs(currentVelocity.x) < 0.3f && !HeldEntity) {
                Transform spnr = onSpinner.transform;
                float diff = body.position.x - spnr.transform.position.x;
                if (Mathf.Abs(diff) >= 0.02f)
                    body.position += -0.6f * Mathf.Sign(diff) * Time.fixedDeltaTime * Vector2.right;
            }
        } else {
            koyoteTime += delta;
            landing = 0;
            skidding = false;
            turnaround = false;
            if (!jumping)
                properJump = false;
        }

        //Crouching
        HandleCrouching(down);

        HandleWallslide(left, right, doJump);

        HandleSlopes();

        if (down && !alreadyGroundpounded) {
            HandleGroundpoundStart(left, right);
        } else {
            groundpoundStartTimer = 0;
        }
        HandleGroundpound();

        HandleSliding(up, down, left, right);

        if (IsOnGround) {
            if (IsPropellerFlying) {
                float remainingTime = PropellerLaunchTimer.RemainingTime(Runner) ?? 0f;
                if (remainingTime < 0.5f) {
                    IsPropellerFlying = false;
                    PropellerLaunchTimer = TickTimer.None;
                }
            }
            IsSpinnerFlying = false;
            IsDrilling = false;
            if (landing <= Time.fixedDeltaTime + 0.01f && !IsGroundpounding && !IsCrouching && !IsInShell && !HeldEntity && State != Enums.PowerupState.MegaMushroom) {
                bool edge = !Physics2D.BoxCast(body.position, MainHitbox.size * 0.75f, 0, Vector2.down, 0, Layers.MaskAnyGround);
                bool edgeLanding = false;
                if (edge) {
                    bool rightEdge = edge && Utils.IsTileSolidAtWorldLocation(body.position + new Vector2(0.25f, -0.25f));
                    bool leftEdge = edge && Utils.IsTileSolidAtWorldLocation(body.position + new Vector2(-0.25f, -0.25f));
                    edgeLanding = (leftEdge || rightEdge) && properJump && edge && (FacingRight == rightEdge);
                }

                if ((triplejump && !(left ^ right))
                    || edgeLanding
                    || (Mathf.Abs(currentVelocity.x) < 0.1f)) {

                    if (!onIce)
                        currentVelocity = Vector2.zero;

                    animator.Play("jumplanding" + (edgeLanding ? "-edge" : ""));
                    if (edgeLanding)
                        jumpLandingTimer = 0.15f;
                }
            }
            if (landing > 0.2f) {
                singlejump = false;
                doublejump = false;
                triplejump = false;
            }
        }


        if (!(IsGroundpounding && !IsOnGround)) {
            //Normal walking/running
            HandleWalkingRunning(left, right);

            //Jumping
            HandleJumping(jump, doJump);
        }

        if (GiantTimer.Expired(Runner)) {
            EndMega();
            GiantTimer = TickTimer.None;
        }

        HandleSlopes();
        HandleFacingDirection();

        //slow-rise check
        if (IsSpinnerFlying || IsPropellerFlying) {
            body.gravityScale = flyingGravity;
        } else {
            float gravityModifier = State switch {
                Enums.PowerupState.MiniMushroom => 0.4f,
                _ => 1,
            };
            float slowriseModifier = State switch {
                Enums.PowerupState.MegaMushroom => 3f,
                _ => 1f,
            };
            if (IsGroundpounding)
                gravityModifier *= 1.5f;

            if (currentVelocity.y > 2.5) {
                if (jump || State == Enums.PowerupState.MegaMushroom) {
                    body.gravityScale = slowriseGravity * slowriseModifier;
                } else {
                    body.gravityScale = normalGravity * 1.5f * gravityModifier;
                }
            } else if (IsOnGround || (IsGroundpounding && !GroundpoundStartTimer.ExpiredOrNotRunning(Runner))) {
                body.gravityScale = 0f;
            } else {
                body.gravityScale = normalGravity * (gravityModifier / 1.2f);
            }
        }

        //Terminal velocity
        float terminalVelocityModifier = State switch {
            Enums.PowerupState.MiniMushroom => 0.625f,
            Enums.PowerupState.MegaMushroom => 2f,
            _ => 1f,
        };
        if (IsSpinnerFlying) {
            if (IsDrilling) {
                currentVelocity.y = -drillVelocity;
            } else {
                currentVelocity.y = Mathf.Max(currentVelocity.y, -flyingTerminalVelocity);
            }
        } else if (IsPropellerFlying) {
            if (IsDrilling) {
                currentVelocity = new(Mathf.Clamp(currentVelocity.x, -WalkingMaxSpeed, WalkingMaxSpeed), -drillVelocity);
            } else {
                float remainingTime = PropellerLaunchTimer.RemainingTime(Runner) ?? 0f;
                float htv = WalkingMaxSpeed * 1.18f + (remainingTime * 2f);
                currentVelocity = new(Mathf.Clamp(currentVelocity.x, -htv, htv), Mathf.Max(currentVelocity.y, !PropellerSpinTimer.ExpiredOrNotRunning(Runner) ? -propellerSpinFallSpeed : -propellerFallSpeed));
            }
        } else if (wallSlideLeft || wallSlideRight) {
            currentVelocity.y = Mathf.Max(currentVelocity.y, wallslideSpeed);
        } else if (IsGroundpounding) {
            currentVelocity = new(currentVelocity.x, Mathf.Max(currentVelocity.y, -groundpoundVelocity));
        } else {
            currentVelocity.y = Mathf.Max(currentVelocity.y, terminalVelocity * terminalVelocityModifier);
        }

        if (IsCrouching || sliding || skidding) {
            wallSlideLeft = false;
            wallSlideRight = false;
        }

        if (previousOnGround && !IsOnGround && !properJump && IsCrouching && !IsInShell && !IsGroundpounding)
            currentVelocity.y = -3.75f;
    }

    public void SetHoldingOffset() {
        if (HeldEntity is FrozenCube) {
            HeldEntity.holderOffset = new(HeldEntity.hitbox.size.x * 0.25f, MainHitbox.size.y * (1f - Utils.QuadraticEaseOut(1f - (pickupTimer / pickupTime))), -2);
        } else {
            HeldEntity.holderOffset = new((FacingRight ? 1 : -1) * 0.25f, State >= Enums.PowerupState.Mushroom ? 0.5f : 0.25f, !FacingRight ? -0.09f : 0f);
        }
    }

    private void ThrowHeldItem(bool left, bool right, bool crouch) {
        if (!((!functionallyRunning || State == Enums.PowerupState.MiniMushroom || State == Enums.PowerupState.MegaMushroom || IsStarmanInvincible || IsSpinnerFlying || IsPropellerFlying) && HeldEntity))
            return;

        bool throwRight = FacingRight;
        if (left ^ right)
            throwRight = right;

        crouch &= HeldEntity.canPlace;

        //TODO:
        //holdingOld = HeldEntity;
        //throwInvincibility = 0.15f;

        AttemptThrowHeldItem(throwRight, crouch);

        if (!crouch && !IsInKnockback) {
            PlaySound(Enums.Sounds.Player_Voice_WallJump, 2);
            //throwInvincibility = 0.5f;
            animator.SetTrigger("throw");
        }
    }

    private void HandleGroundpoundStart(bool left, bool right) {

        if (groundpoundStartTimer == 0)
            groundpoundStartTimer = 0.065f;

        Utils.TickTimer(ref groundpoundStartTimer, 0, Time.fixedDeltaTime);

        if (groundpoundStartTimer != 0)
            return;

        if (IsOnGround || IsInKnockback || IsGroundpounding || IsDrilling
            || HeldEntity || IsCrouching || sliding || IsInShell
            || wallSlideLeft || wallSlideRight)
            return;

        if (!IsPropellerFlying && !IsSpinnerFlying && (left || right))
            return;

        if (IsSpinnerFlying) {
            //start drill
            if (currentVelocity.y < 0) {
                IsDrilling = true;
                hitBlock = true;
                currentVelocity.x = 0;
            }
        } else if (IsPropellerFlying) {
            //start propeller drill
            float remainingTime = PropellerLaunchTimer.RemainingTime(Runner) ?? 0f;
            if (remainingTime < 0.6f && currentVelocity.y < 4) {
                IsDrilling = true;
                PropellerLaunchTimer = TickTimer.None;
                hitBlock = true;
            }
        } else {
            //start groundpound
            //check if high enough above ground
            if (Runner.GetPhysicsScene().BoxCast(body.position, WorldHitboxSize * Vector2.right * 0.5f, Vector3.down, out _, Quaternion.identity, 0.15f * (State == Enums.PowerupState.MegaMushroom ? 2.5f : 1), Layers.MaskAnyGround))
                return;

            wallSlideLeft = false;
            wallSlideRight = false;
            IsGroundpounding = true;
            singlejump = false;
            doublejump = false;
            triplejump = false;
            hitBlock = true;
            sliding = false;
            currentVelocity = Vector2.up * 1.5f;
            GroundpoundStartTimer = TickTimer.CreateFromSeconds(Runner, groundpoundTime * (State == Enums.PowerupState.MegaMushroom ? 1.5f : 1));
            PlaySound(Enums.Sounds.Player_Sound_GroundpoundStart);
            alreadyGroundpounded = true;
            //groundpoundDelay = 0.75f;
        }
    }

    void HandleGroundpound() {
        if (IsGroundpounding && GroundpoundStartTimer.IsRunning && GroundpoundStartTimer.RemainingTime(Runner) <= .1f)
            currentVelocity = Vector2.zero;

        if (IsGroundpounding && GroundpoundStartTimer.Expired(Runner)) {
            currentVelocity = Vector2.down * groundpoundVelocity;
            GroundpoundStartTimer = TickTimer.None;
        }

        if (!(IsOnGround && (IsGroundpounding || IsDrilling) && hitBlock))
            return;

        bool tempHitBlock = false, hitAnyBlock = false;
        foreach (Vector3Int tile in tilesStandingOn) {
            int temp = InteractWithTile(tile, InteractableTile.InteractionDirection.Down);
            if (temp != -1) {
                hitAnyBlock = true;
                tempHitBlock |= temp == 1;
            }
        }
        hitBlock = tempHitBlock;
        if (IsDrilling) {
            IsSpinnerFlying &= hitBlock;
            IsPropellerFlying &= hitBlock;
            IsDrilling = hitBlock;
            if (hitBlock)
                IsOnGround = false;
        } else {
            //groundpound
            if (hitAnyBlock) {
                if (State != Enums.PowerupState.MegaMushroom) {
                    Enums.Sounds sound = State switch {
                        Enums.PowerupState.MiniMushroom => Enums.Sounds.Powerup_MiniMushroom_Groundpound,
                        _ => Enums.Sounds.Player_Sound_GroundpoundLanding,
                    };
                    PlaySound(sound);
                    SpawnParticle(PrefabList.Instance.Particle_Groundpound, body.position);
                    GroundpoundStartTimer = TickTimer.CreateFromSeconds(Runner, 0.2f);
                } else {
                    CameraController.ScreenShake = 0.15f;
                }
            }
            if (hitBlock) {
                koyoteTime = 1.5f;
            } else if (State == Enums.PowerupState.MegaMushroom) {
                PlaySound(Enums.Sounds.Powerup_MegaMushroom_Groundpound);
                SpawnParticle(PrefabList.Instance.Particle_Groundpound, body.position);
                CameraController.ScreenShake = 0.35f;
            }
        }
    }

    public bool CanPickup() {
        return State != Enums.PowerupState.MiniMushroom && !skidding && !turnaround && !HeldEntity && currentInputs.buttons.IsSet(PlayerControls.Sprint) && !IsPropellerFlying && !IsSpinnerFlying && !IsCrouching && !IsDead && !wallSlideLeft && !wallSlideRight && !doublejump && !triplejump && !IsGroundpounding;
    }

    public void OnDrawGizmos() {
        if (!body)
            return;

        Gizmos.DrawRay(body.position, body.velocity);
        Gizmos.DrawCube(body.position + new Vector2(0, WorldHitboxSize.y * 0.5f) + (body.velocity * Time.fixedDeltaTime), WorldHitboxSize);

        Gizmos.color = Color.white;
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
            if (r is ParticleSystemRenderer)
                continue;

            Gizmos.DrawWireCube(r.bounds.center, r.bounds.size);
        }
    }
}
