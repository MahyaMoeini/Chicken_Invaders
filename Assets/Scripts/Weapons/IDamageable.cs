/// <summary>
/// Anything that can receive combat damage (chickens, eggs, asteroids, future bosses).
/// </summary>
public interface IDamageable
{
    bool IsAlive { get; }
    bool IsBoss { get; }
    float MaxHealth { get; }
    float CurrentHealth { get; }

    /// <summary>
    /// Apply damage once. Implementations must ignore calls after death.
    /// </summary>
    void ApplyDamage(float amount);
}
