using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class UnitAnimations : NetworkBehaviour
{
    private Unit unit;
    private UnitAttack unitAttack;
    private UnitMovement unitMovement;
    private Animator animator;
    private UnitParts parts;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        unitAttack = GetComponent<UnitAttack>();
        unitMovement = GetComponent<UnitMovement>();
        unit.AfterNetworkSpawn += () => { 
            animator = unit.model.GetComponent<Animator>();
            parts = unit.model.GetComponent<UnitParts>();
            UpdateWeaponTransform(false, false); 
        };
        unitAttack.AfterAttack += SetAttackAnimationTrigger;
        unitAttack.inCombat.OnValueChanged += UpdateWeaponTransform;
    }

    private void Update()
    {
        SetAnimationVariables();
    }

    private void SetAnimationVariables()
    {
        if (animator == null) return;

        animator.SetBool("isMoving", unitMovement.isMoving.Value);
        animator.SetBool("isDead", unit.isDead);
        animator.SetBool("inCombat", unitAttack.inCombat.Value);
    }

    private void SetAttackAnimationTrigger()
    {
        if (unit.unitType.name == "Spearman")
        {
            animator.SetTrigger("spear attack");
        }
        if (unit.unitType.name == "Swordman")
        {
            animator.SetTrigger("sword attack");
        }
        if (unit.unitType.name == "Shieldbearer")
        {
            animator.SetTrigger("shield attack");
        }
    }

    private void UpdateWeaponTransform(bool prev, bool curr)
    {
        if (curr)
        {
            parts.weapon.transform.SetParent(parts.rightHand.transform);
            parts.weapon.transform.localPosition = parts.positionOffsetFighting;
            parts.weapon.transform.localRotation = Quaternion.Euler(parts.rotationOffsetFighting);
        }
        else
        {
            parts.weapon.transform.SetParent(parts.boneWhileNotFighting.transform);
            parts.weapon.transform.localPosition = parts.positionOffsetNotFighting;
            parts.weapon.transform.localRotation = Quaternion.Euler(parts.rotationOffsetNotFighting);
        }

        Transform weapon = parts.weapon.transform;
        Transform parent = weapon.parent;

        Vector3 parentScale = parent.lossyScale;

        Vector3 targetWorldScale = Vector3.one * parts.scaleFactor;

        weapon.localScale = new Vector3(
            targetWorldScale.x / parentScale.x,
            targetWorldScale.y / parentScale.y,
            targetWorldScale.z / parentScale.z
        );
    }
}
