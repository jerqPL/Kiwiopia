using UnityEngine;
using Unity.Netcode;
using System;

public class Health : NetworkBehaviour
{
    public UnitType unitType;
    public NetworkVariable<int> health = new NetworkVariable<int>(1);

    public void SetUnitType(int unitTypeIndex)
    {
        unitType = Global.unitTypes[unitTypeIndex];
    }

    public void RecieveDamage(int damage)
    {
        health.Value -= Math.Max(0, damage - unitType.resistance);
        if (health.Value <= 0)
        {
            if (GetComponent<Unit>() != null)
            {
                Global.unitsHandler.KillUnitServerRpc(Global.unitsHandler.GetIndexOf(GetComponent<Unit>()));
            }
        }
    }

    public void SetHealth(int value)
    {
        health.Value = value;
    }
}
