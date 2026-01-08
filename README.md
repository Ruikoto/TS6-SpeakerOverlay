<div align="center">

# TS6 Speaker Overlay

**A lightweight, high-performance voice overlay tool for TeamSpeak 6.**

<!-- 下载按钮 -->
[![Download Latest](https://img.shields.io/github/v/release/beka2nt/TS6-SpeakerOverlay?label=Download%20EXE&style=for-the-badge&color=orange)](https://github.com/beka2nt/TS6-SpeakerOverlay/releases/latest)

[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)]()
[![License](https://img.shields.io/badge/License-MIT-green.svg)]()

<p align="center">
  <a href="#english">
    <img src="https://img.shields.io/badge/Language-English-blue?style=flat-square" alt="English">
  </a>
  <a href="#chinese">
    <img src="https://img.shields.io/badge/语言-中文-red?style=flat-square" alt="Chinese">
  </a>
</p>

</div>

---

<a id="english"></a>

##  English Description

###  Key Features (v1.1)

- **True Click-Through**: The overlay acts like "air". Mouse clicks pass directly through to the game.
- **System Tray Support**: Minimizes to the system tray. Right-click to Lock/Unlock or Exit.
- **Visual Notifications**: Shows a popup bubble when users join or leave your channel.
- **Status Indicators**: New vector icons for Input Muted (Mic), Output Muted (Sound), and Away status.
- **Ultra-Low Resource**: Built with **.NET 10 Native AOT**. Minimal memory footprint.
- **Seamless Auto-Connect**: Auto-saves API Key and reconnects silently.

###  How to Use

1. **Download**: Click the **Download EXE** badge above.
2. **Run**: Run `TS6-SpeakerOverlay.exe`. (Allow connection in TS6 client for the first time).
3. **Controls**:
   - **Unlock**: Default state. Drag to move.
   - **Lock**: Press **`Ctrl + L`** or use the Tray Icon menu.
   - **Tray**: Closing the window minimizes it to the tray. Right-click the tray icon to exit fully.

###  License
MIT License

---

<a id="chinese"></a>

##  中文说明 (Chinese)

###  核心功能 (v1.1 更新)

- ** 真正的鼠标穿透**：悬浮窗如同空气一般，鼠标点击直接穿透至下方游戏，绝不干扰操作。
- ** 托盘最小化**：点击关闭按钮不再退出，而是隐藏到系统托盘。右键托盘图标可快速锁定/解锁。
- ** 进出频道通知**：当有人进入或离开你的频道时，顶部会弹出渐入渐出的气泡提示。
- ** 详细状态显示**：全新的矢量图标，实时显示成员的 **闭麦、静音、离开** 状态。
- ** 极低资源占用**：基于 **.NET 10** 编译，无浏览器内核，内存占用极低。
- ** 自动无感连接**：首次授权后自动保存 Key，后续启动自动重连 TeamSpeak 6。

###  如何使用

1. **下载程序**：点击顶部的 **Download EXE** 按钮下载最新版本。
2. **首次运行**：双击运行。TS6 客户端会弹出请求，请点击 **"允许 (Allow)"**。
3. **操作说明**：
   - **[解锁模式]**：启动后默认为解锁状态，拖拽黑色背景调整位置。
   - **[锁定模式]**：按下 **`Ctrl + L`** 或在托盘图标右键选择锁定。
   - **[退出程序]**：点击窗口右上角的 X 会最小化到托盘。要彻底退出，请在托盘图标上右键选择退出程序。

###  注意事项
- 程序会在目录下生成 `apikey.txt` 保存授权信息，请勿分享给他人。
- 建议以 **管理员身份运行** 以确保在《猎杀：对决》等 EAC 反作弊游戏中正常置顶。

###  开源协议
本项目基于 MIT License 开源。
