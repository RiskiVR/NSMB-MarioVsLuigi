using UnityEngine;

using Fusion;

[CreateAssetMenu(fileName = "New Prefab List", menuName = "ScriptableObjects/PrefabList")]
public class PrefabList : ScriptableObject {

    public static PrefabList Instance => GlobalController.Instance.prefabs;

    //---Network Helpers
    public NetworkPrefabRef PlayerDataHolder;

    //---World Elements
    public NetworkPrefabRef Obj_Fireball, Obj_Iceball;
    public NetworkPrefabRef Obj_LooseCoin;
    public NetworkPrefabRef Obj_BigStar;
    public NetworkPrefabRef Obj_BlockBump;

    //---Enemies
    public NetworkPrefabRef BulletBill;

    //---Powerups
    public NetworkPrefabRef Powerup_1Up;
    public NetworkPrefabRef Powerup_Starman, Powerup_MegaMushroom;
    public NetworkPrefabRef Powerup_Mushroom, Powerup_FireFlower, Powerup_BlueShell, Powerup_PropellerMushroom, Powerup_IceFlower;
    public NetworkPrefabRef Powerup_MiniMushroom;

    //---Particles
    public GameObject Particle_1Up, Particle_Giant;
    public GameObject Particle_CoinCollect, Particle_CoinFromBlock;
    public GameObject Particle_Respawn;
    public GameObject Particle_FireballWall, Particle_IceballWall;
}