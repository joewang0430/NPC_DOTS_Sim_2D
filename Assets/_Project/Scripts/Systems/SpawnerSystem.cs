using Unity.Burst;
using Unity.Collections; // 用于 NativeArray
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnerSystem : ISystem
{
    // 我们只需要运行一次，所以用 RequireForUpdate 确保只有存在 SpawnerData 时才运行
    // 并且我们会在运行后删除 SpawnerData 组件，防止它下一帧又跑
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. 获取唯一的 Spawner 配置数据
        var spawnerEntity = SystemAPI.GetSingletonEntity<SpawnerData>();
        var spawnerData = SystemAPI.GetComponent<SpawnerData>(spawnerEntity);
        
        // 2. 及其高效地实例化 Entity
        // Instantiate 是并行友好的，速度极快
        var instances = state.EntityManager.Instantiate(spawnerData.PrefabToSpawn, spawnerData.Count, Allocator.Temp);
    
        // 3. 随机定位并设置 Z-as-Y
        var random = Random.CreateFromIndex(1234); // 固定的随机种子，保证每次测试结果一样

        // 我们遍历刚刚生成的所有 Entity
        foreach (var entity in instances)
        {
            // 随机坐标
            // nextFloat2 范围是 0~1，所以我们要减去 0.5 变成 -0.5~0.5，再乘区域
            float2 pos2D = (random.NextFloat2() - new float2(0.5f)) * spawnerData.SpawnArea;

            // Z-as-Y 核心逻辑：
            // Z = Y * 0.001f (系数越小越好，只要能区别深度即可)
            // 这样 Y 越大的单位 Z 越大（离相机越远，被遮挡），或者反过来取决于相机方向
            // 在 2D 中，通常 Y 越大越在上面（后面），所以 Z 应该大。
            // 假设相机看向 -Z 方向，Z 越大越远。
            float z = pos2D.y * 0.01f; 

            // 设置 LocalTransform
            var transform = LocalTransform.FromPosition(new float3(pos2D.x, pos2D.y, z));

            // 还可以随机一下初始动画时间，这样大家不会动作整齐划一
            float randomTimer = random.NextFloat(0, 100f);

            // 写入坐标
            state.EntityManager.SetComponentData(entity, transform);

            // 写入随机动画起始点（如果那个 Entity 有 FlipbookData 的话）
            // 这是一个稍微慢一点的写法，但对于初始化是可以接受的
            if (state.EntityManager.HasComponent<FlipbookData>(entity))
            {
                var flipbook = state.EntityManager.GetComponentData<FlipbookData>(entity);
                flipbook.Timer = randomTimer;
                state.EntityManager.SetComponentData(entity, flipbook);
            }
        }

        // 4. 打扫战场
        // 销毁这个 SpawnerData 组件，确保这个系统只跑这么一次
        state.EntityManager.DestroyEntity(spawnerEntity);
    }
}