using Photon.Deterministic;
using Quantum;
using System;
using UnityEngine;

public static unsafe class QuantumUtils {

    private static readonly SoundEffect[] ComboSounds = {
        SoundEffect.Enemy_Shell_Kick,
        SoundEffect.Enemy_Shell_Combo1,
        SoundEffect.Enemy_Shell_Combo2,
        SoundEffect.Enemy_Shell_Combo3,
        SoundEffect.Enemy_Shell_Combo4,
        SoundEffect.Enemy_Shell_Combo5,
        SoundEffect.Enemy_Shell_Combo6,
        SoundEffect.Enemy_Shell_Combo7,
    };

    public static SoundEffect GetComboSoundEffect(int combo) {
        return ComboSounds[Mathf.Clamp(combo, 0, ComboSounds.Length - 1)];
    }

    public static Vector2Int WorldToUnityTile(Frame f, FPVector2 worldPos) {
        return WorldToUnityTile(f.FindAsset<VersusStageData>(f.Map.UserAsset), worldPos);
    }

    public static Vector2Int WorldToUnityTile(VersusStageData stage, FPVector2 worldPos) {
        worldPos -= stage.TilemapWorldPosition;
        worldPos *= 2;
        return new Vector2Int(FPMath.FloorToInt(worldPos.X), FPMath.FloorToInt(worldPos.Y));
    }

    public static Vector2Int UntiyTileToRelativeTile(Frame f, Vector2Int unityTile) {
        return UnityTileToRelativeTile(f.FindAsset<VersusStageData>(f.Map.UserAsset), unityTile);
    }

    public static Vector2Int UnityTileToRelativeTile(VersusStageData stage, Vector2Int unityTile) {
        int x = unityTile.x - stage.TileOrigin.x;
        x = (x % stage.TileDimensions.x + stage.TileDimensions.x) % stage.TileDimensions.x; // Wrapping
        int y = unityTile.y - stage.TileOrigin.y;
        return new Vector2Int(x, y);
    }

    public static Vector2Int WorldToRelativeTile(Frame f, FPVector2 worldPos) {
        return WorldToRelativeTile(f.FindAsset<VersusStageData>(f.Map.UserAsset), worldPos);
    }

    public static Vector2Int WorldToRelativeTile(VersusStageData stage, FPVector2 worldPos) {
        return UnityTileToRelativeTile(stage, WorldToUnityTile(stage, worldPos));
    }

    public static FPVector2 UnityTileToWorld(Frame f, Vector2Int unityTile) {
        return UnityTileToWorld(f.FindAsset<VersusStageData>(f.Map.UserAsset), unityTile);
    }

    public static FPVector2 UnityTileToWorld(VersusStageData stage, Vector2Int unityTile) {
        return (new FPVector2(unityTile.x, unityTile.y) / 2) + stage.TilemapWorldPosition;
    }

    public static Vector2Int RelativeTileToUnityTile(Frame f, Vector2Int relativeTile) {
        return RelativeTileToUnityTile(f.FindAsset<VersusStageData>(f.Map.UserAsset), relativeTile);
    }

    public static Vector2Int RelativeTileToUnityTile(VersusStageData stage, Vector2Int relativeTile) {
        int x = relativeTile.x + stage.TileOrigin.x;
        int y = relativeTile.y + stage.TileOrigin.y;
        return new Vector2Int(x, y);
    }

    public static FPVector2 RelativeTileToWorld(Frame f, Vector2Int relativeTile) {
        return RelativeTileToWorld(f.FindAsset<VersusStageData>(f.Map.UserAsset), relativeTile);
    }

    public static FPVector2 RelativeTileToWorld(VersusStageData stage, Vector2Int relativeTile) {
        return UnityTileToWorld(stage, RelativeTileToUnityTile(stage, relativeTile));
    }

    public static FPVector2 RelativeTileToWorldRounded(VersusStageData stage, Vector2Int relativeTile) {
        return RelativeTileToWorld(stage, relativeTile) + FPVector2.One * FP._0_25;
    }

    public static FPVector2 WrapUnityTile(Frame f, FPVector2 unityTile, out WrapDirection wrapDirection) {
        return WrapUnityTile(f.FindAsset<VersusStageData>(f.Map.UserAsset), unityTile, out wrapDirection);
    }

    public static FPVector2 WrapUnityTile(VersusStageData stage, FPVector2 unityTile, out WrapDirection wrapDirection) {
        if (unityTile.X < stage.TileOrigin.x) {
            unityTile.X += stage.TileDimensions.x;
            wrapDirection = WrapDirection.Left;

        } else if (unityTile.X >= stage.TileOrigin.x + stage.TileDimensions.x) {
            unityTile.X -= stage.TileDimensions.x;
            wrapDirection = WrapDirection.Right;

        } else {
            wrapDirection = WrapDirection.NoWrap;
        }

        return unityTile;
    }

