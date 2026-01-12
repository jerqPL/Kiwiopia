using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitUI : NetworkBehaviour
{
    private Unit unit;
    private Health health;
    private UnitAttack unitAttack;

    [SerializeField] private LineRenderer lineRendererPrefab;
    [SerializeField] private BarUI healthBar;
    [SerializeField] private BarUI attackCooldownBar;

    private LineRenderer progressLine;
    private void Awake()
    {
        unit = GetComponent<Unit>();
        health = GetComponent<Health>();
        unitAttack = GetComponent<UnitAttack>();
        health.health.OnValueChanged += UpdateHealthBar;
        unitAttack.inCombat.OnValueChanged += ChangeVisibilityAttackCooldown;
        unit.AfterDie += () => {
            healthBar.transform.gameObject.SetActive(false);
            DestroyProgressLine();
        };
    }

    public void CreateProgressLine(List<Tile> path)
    {
        DestroyProgressLine();
        progressLine = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
        progressLine.numCornerVertices = 8;
        progressLine.numCapVertices = 8;
        progressLine.material = Global.inProgressLineMaterial;
        progressLine.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            progressLine.SetPosition(i, Global.AddToYVector3(Global.ZeroYVector3(path[i].transform.position), Global.lineHegithAboveTiles));
    }

    public void DestroyProgressLine()
    {
        Destroy(progressLine.gameObject);
    }

    private void UpdateHealthBar(int prev, int curr)
    {
        if (curr == unit.unitType.health || curr == 0)
        {
            healthBar.Disable();
        }
        else
        {
            healthBar.Enable();
            healthBar.UpdateValue((float)curr / unit.unitType.health);
        }
    }

    private void UpdateAttackCooldownBar()
    {
        attackCooldownBar.UpdateValue((unit.unitType.attackCooldown - (float)unitAttack.attackCooldown) / unit.unitType.attackCooldown);
    }

    void LateUpdate()
    {
        if (unitAttack.inCombat.Value)
        {
            UpdateAttackCooldownBar();
        }
    }

    private void ChangeVisibilityAttackCooldown(bool prev, bool curr)
    {
        if (curr)
        {
            attackCooldownBar.Enable();
        }
        else
        {
            attackCooldownBar.Disable();
        }
    }
}