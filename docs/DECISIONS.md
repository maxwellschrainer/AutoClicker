# Decisions

Why MaxClicks is built the way it is. Each item says what was chosen and why, so it isn't undone by accident.
For how the code works, see [HOW-IT-WORKS.md](HOW-IT-WORKS.md).

---

## Window and screens

- **One window + UserControls instead of two Forms** for Clicker/Macro: no window flashing or jumping, each screen has its own Designer and code, and shared things stay in one place. Switching is done with a button (no tabs), with a short linear slide.
- **Screens are never destroyed when switching**, only moved and hidden, so a running clicker and its hotkey keep working on the Macro screen.
- **Fixed controls instead of a menu**: quicker access to the clicker controls.
- **Window is `TopMost`, not resizable, no maximize**: the clicker stays visible over other apps.
- **The window's height follows the screen on display** (Macro 480 px, Auto Clicker 334 px) instead of one height for both: the Macro needed more room, and a single tall window would leave the Auto Clicker with a large empty area. The width stays the same.
- **Click options as DropDownList combos** (not radio buttons): compact, one row, and two radio groups would need extra panels. Code uses `SelectedIndex`, so the item texts can change freely.
- **Click limit as a CheckBox + number** (not "0 = no limit"): clearer for the user, and the last number stays saved when the limit is turned off.

## Clicking

