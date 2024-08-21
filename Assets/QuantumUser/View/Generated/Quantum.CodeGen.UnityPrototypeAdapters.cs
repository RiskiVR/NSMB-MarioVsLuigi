// <auto-generated>
// This code was auto-generated by a tool, every time
// the tool executes this code will be reset.
//
// If you need to extend the classes generated to add
// fields or methods to them, please create partial
// declarations in another file.
// </auto-generated>
#pragma warning disable 0109
#pragma warning disable 1591


namespace Quantum.Prototypes.Unity {
  using Photon.Deterministic;
  using Quantum;
  using Quantum.Core;
  using Quantum.Collections;
  using Quantum.Inspector;
  using Quantum.Physics2D;
  using Quantum.Physics3D;
  using Byte = System.Byte;
  using SByte = System.SByte;
  using Int16 = System.Int16;
  using UInt16 = System.UInt16;
  using Int32 = System.Int32;
  using UInt32 = System.UInt32;
  using Int64 = System.Int64;
  using UInt64 = System.UInt64;
  using Boolean = System.Boolean;
  using String = System.String;
  using Object = System.Object;
  using FlagsAttribute = System.FlagsAttribute;
  using SerializableAttribute = System.SerializableAttribute;
  using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
  using MethodImplOptions = System.Runtime.CompilerServices.MethodImplOptions;
  using FieldOffsetAttribute = System.Runtime.InteropServices.FieldOffsetAttribute;
  using StructLayoutAttribute = System.Runtime.InteropServices.StructLayoutAttribute;
  using LayoutKind = System.Runtime.InteropServices.LayoutKind;
  #if QUANTUM_UNITY //;
  using TooltipAttribute = UnityEngine.TooltipAttribute;
  using HeaderAttribute = UnityEngine.HeaderAttribute;
  using SpaceAttribute = UnityEngine.SpaceAttribute;
  using RangeAttribute = UnityEngine.RangeAttribute;
  using HideInInspectorAttribute = UnityEngine.HideInInspector;
  using PreserveAttribute = UnityEngine.Scripting.PreserveAttribute;
  using FormerlySerializedAsAttribute = UnityEngine.Serialization.FormerlySerializedAsAttribute;
  using MovedFromAttribute = UnityEngine.Scripting.APIUpdating.MovedFromAttribute;
  using CreateAssetMenu = UnityEngine.CreateAssetMenuAttribute;
  using RuntimeInitializeOnLoadMethodAttribute = UnityEngine.RuntimeInitializeOnLoadMethodAttribute;
  #endif //;
  
