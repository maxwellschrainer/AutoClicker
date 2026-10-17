# NOTES — MaxClicks (AutoClicker)

Where the project stands right now. Updated at the end of each step.
History is in `git log`; how things work is in [docs/HOW-IT-WORKS.md](docs/HOW-IT-WORKS.md); why, in [docs/DECISIONS.md](docs/DECISIONS.md).

_Last updated: 2026-10-17_

## State

Windows Forms app (C#, .NET Framework 4.8): an **Auto Clicker + Macro, two in one**. One window (`MaxClicks`) with two screens (`ClickerView`, `MacroView`), switched by a button with a slide animation.

**Auto Clicker: done**
- Interval 15–60000 ms, left/right button, single/double click.
- Click counter (a double click counts 2; clicks Windows refused are not counted) and optional click limit with automatic stop.
- Global hotkey to start/stop (default F6, any key or combination; click the key to change it). Uses the shared `HotkeyButton`.
- All options saved between runs (`Properties.Settings`), also on Start.
- Clicks sent with `SendInput` (tested: left, right and double click, as 64-bit and 32-bit process).

**Macro: done** (recording and playback checked by hand by the user)
- Record (button or F8): every key and mouse button/wheel event in Windows, with the wait before each one. Stop with the button or F8.
- Step list with editing: change a step's wait, delete a step, clear all.
- Play (button or F7): repeat N times or forever, pause between runs, current step highlighted, run/step shown. Stop releases anything still held.
- Playback speed (0.1x–10x), "Add to the end of the current macro", "Record mouse moves too".
- Save / load macros to `.macro` files.
- Macro, options and hotkeys saved between runs.
- The window's height follows the screen on display (the Macro screen is taller).

## Next session

The user reviews this round (HotkeyButton on the Auto Clicker, Macro files, speed, append, mouse moves, window height) and tests it by hand; then pick from the To do list.

## To do

Macro:
- [ ] *(Suggested)* **Undo** for step edits (delete / clear) and for the last recording.
- [ ] *(Suggested)* **Select several steps** at once (delete or change their waits together).

General:
- [ ] *(Suggested)* An **app icon** (the window and the exe use the default one). Would use `Properties/Resources` (see below).
- [ ] *(Future, only if more speed/precision is ever needed)* **Exact interval**: clicks on a background thread timed with a `Stopwatch` (or a high-resolution waitable timer) instead of the WinForms Timer; the screen reads the counter with a slow UI timer. Would allow < 15 ms and exact intervals (also for macro waits under ~15 ms). New concepts: threads, `Interlocked`, stopping a thread safely.
- [ ] *(Optional)* `Properties/Resources.resx` + `Resources.Designer.cs` are unused template files. Keep them if an app icon or images are added later; otherwise they can be removed (also from the csproj).
- [ ] If the slide flickers too much on some PCs, try double buffering (`WS_EX_COMPOSITED`) on the form.

## Not tested yet

- A click refused by Windows really not being counted (needs a locked PC or a UAC prompt while clicking; only the normal path, where every click is accepted, was tested).
- **Macro recording with real input**: right/middle/side buttons, wheel, modifier combos (Ctrl+C...), removal of unpaired steps, mouse moves from a real mouse. (Tested with a scratch copy that accepts injected input: key F13 while MaxClicks is active, left click on another window, click on MaxClicks ignored, F8 ignored and stops recording, steps saved; "Add to the end" kept the old steps; moves recorded at most every 20 ms.)
- **Macro playback**: tested only with a hand-written macro (left click + key A, 3 runs, 300 ms pause): the 3 clicks landed at the right point; keys went to whichever window was active (the user was using the PC, so some went to their browser). Speed 2x tested (3 clicks came about twice as fast). Load/Save tested through the real dialogs (valid file, invalid file refused, saved file content). Not tested: right/middle/side buttons, wheel, drags, playing back mouse moves, extended keys, games, Forever, Stop in the middle (release of held keys), display scaling above 100% (this PC is at 100%).
- Editing steps (wait, delete, clear) and the "taken key" warning of the hotkey buttons. (Changing the Auto Clicker hotkey F6 → F9 and Esc cancel were tested.)
- The window moving up when growing near the bottom of the screen.
- Both screens in the Visual Studio Designer (`HotkeyButton` in the Toolbox, no hotkeys registered inside Visual Studio).
