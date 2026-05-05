using Unity.Netcode;
using UnityEngine;

[RequireComponent (typeof(Unit))]
public class UnitPay : NetworkBehaviour
{
    private Unit unit;
    private float priceTimer = 0f;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Update()
    {
        if (IsServer && !unit.isLeader.Value && !unit.isDead)
        {
            TakeMoney();
        }
    }

    private void TakeMoney()
    {
        priceTimer += Time.deltaTime;
        if (priceTimer > unit.unitType.timePerCoin)
        {
            int coinsToTake = (int)Mathf.Floor(priceTimer / unit.unitType.timePerCoin);

            priceTimer -= coinsToTake * unit.unitType.timePerCoin;

            if (!unit.owner.TakeResources(coinsToTake))
            {
                Global.unitsHandler.KillUnitServerRpc(Global.unitsHandler.GetIndexOf(unit));
            }
        }
    }
}
