<div align="center">

# TS6 Speaker Overlay

**为 TeamSpeak 6 打造，轻量、高性能的信息叠加层。**

A lightweight, high-performance voice overlay tool for TeamSpeak 6.

[![GitHub Release](https://img.shields.io/github/v/release/Ruikoto/TS6-SpeakerOverlay?style=for-the-badge&label=Download&color=%2379a206)](https://github.com/Ruikoto/TS6-SpeakerOverlay/releases)

[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)]()
[![License](https://img.shields.io/badge/License-MIT-green.svg)]()

---

### 🎯 Original Author

**[beka2nt](https://github.com/beka2nt)** - [Original Repository](https://github.com/beka2nt/TS6-SpeakerOverlay)

---

<!-- 语言切换 / Language Switch -->
<p align="center">
  <a href="#chinese">
    <img src="https://img.shields.io/badge/语言-中文-red?style=flat-square" alt="Chinese">
  </a>
  <a href="#english">
    <img src="https://img.shields.io/badge/Language-English-blue?style=flat-square" alt="English">
  </a>
</p>

</div>

---

<a id="chinese"></a>

## 📖 中文

### 📸 界面预览

<table>
  <tr align="Center">
    <td><img src="Assets/Screenshot1.png" alt="Screenshot 1"/></td>
  </tr>
  <tr align="Center">
    <td><img src="Assets/Screenshot2.png" alt="Screenshot 2"/></td>
  </tr>
  <tr align="Center">
    <td><img src="Assets/Screenshot3.png" alt="Screenshot 3"/></td>
  </tr>
</table>

### ✨ 核心功能

- **🖱️ 鼠标穿透**：点击穿透至游戏，零干扰
- **⚡ 极低占用**：基于 .NET 10 Native AOT，内存占用极低
- **🎤 智能声控**：说话时高亮，静默时半透明
- **🔒 隐私保护**：仅显示当前频道成员
- **🔄 自动连接**：首次授权后自动重连

### 🚀 快速开始

1. **下载** [最新版本](https://github.com/Ruikoto/TS6-SpeakerOverlay/releases)
2. **运行** `TS6-SpeakerOverlay.exe`，在 TS6 弹窗中点击**"允许"**
3. **拖拽** 黑色背景调整位置
4. **锁定** 按 `Ctrl + L` 启用鼠标穿透
5. **退出** 通过托盘图标或退出按钮

### 📝 注意事项

- 程序自动在 `%APPDATA%\TS6-SpeakerOverlay` 目录生成 `apikey.txt` 保存授权，请勿分享
- 若授权失败，可删除 `apikey.txt` 文件后重新授权

### 📄 开源协议

MIT License

---
<a id="english"></a>

## 📖 English

### 📸 Preview

<table>
  <tr align="Center">
    <td><img src="Assets/Screenshot1.png" alt="Screenshot 1"/></td>
  </tr>
  <tr align="Center">
    <td><img src="Assets/Screenshot2.png" alt="Screenshot 2"/></td>
  </tr>
  <tr align="Center">
    <td><img src="Assets/Screenshot3.png" alt="Screenshot 3"/></td>
  </tr>
</table>

### ✨ Key Features

- **🖱️ True Click-Through**: Mouse clicks pass through to the game - zero interference
- **⚡ Ultra-Low Resources**: Built with .NET 10 Native AOT - minimal memory footprint
- **🎤 Voice Activation**: Auto-highlights speakers, dims when silent
- **🔒 Privacy Focused**: Shows only your current channel members
- **🔄 Auto-Connect**: Saves API Key after first auth - seamless reconnection

### 🚀 Quick Start

1. **Download** the latest release from [Releases](https://github.com/Ruikoto/TS6-SpeakerOverlay/releases)
2. **Run** `TS6-SpeakerOverlay.exe` and click **"Allow"** in TS6 prompt
3. **Drag** the overlay to position (click black background)
4. **Lock** with `Ctrl + L` to enable click-through
5. **Exit** via tray icon or Exit button

### 📝 Notes

- `apikey.txt` is auto-generated in `%APPDATA%\TS6-SpeakerOverlay` for authorization - keep it private
- If authorization fails, delete the `apikey.txt` file and re-authorize

### 📄 License

MIT License
