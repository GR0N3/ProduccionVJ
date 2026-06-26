using UnityEngine;

// TO FIX :
// LOOP FROM ANIMATIONSTAE DOESN´T WORK

public class AnimatorBrain
{
    private Animator animator;

    private AnimationState current;

    private bool locked;

    private AnimationState idleAnimation;

    public AnimationState Current => current;

    public void Init(Animator animator)
    {
        this.animator = animator;
        animator.Play(idleAnimation.Hash);
    }
    public void SetIdle(AnimationState idle)
    {
        idleAnimation = idle;
    }

    public bool Play(AnimationState next, bool lockAnimation = false, bool overrideLock = false, float fade = 0.1f, float? speed = null )
    {
        if (locked && !overrideLock)
        {
            return false;
        }

        if (current.Hash == next.Hash)
        {
            return false;
        }

        current = next;

        locked = lockAnimation;

        animator.speed = speed ?? next.Speed;

        animator.CrossFade(next.Hash, fade, 0);

        return true;
    }

    public void Unlock()
    {
        locked = false;
    }

    public bool IsLocked()
    {
        return locked;
    }

    public bool IsPlaying(AnimationState state)
    {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == state.Hash;
    }

    public bool Finished()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        return info.shortNameHash == current.Hash && info.normalizedTime >= 1f;
    }

    public void SetPlaybackSpeed(float speed)
    {
        animator.speed = speed;
    }

    public void ResetPlaybackSpeed()
    {
        animator.speed = 1f;
    }

    public void Tick()
    {
        if (locked && Finished())
        {
            locked = false;

            if (!current.Loop)
            {
                Play(idleAnimation);
            }
        }
    }
}