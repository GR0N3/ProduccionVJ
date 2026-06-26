public readonly struct AnimationState
{
    public readonly int Hash;
    public readonly bool Loop;
    public readonly float Speed;

    public AnimationState(int hash, bool loop = false, float speed = 1f)
    {
        Hash = hash;
        Loop = loop;
        Speed = speed;
    }
}