using UnityEngine;

public static class PlayerAnimations
{
    public static readonly AnimationState Idle =
        new(Animator.StringToHash("Idle_PJ"), true);

    public static readonly AnimationState Run =
        new(Animator.StringToHash("Run_PJ"), true);

    public static readonly AnimationState Jump =
        new(Animator.StringToHash("Jump_PJ"), false);

    public static readonly AnimationState Attack =
        new(Animator.StringToHash("Attack_PJ"), false);

    public static readonly AnimationState Hurt =
        new(Animator.StringToHash("Hurt_PJ"), false);

    public static readonly AnimationState Climb =
        new(Animator.StringToHash("Climb_PJ"), true);

    public static readonly AnimationState Death =
        new(Animator.StringToHash("Death_PJ"), false);

    public static readonly AnimationState Fall =
        new(Animator.StringToHash("Fall_PJ"), false);
}
