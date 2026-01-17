using Unity.Entities;
using Unity.Mathematics;

public struct SpawnerData : IComponentData
{
    // 我们要生成的那个“原型”实体
    public Entity PrefabToSpawn;
    
    // 生成数量
    public int Count;
    
    // 生成范围（我们会随机撒在这个范围内）
    public float2 SpawnArea;
}