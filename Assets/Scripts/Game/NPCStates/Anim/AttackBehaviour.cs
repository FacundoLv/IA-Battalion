using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
{
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(IsAttacking, true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(IsAttacking, false);
    }
}