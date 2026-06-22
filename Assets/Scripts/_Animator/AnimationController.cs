using UnityEngine;

public class AnimationController
{
    private Animator animator;

    private int currentAnimation = -1;

    public void Init(Animator animator)
    {
        this.animator = animator;
    }

    public void Play(int animation, float crossFade = 0.1f)
    {
        if (currentAnimation == animation)
            return;

        currentAnimation = animation;

        animator.CrossFade(animation,crossFade);
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