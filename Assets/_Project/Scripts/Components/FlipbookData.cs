using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

// 1. 我们把原有组件拆分
// 这个组件只负责由于游戏逻辑需要的动画数据（速度、总帧数、计时器）
public struct FlipbookData : IComponentData
{
    // 播放速度 (帧/秒)
    public float PlaySpeed;

    // 动画序列的总帧数
    public int TotalFrames;

    // 内部计时器，用于记录时间流逝
    public float Timer;
}

// 2. 这个组件专门用于传递给 Shader
// [MaterialProperty] 现在放在 struct 上面，并且参数是 shader 属性名
[MaterialProperty("_FrameIndex")] 
public struct FlipbookFrameIndex : IComponentData
{
    public float Value;
}