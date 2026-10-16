# NOTES — MaxClicks (AutoClicker)

Current state of the project. Updated at the end of each step.

_Last updated: 2026-10-16_

## Overview

Windows Forms app written in C# (.NET Framework 4.8): an **Auto Clicker + Macro, two in one**. One window (`MaxClicks`) with two screens (`ClickerView`, `MacroView`), switched by a button with a slide animation.

## Done

- Visual Studio project and solution (`AutoClicker.sln`), `.gitignore`, `.gitattributes`, MIT license.
- `Mouse.cs`: `Mouse.LeftClick()` sends a left click at the current cursor position through `user32.dll` `mouse_event`.
- Replaced the top `MenuStrip` (Program / Clicker / Macro / Help) with fixed controls:
  - interval input (`NumericUpDown`, 15–60000 ms, default 100) inside a `GroupBox`
  - Start / Stop buttons
  - click counter label
- Clicking loop: Start sets `Timer.Interval` from the input and starts the timer; each `Tick` calls `Mouse.LeftClick()` and increments the counter; Stop stops the timer.
- All identifiers, UI text and comments in English.
- `Mouse.cs` moved into the `AutoClicker` namespace.
- Global hotkey to start/stop (default F6), chosen by the user:
  - `GlobalHotkey.cs` wraps `user32.dll` `RegisterHotKey` / `UnregisterHotKey` (supports Ctrl/Alt/Shift + key, no auto-repeat).
  - "Hotkey (start / stop)" group: key shown in a bordered label + "Change" button. Clicking "Change" waits for the next key (`ProcessCmdKey`); Esc or "Cancel" keeps the old one; a combination already taken by another program shows a warning and keeps the old one.
  - `WndProc` receives `WM_HOTKEY` and calls `ToggleClicking()`.
  - Start/Stop logic in `StartClicking()` / `StopClicking()`, shared by buttons and hotkey.
  - If the hotkey can't be registered, it becomes `Keys.None` and the screen shows "(none)".
  - `UpdateControls()` is the only place that enables/disables controls and sets the hotkey texts; it reads the state instead of receiving it.
- **Two screens in one window** (Auto Clicker / Macro):
  - The clicker moved from the form into the `ClickerView` UserControl (same controls and logic). `OnFormClosed` became `OnHandleDestroyed`; `OnVisibleChanged` cancels a hotkey capture when the screen is hidden.
  - `MacroView` UserControl: empty, only a "Macro - coming soon" label.
  - `MaxClicks` is now the frame: header (`pnlHeader` with `lblTitle` + `btnSwitchView`) and `pnlViews` holding both screens.
  - `btnSwitchView` slides to the other screen (`slideTimer`, 12 steps × 15 ms, linear). Clicker → Macro comes from the right, back comes from the left.
  - The clicker keeps running and its hotkey keeps working while the Macro screen is shown.
  - Launched the app and checked: opens fine, slides to Macro and back, closes cleanly.
- **Bug fixed: F6 stuck as hotkey.** The Visual Studio Designer creates a real `ClickerView` to draw `MaxClicks`, so its `OnLoad` registered F6 **inside Visual Studio**: F6 kept starting clicks (from devenv) and the real app said F6 was taken. `ClickerView` now skips registration in the Designer (`LicenseManager.UsageMode` in the constructor + `DesignMode`). Verified: app open → F6 taken; change to F7 → F6 freed, F7 taken; app closed → both free.

