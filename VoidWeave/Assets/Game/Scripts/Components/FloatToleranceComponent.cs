namespace Game.Scripts.Components
{
    using Unity.Entities;
    
    /// <summary>
    /// Defines the acceptable margin of error (epsilon) when comparing floating-point numbers.
    /// Due to floating-point imprecision, exact equality checks (==) often fail. 
    /// Use this value in systems via math.abs(a - b) > Value to safely determine if two floats are meaningfully different.
    /// </summary>

    public struct FloatToleranceComponent : IComponentData
    {
        public float Value;
    }
}