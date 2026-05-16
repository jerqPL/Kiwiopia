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

    private LineRenderer movementProgressLine;
    private LineRenderer movementPathLine;
    private void Awake()
    {
        unit = GetComponent<Unit>();
        health = GetComponent<Health>();
        unitAttack = GetComponent<UnitAttack>();
        health.health.OnValueChanged += UpdateHealthBar;
        unitAttack.inCombat.OnValueChanged += ChangeVisibilityAttackCooldown;
        unit.AfterDie += () => {
            healthBar.transform.gameObject.SetActive(false);
            DestroyMovementProgressLine();
        };
    }

    public void CreateMovementProgressLine(List<Tile> path)
    {
        DestroyMovementProgressLine();
        movementProgressLine = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
        movementProgressLine.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            movementProgressLine.SetPosition(i, Global.AddToYVector3(Global.ZeroYVector3(path[i].transform.position), Global.lineHegithAboveTiles));
    }

    public void DestroyMovementProgressLine()
    {
        if (movementProgressLine != null)
            Destroy(movementProgressLine.gameObject);
    }

    public void CreateMovementPathLine(List<Tile> path)
    {
        DestroyMovementPathLine();
        movementPathLine = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.Euler(90, 0, 0));
        movementPathLine.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            movementPathLine.SetPosition(i, Global.AddToYVector3(Global.ZeroYVector3(path[i].transform.position), Global.lineHegithAboveTiles));
    }

    public void DestroyMovementPathLine()
    {
        if (movementPathLine != null)
            Destroy(movementPathLine.gameObject);
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

    private void MakeInvisible()
    {
        healthBar.Disable();
        attackCooldownBar.Disable();
        DestroyMovementProgressLine();
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            rend.enabled = false;
        }
    }

    private void MakeVisible()
    {
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            rend.enabled = true;
        }
        UpdateHealthBar(0, health.health.Value);
        if (unitAttack.inCombat.Value)
        {
            attackCooldownBar.Enable();
        }
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            MakeVisible();
        }
        else
        {
            MakeInvisible();
        }
    }
}