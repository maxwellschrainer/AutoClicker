# NOTES — MaxClicks (AutoClicker)

Where the project stands right now. Updated at the end of each step.
History is in `git log`; how things work is in [docs/HOW-IT-WORKS.md](docs/HOW-IT-WORKS.md); why, in [docs/DECISIONS.md](docs/DECISIONS.md).

_Last updated: 2026-10-17_

## State

Windows Forms app (C#, .NET Framework 4.8): an **Auto Clicker + Macro, two in one**. One window (`MaxClicks`) with two screens (`ClickerView`, `MacroView`), switched by a button with a slide animation.

**Auto Clicker: done**
- Interval 15–60000 ms, left/right button, single/double click.
- Click counter (a double click counts 2) and optional click limit with automatic stop.
- Global hotkey to start/stop (default F6, any key or combination, changeable in the UI).
- All options saved between runs (`Properties.Settings`).
- Clicks sent with `SendInput` (tested: left, right and double click, as 64-bit and 32-bit process).

**Macro: not started** (`MacroView` is a "coming soon" placeholder).

**In progress:** `SendInput` change in `Mouse.cs` + docs, built and tested, waiting for the user's review and commit.

## Next session

Auto Clicker is done once the `SendInput` change is committed. Next: design the Macro screen with the user.

## To do

Auto Clicker:
- [ ] *(Suggestion)* Use the `bool` returned by `Mouse.Click`/`DoubleClick` in `ClickerView` (e.g. don't count clicks Windows refused). Note: clicks blocked by a window running as administrator are NOT reported by `SendInput`.

Macro:
- [ ] **Macro screen**: design and implement in `MacroView` (record/play mouse and keyboard actions? to be defined with the user). It will need its own hotkey(s): follow the `ClickerView` pattern, use a different hotkey id, and skip registration in the Designer.

Docs:
- [ ] Retake `docs/images/auto-clicker.png` when the Auto Clicker screen changes, and add a Macro screenshot once it exists.

General:
- [ ] *(Future, only if more speed/precision is ever needed)* **Exact interval**: clicks on a background thread timed with a `Stopwatch` (or a high-resolution waitable timer) instead of the WinForms Timer; the screen reads the counter with a slow UI timer. Would allow < 15 ms and exact intervals. New concepts: threads, `Interlocked`, stopping a thread safely.
- [ ] If the slide flickers too much on some PCs, try double buffering (`WS_EX_COMPOSITED`) on the form.

## Not tested yet

- Typing a new interval in the UI and reopening (only loading/saving through `user.config` was exercised).
- Click settings being saved on Start (so they survive a crash): Start was tested, the save-on-Start path was not checked on its own.
