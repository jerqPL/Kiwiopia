using UnityEngine;

[CreateAssetMenu(fileName = "UnitType", menuName = "Scriptable Objects/UnitType")]
public class UnitType : ScriptableObject
{
    public int id;
    public string description;

    public int health;
    public int range;
    public int damage;
    public int resistance;
    public float attackCooldown;

    public float speed;
    public bool canClimb;

    public int cost;
    public float timePerCoin;

    public int scoutDistance;

    public GameObject model;
}
