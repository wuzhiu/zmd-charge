# EndfieldCharge · 终末地风格电量 HUD

插上 / 拔掉充电器时，从屏幕顶部弹出一块"灵动岛"式 HUD，显示当前电量（mWh 与百分比）。
视觉与动画风格复刻《终末地》工业 / 超充模式 HUD。

## Fork 说明

本仓库 Fork 自 [QinAnze/zmd-charge](https://github.com/QinAnze/zmd-charge)，保留上游的
Windows HUD、动画和设置功能，并在此基础上增加 Arch Linux x86_64 支持、Linux 电池读取、
托盘适配、XDG 登录自启、跨平台构建与测试。原程序作者署名 `Lenkmat` 保留在项目元数据中。

当前 Fork 的下载、问题反馈与更新发布均以
[wuzhiu/zmd-charge](https://github.com/wuzhiu/zmd-charge) 为准。

- **插电**：完整三态动画 —— 电标弹出 → 胶囊撑高成圆角矩形显示「超充模式」→ 收成圆胶囊显示电量 → 停留 → 整体缩小收回
- **拔电**：简化动画 —— 只弹电量圆胶囊，内容在胶囊完全出来后快速显现 → 停留 → 收回

## 下载与安装

从本仓库的 [Releases](https://github.com/wuzhiu/zmd-charge/releases/latest) 下载对应平台的文件。

### Windows 10 / 11（x64）

| 文件 | 安装方式 |
|------|----------|
| `EndfieldCharge-x.y.z-setup.exe` | 双击运行安装向导，可选桌面快捷方式和开机自启 |
| `EndfieldCharge-x.y.z-portable.zip` | 解压整个目录后运行 `EndfieldCharge.exe` |

Windows 包需要 [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)。
便携版中的原生库必须和 `EndfieldCharge.exe` 放在同一目录。

### Arch Linux（x86_64，首版）

下载 `EndfieldCharge-x.y.z-linux-x64.tar.gz`，然后执行：

```sh
sudo pacman -S --needed libx11 libice libsm fontconfig
# Wayland 会话还需要 XWayland：sudo pacman -S --needed xorg-xwayland

mkdir -p ~/.local/opt/endfield-charge ~/.local/bin
tar -xzf ~/Downloads/EndfieldCharge-*-linux-x64.tar.gz \
  -C ~/.local/opt/endfield-charge
chmod +x ~/.local/opt/endfield-charge/EndfieldCharge
ln -sfn ~/.local/opt/endfield-charge/EndfieldCharge \
  ~/.local/bin/endfield-charge

~/.local/bin/endfield-charge --demo  # 测试 HUD 动画
~/.local/bin/endfield-charge         # 正常启动并监听电源插拔
```

Linux 包已包含 .NET 运行时，无需另装 .NET。中文显示建议安装 `noto-fonts-cjk`。
KDE Plasma 托盘已实机验证；GNOME 需要支持 StatusNotifierItem/AppIndicator 的托盘扩展。
如果托盘不可见，可先退出已有进程，再用 `endfield-charge --settings` 打开设置。
安装到固定目录后，可在设置的「通用」页面启用登录自启。

Linux 版直接读取 `/sys/class/power_supply`，不需要 root 或 UPower，支持多电池以及
`energy_*` / `charge_*` 两类设备数据。当前通过 X11 / XWayland 运行；省电模式通知和
原生 Wayland 定位仍待完善。

## 功能

| 功能 | 说明 |
|------|------|
| 电量显示 | 剩余 / 满充容量（mWh，整数）与百分比，读取 `CallNtPowerInformation`，WMI 兜底 |
| 电源监听 | `RegisterPowerSettingNotification` 订阅 GUID_ACDC_POWER_SOURCE，2s 轮询兜底，400ms 双向去抖（过滤 Windows 满电瞬时抖动） |
| 低电量变色 | 电量 < 20% 时黄绿电量圈变红（#FF4D4F） |
| 提醒通知 | 低电量提醒（阈值可调 5–40%）与充满提醒（≥99%），卡牌风格弹窗，4s 自动消失 |
| 设置窗口 | 全局缩放（0.4–1.2）、显示时长（2–10s）、HUD 位置（顶部居中/靠右/靠左）、显示器选择、语言、开机自启，保存即生效并持久化 |
| 托盘菜单 | 左键单击弹出自定义深色菜单（预览 / 设置 / 检查更新 / 退出） |
| 动画微调 | 设置窗口「动画」页实时预览并微调时长 / 回弹 / 波纹参数，保存即生效并持久化 |
| 节能模式提示 | 开 / 关节能（省电）模式时弹出对应 HUD。24H2+（build 26100+）订阅 GUID_ENERGY_SAVER_STATUS 通知、轮询注册表 EnergySaverState；旧系统用 GUID_POWER_SAVING_STATUS + SystemStatusFlag。设置「通知」页可开关 |
| 检查更新 | 读取当前 Fork 的 GitHub Releases，比较程序集版本，一键跳转下载页 |
| 多语言 | 中文 / 英文，默认跟随系统，可在设置中手动切换 |
| 开机自启 | 设置窗口「通用」页开关，写 `HKCU\...\CurrentVersion\Run`（当前用户级，无需管理员） |
| 统一图标 | 托盘 / 各窗口 / exe / 安装器 / 卸载器统一使用 `Assets\tray_bolt` 图标 |
| 日志 | `%TEMP%\EndfieldCharge\log-YYYYMMDD.txt`，方便排查托盘菜单定位等问题 |

## 运行要求

- Windows：Windows 10 1809+ / Windows 11 x64，并安装 .NET 8 Desktop Runtime
- Arch Linux：x86_64 图形桌面及上述 X11 依赖；Wayland 会话通过 XWayland 运行

## 构建

```bash
# 在当前平台调试
dotnet build -c Debug

# Windows x64（需要目标机安装 .NET 8 Desktop Runtime）
dotnet publish -c Release -r win-x64 --self-contained false -o publish/windows-x64

# Linux x64（自包含运行时）
dotnet publish -c Release -r linux-x64 --self-contained true -o publish/linux-x64

# Windows 安装包（需安装 Inno Setup，iscc 在 PATH 中）
iscc installer\EndfieldCharge.iss
```

> 注意：`PublishSingleFile` 只把托管 dll 打进 exe，SkiaSharp 的 native dll
> （libSkiaSharp / libHarfBuzzSharp / av_libglesv2）仍需与 exe 同目录 ——
> 便携分发请打包整个 `publish/` 目录，不要只拷 exe。

### CI / 发布（GitHub Actions）

推送到 `main` 分支会分别运行 Windows 和 Linux 工作流，可从 Actions 下载构建产物。
推送 `v*` 标签（如 `v1.1.0`）会构建 Windows 安装包与便携包、创建 GitHub Release，
并把标签版本号写入程序集和文件名。Linux Release 包当前单独构建后上传：

```bash
git tag v1.1.0
git push origin v1.1.0
```

## 调试参数

启动时追加参数，无需真的插拔电源：

| 参数 | 作用 |
|------|------|
| `--demo` | 用示例数据播放一次**完整**动画（插电） |
| `--preview` | 用本机真实电池数据播放一次完整动画 |
| `--preview-unplug` | 用示例数据播放一次**简化**动画（拔电） |
| `--debug-ring` | 静态呈现状态 C（电量态）1.5s |
| `--power-log` | 输出电源事件日志到 `%TEMP%\power-log.txt` |
| `--settings` | 直接打开设置窗口，适合托盘不可见的 Linux 桌面 |

> 注意：这几个参数互斥，按 `--demo` → `--preview-unplug` → `--preview` 的优先级生效。

## 项目结构

```
EndfieldCharge/
├─ Animations/
│  └─ HudAnimations.cs      # 时间线与动画轨道（KeySpline 逐段缓动）
├─ Services/
│  ├─ AutoStart.cs          # 开机自启（HKCU Run 键读写）
│  ├─ BatteryService.cs     # 电池快照（剩余/满充 mWh、百分比、AC 状态）
│  ├─ Logger.cs             # 文件日志（%TEMP%\EndfieldCharge\）
│  ├─ PowerNative.cs        # P/Invoke：powrprof、message-only 窗口
│  ├─ PowerWatcher.cs       # 电源变化监听 + 去抖确认
│  ├─ Linux/                # Linux sysfs 电池读取、轮询监听与 XDG 自启
│  └─ UpdateChecker.cs      # GitHub Releases 更新检查
├─ Settings/
│  ├─ AppSettings.cs        # 设置模型（缩放/动画微调/位置/显示器/语言/提醒）
│  ├─ SettingsManager.cs    # 设置加载与持久化
│  ├─ SettingsWindow.axaml  # 设置窗口（通用 / 动画 / 通知 / 关于）
│  └─ SettingsWindow.axaml.cs
├─ Views/
│  ├─ HudWindow.axaml(.cs)  # HUD 视觉树（胶囊 / 电标 / 标题 / 数字 / 徽章 / 波纹）
│  └─ TrayMenuWindow.axaml(.cs)    # 左键自定义托盘菜单
├─ Styles/                  # 颜色主题与图标几何（StreamGeometry）
├─ Assets/                  # tray_bolt.png（运行时图标）+ tray_bolt.ico（exe/安装器图标）
├─ installer/
│  ├─ EndfieldCharge.iss    # Inno Setup 安装脚本
│  └─ Languages/            # 中文本地化（随仓库分发）
├─ tests/LinuxBatteryTests/ # Linux 电池后端模拟测试
└─ .github/workflows/       # Windows / Linux 独立构建工作流
```

## 动画实现要点

- Avalonia 11 的 `KeyFrame` 使用 **`KeySpline`（贝塞尔控制点）** 做逐段缓动，多关键帧下 `Animation.Easing` 不生效 —— 每段必须显式指定 `KeySpline`，否则该段为线性。
- `Border.HeightProperty`（即 `Layoutable.HeightProperty`）可直接动画，因此胶囊高度的 `60 → 90 → 60` 用独立轨道驱动。
- 收尾「整体缩小关没」由外层 `ScaleHost` 的 `RenderTransform` 统一缩放，胶囊本身宽度不动。

## 许可证

MIT
