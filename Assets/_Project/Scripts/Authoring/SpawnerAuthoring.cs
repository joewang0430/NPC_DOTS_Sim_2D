using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject PrefabToSpawn; // 拖入刚才做好的 UnitPrefab
    public int Count = 3000;         // 目标数量
    public Vector2 SpawnArea = new Vector2(100, 100); // 100x100米的范围

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SpawnerData
            {
                // GetEntity 会自动把 GameObject Prefab 转换成 Entity Prefab
                PrefabToSpawn = GetEntity(authoring.PrefabToSpawn, TransformUsageFlags.Dynamic),
                Count = authoring.Count,
                SpawnArea = authoring.SpawnArea
            });
        }
    }
}