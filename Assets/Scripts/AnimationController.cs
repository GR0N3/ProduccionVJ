using UnityEngine;
using UnityEngine.Rendering;
//https://www.youtube.com/watch?v=I3_i-x9nCjs tutorial
public class AnimationController
{
    Animator animator;
    string currentAnimation;

    void SetAnimator(Animator animator) 
    {
        this.animator = animator;
    }

    void ChangeAnimtaion(string animation, float crossFade) 
    {
        if (currentAnimation != animation) 
        {
            currentAnimation = animation;
            animator.CrossFade(animation, crossFade);
        }
    }

    void StopAnimation(string animation) 
    {
        
    }
}
