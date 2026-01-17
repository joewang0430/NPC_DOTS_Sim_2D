# Project Specification: High-Performance 2D DOTS Flipbook System (Unity 2022.3 LTS)

## 1. 目标与背景 (Objective & Context)
本项目旨在 Unity 2022.3 LTS 环境下实现一个企业级的 2D 像素风格渲染与逻辑系统。核心目标是在保证 3000+ 动态单位同屏的前提下，维持极高的帧率（60+ FPS）与极低的 Draw Calls（目标值为 1-5 个）。

**核心限制：**
* **引擎版本**：必须限定在 Unity 2022.3 LTS（出于合规与稳定性考量）。
* **技术栈**：全量使用 DOTS (Entities 1.0+, Burst, Job System)。
* **动画方案**：严禁使用 VAT (Vertex Animation Texture)，必须使用 2D Flipbook (序列帧) 逻辑，通过纹理数组 (Texture2DArray) 实现。

---

## 2. 架构设计规范 (Architectural Specs)

### A. 渲染管线与组件 (Rendering & Components)
* **组件替代**：严禁使用传统的 `SpriteRenderer` 或 `GameObject`。所有单位必须作为 **Entities** 存在。
* **渲染载体**：使用 `MeshRenderer` 配合 Quad (四边形面片) 网格。
* **材质属性桥接**：利用 `BatchRendererGroup` (通过 `Entities Graphics` 自动实现)。必须通过 `[MaterialProperty]` 属性标签将 ECS 中的数据（如帧索引）同步至 GPU 常量缓冲区 (Constant Buffer)。
* **合批策略**：必须确保所有单位共享同一个材质球，利用 **Texture2DArray** 存储所有动画序列帧，避免因纹理切换导致的合批断裂。

### B. 动画驱动逻辑 (Animation System)
* **数据结构**：定义 `IComponentData` 存储动画状态（Timer, FrameIndex, Speed, TotalFrames）。
* **逻辑分发**：使用 `ISystem` 配合 `IJobEntity` 进行并行计算。
* **计算精度**：使用 `math.floor` 处理帧索引，并确保在 `Burst Compiler` 下运行以榨干 CPU 性能。

### C. 2D 空间排序 (2D Sorting & Culling)
* **深度策略**：由于不使用传统的 Sorting Layer，必须采用 **"Z-as-Y"** 方案。
* **实施细节**：在渲染位移系统（Transform System）中，动态将 `Position.z` 设置为 `Position.y * 0.001f`。
* **硬件加速**：Shader 必须开启 `Z-Test (LEqual)` 和 `Z-Write (On)`，利用硬件深度缓冲区 (Z-Buffer) 实现透明像素外的正确遮挡，消除 CPU 端的排序负担。

---

## 3. 给 AI 助手 (Copilot) 的实施指令 (Directives for Copilot)

> [!CAUTION]
> **请遵循以下 Unity 2022.3 LTS 的最优性能解法，严禁生成基于旧版 GameObject 的代码。**

### 1. 编写 Shader Graph 指令
* 创建一个 URP Shader Graph。
* 使用 `Sample Texture 2D Array` 节点。
* 定义 `_FrameIndex` 属性，并在其 Reference 设置中启用 **"Support DOTS Instancing"**。
* 确保材质类型设置为 **Opaque** 或 **Alpha Clipping**（像素风格优先选 Clipping），以获得最佳 Overdraw 性能。

### 2. 定义数据组件 (Data Components)
* 编写符合 ECS 规范的 `struct`，必须包含位置、移动向量、动画帧索引。
* 为需要传递给 Shader 的字段添加 `[MaterialProperty("_PropertyName")]` 属性标签。

### 3. 实现高效系统 (Systems)
* 编写 `ISystem`。
* 在 `OnUpdate` 中通过 `SystemAPI.Query` 驱动逻辑。
* 动画更新必须使用 `delta时间` 同步，确保帧率无关性。

### 4. 实例化与烘焙 (Baking & Spawning)
* 编写 `Baker` 脚本，将 Prefab 转换为 Entities，并正确附加 `MaterialMeshInfo` 和自定义数据。
* 编写一个专门的 Spawner 系统，在 `OnCreate` 时使用 `EntityManager.Instantiate` 进行内存预分配并瞬间生成 3000+ 个实体。

---

## 4. 性能验证指标 (Validation Criteria)
* **Draw Calls**: 3000+ 单位同屏时，Draw Calls 必须小于 10。
* **Batching**: 在 Frame Debugger 中，所有渲染必须显示为 "SRP Batch" 或 "GPU Instancing"。
* **Main Thread**: CPU 主线程耗时应主要集中在 `JobHandle.Complete`，确保逻辑完全并入后台 Worker Threads。

---

**Next Step for Copilot:**
请根据以上规范，首先编写核心的渲染组件（Component）与对应的 Baker 脚本，确保渲染属性与 Shader 的对齐。