<div align="center">

# TS6 Speaker Overlay

**A lightweight, high-performance voice overlay tool for TeamSpeak 6.**

<!-- Downloads -->
[![Download Latest](https://img.shields.io/github/v/release/beka2nt/TS6-SpeakerOverlay?label=Download%20EXE&style=for-the-badge&color=orange)](https://github.com/beka2nt/TS6-SpeakerOverlay/releases/latest)

<!-- Status -->
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

## 🇬🇧 English Description

### ✨ Key Features

- **Mouse Click-Through**: Implements low-level Windows API to allow mouse events to pass directly to the game application, ensuring uninterrupted gameplay.
- **System Tray Integration**: Supports minimizing to the system tray. Includes a context menu for quick state management (Lock/Unlock/Exit).
- **Event Notifications**: Visual toast notifications provide immediate feedback when users join or leave the current channel.
- **Status Visualization**: Utilizes vector icons to display real-time statuses including Talking, Input Muted, Output Muted, and Away.
- **Native AOT Architecture**: Compiled with **.NET 10 Native AOT**, eliminating the need for heavy browser engines (Electron) and optimizing memory usage.
- **Auto-Connection**: Automatically persists authorization credentials locally to establish a connection with the TeamSpeak 6 client upon startup.

### 📦 How to Use

1. **Download**: Click the **Download EXE** badge above or visit the [Releases](https://github.com/beka2nt/TS6-SpeakerOverlay/releases/latest) page.
2. **Launch**: Run `TS6-SpeakerOverlay.exe`. A connection request will appear in the TeamSpeak 6 client; click **"Allow"**.
3. **Controls**:
   - **Unlock Mode**: Default state. Allows window positioning via drag-and-drop.
   - **Lock Mode**: Press **`Ctrl + L`** or use the tray menu to lock the window position and enable click-through mode.
   - **Exit**: Right-click the system tray icon and select "Exit".

### 📄 License
MIT License

---

<a id="chinese"></a>

## 🇨🇳 中文说明 (Chinese)

### ✨ 核心功能

- **鼠标事件穿透**：通过 Windows API 实现窗口透明与点击穿透，确保覆盖层在游戏运行时不拦截鼠标指令，维持正常游戏操作。
- **系统托盘集成**：支持最小化至系统托盘运行。提供右键菜单，可快速切换锁定状态或退出程序。
- **频道动态通知**：内置非阻塞式通知系统，当成员进入或离开当前频道时，提供视觉反馈。
- **多状态可视化**：采用矢量图标实时显示成员状态，包括正在说话、麦克风禁用、声音禁用及离开状态。
- **原生 AOT 编译**：基于 **.NET 10** 构建，不依赖 Electron 等 Web 容器，显著降低内存占用与启动时间。
- **自动连接管理**：首次授权后自动在本地保存 API 凭证，后续启动将自动连接至 TeamSpeak 6 客户端。

### 📦 使用指南

1. **获取程序**：点击顶部的 **Download EXE** 按钮下载最新版本可执行文件。
2. **首次配置**：运行程序后，TeamSpeak 6 客户端将弹出连接请求，请点击 **"允许 (Allow)"**。
3. **操作交互**：
   - **[调整位置]**：程序启动时默认为解锁状态，可拖拽黑色背景区域调整显示位置。
   - **[锁定模式]**：位置调整完毕后，按下 **`Ctrl + L`** 或在托盘菜单选择“锁定”。此时窗口将固定并开启鼠标穿透。
   - **[退出程序]**：关闭窗口将默认最小化至托盘。如需彻底退出，请在托盘图标右键菜单中选择“退出程序”。

### ⚠️ 注意事项
- 程序运行后会在同级目录下生成 `apikey.txt` 用于存储授权信息，请妥善保管，勿发送给他人。
- 若在移动 EXE 文件后无法自动连接，请删除旧的 `apikey.txt` 并重新进行授权流程。
- 建议以 **管理员身份运行**，以确保在部分启用反作弊系统（如 EAC）的游戏中能正常置顶显示。

### 📄 开源协议
本项目基于 MIT License 开源。
