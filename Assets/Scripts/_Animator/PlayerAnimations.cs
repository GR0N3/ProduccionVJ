using UnityEngine;

public static class PlayerAnimations
{
    public static readonly AnimationState Idle =
        new(Animator.StringToHash("Idle_PJ"), 0);

    public static readonly AnimationState Run =
        new(Animator.StringToHash("Run_PJ"), 10);

    public static readonly AnimationState Jump =
        new(Animator.StringToHash("Jump_PJ"), 20);

    public static readonly AnimationState Attack =
        new(Animator.StringToHash("Attack_PJ"), 50);

    public static readonly AnimationState Hurt =
        new(Animator.StringToHash("Hurt_PJ"), 80);

    public static readonly AnimationState Climb =
        new(Animator.StringToHash("Climb_PJ"), 70);

    public static readonly AnimationState Death =
        new(Animator.StringToHash("Death_PJ"), 100);
}
