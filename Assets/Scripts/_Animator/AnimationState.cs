public readonly struct AnimationState
{
    public readonly int Hash;
    public readonly bool Loop;

    public AnimationState(int hash, bool loop = false)
    {
        Hash = hash;
        Loop = loop;
    }
}