  [System.SerializableAttribute()]
  public unsafe partial class BlockBumpPrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.BlockBumpPrototype> {
    public Byte Lifetime;
    public AssetRef<StageTile> StartTile;
    public Quantum.Prototypes.StageTileInstancePrototype ResultTile;
    public QBoolean IsDownwards;
    public AssetRef<EntityPrototype> Powerup;
    public FPVector2 Origin;
    public Int32 TileX;
    public Int32 TileY;
    public Quantum.QuantumEntityPrototype Owner;
    public QBoolean HasBumped;
    partial void ConvertUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.BlockBumpPrototype prototype);
    public override Quantum.Prototypes.BlockBumpPrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.BlockBumpPrototype();
      converter.Convert(this.Lifetime, out result.Lifetime);
      converter.Convert(this.StartTile, out result.StartTile);
      converter.Convert(this.ResultTile, out result.ResultTile);
      converter.Convert(this.IsDownwards, out result.IsDownwards);
      converter.Convert(this.Powerup, out result.Powerup);
      converter.Convert(this.Origin, out result.Origin);
      converter.Convert(this.TileX, out result.TileX);
      converter.Convert(this.TileY, out result.TileY);
      converter.Convert(this.Owner, out result.Owner);
      converter.Convert(this.HasBumped, out result.HasBumped);
      ConvertUser(converter, ref result);
      return result;
    }
  }
  [System.SerializableAttribute()]
  public unsafe partial class BooPrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.BooPrototype> {
    public Quantum.QuantumEntityPrototype CurrentTarget;
    public Byte UnscaredFrames;
    partial void ConvertUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.BooPrototype prototype);
    public override Quantum.Prototypes.BooPrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.BooPrototype();
      converter.Convert(this.CurrentTarget, out result.CurrentTarget);
      converter.Convert(this.UnscaredFrames, out result.UnscaredFrames);
      ConvertUser(converter, ref result);
      return result;
    }
  }
  [System.SerializableAttribute()]
  public unsafe partial class BulletBillPrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.BulletBillPrototype> {
    public FP Speed;
    public FP DespawnRadius;
    public Byte DespawnFrames;
    public Quantum.QuantumEntityPrototype Owner;
    partial void ConvertUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.BulletBillPrototype prototype);
    public override Quantum.Prototypes.BulletBillPrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.BulletBillPrototype();
      converter.Convert(this.Speed, out result.Speed);
      converter.Convert(this.DespawnRadius, out result.DespawnRadius);
      converter.Convert(this.DespawnFrames, out result.DespawnFrames);
      converter.Convert(this.Owner, out result.Owner);
      ConvertUser(converter, ref result);
      return result;
    }
  }
  [System.SerializableAttribute()]
  public unsafe partial class HoldablePrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.HoldablePrototype> {
    public Quantum.QuantumEntityPrototype Holder;
    public Quantum.QuantumEntityPrototype PreviousHolder;
    public Byte IgnoreOwnerFrames;
    partial void ConvertUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.HoldablePrototype prototype);
    public override Quantum.Prototypes.HoldablePrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.HoldablePrototype();
      converter.Convert(this.Holder, out result.Holder);
      converter.Convert(this.PreviousHolder, out result.PreviousHolder);
      converter.Convert(this.IgnoreOwnerFrames, out result.IgnoreOwnerFrames);
      ConvertUser(converter, ref result);
      return result;
    }
  }
  [System.SerializableAttribute()]
  public unsafe partial class LiquidPrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.LiquidPrototype> {
    public LiquidType LiquidType;
    public Int32 WidthTiles;
    public FP HeightTiles;
    [FreeOnComponentRemoved()]
    [DynamicCollectionAttribute()]
    public Quantum.QuantumEntityPrototype[] SplashedEntities = {};
    partial void ConvertUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.LiquidPrototype prototype);
    public override Quantum.Prototypes.LiquidPrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.LiquidPrototype();
      converter.Convert(this.LiquidType, out result.LiquidType);
      converter.Convert(this.WidthTiles, out result.WidthTiles);
      converter.Convert(this.HeightTiles, out result.HeightTiles);
      converter.Convert(this.SplashedEntities, out result.SplashedEntities);
      ConvertUser(converter, ref result);
      return result;
    }
  }
  [System.SerializableAttribute()]
  public unsafe partial class MarioPlayerPrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.MarioPlayerPrototype> {
    public AssetRef<MarioPlayerPhysicsInfo> PhysicsAsset;
    public AssetRef<CharacterAsset> CharacterAsset;
    public PlayerRef PlayerRef;
    public Byte SpawnpointIndex;
    public Byte Team;
    public PowerupState CurrentPowerupState;
    public PowerupState PreviousPowerupState;
    public AssetRef<PowerupAsset> ReserveItem;
    public Byte Stars;
    public Byte Coins;
    public Byte Lives;
    public QBoolean Disconnected;
    public QBoolean IsDead;
    public QBoolean FireDeath;
    public QBoolean IsRespawning;
    public Byte DeathAnimationFrames;
    public Byte PreRespawnFrames;
    public Byte RespawnFrames;
    public Byte NoLivesStarDirection;
    public QBoolean FacingRight;
    public QBoolean IsSkidding;
    public QBoolean IsTurnaround;
    public Byte FastTurnaroundFrames;
    public Byte SlowTurnaroundFrames;
    public JumpState JumpState;
    public JumpState PreviousJumpState;
    public Byte JumpLandingFrames;
    public Byte JumpBufferFrames;
    public Byte CoyoteTimeFrames;
    public Int32 LandedFrame;
    public QBoolean WasTouchingGroundLastFrame;
    public QBoolean DoEntityBounce;
    public QBoolean WallslideLeft;
    public QBoolean WallslideRight;
    public Byte WallslideEndFrames;
    public Byte WalljumpFrames;
    public QBoolean IsGroundpounding;
    public QBoolean IsGroundpoundActive;
    public Byte GroundpoundStartFrames;
    public Byte GroundpoundCooldownFrames;
    public Byte GroundpoundStandFrames;
    public Byte WaterColliderCount;
    public QBoolean SwimExitForceJump;
    public QBoolean IsInKnockback;
    public QBoolean IsInWeakKnockback;
    public QBoolean KnockbackWasOriginallyFacingRight;
    public Byte DamageInvincibilityFrames;
    public QBoolean IsCrouching;
    public QBoolean IsSliding;
    public QBoolean IsSpinnerFlying;
    public QBoolean IsDrilling;
    public Byte Combo;
    public UInt16 InvincibilityFrames;
    public Byte MegaMushroomStartFrames;
    public UInt16 MegaMushroomFrames;
    public Byte MegaMushroomEndFrames;
    public QBoolean MegaMushroomStationaryEnd;
    public Byte ProjectileDelayFrames;
    public Byte ProjectileVolleyFrames;
    public Byte CurrentProjectiles;
    public Byte CurrentVolley;
    public QBoolean IsInShell;
    public Byte ShellSlowdownFrames;
    public QBoolean IsPropellerFlying;
    public Byte PropellerLaunchFrames;
    public Byte PropellerSpinFrames;
    public QBoolean UsedPropellerThisJump;
    public Byte PropellerDrillCooldown;
    public Quantum.QuantumEntityPrototype HeldEntity;
    public Quantum.QuantumEntityPrototype CurrentPipe;
    partial void ConvertUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.MarioPlayerPrototype prototype);
    public override Quantum.Prototypes.MarioPlayerPrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.MarioPlayerPrototype();
      converter.Convert(this.PhysicsAsset, out result.PhysicsAsset);
      converter.Convert(this.CharacterAsset, out result.CharacterAsset);
      converter.Convert(this.PlayerRef, out result.PlayerRef);
      converter.Convert(this.SpawnpointIndex, out result.SpawnpointIndex);
      converter.Convert(this.Team, out result.Team);
      converter.Convert(this.CurrentPowerupState, out result.CurrentPowerupState);
      converter.Convert(this.PreviousPowerupState, out result.PreviousPowerupState);
      converter.Convert(this.ReserveItem, out result.ReserveItem);
      converter.Convert(this.Stars, out result.Stars);
      converter.Convert(this.Coins, out result.Coins);
      converter.Convert(this.Lives, out result.Lives);
      converter.Convert(this.Disconnected, out result.Disconnected);
      converter.Convert(this.IsDead, out result.IsDead);
      converter.Convert(this.FireDeath, out result.FireDeath);
      converter.Convert(this.IsRespawning, out result.IsRespawning);
      converter.Convert(this.DeathAnimationFrames, out result.DeathAnimationFrames);
      converter.Convert(this.PreRespawnFrames, out result.PreRespawnFrames);
      converter.Convert(this.RespawnFrames, out result.RespawnFrames);
      converter.Convert(this.NoLivesStarDirection, out result.NoLivesStarDirection);
      converter.Convert(this.FacingRight, out result.FacingRight);
      converter.Convert(this.IsSkidding, out result.IsSkidding);
      converter.Convert(this.IsTurnaround, out result.IsTurnaround);
      converter.Convert(this.FastTurnaroundFrames, out result.FastTurnaroundFrames);
      converter.Convert(this.SlowTurnaroundFrames, out result.SlowTurnaroundFrames);
      converter.Convert(this.JumpState, out result.JumpState);
      converter.Convert(this.PreviousJumpState, out result.PreviousJumpState);
      converter.Convert(this.JumpLandingFrames, out result.JumpLandingFrames);
      converter.Convert(this.JumpBufferFrames, out result.JumpBufferFrames);
      converter.Convert(this.CoyoteTimeFrames, out result.CoyoteTimeFrames);
      converter.Convert(this.LandedFrame, out result.LandedFrame);
      converter.Convert(this.WasTouchingGroundLastFrame, out result.WasTouchingGroundLastFrame);
      converter.Convert(this.DoEntityBounce, out result.DoEntityBounce);
      converter.Convert(this.WallslideLeft, out result.WallslideLeft);
      converter.Convert(this.WallslideRight, out result.WallslideRight);
      converter.Convert(this.WallslideEndFrames, out result.WallslideEndFrames);
      converter.Convert(this.WalljumpFrames, out result.WalljumpFrames);
      converter.Convert(this.IsGroundpounding, out result.IsGroundpounding);
      converter.Convert(this.IsGroundpoundActive, out result.IsGroundpoundActive);
      converter.Convert(this.GroundpoundStartFrames, out result.GroundpoundStartFrames);
      converter.Convert(this.GroundpoundCooldownFrames, out result.GroundpoundCooldownFrames);
      converter.Convert(this.GroundpoundStandFrames, out result.GroundpoundStandFrames);
      converter.Convert(this.WaterColliderCount, out result.WaterColliderCount);
      converter.Convert(this.SwimExitForceJump, out result.SwimExitForceJump);
      converter.Convert(this.IsInKnockback, out result.IsInKnockback);
      converter.Convert(this.IsInWeakKnockback, out result.IsInWeakKnockback);
      converter.Convert(this.KnockbackWasOriginallyFacingRight, out result.KnockbackWasOriginallyFacingRight);
      converter.Convert(this.DamageInvincibilityFrames, out result.DamageInvincibilityFrames);
      converter.Convert(this.IsCrouching, out result.IsCrouching);
      converter.Convert(this.IsSliding, out result.IsSliding);
      converter.Convert(this.IsSpinnerFlying, out result.IsSpinnerFlying);
      converter.Convert(this.IsDrilling, out result.IsDrilling);
      converter.Convert(this.Combo, out result.Combo);
      converter.Convert(this.InvincibilityFrames, out result.InvincibilityFrames);
      converter.Convert(this.MegaMushroomStartFrames, out result.MegaMushroomStartFrames);
      converter.Convert(this.MegaMushroomFrames, out result.MegaMushroomFrames);
      converter.Convert(this.MegaMushroomEndFrames, out result.MegaMushroomEndFrames);
      converter.Convert(this.MegaMushroomStationaryEnd, out result.MegaMushroomStationaryEnd);
      converter.Convert(this.ProjectileDelayFrames, out result.ProjectileDelayFrames);
      converter.Convert(this.ProjectileVolleyFrames, out result.ProjectileVolleyFrames);
      converter.Convert(this.CurrentProjectiles, out result.CurrentProjectiles);
      converter.Convert(this.CurrentVolley, out result.CurrentVolley);
      converter.Convert(this.IsInShell, out result.IsInShell);
      converter.Convert(this.ShellSlowdownFrames, out result.ShellSlowdownFrames);
      converter.Convert(this.IsPropellerFlying, out result.IsPropellerFlying);
      converter.Convert(this.PropellerLaunchFrames, out result.PropellerLaunchFrames);
      converter.Convert(this.PropellerSpinFrames, out result.PropellerSpinFrames);
      converter.Convert(this.UsedPropellerThisJump, out result.UsedPropellerThisJump);
      converter.Convert(this.PropellerDrillCooldown, out result.PropellerDrillCooldown);
      converter.Convert(this.HeldEntity, out result.HeldEntity);
      converter.Convert(this.CurrentPipe, out result.CurrentPipe);
      ConvertUser(converter, ref result);
      return result;
    }
  }
  [System.SerializableAttribute()]
  public unsafe partial class PhysicsContactPrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.PhysicsContactPrototype> {
    public FPVector2 Position;
    public FPVector2 Normal;
    public FP Distance;
    public Int32 Frame;
    public Int32 TileX;
    public Int32 TileY;
    public Quantum.QuantumEntityPrototype Entity;
    partial void ConvertUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.PhysicsContactPrototype prototype);
    public override Quantum.Prototypes.PhysicsContactPrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.PhysicsContactPrototype();
      converter.Convert(this.Position, out result.Position);
      converter.Convert(this.Normal, out result.Normal);
      converter.Convert(this.Distance, out result.Distance);
      converter.Convert(this.Frame, out result.Frame);
      converter.Convert(this.TileX, out result.TileX);
      converter.Convert(this.TileY, out result.TileY);
      converter.Convert(this.Entity, out result.Entity);
      ConvertUser(converter, ref result);
      return result;
    }
  }
  [System.SerializableAttribute()]
  public unsafe partial class PhysicsObjectPrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.PhysicsObjectPrototype> {
    [HideInInspector()]
    public FPVector2 Velocity;
    [HideInInspector()]
    public FPVector2 ParentVelocity;
    [HideInInspector()]
    public FPVector2 PreviousVelocity;
    public FPVector2 Gravity;
    public FP TerminalVelocity;
    public QBoolean IsFrozen;
    public QBoolean DisableCollision;
    [HideInInspector()]
    public QBoolean IsTouchingLeftWall;
    [HideInInspector()]
    public QBoolean IsTouchingRightWall;
    [HideInInspector()]
    public QBoolean IsTouchingCeiling;
    [HideInInspector()]
    public QBoolean IsTouchingGround;
    [HideInInspector()]
    public FP FloorAngle;
    [HideInInspector()]
    public QBoolean IsOnSlipperyGround;
    [HideInInspector()]
    public QBoolean IsOnSlideableGround;
    [FreeOnComponentRemoved()]
    [DynamicCollectionAttribute()]
    public Quantum.Prototypes.Unity.PhysicsContactPrototype[] Contacts = {};
    partial void ConvertUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.PhysicsObjectPrototype prototype);
    public override Quantum.Prototypes.PhysicsObjectPrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.PhysicsObjectPrototype();
      converter.Convert(this.Velocity, out result.Velocity);
      converter.Convert(this.ParentVelocity, out result.ParentVelocity);
      converter.Convert(this.PreviousVelocity, out result.PreviousVelocity);
      converter.Convert(this.Gravity, out result.Gravity);
      converter.Convert(this.TerminalVelocity, out result.TerminalVelocity);
      converter.Convert(this.IsFrozen, out result.IsFrozen);
      converter.Convert(this.DisableCollision, out result.DisableCollision);
      converter.Convert(this.IsTouchingLeftWall, out result.IsTouchingLeftWall);
      converter.Convert(this.IsTouchingRightWall, out result.IsTouchingRightWall);
      converter.Convert(this.IsTouchingCeiling, out result.IsTouchingCeiling);
      converter.Convert(this.IsTouchingGround, out result.IsTouchingGround);
      converter.Convert(this.FloorAngle, out result.FloorAngle);
      converter.Convert(this.IsOnSlipperyGround, out result.IsOnSlipperyGround);
      converter.Convert(this.IsOnSlideableGround, out result.IsOnSlideableGround);
      converter.Convert(this.Contacts, out result.Contacts);
      ConvertUser(converter, ref result);
      return result;
    }
  }
  [System.SerializableAttribute()]
  public unsafe partial class PowerupPrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.PowerupPrototype> {
    public AssetRef<PowerupAsset> Scriptable;
    public QBoolean FacingRight;
    public Int32 Lifetime;
    public QBoolean BlockSpawn;
    public QBoolean LaunchSpawn;
    public FPVector2 BlockSpawnOrigin;
    public FPVector2 BlockSpawnDestination;
    public Byte BlockSpawnAnimationLength;
    public Byte SpawnAnimationFrames;
    public Byte IgnorePlayerFrames;
    public Quantum.QuantumEntityPrototype ParentMarioPlayer;
    public FPVector2 AnimationCurveOrigin;
    public FP AnimationCurveTimer;
    partial void ConvertUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.PowerupPrototype prototype);
    public override Quantum.Prototypes.PowerupPrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.PowerupPrototype();
      converter.Convert(this.Scriptable, out result.Scriptable);
      converter.Convert(this.FacingRight, out result.FacingRight);
      converter.Convert(this.Lifetime, out result.Lifetime);
      converter.Convert(this.BlockSpawn, out result.BlockSpawn);
      converter.Convert(this.LaunchSpawn, out result.LaunchSpawn);
      converter.Convert(this.BlockSpawnOrigin, out result.BlockSpawnOrigin);
      converter.Convert(this.BlockSpawnDestination, out result.BlockSpawnDestination);
      converter.Convert(this.BlockSpawnAnimationLength, out result.BlockSpawnAnimationLength);
      converter.Convert(this.SpawnAnimationFrames, out result.SpawnAnimationFrames);
      converter.Convert(this.IgnorePlayerFrames, out result.IgnorePlayerFrames);
      converter.Convert(this.ParentMarioPlayer, out result.ParentMarioPlayer);
      converter.Convert(this.AnimationCurveOrigin, out result.AnimationCurveOrigin);
      converter.Convert(this.AnimationCurveTimer, out result.AnimationCurveTimer);
      ConvertUser(converter, ref result);
      return result;
    }
  }
  [System.SerializableAttribute()]
  public unsafe partial class ProjectilePrototype : Quantum.QuantumUnityPrototypeAdapter<Quantum.Prototypes.ProjectilePrototype> {
    public AssetRef<ProjectileAsset> Asset;
    public FP Speed;
    public Quantum.QuantumEntityPrototype Owner;
    public QBoolean FacingRight;
    public QBoolean HasBounced;
    public QBoolean PlayDestroySound;
    public QBoolean CheckedCollision;
    partial void ConvertUser(Quantum.QuantumEntityPrototypeConverter converter, ref Quantum.Prototypes.ProjectilePrototype prototype);
    public override Quantum.Prototypes.ProjectilePrototype Convert(Quantum.QuantumEntityPrototypeConverter converter) {
      var result = new Quantum.Prototypes.ProjectilePrototype();
      converter.Convert(this.Asset, out result.Asset);
      converter.Convert(this.Speed, out result.Speed);
      converter.Convert(this.Owner, out result.Owner);
      converter.Convert(this.FacingRight, out result.FacingRight);
      converter.Convert(this.HasBounced, out result.HasBounced);
      converter.Convert(this.PlayDestroySound, out result.PlayDestroySound);
      converter.Convert(this.CheckedCollision, out result.CheckedCollision);
      ConvertUser(converter, ref result);
      return result;
    }
  }
}
#pragma warning restore 0109
#pragma warning restore 1591
