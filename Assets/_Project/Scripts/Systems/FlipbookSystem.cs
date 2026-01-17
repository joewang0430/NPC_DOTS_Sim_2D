using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

// UpdateInGroup 确保这个系统在正常的模拟循环中运行
[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile] // 开启 Burst 编译，这对性能至关重要！
public partial struct FlipbookSystem : ISystem
{
    // 系统初始化时调用（我们暂时不需要初始化什么）
    public void OnCreate(ref SystemState state)
    {
        // 这里可以根据需要开启 ISystem 的回调需求，现在留空即可
    }

    // 系统销毁时调用
    public void OnDestroy(ref SystemState state)
    {
    }

    // 每一帧调用
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 获取每一帧流逝的时间 (Delta Time)
        float deltaTime = SystemAPI.Time.DeltaTime;

        // 使用 Job System 并行处理所有拥有 FlipbookData 和 FlipbookFrameIndex 的实体
        // 这里的 state.Dependency 用于自动管理依赖关系
        state.Dependency = new FlipbookJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel(state.Dependency);
    }

    // 定义这具体的 Job 工作内容
    // IJobEntity 是最方便的写法，它会自动为每个匹配的实体执行 Execute
    [BurstCompile]
    public partial struct FlipbookJob : IJobEntity
    {
        public float DeltaTime;

        // ref 表示我们要修改这个组件的数据
        // in 表示我们只读取（虽然这里PlaySpeed在struct里也是值类型，但习惯上没变的数据我不加ref）
        // 注意：Execute 的参数名和顺序不重要，重要的是类型。Unity 会自动注入组件。
        void Execute(ref FlipbookData data, ref FlipbookFrameIndex frameIndex)
        {
            // 1. 累加计时器
            data.Timer += DeltaTime * data.PlaySpeed;

            // 2. 根据总帧数取模，算出当前的【逻辑帧号】(比如 5.123 帧)
            // math.fmod 类似于 % 运算符，但对浮点数更友好
            float currentFrame = math.fmod(data.Timer, data.TotalFrames);

            // 3. 取整，得到【渲染帧号】(比如 第5帧)
            // math.floor 向下取整
            float floorFrame = math.floor(currentFrame);

            // 4. 将计算结果写入到材质属性组件中
            // 这个值不仅会被保存，还会被 Entities Graphics 自动上传到 GPU
            frameIndex.Value = floorFrame;

        }
    }
}