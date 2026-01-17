using Unity.Entities;
using UnityEngine;

public class FlipbookAuthoring : MonoBehaviour
{
    [Header("Animation Settings")]
    public float PlaySpeed = 12f;
    public int TotalFrames = 6;

    class Baker : Baker<FlipbookAuthoring>
    {
        public override void Bake(FlipbookAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // 添加逻辑数据组件
            AddComponent(entity, new FlipbookData
            {
                PlaySpeed = authoring.PlaySpeed,
                TotalFrames = authoring.TotalFrames,
                Timer = 0
            });

            // 添加渲染数据组件
            // 初始帧设为 0
            AddComponent(entity, new FlipbookFrameIndex
            {
                Value = 0
            });
        }
    }
}