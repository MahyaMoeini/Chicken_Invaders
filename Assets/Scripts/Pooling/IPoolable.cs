/// <summary>
/// Optional reset hook for pooled objects.
/// Implement on prefab components that need cleanup before reuse.
/// </summary>
public interface IPoolable
{
    void OnSpawnFromPool();
    void OnReturnToPool();
}
