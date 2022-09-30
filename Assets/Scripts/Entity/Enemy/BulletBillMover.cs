using UnityEngine;

using NSMB.Utils;

public class BulletBillMover : KillableEntity {

    //---Serialized Variables
    [SerializeField] private float speed, playerSearchRadius = 4, despawnDistance = 8;

    //---Misc Variables
    private Vector2 searchVector;

    public override void Awake() {
        base.Awake();
        searchVector = new(playerSearchRadius * 2, 100);
    }

    public void OnBeforeSpawned(bool shootRight) {
        FacingRight = shootRight;
    }

    public override void Spawned() {

        body.velocity = new(speed * (FacingRight ? 1 : -1), body.velocity.y);

        Transform t = transform.GetChild(1);
        ParticleSystem ps = t.GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shape = ps.shape;
        if (FacingRight) {
            Transform tf = transform.GetChild(0);
            tf.localPosition *= new Vector2(-1, 1);
            shape.rotation = new Vector3(0, 0, -33);
        }

        ps.Play();
        sRenderer.flipX = FacingRight;
    }

    public override void FixedUpdateNetwork() {
        if (GameManager.Instance && GameManager.Instance.gameover) {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0;
            animator.enabled = false;
            body.isKinematic = true;
            return;
        }

        if (IsFrozen)
            return;

        body.velocity = new(speed * (FacingRight ? 1 : -1), body.velocity.y);
        DespawnCheck();
    }
    public override void InteractWithPlayer(PlayerController player) {
        if (IsFrozen || player.IsFrozen)
            return;

        Vector2 damageDirection = (player.body.position - body.position).normalized;
        bool attackedFromAbove = Vector2.Dot(damageDirection, Vector2.up) > 0.5f;

        if (player.IsStarmanInvincible || player.IsInShell || player.sliding
            || ((player.IsGroundpounding || player.IsDrilling) && player.State != Enums.PowerupState.MiniMushroom && attackedFromAbove)
            || player.State == Enums.PowerupState.MegaMushroom) {

            if (player.IsDrilling) {
                player.bounce = true;
                player.IsDrilling = false;
            }
            Kill();
            return;
        }
        if (attackedFromAbove) {
            if (!(player.State == Enums.PowerupState.MiniMushroom && !player.IsGroundpounding)) {
                Kill();
            }
            PlaySound(Enums.Sounds.Enemy_Generic_Stomp);
            player.IsDrilling = false;
            player.IsGroundpounding = false;
            player.bounce = true;
            return;
        }

        player.Powerdown(false);
    }

    public override bool InteractWithFireball(FireballMover fireball) {
        if (fireball.isIceball) {
            Runner.Spawn(PrefabList.Instance.Obj_FrozenCube, body.position, onBeforeSpawned: (runner, obj) => {
                FrozenCube cube = obj.GetComponent<FrozenCube>();
                cube.OnBeforeSpawned(this);
            });
            return true;
        }

        //don't die to fireballs, but still destroy them.
        return true;
    }

    private void DespawnCheck() {
        foreach (PlayerController player in GameManager.Instance.players) {
            if (!player)
                continue;

            if (Utils.WrappedDistance(player.body.position, body.position) < despawnDistance)
                return;
        }

        Runner.Despawn(Object);
    }

    public override void Kill() {
        SpecialKill(FacingRight, false, 0);
    }

    public override void SpecialKill(bool right, bool groundpound, int combo) {
        Dead = true;
        body.velocity = Vector2.right * 2.5f;
        body.constraints = RigidbodyConstraints2D.None;
        body.angularVelocity = 400f * (right ? 1 : -1);
        body.gravityScale = 1.5f;
        body.isKinematic = false;
        hitbox.enabled = false;
        animator.speed = 0;
        gameObject.layer = Layers.LayerHitsNothing;

        if (groundpound)
            Instantiate(Resources.Load("Prefabs/Particle/EnemySpecialKill"), body.position + Vector2.right * 0.5f, Quaternion.identity);

        PlaySound(Enums.Sounds.Enemy_Shell_Kick);
    }

    public void OnDrawGizmosSelected() {
        if (!GameManager.Instance)
            return;

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(body.position, searchVector);
        //left border check
        if (body.position.x - playerSearchRadius < GameManager.Instance.GetLevelMinX())
            Gizmos.DrawCube(body.position + new Vector2(GameManager.Instance.levelWidthTile * 0.5f, 0), searchVector);
        //right border check
        if (body.position.x + playerSearchRadius > GameManager.Instance.GetLevelMaxX())
            Gizmos.DrawCube(body.position - new Vector2(GameManager.Instance.levelWidthTile * 0.5f, 0), searchVector);
    }
}