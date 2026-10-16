# NOTES — MaxClicks (AutoClicker)

Current state of the project. Updated at the end of each step.

_Last updated: 2026-10-16_

## Overview

Windows Forms app written in C# (.NET Framework 4.8): an **Auto Clicker + Macro, two in one**. One window (`MaxClicks`) with two screens (`ClickerView`, `MacroView`), switched by a button with a slide animation.

## Done

- Visual Studio project and solution (`AutoClicker.sln`), `.gitignore`, `.gitattributes`, MIT license.
- `Mouse.cs`: `Mouse.LeftClick()` sends a left click at the current cursor position through `user32.dll` `mouse_event`.
- Replaced the top `MenuStrip` (Program / Clicker / Macro / Help) with fixed controls:
  - interval input (`NumericUpDown`, 1–60000 ms, default 100) inside a `GroupBox`
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

## In progress

- Nothing. Everything above is committed and pushed (last commit: "Starting the creation of the macro").

## Next session

The user wants to **finish the Auto Clicker first**, then start the Macro. Pick from the To do list below (Auto Clicker items first) and confirm with the user which one to do.

## To do

Auto Clicker (finish first):
- [ ] Save the chosen hotkey (and interval) between runs (`Properties.Settings`).
- [ ] Choose mouse button (left / right) and single / double click.
- [ ] Optional click limit (stop after N clicks).
- [ ] Consider replacing `mouse_event` with `SendInput` (the recommended API).

Macro (after the Auto Clicker):
- [ ] **Macro screen**: design and implement in `MacroView` (record/play mouse and keyboard actions? to be defined with the user). It will need its own hotkey(s): follow the `ClickerView` pattern, use a different hotkey id, and skip registration in the Designer.

General:
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
- **Language**: code, comments, README, NOTES.md and commit messages in English.