    public static FPVector2 WrapWorld(Frame f, FPVector2 worldPos, out WrapDirection wrapDirection) {
        return WrapWorld(f.FindAsset<VersusStageData>(f.Map.UserAsset), worldPos, out wrapDirection);
    }

    public static FPVector2 WrapWorld(VersusStageData stage, FPVector2 worldPos, out WrapDirection wrapDirection) {
        if (worldPos.X < stage.StageWorldMin.X) {
            worldPos.X += stage.TileDimensions.x / 2;
            wrapDirection = WrapDirection.Left;

        } else if (worldPos.X >= stage.StageWorldMax.X) {
            worldPos.X -= stage.TileDimensions.x / 2;
            wrapDirection = WrapDirection.Right;

        } else {
            wrapDirection = WrapDirection.NoWrap;
        }

        return worldPos;
    }

    public static void UnwrapWorldLocations(Frame f, FPVector2 a, FPVector2 b, out FPVector2 newA, out FPVector2 newB) {
        UnwrapWorldLocations(f.FindAsset<VersusStageData>(f.Map.UserAsset), a, b, out newA, out newB);
    }

    public static void UnwrapWorldLocations(VersusStageData stage, FPVector2 a, FPVector2 b, out FPVector2 newA, out FPVector2 newB) {
        newA = a;
        newB = b;

        if (!stage.IsWrappingLevel) {
            return;
        }

        FP width = stage.TileDimensions.x * FP._0_50;
        if (FPMath.Abs(newA.X - newB.X) > width / 2) {
            newB.X += width * (newB.X > stage.StageWorldMin.X + (width / 2) ? -1 : 1);
        }
    }

    public enum WrapDirection {
        NoWrap,
        Left,
        Right
    }

    public static int GetTeamStars(Frame f, int team) {
        int sum = 0;
        var allPlayers = f.Filter<MarioPlayer>();
        while (allPlayers.Next(out _, out MarioPlayer mario)) {
            if (mario.Team == team) {
                sum += mario.Stars;
            }
        }

        return sum;
    }

    public static int GetFirstPlaceStars(Frame f) {
        Span<int> teamStars = stackalloc int[10];

        var allPlayers = f.Filter<MarioPlayer>();
        while (allPlayers.Next(out _, out MarioPlayer mario)) {
            teamStars[mario.Team] += mario.Stars;
        }

        int max = 0;
        foreach (int stars in teamStars) {
            if (stars > max) {
                max = stars;
            }
        }

        return max;
    }

    // MAX(0,$B15+(IF(stars behind >0,LOG(B$1+1, 2.71828),0)*$C15*(1-(($M$15-$M$14))/$M$15)))
    public static PowerupAsset GetRandomItem(Frame f, MarioPlayer mario) {
        var stage = f.FindAsset<VersusStageData>(f.Map.UserAsset);

        // "Losing" variable based on ln(x+1), x being the # of stars we're behind

        //gm.teamManager.GetTeamStars(player.Data.Team, out int ourStars);
        int ourStars = GetTeamStars(f, mario.Team);
        int leaderStars = GetFirstPlaceStars(f);
        int starsToWin = f.RuntimeConfig.StarsToWin;
        bool custom = f.RuntimeConfig.CustomPowerupsEnabled;
        bool lives = f.RuntimeConfig.LivesEnabled;

        bool big = stage.SpawnBigPowerups;
        bool vertical = stage.SpawnVerticalPowerups;

        bool canSpawnMega = true;
        var allPlayers = f.Filter<MarioPlayer>();
        while (allPlayers.Next(out _, out MarioPlayer otherPlayer)) {
            if (otherPlayer.CurrentPowerupState == PowerupState.MegaMushroom) {
                canSpawnMega = false;
                break;
            }
        }

        FP totalChance = 0;
        foreach (PowerupAsset powerup in f.SimulationConfig.AllPowerups) {
            if (powerup.State == PowerupState.MegaMushroom && !canSpawnMega) {
                continue;
            }

            if ((powerup.BigPowerup && !big)
                || (powerup.VerticalPowerup && !vertical)
                || (powerup.CustomPowerup && !custom)
                || (powerup.LivesOnlyPowerup && !lives)) {
                continue;
            }

            totalChance += powerup.GetModifiedChance(starsToWin, leaderStars, ourStars);
        }

        FP rand = f.RNG->Next(0, totalChance);
        foreach (PowerupAsset powerup in f.SimulationConfig.AllPowerups) {
            if (powerup.State == PowerupState.MegaMushroom && !canSpawnMega) {
                continue;
            }

            if ((powerup.BigPowerup && !big)
                || (powerup.VerticalPowerup && !vertical)
                || (powerup.CustomPowerup && !custom)
                || (powerup.LivesOnlyPowerup && !lives)) {
                continue;
            }

            FP chance = powerup.GetModifiedChance(starsToWin, leaderStars, ourStars);

            if (rand < chance) {
                return powerup;
            }

            rand -= chance;
        }

        return f.SimulationConfig.FallbackPowerup;
    }

