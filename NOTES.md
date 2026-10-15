# NOTES — MaxClicks (AutoClicker)

Current state of the project. Updated at the end of each step.

_Last updated: 2026-10-15_

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
- `MaxClicks.cs` organized in regions: Fields, Constructor, Event Handlers, Helper Methods.

## In progress

- Nothing. Translation and reviewed comments in `MaxClicks.cs` awaiting the user's review.

## To do

- [ ] Global hotkey (e.g. F6) to start/stop without moving the mouse to the Stop button.
- [ ] Choose mouse button (left / right) and single / double click.
- [ ] Optional click limit (stop after N clicks).
- [ ] Consider replacing `mouse_event` with `SendInput` (the recommended API).
- [ ] Push local commits to `origin` (GitHub `maxwellschrainer/AutoClicker`).

## Decisions

- **Fixed controls instead of a menu**: quicker access to the clicker controls.
- **`System.Windows.Forms.Timer`** (not `System.Timers.Timer`): it raises `Tick` on the UI thread, so controls can be updated directly without cross-thread calls.
- **Window is `TopMost`, fixed size, no maximize**: the clicker stays visible over other apps.
- **Counter resets on every Start.**
- **Regions by role** (fields / constructor / event handlers / helpers), not by access modifier.
- **Language**: code, comments, README, NOTES.md and commit messages in English.
