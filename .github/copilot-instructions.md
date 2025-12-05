# TopNotify - AI Coding Agent Instructions

## Architecture Overview

TopNotify is a Windows notification customization tool with a **dual-mode architecture**:

- **Daemon Mode** (default): Background process that intercepts and repositions Windows notifications using WinAPI hooks
- **Settings Mode** (`--settings` flag): GUI for configuration, built with [IgniteView](https://github.com/SamsidParty/IgniteView) (WebView2 + React)

The same `TopNotify.exe` serves both modes - the entry point in `Program.cs` determines which mode to launch based on command-line args and existing instances.

### Key Components

| Component | Purpose |
|-----------|---------|
| `Daemon/InterceptorManager.cs` | Main loop coordinating all interceptors, handles notification events |
| `Daemon/Interceptors/*.cs` | Modular notification handlers (position, sound, discovery, read-aloud) |
| `TopNotify.Native/` | C++ DLL for low-level window enumeration (`NtUserBuildHwndList`) |
| `src-vite/` | React + Chakra UI frontend, communicates with C# via IgniteView commands |
| `Common/Settings.cs` | JSON settings stored in `%LocalAppData%\SamsidParty\TopNotify\` |

### IPC Pattern

GUI ↔ Daemon communication uses **MailSlots** (`samsidparty_topnotify`). When settings change:
1. GUI calls `Daemon.SendCommandToDaemon("UpdateConfig")`
2. Daemon's `MailSlotListener` receives message and calls `OnSettingsChanged()`

## Build & Development

### Prerequisites
- .NET 9.0 SDK with Windows 10.0.17763.0 targeting pack
- Node.js (for Vite frontend)
- Visual Studio 2022 with C++ desktop workload (for native DLL)

### Building

```powershell
# Frontend (runs automatically via MSBuild pre/post-build hooks)
cd TopNotify/src-vite
npm install
npm run build  # Outputs to TopNotify/dist/

# Full build
dotnet build TopNotify/TopNotify.csproj -c Release

# For ARM64
dotnet build TopNotify/TopNotify.csproj -c Release -r win-arm64
```

The project uses IgniteView's remote build scripts that run automatically during MSBuild - see `<PreBuildCommand>` in `.csproj`.

### Debugging

- **GUI Debug configuration**: Skips daemon logic, always opens GUI (`#if GUI_DEBUG`)
- Launch with `--settings` to test GUI while daemon runs separately
- Logs written to `%LocalAppData%\SamsidParty\TopNotify\{daemon|gui}.log`

## Code Patterns

### Interceptor Pattern

Create new notification handlers by extending `Interceptor`:

```csharp
public class MyInterceptor : Interceptor
{
    public override bool ShouldEnable() => Settings.SomeFeatureEnabled;
    public override void OnNotification(UserNotification notification) { /* handle */ }
    public override void Update() { /* called every 10ms */ }
    public override void Reflow() { /* called every ~500ms for rediscovery */ }
}
```

Register in `InterceptorManager.InstalledInterceptors` array.

### IgniteView Commands

C# ↔ JavaScript bridge uses `[Command]` attributes:

```csharp
// C# (GUI/MainCommands.cs)
[Command("SpawnTestNotification")]
public static void SpawnTestNotification() { /* ... */ }
```

```javascript
// JavaScript
igniteView.commandBridge.SpawnTestNotification();
```

### Settings Access

- **From Interceptors**: Use `this.Settings` property (auto-refreshed)
- **From GUI**: `Settings.Get()` returns deserialized JSON
- **For IPC**: `Settings.GetForIPC()` includes dynamic fields (`__ScreenWidth`, etc.)

## Platform Considerations

- **MSIX packaging required** for `UserNotificationListener` - throws COM exception if running unpackaged
- WinForms loaded via reflection (`TrayIcon.cs`) to avoid direct reference
- Native DLL handles window enumeration because `EnumWindows` misses UWP/immersive windows
- Sound interception modifies registry (`HKCU\AppEvents\Schemes\Apps\.Default`) via `cmd.exe` due to MSIX sandbox

## File Locations

- Settings: `%LocalAppData%\SamsidParty\TopNotify\Settings.json`
- Custom sounds: `%LocalAppData%\SamsidParty\TopNotify\audio\`
- Frontend assets: `TopNotify/src-vite/public/` → built to `TopNotify/dist/`
