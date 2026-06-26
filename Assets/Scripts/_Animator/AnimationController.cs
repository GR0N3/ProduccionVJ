using System;
using UnityEngine;


//DEPRECATED
[Obsolete("Old animation controller - Not Working")]
public class AnimationController
{
    private Animator animator;

    private AnimationState current = PlayerAnimations.Idle;

    public AnimationState Current => current;

    public void Init(Animator animator)
    {
        this.animator = animator;
    }

    public bool Play(AnimationState next, float fade = 0.1f, bool force = false)
    {
        //if (!force && next.Priority < current.Priority)
        //{
        //    return false;
        //}
        //if (current.Hash == next.Hash) 
        //{
        //    return false;
        //}
        //current = next;
        //animator.CrossFade(next.Hash, fade, 0);
        return true;
    
    }

    public bool IsPlaying(int animation)
    {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash== animation;
    }

    public void Stop()
    {
        animator.speed = 0;
    }

    public void Resume()
    {
        animator.speed = 1;
    }

    public void SetBool(string parameter, bool value)
    {
        animator.SetBool(parameter, value);
    }

    public void SetFloat(string parameter, float value)
    {
        animator.SetFloat(parameter, value);
    }

    public void Trigger(string parameter)
    {
        animator.SetTrigger(parameter);
    }

    public void ResetTrigger(string parameter)
    {
        animator.ResetTrigger(parameter);
    }

}