    public static FP WrappedDistance(Frame f, FPVector2 a, FPVector2 b) {
        return WrappedDistance(f, a, b, out _);
    }

    public static FP WrappedDistance(VersusStageData stage, FPVector2 a, FPVector2 b) {
        return WrappedDistance(stage, a, b, out _);
    }

    public static FP WrappedDistance(Frame f, FPVector2 a, FPVector2 b, out FP xDifference) {
        return WrappedDistance(f.FindAsset<VersusStageData>(f.Map.UserAsset), a, b, out xDifference);
    }

    public static FP WrappedDistance(VersusStageData stage, FPVector2 a, FPVector2 b, out FP xDifference) {
        FP width = stage.TileDimensions.x * FP._0_50;
        if (stage.IsWrappingLevel && FPMath.Abs(a.X - b.X) > width * FP._0_50) {
            a.X -= width * FPMath.Sign(a.X - b.X);
        }

        xDifference = a.X - b.X;
        return FPVector2.Distance(a, b);
    }

    public static FPVector2 SmoothDamp(FPVector2 current, FPVector2 target, ref FPVector2 currentVelocity, FP smoothTime, FP maxSpeed, FP deltaTime) {
        smoothTime = FPMath.Max(FP.FromString("0.0001"), smoothTime);
        FP twoOverSmoothTime = 2 / smoothTime;
        FP twoOverDelta = twoOverSmoothTime * deltaTime;

        FP magicPolynomial = 1 / ((1 + twoOverDelta) + (FP.FromString("0.48") * twoOverDelta * twoOverDelta)) + (FP.FromString("0.235") * twoOverDelta * twoOverDelta * twoOverDelta);

        FPVector2 diff = current - target;
        FP xDiff = current.X - target.X;
        FP yDiff = current.Y - target.Y;
        FPVector2 originalTarget = target;
        FP maxDistance = maxSpeed * smoothTime;

        if (diff.SqrMagnitude > maxDistance * maxDistance) {
            FP magnitude = diff.Magnitude;
            xDiff = xDiff / magnitude * maxDistance;
            yDiff = yDiff / magnitude * maxDistance;
        }

        target.X = current.Y - xDiff;
        target.X = current.Y - yDiff;

        FP num9 = (currentVelocity.X + twoOverSmoothTime * xDiff) * deltaTime;
        FP num10 = (currentVelocity.Y + twoOverSmoothTime * yDiff) * deltaTime;

        currentVelocity.X = (currentVelocity.X - twoOverSmoothTime * num9) * magicPolynomial;
        currentVelocity.Y = (currentVelocity.Y - twoOverSmoothTime * num10) * magicPolynomial;

        FP x = target.X + (xDiff + num9) * magicPolynomial;
        FP y = target.Y + (yDiff + num10) * magicPolynomial;

        FP xDiffNeg = originalTarget.X - current.X;
        FP yDiffNeg = originalTarget.Y - current.Y;
        FP xDist = x - originalTarget.X;
        FP yDist = y - originalTarget.Y;

        if ((xDiffNeg * xDist) + (yDiffNeg * yDist) > 0) {
            x = originalTarget.X;
            y = originalTarget.Y;
            currentVelocity.X = (x - originalTarget.X) / deltaTime;
            currentVelocity.X = (y - originalTarget.Y) / deltaTime;
        }
        return new FPVector2(x, y);
    }

    public static int WrappedDirectionSign(Frame f, FPVector2 a, FPVector2 b) {
        return WrappedDirectionSign(f.FindAsset<VersusStageData>(f.Map.UserAsset), a, b);
    }

    public static int WrappedDirectionSign(VersusStageData stage, FPVector2 a, FPVector2 b) {
        if (!stage.IsWrappingLevel) {
            return a.X > b.X ? 1 : -1;
        }

        return (a.X > b.X ^ FPMath.Abs(a.X - b.X) > stage.TileDimensions.x * FP._0_25) ? 1 : -1;
    }

    public static PowerupAsset FindPowerupAsset(Frame f, PowerupState state) {
        foreach (var powerup in f.SimulationConfig.AllPowerups) {
            if (powerup.State == state) {
                return powerup;
            }
        }
        return null;
    }

    public static bool Decrement(ref byte timer) {
        if (timer > 0) {
            return --timer == 0;
        }

        return true;
    }

    public static bool Decrement(ref ushort timer) {
        if (timer > 0) {
            return --timer == 0;
        }

        return true;
    }

    public static bool Decrement(ref int timer) {
        if (timer > 0) {
            return --timer == 0;
        }

        return true;
    }
}