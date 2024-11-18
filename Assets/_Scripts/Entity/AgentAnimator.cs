using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class AgentAnimations : MonoBehaviour
{
    private Animator animator;
    [SerializeField]
    private UnityEvent OnFinshWaitingForAnimation;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void RotateToPointer(Vector2 lookDirection)
    {
        Vector3 scale = transform.localScale;
        if (lookDirection.x > 0 )
        {
            scale.x = Mathf.Abs(scale.x) * 1;
        }
        else if (lookDirection.x < 0 && scale.x > 0)
        {
            scale.x = Mathf.Abs(scale.x) * -1;
        }
        transform.localScale = scale;
    }

    public void PlayAnimation(Vector2 movementInout)
    {
        animator.SetBool("Running", movementInout.magnitude > 0);
    }

    public void PlayAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    public void PlayHurtAnimaiton()
    {
        animator.SetTrigger("Hurt");
    }

    public void PlayDeadAnimation()
    {
        animator.SetBool("Died", true);
    }

    public void AnimationHandler()
    {
        OnFinshWaitingForAnimation?.Invoke();
    }
}
