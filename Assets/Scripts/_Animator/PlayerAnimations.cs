using UnityEngine;

public static class PlayerAnimations
{
    public static readonly int Idle =
        Animator.StringToHash("Idle_PJ");

    public static readonly int Run =
        Animator.StringToHash("Run_PJ");

    public static readonly int Jump =
        Animator.StringToHash("Jump_PJ");

    public static readonly int Attack =
        Animator.StringToHash("Attack_PJ");

    public static readonly int Hurt =
        Animator.StringToHash("Hurt_PJ");

    public static readonly int Climb =
    Animator.StringToHash("Climb_PJ");

    public static readonly int Fall =
    Animator.StringToHash("Fall_PJ");
}
