# Plan: Resolve 77 C# Compiler Warnings

Address all compiler warnings methodically, using research-backed approaches for each warning type.

## Steps

1. **Fix CS8618 (28 instances)** - For JSON-deserialized DTOs (`MonitorData`, `AppReference`, `AppDiscovery`), use `required` modifier since .NET 9 supports it. For singleton/lazy fields (`Daemon.Instance`, `InterceptorManager.Instance`), use `= null!` with the understanding they're set before use in `MonitorData.cs`, `AppReference.cs`, `Settings.cs`, `Daemon.cs`, `InterceptorManager.cs`, `Language.cs`, `SoundInterceptor.cs`, `ReadAloudInterceptor.cs`, `NativeInterceptor.cs`, `TrayIcon.cs`, `DragMode.cs`, `Program.cs`

2. **Fix CS0168 (12 instances)** - Replace `catch (Exception ex)` with `catch (Exception)` (no variable name) where exceptions are intentionally ignored - this is the most idiomatic C# approach in `Settings.cs`, `Daemon.cs`, `InterceptorManager.cs`, `SoundInterceptor.cs`, `TrayIcon.cs`

3. **Suppress warnings in ProcessCommandLine.cs** - Add `#nullable disable` at file level since this is third-party code (indicated by header comment). This preserves the original code and avoids merge conflicts if upstream updates in `ProcessCommandLine.cs`

4. **Fix CS8603/CS8600 (7 instances)** - Add nullable return types (`?`) for methods that legitimately can return null, or use null-forgiving operator where caller guarantees non-null in `Settings.cs`, `AppReference.cs`, `WallpaperFinder.cs`, `SoundFinder.cs`

5. **Fix CS8602/CS8604 (14 instances)** - Add null checks or null-conditional operators (`?.`) before dereferencing, or use `!` where runtime guarantees non-null in `BugReport.cs`, `Util.cs`, `MainCommands.cs`, `SoundFinder.cs`, `TrayIcon.cs`, `DragMode.cs`, `InterceptorManager.cs`, `SoundInterceptor.cs`, `Program.cs`

6. **Fix CS8625 (8 instances)** - Use `null!` for intentional null assignments to non-nullable types, or make the type nullable if null is a valid state in `AppDiscovery.cs`, `TrayIcon.cs`, `DragMode.cs`, `Program.cs`

7. **Fix CS8622 & CS4014 (2 instances)** - Change `LaunchSettingsMode(object Sender, ...)` to `LaunchSettingsMode(object? Sender, ...)` to match `EventHandler` delegate signature, and use discard `_ = AsyncMethod();` for fire-and-forget in `TrayIcon.cs` and `Program.cs`

## Implementation Order

Process files in order of fewest dependencies first to catch any cascade effects early:

1. Model/DTO classes (`MonitorData`, `AppReference`, `AppDiscovery`, `Language`)
2. Third-party code (`ProcessCommandLine` - just add `#nullable disable`)
3. Utility classes (`Util`, `BugReport`, `Settings`, `SoundFinder`, `WallpaperFinder`)
4. Core daemon (`Daemon`, `InterceptorManager`, interceptors)
5. GUI layer (`TrayIcon`, `DragMode`, `MainCommands`)
6. Entry point (`Program.cs`)

## Research Summary

### CS8618 (Non-nullable fields)
- For JSON DTOs: Use `required` modifier (.NET 7+) - expresses "JSON must provide this property"
- For singletons/lazy fields: Use `= null!` sparingly when serializer/initialization guarantees value
- Source: Microsoft Learn - System.Text.Json nullable annotations

### CS0168 (Unused exception variable)
- Use `catch (Exception)` without variable name - most idiomatic C# approach
- Explicitly communicates intent to ignore the exception
- Source: Official C# documentation

### Third-party code (ProcessCommandLine.cs)
- Use `#nullable disable` at file level to preserve original code
- Avoids merge conflicts with upstream updates
- Document choice for future maintainers
- Source: Microsoft Learn - Nullable warnings

### CS8603/CS8600/CS8601 (Null reference returns/assignments)
- Make return type nullable (`T?`) if null is valid
- Use `!` only when runtime guarantees non-null

### CS8602/CS8604 (Null dereference/argument)
- Add null checks or null-conditional operators (`?.`)
- Use `!` where runtime guarantees non-null (e.g., after validation)

### CS8625 (Null to non-nullable)
- Use `null!` for intentional null assignments
- Or change type to nullable if null is valid state

### CS8622 (Delegate nullability mismatch)
- Match parameter nullability to delegate signature

### CS4014 (Unawaited async call)
- Use discard `_ = AsyncMethod();` for intentional fire-and-forget
