# URP-12-Extension-Fixed

基于 [killop/URP-12-Extension](https://github.com/killop/URP-12-Extension) 的优化版本。

**环境**：URP 12.1.10 嵌入式包 / Unity 2021.3.x（开发验证于 2021.3.45f2）

## 这是什么

在原版「场景/UI 分辨率分离 + swap buffer + Gamma 矫正」管线魔改的基础上，修复了原版与后续开发中发现的十余处缺陷。

原版三大特性：

- **场景/UI 分辨率分离**（`sUISplitEnable`）：场景按 renderScale 降分辨率渲染，UI 保持全分辨率
- **UI 相机独立 swap buffer 合成**（`sEnableUICameraUseSwapBuffer`）
- **Gamma 矫正框架**（`sIsGammaCorrectEnable`）：UI Shader 经 `_IsInUICamera` 在 Gamma 空间混合

## 目录结构与安装

| 目录 / 文件                                           | 放到工程的哪里                                                | 说明                                                                   |
| ------------------------------------------------- | ------------------------------------------------------ | -------------------------------------------------------------------- |
| `com.unity.render-pipelines.universal@12.1.10/`   | `Packages/`                                            | **必需**                                                               |
| `com.happyelements.adaptiveperformance/`          | `Packages/`                                            | **必需**                                                               |
| `timeline-volume/`                                | `Assets/`                                              | Timeline 驱动 Volume 的轨道（4 个脚本），可选                                     |
| `shaders/URP-UI-Default.shader` + `UI_Pixel.hlsl` | `Assets/` 下任意目录                                        | **两个文件必须放在同一目录，建议手动替换项目的默认UI材质**                                     |
| `shaders/TMP_SDF-Mobile.shader`                   | 覆盖 `Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader` | ⚠️ **只覆盖 .shader 文件，保留你工程里原有的 .meta**（meta 的 GUID 被 TMP 默认材质引用，换了会断） |

**PS． manifest.json里的URP要删除，否则会被官方版本覆盖，导入包无用**

## 使用

三个核心开关（静态变量，可在代码/启动时设置）：

```csharp
UniversalRenderPipeline.sUISplitEnable               // 场景降分辨率、UI 全分辨率
UniversalRenderPipeline.sIsGammaCorrectEnable        // UI Gamma 矫正（依赖 swap buffer）
UniversalRenderPipeline.sEnableUICameraUseSwapBuffer // UI 相机走 swap buffer（开后处理需要）
```

- **推荐形态**：split 开 + swap 关 —— FXAA / FSR / 上采样都能正确作用于场景，UI 全分辨率；
- **FSR** 只在 swap 关时生效（swap 开时由 BlitPass 承担上采样，退化为硬件 Bilinear）；
- **NN/Bilinear** 两种模式都生效；`Auto` 整数倍缩放解析为 Point ；
- **Gamma 矫正**只在 swap 开时执行；

## 修改清单（相对 killop 原版）

**1. 后处理归属：整个相机栈只执行一次**
新增 `CameraData.nextCameraHasPostProcessing`，基底相机按栈中下一台相机（UICamera）的后处理开关三分支决定行为，消除 Bloom/调色被叠加两次或整体丢失的问题。

**2. Gamma 矫正链路修复**
FinalBlit 双重编码泛白 / 非 swap 模式 UI 发黑发白 / UI 相机开后处理整屏泛白；

**3. FXAA 修复**
FXAA 在「split 开 + swap 开」下正确作用于场景；

**4. renderScale 修复**
`BlitPass.Setup` 原硬编码 `Screen.width/height` 导致帧中途反复重建 swap buffer + depth、renderScale 静默作废、帧时间暴涨。三处改用 `cameraTargetDescriptor` ⇒ 帧内零重建。

**5. 上采样器接线：NN / Bilinear / FSR**
NN 在 swap 开（BlitPass `FilterMode.Point`）与 swap 关（PostProcessPass PointSampling）两模式都正确生效；FSR** 只在 swap 关时生效（swap 开时由 BlitPass 承担上采样，退化为硬件 Bilinear）；

**6. SceneLight（RenderSettings 覆盖器）修复**
修复 cubemap 为 null 时全场反射全黑，改为无 cubemap 时退回 Skybox； Lighting 面板恢复可用；删除原版 11 个零消费者死参数（`my*` 系列）。

**7. 游戏内设置面板整改**：FSR 恢复可选并提示实际生效路径；FXAA/SMAA 提示修正（Overlay 相机继承基底 AA）；MSAA 行隐藏；枚举行支持条件置灰。

## 已知约束

- MSAA 必须保持 Disabled（URP Asset `m_MSAA: 1`），或仅在RenderScale ！= 1时使用
- 非 swap buffer 模式下 Gamma 矫正不可用（实现依赖 swap buffer 合成路径）。
- **FSR 未支持**（源版即未放行，选项会退化为 Bilinear）
- 渲染到 RT / XR 未适配（`BlitPass` 等处仍按屏幕尺寸处理）。
- TMP 仅覆盖 `TMP_SDF-Mobile` 一个 shader；Overlay / SSD 等 variant 未接 Gamma 链路。

## 致谢

- [killop/URP-12-Extension](https://github.com/killop/URP-12-Extension) —— 本项目的基底
- [URP-12-GammaUIAndSplitResolution](https://github.com/killop/URP-12-GammaUIAndSplitResolution) —— Gamma UI / 分辨率分离思路来源
- [X-PostProcessing-URP](https://github.com/tkonexhh/X-PostProcessing-URP) —— 原版集成的 70+ 后处理效果
