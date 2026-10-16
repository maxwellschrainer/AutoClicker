# NOTES — MaxClicks (AutoClicker)

Current state of the project. Updated at the end of each step.

_Last updated: 2026-10-16_

## Overview

Windows Forms auto clicker written in C# (.NET Framework 4.8). The main form is `MaxClicks`.

## Done

- Visual Studio project and solution (`AutoClicker.sln`), `.gitignore`, `.gitattributes`, MIT license.
- `Mouse.cs`: `Mouse.LeftClick()` sends a left click at the current cursor position through `user32.dll` `mouse_event`.
- Replaced the top `MenuStrip` (Program / Clicker / Macro / Help) with fixed controls on the form:
  - interval input (`NumericUpDown`, 1–60000 ms, default 100) inside a `GroupBox`
  - Start / Stop buttons
  - click counter label
- Clicking loop: Start sets `Timer.Interval` from the input and starts the timer; each `Tick` calls `Mouse.LeftClick()` and increments the counter; Stop stops the timer.
- While running, only Stop is enabled and the interval input is locked.
- All identifiers, UI text and comments translated to English (`grpInterval`, `numInterval`, `btnStart`, `btnStop`, `lblCount`, `clickTimer`, `UpdateCount`, `UpdateButtons`). Builds with no errors or warnings.
- `MaxClicks.cs` organized in regions: Fields, Constructor, Form Overrides, Event Handlers, Helper Methods.
- `Mouse.cs` moved into the `AutoClicker` namespace.
- Global hotkey to start/stop (default F6), chosen by the user:
  - `GlobalHotkey.cs` wraps `user32.dll` `RegisterHotKey` / `UnregisterHotKey` (supports Ctrl/Alt/Shift + key, no auto-repeat).
  - "Hotkey (start / stop)" group: key shown in a bordered label + "Change" button. Clicking "Change" waits for the next key (`ProcessCmdKey`); Esc or "Cancel" keeps the old one; a combination already taken by another program shows a warning and keeps the old one.
  - `WndProc` receives `WM_HOTKEY` and calls `ToggleClicking()`.
  - Hotkey released in `OnFormClosed`.
  - Start/Stop logic moved into `StartClicking()` / `StopClicking()` so buttons and hotkey share it.

## In progress

- Nothing. Hotkey feature committed and pushed.
- `README.md` and `docs/HOW-IT-WORKS.md` (code manual) created, not committed yet.

## To do

- [ ] Save the chosen hotkey (and interval) between runs (`Properties.Settings`).
- [ ] Choose mouse button (left / right) and single / double click.
- [ ] Optional click limit (stop after N clicks).
- [ ] Consider replacing `mouse_event` with `SendInput` (the recommended API).

## Documentation

- `README.md`: what the app is and how to use it.
- `docs/HOW-IT-WORKS.md`: manual of the code (files, concepts, each feature's flow, glossary). **Update it whenever a feature or concept is added or changed.**

## Decisions

- **Fixed controls instead of a menu**: quicker access to the clicker controls.
- **`System.Windows.Forms.Timer`** (not `System.Timers.Timer`): it raises `Tick` on the UI thread, so controls can be updated directly without cross-thread calls.
- **Window is `TopMost`, fixed size, no maximize**: the clicker stays visible over other apps.
- **Counter resets on every Start.**
- **Regions by role** (fields / constructor / event handlers / helpers), not by access modifier.
- **`RegisterHotKey` instead of a keyboard hook**: simpler, and Windows delivers the key even when another app has focus. The current hotkey is unregistered while capturing a new one, otherwise Windows would swallow it.
- **Hotkey shown in a "keycap" label inside a GroupBox**, matching the interval group, instead of a custom-styled button (keeps the native look consistent).
- **Language**: code, comments, README, NOTES.md and commit messages in English.