- **`System.Windows.Forms.Timer`** (not `System.Timers.Timer`): it raises `Tick` on the UI thread, so controls can be updated directly without cross-thread calls.
- **15 ms minimum interval instead of a thread-based clicker** (user's choice): the Timer can't go faster (see "Known limit" in [HOW-IT-WORKS.md](HOW-IT-WORKS.md#known-limit-the-interval-is-not-exact)). An honest UI was a one-line change; ~63 clicks/s is enough for normal use, and a thread adds complexity and new concepts for little gain now.
- **Counter resets on every Start.**
- **A double click counts as 2** in the counter and in the limit: the screen shows what the mouse really did. (It was 1 per Tick before; changed because single and double looked the same on the counter.)
- **Clicks Windows refused are not counted** (and don't use up the limit): the clicker keeps running instead of stopping, so it picks up again by itself when the PC is unlocked.
- **Exact click limit**: with double clicks and an odd limit, the last Tick is a single click instead of going past the limit.
- **`SendInput` instead of `mouse_event`**: `mouse_event` is deprecated. `SendInput` sends all press/release events of a click (or double click) in one call, so no other input can land in between, and it reports how many events went through. It will also send keys for the Macro.
- **`INPUT` with the full mouse + keyboard union, in its own file (`NativeInput.cs`)**: the Macro sends keys too, and `Mouse` and `Keyboard` share one definition instead of two copies.

## Macro

- **Recorded live, not built step by step**: the user presses Record and just does the actions; the app stores each key/button event with the wait before it. (User's choice.) Steps can still be fine-tuned afterwards: change a wait, delete a step, clear.
- **Low-level hooks (`WH_KEYBOARD_LL` / `WH_MOUSE_LL`) for recording**: the only way to see input that goes to other programs. They only look and always pass the input on.
- **Press and release are separate steps** (a click = down + up): holds and drags replay exactly, and games that need a key held work. Costs two lines per click in the list.
- **Mouse moves are off by default** ("Record mouse moves too" turns them on): without them a macro stays short and readable, and drags still work (down at A, up at B). With them, at most one move every 20 ms is kept: smooth enough to replay a path, far fewer steps than the mouse reports.
- **The first step's wait is 0**: the time to go from the Record button to the first action is setup, not part of the macro. "Pause between runs" covers the wait between runs.
- **Ignored while recording**: injected input (e.g. the Auto Clicker running), key repeats, clicks on MaxClicks' own window, and the Record/Play hotkeys. Otherwise stopping the recording would itself be recorded.
- **Keys typed while MaxClicks is active are recorded** (and swallowed so they don't act on it). Ignoring them was tried first: right after clicking Record MaxClicks is still active, so the user's first keys were silently lost and it looked like recording didn't work.
- **Unpaired presses/releases are removed when recording stops**, and a removed step's wait goes to the next step: a press with no release would leave a key stuck forever during playback.
- **A new recording replaces the macro, unless "Add to the end" is ticked.** It is saved right after recording. When adding, the first new step's wait is 0 (like any recording's first step) and only the new part is cleaned of unpaired presses/releases, so earlier edits are kept.
- **Playback on the UI Timer, with planned moments counted from Play (`Stopwatch`)**: keeps the single-thread design of the app, and the Timer's ~15 ms lateness doesn't add up over a long macro. A wait under ~15 ms can still be played up to ~15 ms late.
- **Everything still held is released on Stop**, so stopping in the middle never leaves a key or button down.
- **Keys are sent with both key code and scan code** (like AutoHotkey's SendInput): normal apps read the key code, many games read only the scan code.
- **Real pixels for mouse positions (`PhysicalScreen`)**: the hook reports real pixels, but this app is not DPI aware, so with display scaling above 100% the cursor would land in the wrong place. Switching only the calling thread for one call fixes it without changing how the whole app is drawn.
- **Repeat as "N times" + a "Forever" box** (like the click limit's CheckBox + number), and a **pause between runs** instead of a fixed interval per run: the macro's own waits already set its speed.
- **Speed multiplies the step waits only**, not the pause between runs: the pause is typed in real milliseconds, so scaling it too would be surprising. Range 0.1x to 10x.
- **Macro files are plain text** (`.macro`): a header line `MaxClicks macro 1`, then one step per line in the settings format. Readable and editable by hand, and the header's version number allows a different format later. They hold only the steps, not the playback options. A file with any invalid line is refused as a whole, never half-loaded.
- **Hotkeys F8 (Record) and F7 (Play)**, next to the Auto Clicker's F6.

## Hotkey

- **`RegisterHotKey` instead of a keyboard hook**: simpler, and Windows delivers the key even when another app has focus. The current hotkey is unregistered while capturing a new one, otherwise Windows would swallow it.
- **Every hotkey is a `HotkeyButton`**, one reusable Button subclass, instead of the same capture/register code repeated per screen. (The Auto Clicker had its own label + "Change" button first; it was moved to `HotkeyButton` once the Macro needed two more hotkeys.) Click the key itself to change it, no separate "Change" button.
- **The big buttons show their hotkey** ("Start (F6)", "Record (F8)", "Play (F7)") and the hotkey groups say "click to change": the small key buttons first looked like Record/Play themselves (the user clicked them to record).
- **Any key can be the hotkey.** A restriction to F1–F24 / Ctrl/Alt combinations was tried and removed: the user should decide. Trade-off: a single key (e.g. `A`) is taken from every program while MaxClicks is open.
- **Each hotkey belongs to the control that uses it** (registered on its own handle), not to the main window: `ClickerView` (id 1), the Macro's Record and Play `HotkeyButton`s (ids 2 and 3).
- **The Macro screen is created at startup** (starts visible out of sight, hidden in `MaxClicks.OnShown`) so its hotkeys work before the user ever opens it.

## Saved settings

- **Hotkey saved only on a successful change**: if the saved hotkey is taken at startup, the screen shows "(none)" but the file keeps the user's choice, so the next run tries it again.
- **Click settings (interval, button, single/double, limit) saved on Start and on close**: not on every change; on Start so they survive a crash, on close so a choice made without starting is kept too.
- **Settings are per exe path**: .NET stores `user.config` under a hash of the exe's path, so Debug, Release and copies elsewhere keep separate settings. Accepted as is.

## Code structure

- **Regions by feature, read top to bottom as a story**: handlers and overrides sit with the feature they belong to, instead of separate "Event Handlers" / "Form Overrides" regions.
- **Screen state derived in one method per screen** (`UpdateControls`) from the current state (`clickTimer.Enabled`; `recorder.Running`, `playTimer.Enabled`), instead of each method toggling controls.
- **No side effects in the Designer**: UserControls check `inDesigner || DesignMode` before touching Windows (hotkeys, clicks...), because the Designer runs their code inside Visual Studio.

## Naming and language

- **Exe named `MaxClicks.exe`, code still `AutoClicker`**: the user-visible name matches the app; renaming the project/namespace/repo would be churn with no gain.
- **English for code, comments, UI text, docs and commit messages.**
- **Docs split by purpose**: [README](../README.md) for visitors, [CLAUDE.md](../CLAUDE.md) for the workflow, [NOTES.md](../NOTES.md) for the current state, and `docs/` for how it works, C# basics, decisions and testing. Each fact lives in one place; the others link to it.