- **Hotkey and interval saved between runs** (`Properties.Settings`, scope User):
  - `Settings.settings` has `Hotkey` (`Keys`, default F6) and `Interval` (`int`, default 100); `Settings.Designer.cs` and `App.config` updated to match (written by hand in the VS-generated format).
  - `ClickerView`: `LoadSettings()` in `OnLoad` (interval clamped to the field's range), `SaveHotkey()` after a successful hotkey change, `SaveInterval()` in `OnHandleDestroyed`. Nothing is loaded/saved in the Designer.
  - Tested with the built exe: changed F6 -> F7, closed, `user.config` had F7; edited the file to `Ctrl+F8` / 250 -> app opened showing both; 999999 -> clamped and saved as 60000. Not tested: typing a new interval in the UI and reopening (only loading/saving through the file was exercised).
  - The interval is also saved on Start (survives a crash). Not tested automatically: Start makes real clicks.
- **Exe renamed to `MaxClicks.exe`** (`AssemblyName`, plus `AssemblyTitle`/`AssemblyProduct` in `AssemblyInfo.cs`). Project, solution and namespace stay `AutoClicker`. Checked: window title and file description show "MaxClicks"; settings now go to `%LOCALAPPDATA%\AutoClicker\MaxClicks.exe_Url_...`.
- **Measured the Timer's real precision** (see "Known limit" in `docs/HOW-IT-WORKS.md`): it steps every ~15.6 ms, so 1–15 ms all give ~63 clicks/s, and `timeBeginPeriod(1)` didn't help.
- **Click options: left / right button, single / double click**:
  - `Mouse.cs`: `LeftClick()` replaced by `Click(MouseButtons)` and `DoubleClick(MouseButtons)` (two clicks in a row).
  - New "Click" group (`grpClick`) between Interval and Hotkey, with two DropDownList combos: `cmbButton` (Left/Right button), `cmbClickType` (Single/Double click). Window 61 px taller (`ClientSize` 284x313, views 284x273).
  - `ClickerView`: `SelectedButton` / `SelectedDoubleClick` properties read the combos; `clickTimer_Tick` uses them; combos locked while running in `UpdateControls`.
  - Settings `ClickButton` (`MouseButtons`, Left) and `DoubleClick` (`bool`, False); `SaveInterval` became `SaveClickSettings` (interval + button + double, on Start and on close).
  - Tested with the built exe: defaults show Left/Single; changed both combos to item 1 -> closed -> `user.config` had Right/True -> reopened showing Right button / Double click (screenshot). Macro screen checked at the new height. **Not tested: real right/double clicks** (Start makes real clicks).
- **Interval minimum raised from 1 to 15 ms** (`numInterval.Minimum` in `ClickerView.Designer.cs`), the fastest the Timer really goes. A saved value below 15 is clamped by `LoadSettings` (tested: saved 1 -> opened and saved 15).

- **Click limit (stop after N clicks)**:
  - New "Click limit" group (`grpLimit`) between Click and Hotkey: `chkLimit` ("Stop after") + `numLimit` (1-1,000,000, default 100, thousands separator) + "clicks" label. Window 61 px taller again (`ClientSize` 284x374, views 284x334).
  - `clickTimer_Tick` calls `StopClicking()` when `LimitReached` (`chkLimit.Checked && count >= numLimit.Value`). Counts actions, so a double click counts as 1.
  - Counter shows `Clicks: N / limit` while the box is checked. `numLimit` enabled only when the box is checked; both locked while running. `chkLimit_CheckedChanged` refreshes the controls and the counter.
  - Settings `LimitEnabled` (`bool`, False) and `ClickLimit` (`int`, 100), saved in `SaveClickSettings`, clamped in `LoadSettings`.
  - Tested with the built exe: opens with the box unchecked and the number disabled (screenshot); ticked the box (BM_CLICK) -> closed -> `user.config` had `LimitEnabled` True -> reopened with the box checked, number enabled and "Clicks: 0 / 100" (screenshot). **Not tested: the automatic stop itself** (Start makes real clicks).

## In progress

- Review fixes (not committed): counter "/ limit" now follows the number field (`numLimit_ValueChanged`; bug: a saved limit of 500 showed "/ 100" at startup, and typing a new limit didn't update the label); `\u25BA`/`\u25C4` escapes in `MaxClicks.cs`; unused usings removed from `Program.cs`; class comment in `Mouse.cs`; docs brought up to date. The click limit's automatic stop still needs a manual test by the user (real clicks).

## Next session

The user wants to **finish the Auto Clicker first**, then start the Macro. Remaining Auto Clicker item: `SendInput` instead of `mouse_event`. After that, design the Macro screen with the user.

## To do

Auto Clicker (finish first):
- [ ] Consider replacing `mouse_event` with `SendInput` (the recommended API).

Macro (after the Auto Clicker):
- [ ] **Macro screen**: design and implement in `MacroView` (record/play mouse and keyboard actions? to be defined with the user). It will need its own hotkey(s): follow the `ClickerView` pattern, use a different hotkey id, and skip registration in the Designer.

General:
- [ ] *(Future, only if more speed/precision is ever needed)* **Exact interval**: clicks on a background thread timed with a `Stopwatch` (or a high-resolution waitable timer) instead of the WinForms Timer; the screen reads the counter with a slow UI timer. Would allow < 15 ms and exact intervals. New concepts: threads, `Interlocked`, stopping a thread safely.
- [ ] If the slide flickers too much on some PCs, try double buffering (`WS_EX_COMPOSITED`) on the form.

## Documentation

- `CLAUDE.md`: how to work on the project (session routine, build command, gotchas, how to test, conventions). **Read first.**
- `README.md`: what the app is and how to use it.
- `docs/HOW-IT-WORKS.md`: manual of the code (files, concepts, each feature's flow, glossary). **Update it whenever a feature or concept is added or changed.**

## Decisions

- **Fixed controls instead of a menu**: quicker access to the clicker controls.
- **`System.Windows.Forms.Timer`** (not `System.Timers.Timer`): it raises `Tick` on the UI thread, so controls can be updated directly without cross-thread calls.
- **Window is `TopMost`, fixed size, no maximize**: the clicker stays visible over other apps.
- **Counter resets on every Start.**
- **Regions by feature, read top to bottom as a story**: handlers and overrides sit with the feature they belong to, instead of separate "Event Handlers" / "Form Overrides" regions.
- **`RegisterHotKey` instead of a keyboard hook**: simpler, and Windows delivers the key even when another app has focus. The current hotkey is unregistered while capturing a new one, otherwise Windows would swallow it.
- **Hotkey shown in a "keycap" label inside a GroupBox**, matching the interval group, instead of a custom-styled button (keeps the native look consistent).
- **Any key can be the hotkey.** A restriction to F1–F24 / Ctrl/Alt combinations was tried and removed: the user should decide. Trade-off: a single key (e.g. `A`) is taken from every program while MaxClicks is open.
- **Screen state derived in one method** (`UpdateControls`) from `clickTimer.Enabled` and `waitingForHotkey`, instead of each method toggling controls.
- **One window + UserControls instead of two Forms** for Clicker/Macro: no window flashing or jumping, each screen has its own Designer and code, and shared things stay in one place. Switching is done with a button (no tabs), with a short linear slide.
- **Screens are never destroyed when switching**, only moved and hidden, so a running clicker and its hotkey keep working on the Macro screen.
- **The hotkey belongs to `ClickerView`** (registered on its own handle), not to the main window. The Macro will get its own when it exists.
- **No side effects in the Designer**: UserControls check `inDesigner || DesignMode` before touching Windows (hotkeys, clicks...), because the Designer runs their code inside Visual Studio.
- **Hotkey saved only on a successful change**: if the saved hotkey is taken at startup, the screen shows "(none)" but the file keeps the user's choice, so the next run tries it again. The click settings (interval, button, single/double, limit) are saved on Start and on close.
- **15 ms minimum instead of a thread-based clicker** (user's choice): honest UI with a one-line change; ~63 clicks/s is enough for normal use, and a thread adds complexity and new concepts for little gain now.
- **Exe named `MaxClicks.exe`, code still `AutoClicker`**: the user-visible name matches the app; renaming the project/namespace/repo would be churn with no gain.
- **Settings are per exe path**: .NET stores `user.config` under a hash of the exe's path, so Debug, Release and copies elsewhere keep separate settings.
- **Click options as DropDownList combos** (not radio buttons): compact, one row, and two radio groups would need extra panels. Code uses `SelectedIndex`, so the item texts can change freely.
- **A double click counts as 1** in the counter: it counts actions (Ticks), which is also what a future "stop after N clicks" limit should use.
- **Click limit as a CheckBox + number** (not "0 = no limit"): clearer for the user, and the last number stays saved when the limit is turned off.
- **Language**: code, comments, README, NOTES.md and commit messages in English.
