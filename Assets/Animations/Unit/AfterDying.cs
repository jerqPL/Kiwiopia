using UnityEngine;

public class AfterDying : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Unit unit = animator.transform.parent.GetComponent<Unit>();
        if (unit != null)
            unit.SetRandomRotation();
    }
}
