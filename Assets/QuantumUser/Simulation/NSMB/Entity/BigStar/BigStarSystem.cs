using Photon.Deterministic;
using Quantum.Physics2D;

namespace Quantum {
    public unsafe class BigStarSystem : SystemMainThread, ISignalOnTrigger2D {

        public override void Update(Frame f) {
            var stage = f.FindAsset<VersusStageData>(f.Map.UserAsset);

            if (!f.Exists(f.Global->MainBigStar) && QuantumUtils.Decrement(ref f.Global->BigStarSpawnTimer)) {
                int spawnpoints = stage.BigStarSpawnpoints.Length;
                ref BitSet64 usedSpawnpoints = ref f.Global->UsedStarSpawns;
                for (int i = 0; i < spawnpoints; i++) {
                    // Find a spot...
                    if (f.Global->UsedStarSpawnCount == spawnpoints) {
                        usedSpawnpoints.ClearAll();
                        f.Global->UsedStarSpawnCount = 0;
                    }

                    int count = f.RNG->Next(0, spawnpoints - f.Global->UsedStarSpawnCount);
                    int index = 0;
                    for (int j = 0; j < spawnpoints; j++) {
                        if (!usedSpawnpoints.IsSet(j) && --count == 0) {
                            // This is the index to use
                            index = j;
                            break;
                        }
                    }
                    usedSpawnpoints.Set(index);
                    f.Global->UsedStarSpawnCount++;

                    // Spawn a star.
                    FPVector2 position = stage.BigStarSpawnpoints[index];
                    Shape2D shape = Shape2D.CreateCircle(FP.FromString("2.5"));
                    HitCollection hits = f.Physics2D.OverlapShape(position, 0, shape, f.Layers.GetLayerMask("Player"));

                    if (hits.Count == 0) {
                        // Hit no players
                        EntityRef newEntity = f.Create(f.SimulationConfig.BigStarPrototype);
                        f.Global->MainBigStar = newEntity;
                        var newStarTransform = f.Unsafe.GetPointer<Transform2D>(newEntity);
                        var newStar = f.Unsafe.GetPointer<BigStar>(newEntity);
                        var newStarPhysicsObject = f.Unsafe.GetPointer<PhysicsObject>(newEntity);

                        newStarTransform->Position = position;
                        newStar->IsStationary = true;
                        newStarPhysicsObject->DisableCollision = true;
                        break;
                    }
                }

                if (!f.Exists(f.Global->MainBigStar)) {
                    f.Global->BigStarSpawnTimer = 30;
                }
            }

            var allStars = f.Filter<BigStar>();
            while (allStars.NextUnsafe(out EntityRef entity, out BigStar* bigStar)) {
                HandleStar(f, stage, entity, bigStar);
            }
        }

        private void HandleStar(Frame f, VersusStageData stage, EntityRef entity, BigStar* bigStar) {
            if (bigStar->IsStationary) {
                return;
            }

            var transform = f.Unsafe.GetPointer<Transform2D>(entity);

            if (QuantumUtils.Decrement(ref bigStar->Lifetime) || (transform->Position.Y < stage.StageWorldMin.Y && bigStar->UncollectableFrames == 0)) {
                f.Destroy(entity);
                return;
            }

            var physicsObject = f.Unsafe.GetPointer<PhysicsObject>(entity);
            if (physicsObject->IsTouchingGround) {
                physicsObject->Velocity.Y = bigStar->BounceForce;
            }

            if (physicsObject->IsTouchingLeftWall || physicsObject->IsTouchingRightWall) {
                bigStar->FacingRight = physicsObject->IsTouchingLeftWall;
                physicsObject->Velocity.X = bigStar->Speed * (bigStar->FacingRight ? 1 : -1);
            }

            if (physicsObject->DisableCollision && QuantumUtils.Decrement(ref bigStar->PassthroughFrames)) {
                var physicsCollider = f.Unsafe.GetPointer<PhysicsCollider2D>(entity);
                if ((f.Number % 4) == 0 && !PhysicsObjectSystem.BoxInsideTile(f, transform->Position, physicsCollider->Shape, stage)) {
                    physicsObject->DisableCollision = false;
                }
            }
            QuantumUtils.Decrement(ref bigStar->UncollectableFrames);
        }

        public void OnTrigger2D(Frame f, TriggerInfo2D info) {
            if (f.DestroyPending(info.Other)
                || !f.Unsafe.TryGetPointer(info.Entity, out MarioPlayer* mario)
                || !f.Unsafe.TryGetPointer(info.Other, out BigStar* star)
                || star->UncollectableFrames > 0) {
                return;
            }

            if (mario->IsDead) {
                return;
            }

            mario->Stars++;
            var stage = f.FindAsset<VersusStageData>(f.Map.UserAsset);

            if (star->IsStationary) {
                stage.ResetStage(f, false);
            }

            f.Events.MarioPlayerCollectedStar(f, info.Entity, *mario, f.Unsafe.GetPointer<Transform2D>(info.Other)->Position);
            f.Global->BigStarSpawnTimer = (ushort) (624 - (f.PlayerConnectedCount * 12));
            f.Destroy(info.Other);
        }
    }
}
