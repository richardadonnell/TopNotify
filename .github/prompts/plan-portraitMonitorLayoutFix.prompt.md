# Plan: Hybrid Fix (Taller Window for Portrait + Scrollable)

## Problem Summary

When a portrait-oriented monitor is selected in TopNotify, the preview area dynamically calculates its height based on the monitor's aspect ratio. For portrait monitors (e.g., 1080×1920), the preview height becomes ~626px, consuming nearly the entire 650px fixed window height and pushing all control settings out of view.

**GitHub Issue:** [#91 - Unable to resize window](https://github.com/SamsidParty/TopNotify/issues/91)

## Root Cause

| Component | Value | Problem |
|-----------|-------|---------|
| Window height | 650px × scale | Fixed, non-resizable (`LockedWindowBounds` in Program.cs line 137) |
| Preview height | `352px × aspect_ratio` | For portrait monitors: 352 × 1.78 ≈ 626px |
| CSS overflow | `overflow: hidden` | Prevents scrolling (App.css) |

## Proposed Solution

Combine **two approaches** for the most robust fix:

1. **Detect portrait orientation** using `ResolutionFinder.GetRealResolution()` and use a taller window height (e.g., 850px instead of 650px) for portrait monitors
2. **Add scrollable fallback** with `overflow-y: auto` on `.app.loaded` in CSS

This gives the best of both worlds: a taller window that fits more content for portrait monitors, plus scrolling as a safety net for edge cases.

---

## Implementation Steps

### Phase 1: Verify Environment

Run these commands to verify prerequisites:

```powershell
# Check .NET SDK version (need 9.x)
dotnet --version
dotnet --list-sdks

# Check Node.js version (need 18+ or 20+)
node --version
npm --version
```

### Phase 2: Build & Run (Before Fix)

```powershell
# Install frontend dependencies
cd TopNotify/src-vite
npm install

# Build frontend (outputs to TopNotify/dist/)
npm run build

# Build and run app with settings GUI
cd ..
dotnet run -- --settings
```

**Reproduce the bug:** Select portrait monitor from dropdown, observe controls pushed off-screen.

### Phase 3: Apply Fix

#### File 1: `TopNotify/Program.cs` (around line 137)

Detect portrait orientation and adjust window height:

```csharp
// Before the mainWindow creation, add:
var resolution = ResolutionFinder.GetRealResolution();
var isPortrait = resolution.Height > resolution.Width;
var windowHeight = isPortrait ? 850f : 650f;

// Then modify WithBounds to use windowHeight:
.WithBounds(new LockedWindowBounds((int)(400f * ResolutionFinder.GetScale()), (int)(windowHeight * ResolutionFinder.GetScale())))
```

#### File 2: `TopNotify/src-vite/src/App.css`

Add scrollable fallback to `.app.loaded`:

```css
.app.loaded {
    height: calc(100vh - 27px);
    overflow-y: auto;  /* Add this line */
}
```

### Phase 4: Rebuild & Test

```powershell
# Rebuild frontend
cd TopNotify/src-vite
npm run build

# Run app
cd ..
dotnet run -- --settings
```

**Test checklist:**
- [ ] Select portrait monitor → all controls visible (may need minimal scrolling)
- [ ] Select landscape monitor → no regression, no unnecessary scrolling
- [ ] Test "Spawn Test Notification" button works
- [ ] Test corner selection buttons work

### Phase 5: Submit PR

```powershell
# Create feature branch
git checkout -b fix/portrait-monitor-layout

# Stage changes
git add TopNotify/Program.cs TopNotify/src-vite/src/App.css

# Commit with conventional commit message
git commit -m "fix: adjust window height for portrait monitors and add scroll fallback"

# Push and open PR
git push -u origin fix/portrait-monitor-layout
```

---

## Files to Modify

| File | Change |
|------|--------|
| `TopNotify/Program.cs` | Detect portrait orientation, adjust window height |
| `TopNotify/src-vite/src/App.css` | Add `overflow-y: auto` to `.app.loaded` |

## Notes

- The project targets .NET 9 (`net9.0-windows10.0.17763.0`)
- IgniteView uses `LockedWindowBounds` which sets min=max=initial dimensions
- `ResolutionFinder.GetRealResolution()` already exists and returns monitor width/height
- Portrait detection uses the *selected* monitor (from settings), not primary monitor
