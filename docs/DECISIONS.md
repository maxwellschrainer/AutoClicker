# Decisions

Why MaxClicks is built the way it is. Each item says what was chosen and why, so it isn't undone by accident.
For how the code works, see [HOW-IT-WORKS.md](HOW-IT-WORKS.md).

---

## Window and screens

- **One window + UserControls instead of two Forms** for Clicker/Macro: no window flashing or jumping, each screen has its own Designer and code, and shared things stay in one place. Switching is done with a button (no tabs), with a short linear slide.
- **Screens are never destroyed when switching**, only moved and hidden, so a running clicker and its hotkey keep working on the Macro screen.
- **Fixed controls instead of a menu**: quicker access to the clicker controls.
- **Window is `TopMost`, fixed size, no maximize**: the clicker stays visible over other apps.
- **Hotkey shown in a "keycap" label inside a GroupBox**, matching the interval group, instead of a custom-styled button (keeps the native look consistent).
- **Click options as DropDownList combos** (not radio buttons): compact, one row, and two radio groups would need extra panels. Code uses `SelectedIndex`, so the item texts can change freely.
- **Click limit as a CheckBox + number** (not "0 = no limit"): clearer for the user, and the last number stays saved when the limit is turned off.

## Clicking

- **`System.Windows.Forms.Timer`** (not `System.Timers.Timer`): it raises `Tick` on the UI thread, so controls can be updated directly without cross-thread calls.
- **15 ms minimum interval instead of a thread-based clicker** (user's choice): the Timer can't go faster (see "Known limit" in [HOW-IT-WORKS.md](HOW-IT-WORKS.md#known-limit-the-interval-is-not-exact)). An honest UI was a one-line change; ~63 clicks/s is enough for normal use, and a thread adds complexity and new concepts for little gain now.
- **Counter resets on every Start.**
- **A double click counts as 2** in the counter and in the limit: the screen shows what the mouse really did. (It was 1 per Tick before; changed because single and double looked the same on the counter.)
- **Exact click limit**: with double clicks and an odd limit, the last Tick is a single click instead of going past the limit.
- **`SendInput` instead of `mouse_event`**: `mouse_event` is deprecated. `SendInput` sends all press/release events of a click (or double click) in one call, so no other input can land in between, and it reports how many events went through. It will also send keys for the Macro.
- **`INPUT` declares only the mouse part of the Windows union**: `MOUSEINPUT` is the biggest member, so the struct size is still right on 32 and 64 bits. Keyboard input will need the full union (`[FieldOffset]`).

## Hotkey

- **`RegisterHotKey` instead of a keyboard hook**: simpler, and Windows delivers the key even when another app has focus. The current hotkey is unregistered while capturing a new one, otherwise Windows would swallow it.
- **Any key can be the hotkey.** A restriction to F1–F24 / Ctrl/Alt combinations was tried and removed: the user should decide. Trade-off: a single key (e.g. `A`) is taken from every program while MaxClicks is open.
- **The hotkey belongs to `ClickerView`** (registered on its own handle), not to the main window. The Macro will get its own when it exists.

## Saved settings

- **Hotkey saved only on a successful change**: if the saved hotkey is taken at startup, the screen shows "(none)" but the file keeps the user's choice, so the next run tries it again.
- **Click settings (interval, button, single/double, limit) saved on Start and on close**: not on every change; on Start so they survive a crash, on close so a choice made without starting is kept too.
- **Settings are per exe path**: .NET stores `user.config` under a hash of the exe's path, so Debug, Release and copies elsewhere keep separate settings. Accepted as is.

## Code structure

- **Regions by feature, read top to bottom as a story**: handlers and overrides sit with the feature they belong to, instead of separate "Event Handlers" / "Form Overrides" regions.
- **Screen state derived in one method** (`UpdateControls`) from `clickTimer.Enabled` and `waitingForHotkey`, instead of each method toggling controls.
- **No side effects in the Designer**: UserControls check `inDesigner || DesignMode` before touching Windows (hotkeys, clicks...), because the Designer runs their code inside Visual Studio.

## Naming and language

- **Exe named `MaxClicks.exe`, code still `AutoClicker`**: the user-visible name matches the app; renaming the project/namespace/repo would be churn with no gain.
- **English for code, comments, UI text, docs and commit messages.**
- **Docs split by purpose**: [README](../README.md) for visitors, [CLAUDE.md](../CLAUDE.md) for the workflow, [NOTES.md](../NOTES.md) for the current state, and `docs/` for how it works, C# basics, decisions and testing. Each fact lives in one place; the others link to it.
