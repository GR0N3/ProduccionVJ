using UnityEngine;

public class AnimationState
{
    public int Hash;
    public int Priority;

    public AnimationState(int hash, int priority)
    {
        Hash = hash;
        Priority = priority;
    }
}
