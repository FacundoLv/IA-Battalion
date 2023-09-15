public class BoostData
{
    public float Modifier { get; }
    public float Duration { get; }
    
    public BoostData(float duration, float modifier)
    {
        Duration = duration;
        Modifier = modifier;
    }
}