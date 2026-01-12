using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(UnitMovement))]
public class UnitAttack : NetworkBehaviour
{
    Unit unit;
    UnitMovement unitMovement;
    public float attackCooldown = 0f;
    private List<Tile> tilesInRange = new List<Tile>();
    public NetworkVariable<int> enemyIndex = new NetworkVariable<int>(0);
    public NetworkVariable<bool> inCombat = new NetworkVariable<bool>(false);

    public event System.Action AfterAttack;

    void Awake()
    {
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        unit.AfterNetworkSpawn += UpdateTilesInRange;
        unitMovement.AfterMove += UpdateTilesInRange;
    }
    private void TakeCooldown()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    public void AttackEnemies()
    {
        if (!IsServer)
        {
            if (unitMovement.isMoving.Value) return;
            if (attackCooldown > 0) return;
            foreach (Tile tileInRange in tilesInRange)
            {
                if (tileInRange.unit != null && tileInRange.unit.owner != unit.owner && !tileInRange.unit.isDead)
                {
                    AfterAttack?.Invoke();
                    attackCooldown = unit.unitType.attackCooldown;
                    break;
                }
            }
        }
        else
        {
            if (unitMovement.isMoving.Value) return;
            if (attackCooldown > 0) return;
            bool foundTarget = false;
            foreach (Tile tileInRange in tilesInRange)
            {
                if (tileInRange.unit != null && tileInRange.unit.owner != unit.owner && !tileInRange.unit.isDead)
                {
                    AfterAttack?.Invoke();
                    foundTarget = true;
                    enemyIndex.Value = Global.unitsHandler.GetIndexOf(tileInRange.unit);
                    attackCooldown = unit.unitType.attackCooldown;
                    Global.unitsHandler.DealDamage(Global.unitsHandler.GetIndexOf(GetComponent<Unit>()), Global.unitsHandler.GetIndexOf(tileInRange.unit));
                    break;
                }
            }
            if (foundTarget)
            {
                inCombat.Value = true;
            }
            else
            {
                inCombat.Value = false;
            }
        }
    }

    private void Update()
    {
        if (unit.isDead) return;

        if (inCombat.Value && !unitMovement.isMoving.Value) unit.RotateTowards(Global.unitsHandler.GetUnitAt(enemyIndex.Value).transform.position);
        TakeCooldown();
        AttackEnemies();
    }

    private void UpdateTilesInRange()
    {
        tilesInRange.Clear();
        if (unit.tile != null)
        {
            List<Tile> unitVisibleTiles = new List<Tile> { unit.tile };
            for (int i = 0; i < unit.unitType.range; i++)
            {
                int visTiles = unitVisibleTiles.Count;
                for (int j = 0; j < visTiles; j++)
                {
                    Tile tile = unitVisibleTiles[j];
                    foreach (Tile neighbour in tile.neighbors)
                    {
                        if (!unitVisibleTiles.Contains(neighbour))
                            unitVisibleTiles.Add(neighbour);
                    }
                }
            }
            tilesInRange.AddRange(unitVisibleTiles.Where(x => !tilesInRange.Contains(x)));
        }
    }
}
