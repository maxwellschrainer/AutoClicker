# How MaxClicks works

A manual for the code: what each file does, how the pieces talk to each other, and how each feature works.
If you get lost, start with [The big picture](#the-big-picture), then jump to the part you need.

Related docs:
- [C# basics](CSHARP-BASICS.md): the C# concepts used here (classes, `partial`, `override`, events, P/Invoke...) and a glossary.
- [Decisions](DECISIONS.md): why things were done this way.
- [Testing](TESTING.md): how the app is tested.

---

## The big picture

MaxClicks is a Windows Forms app with **one window and two screens**: the **Auto Clicker** and the **Macro** (see [Feature: macro](#feature-macro)). A button in the header slides from one screen to the other. See [Screens and switching](#screens-and-switching).

The Auto Clicker screen does four things:

1. **Clicks the mouse** at a fixed interval while it is running (left or right button, single or double click).
2. **Counts** the clicks and shows the number on the screen.
3. **Stops by itself** after N clicks, if the click limit is on.
4. **Listens for a global hotkey** (default F6) that starts/stops the clicking from any program.

```
 You ──clicks Start / presses hotkey──> ClickerView (the Auto Clicker screen)
                                            │
                                            ├─ starts the Timer
                                            │
 Timer ──Tick every N ms──────────────────> clickTimer_Tick
                                            ├─ Mouse.Click(...) / Mouse.DoubleClick(...) → Windows clicks the mouse
                                            ├─ count++ / UpdateCount() → label shows the number
                                            └─ LimitReached? → StopClicking()
```

---

## Files

| File | What it is |
|---|---|
| `Program.cs` | Entry point. `Main()` runs first and opens the window: `Application.Run(new MaxClicks())`. |
| `MaxClicks.cs` / `.Designer.cs` | **The main window** (the "frame"): header with the title and the switch button, and the area where the screens are shown. Handles the slide between screens. |
| `MaxClicks.resx` | Extra data for the Designer (e.g. where the timer icon sits in the component tray). |
| `ClickerView.cs` / `.Designer.cs` | **The Auto Clicker screen** (a UserControl): interval, click options (button, single/double), click limit, hotkey, Start/Stop, counter, and all their logic. |
| `MacroView.cs` / `.Designer.cs` | **The Macro screen** (a UserControl): step list, step editing, playback options, hotkeys, Record/Play, and all their logic. |
| `MacroStep.cs` | One step of a macro (wait + key/button press or release, or wheel), how to play it, and how to save it as text. |
| `InputRecorder.cs` | Low-level keyboard and mouse hooks: sees every key and button in Windows while recording, and turns each into a `MacroStep`. |
| `HotkeyButton.cs` | A Button that shows a global hotkey, lets the user change it, and raises `HotkeyPressed` (used for the Macro's Record and Play hotkeys). |
| `NativeInput.cs` | The Windows `SendInput` function and its `INPUT` structs (mouse + keyboard union), shared by `Mouse` and `Keyboard`. |
| `Mouse.cs` | Sends real mouse input: `Click`, `DoubleClick`, `Press`, `Release` (left, right, middle, back, forward) and `Wheel`. |
| `Keyboard.cs` | Sends real key presses and releases. |
| `PhysicalScreen.cs` | Moves the cursor and finds windows using real pixels, so recorded positions are right even with display scaling above 100%. |
| `GlobalHotkey.cs` | Asks Windows to reserve/release a system-wide hotkey (`user32.dll`). |
| `Properties/Settings.settings` | The list of values saved between runs (Auto Clicker options, the macro and its options) and their defaults. Visual Studio generates `Settings.Designer.cs` from it. See [Saved settings](#saved-settings). |
| `App.config` | App configuration. Also holds a copy of the settings' defaults (Visual Studio keeps it in sync). |
| `Properties/` (rest) | Assembly info and resources created by Visual Studio. |

In every `X.cs` / `X.Designer.cs` pair, the **Designer** file says what is on the screen (controls, positions, texts, which method each event calls) and the other file says what happens (the logic).

---

## The Designer runs your code

Visual Studio's **Designer** is the visual editor for screens (see [Form vs UserControl](CSHARP-BASICS.md#form-vs-usercontrol)). To draw `MaxClicks` in the Designer, Visual Studio creates **real** `ClickerView` and `MacroView` objects, so their constructor and `OnLoad` actually run **inside Visual Studio**. Anything with side effects outside the screen (registering a hotkey, clicking the mouse, opening files) must be skipped there:

```csharp
// constructor: the only place where this check is reliable
inDesigner = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

// OnLoad
if (inDesigner || DesignMode)
    return;   // don't register the hotkey inside Visual Studio
```

Without this, while the `MaxClicks.cs [Design]` tab was open, **Visual Studio itself** held F6: pressing F6 started clicking from inside Visual Studio, and the real app could not use F6 ("already used by another program").

---

## Screens and switching

**Where:** `MaxClicks.cs`.

```
MaxClicks (window)
┌──────────────────────────────┐
│ Auto Clicker      [Macro ►]  │  ← pnlHeader: lblTitle + btnSwitchView
├──────────────────────────────┤
│                              │
│  clickerView  |  macroView   │  ← pnlViews: both screens live here,
│  (visible)    |  (hidden)    │    only one is visible at a time
│                              │
└──────────────────────────────┘
```

Both screens are created once (in the Designer) and **never destroyed**: switching only moves and hides them. That is why the clicker keeps running, and the hotkeys keep working, while the other screen is on display.

**Why `macroView` starts "visible":** a UserControl that is hidden (`Visible = false`) is not really created until it is first shown: no window handle, no `OnLoad`. Its hotkeys would then only work after the first visit to the Macro screen. So `macroView` starts visible but out of sight (`Left = 284`, outside `pnlViews`), gets created with the window, and `MaxClicks.OnShown` hides it right after the window appears.

### The slide animation

A screen's position inside `pnlViews` is its `Left` property (0 = fully visible). The slide is just changing `Left` a little at a time with a Timer (`slideTimer`, every 15 ms):

```
Before          During                After
[ Clicker ]     [cker ][ Mac]         [ Macro  ]
              ← both move left ←
```

| Method | Job |
|---|---|
| `btnSwitchView_Click` | Picks the other screen and the direction (Macro comes from the right, Clicker comes back from the left). |
| `SlideTo(view, direction)` | Puts the new screen just outside the edge (`Left = ±width`), makes it visible, disables the button, starts `slideTimer`. |
| `slideTimer_Tick` | Moves both screens `width / SlideSteps` pixels. After `SlideSteps` ticks (12 × 15 ms ≈ 0.2 s) calls `FinishSlide`. |
| `FinishSlide()` | Stops the timer, hides the old screen, re-enables the button, updates the header. |
| `UpdateHeader()` | Title shows where you are; the button shows where it takes you. |

`slideDirection` is `1` or `-1`, so the same math works for both directions:

```csharp
int offset = width * slideStep / SlideSteps;        // how far we have moved
currentView.Left = -slideDirection * offset;        // old screen goes out
nextView.Left = slideDirection * (width - offset);  // new screen comes in
```

The movement is linear (same speed all the way). To make it faster or slower, change `SlideSteps`.

---

## Feature: clicking

**Where:** `ClickerView.cs`.

**Controls:** `numInterval` (milliseconds), `cmbButton` / `cmbClickType` (click options), `chkLimit` / `numLimit` (click limit), `btnStart`, `btnStop`, `lblCount`, `clickTimer`.

The **Timer** is the engine. Once started, it raises `Tick` every `Interval` milliseconds, and **each Tick is one click or one double click**:

```csharp
private void clickTimer_Tick(object sender, EventArgs e)
{
    if (SelectedDoubleClick && !OnlyOneClickLeft)
    {
        if (Mouse.DoubleClick(SelectedButton))   // 2 real clicks
            count += 2;
    }
    else
    {
        if (Mouse.Click(SelectedButton))         // 1 real click
            count++;
    }
    UpdateCount();      // show it

    if (LimitReached)   // "Stop after N clicks" reached?
        StopClicking();
}
```

`count += 2` is short for `count = count + 2`; `count++` is short for `count = count + 1`.

`Mouse.Click` / `DoubleClick` return `true` only when Windows took the click, so the counter (and the limit) only count clicks that really happened. Windows refuses them e.g. while the PC is locked or a UAC prompt is on screen; the timer keeps going and the clicks count again once input is accepted.

| Method | Job |
|---|---|
| `StartClicking()` | Resets `count`, copies the interval into the timer, starts the timer, updates the controls. |
| `StopClicking()` | Stops the timer, updates the controls. |
| `ToggleClicking()` | Start if stopped, stop if running (used by the hotkey). |
| `UpdateCount()` | Writes `"Clicks: N"` in the label (`"Clicks: N / limit"` when the click limit is on). |
| `UpdateControls()` | See [One place for the screen state](#one-place-for-the-screen-state). |

The Start/Stop buttons and the hotkey all call these same methods, so the logic exists in one place only.

**Why `System.Windows.Forms.Timer`:** it raises `Tick` on the same thread as the window, so the label can be changed directly. A `System.Timers.Timer` would run on another thread and changing controls from there causes errors.

**Where the click goes:** wherever the cursor is right now.

### Click options (button, single / double)

The "Click" group has two **ComboBox** controls with `DropDownStyle = DropDownList` (pick from the list only, no typing):

| Combo | Item 0 | Item 1 |
|---|---|---|
| `cmbButton` | Left button | Right button |
| `cmbClickType` | Single click | Double click |

The code works with the selected **position** (`SelectedIndex`), not the text, and turns it into real values with two `=>` properties:

```csharp
private MouseButtons SelectedButton => cmbButton.SelectedIndex == 1 ? MouseButtons.Right : MouseButtons.Left;
private bool SelectedDoubleClick => cmbClickType.SelectedIndex == 1;
```

`MouseButtons` is an **enum** that already exists in WinForms (`Left`, `Right`, `Middle`...), so no new type was needed. It is also what gets saved in the settings.

**In `Mouse.cs`:**
- `Click(button)` and `DoubleClick(button)` both call `SendClicks(button, clicks)`, which builds an array of `INPUT` structs ("press, release" per click; flags: left `0x0002`/`0x0004`, right `0x0008`/`0x0010`) and sends it with **one** `SendInput` call, so nothing else can get in between.
- A double click is just two clicks in that array. Windows turns two clicks into a double click when they happen in the same spot within the double-click time (500 ms by default).
- Both return `true` when Windows took every event; `clickTimer_Tick` only counts the click then. A click blocked by a window running as administrator is NOT reported: `SendInput` still returns `true`.

**Counter:** it counts **real clicks**, so a double click adds **2**. With "Double click" at 500 ms the counter goes up by 4 per second, against 2 with "Single click".

**Single clicks can also look like double clicks:** Windows calls two clicks in the same spot a double click when they are less than the double-click time apart (500 ms by default). So "Single click" with an interval under 500 ms also makes programs see double clicks. Nothing to fix in the app: it is how Windows reads clicks.

The combos are locked while running (`UpdateControls`), like the interval. In the Visual Studio Designer they look empty: the selection is set by `LoadSettings`, which doesn't run there.

### Click limit (stop after N clicks)

The "Click limit" group has a **CheckBox** `chkLimit` ("Stop after") and a **NumericUpDown** `numLimit` (1 to 1,000,000, default 100). Unchecked = no limit, the clicker runs until Stop.

After each click, `clickTimer_Tick` asks one `=>` property:

```csharp
private bool LimitReached => chkLimit.Checked && count >= numLimit.Value;
```

When it is `true`, it calls `StopClicking()`, the same method the Stop button uses, so the buttons and fields go back to "stopped" without any extra code.

- `&&` ("and") stops at the first `false`: with the box unchecked, the number is not even looked at.
- `count` is an `int` and `numLimit.Value` a `decimal`; C# converts the `int` to `decimal` by itself to compare them (no data can be lost that way).
- `>=` instead of `==`: safe even if `count` somehow got past the limit.
- It counts **real clicks**, like the counter. With "Double click" a limit of 10 means 5 double clicks.
- **An odd limit with double clicks** (e.g. 5): a double click when only 1 click is missing would end at 6. So `clickTimer_Tick` checks another `=>` property first, and the last Tick does a single click instead (2 + 2 + 1 = 5):

  ```csharp
  private bool OnlyOneClickLeft => chkLimit.Checked && numLimit.Value - count == 1;
  ```

Two event handlers keep the screen in step:

- `chkLimit_CheckedChanged` runs when the box is ticked or unticked and calls `UpdateControls()` (the number field is enabled only while the box is checked) and `UpdateCount()` (to add or remove the "/ limit" part of the label).
- `numLimit_ValueChanged` runs when the number changes and calls `UpdateCount()`, so the label always shows the number in the field. It also fires when `LoadSettings` puts the saved number in the field. (Without it, a saved limit of 500 showed "Clicks: 0 / 100" at startup.)

Both controls are locked while running.

### Known limit: the interval is not exact

The WinForms Timer only fires on Windows' clock "ticks", about every **15.6 ms**, and always rounds the interval **up** to the next tick. Measured on the dev PC (2026-10-16):

| Interval asked | Real clicks/s | Ideal |
|---|---|---|
| 1, 5, 10, 15 ms | ~63 | 1000, 200, 100, 67 |
| 16 ms | ~35 | 62.5 |
| 50 ms | ~16 | 20 |
| 100 ms | ~9 | 10 |

So any value from 1 to 15 ms gave the same speed, and every other value is a bit slower than asked. Asking Windows for a finer clock (`timeBeginPeriod(1)`) did not change this.

**Decision:** the field's minimum is **15 ms** (the fastest the Timer really goes), so the screen doesn't offer speeds the app can't do. Exact timing would need the clicks on a separate thread; it was left as a possible future improvement (see [Decisions](DECISIONS.md) and the To do list in [NOTES.md](../NOTES.md)).

---

## Feature: macro

**Where:** `MacroView.cs` (screen and logic), `MacroStep.cs` (one step), `InputRecorder.cs` (recording), `Mouse.cs` / `Keyboard.cs` / `PhysicalScreen.cs` (playing), `HotkeyButton.cs` (hotkeys).

```
MacroView (the Macro screen)
┌────────────────────────────────┐
│ #  Wait    Action              │  ← lvSteps: one line per step
│ 1  0 ms    Left down (512, 300)│
│ 2  80 ms   Left up (512, 300)  │
│ 3  400 ms  Key down: A         │
├────────────────────────────────┤
│ Wait [80] ms  [Delete] [Clear] │  ← edit the selected step
│ Playback: Repeat [10] times    │
│   [ ] Forever   Pause [500] ms │
│ Change hotkeys: Record [F8]    │
│                 Play   [F7]    │
│ [Record (F8)]    [Play (F7)]   │
│ Run 2 / 10  -  step 3 / 4      │  ← lblStatus
└────────────────────────────────┘
```

### What a macro is

A macro is a list of `MacroStep`. Each step is **"wait `Delay` ms, then do one thing"**:

| `Kind` | Does | Uses |
|---|---|---|
| `KeyDown` / `KeyUp` | Press / release a key | `Key`, `ScanCode`, `Extended` |
| `MouseDown` / `MouseUp` | Move the cursor to `Position`, press / release a button | `Button` (left, right, middle, back, forward), `Position` |
| `Wheel` | Move the cursor to `Position`, turn the wheel | `WheelDelta` (120 per notch, + up / - down) |

A click is **two steps** (down, then up). That way holding a key, holding a button, and dragging (down in one place, up in another) all replay exactly as recorded. Mouse **moves** are not recorded, only where each button/wheel event happened.

### Recording

```
Record (button or hotkey F8)
  └─> StartRecording ─> recorder.Start()   installs the hooks, clears the old macro

You press keys / click anywhere in Windows
  └─> Windows calls InputRecorder.KeyboardHookProc / MouseHookProc
        └─> builds a MacroStep ─> Captured event ─> MacroView.recorder_Captured
              ├─ IsOwnInput? (click on MaxClicks, Record/Play hotkey) → ignored
              ├─ Delay = event time - previous event time   (the first step gets 0)
              └─ steps.Add + one line in the list

Stop recording (button or hotkey F8 again)
  └─> StopRecording ─> recorder.Stop() ─> RemoveUnpairedSteps ─> SaveMacroSettings
```

**Low-level hooks** (`SetWindowsHookEx` with `WH_KEYBOARD_LL` / `WH_MOUSE_LL`): Windows calls our method for every key and mouse event in the whole system, before any program gets it. The method only looks; it always passes the event on (`CallNextHookEx`), so the other programs still receive it. It must be quick: Windows waits for it. The hook methods are kept in fields (`keyboardProc`, `mouseProc`): Windows only holds a raw pointer to them, and if the garbage collector threw them away the app would crash on the next key.

What the recorder leaves out:

- **Injected input** (sent by a program with `SendInput`, e.g. the Auto Clicker running at the same time): the hook flags it (`LLKHF_INJECTED` / `LLMHF_INJECTED`).
- **Key repeats**: holding a key makes Windows send "key down" many times a second; `keysDown` (a `HashSet`) keeps only the first.
- **Input for MaxClicks itself** (`IsOwnInput`): clicks on its window (so pressing "Stop recording" is not recorded) and the Record/Play hotkeys' keys.

Keys **are** recorded while MaxClicks is the active window (it still is right after clicking Record). So they don't act on MaxClicks (e.g. Space pressing the focused "Stop recording" button), `MacroView.ProcessCmdKey` swallows every key while recording.

**The waits** come from the time Windows stamped on each event (`time`, in ms), not from when our code ran, so they are as exact as the events themselves. The first step gets wait 0: the time spent going from the Record button to the first click is not part of the macro.

**Cleaning up (`RemoveUnpairedSteps`)**: a press without a release (e.g. the Ctrl of a Ctrl+F8 hotkey) would leave a key stuck during playback, and a release without a press (a key already held when recording started) is noise. Both are removed. A removed step's wait goes to the next step (`RemoveStep`), so the others keep their timing.

### Playing

```
Play (button or hotkey F7)
  └─> StartPlaying ─> run = 1, stepIndex = 0, playClock.Restart(), playTimer.Start()
playTimer.Tick (~every 15 ms)
  └─> PlayDueSteps: while it is time for the next step
        ├─ PlayStep ─> step.Play() ─> Keyboard.Send / PhysicalScreen.MoveCursor + Mouse.Press...
        │            └─ selects the step in the list, tracks what is held down
        └─ end of a run: last run? → StopPlaying
                         otherwise  → run++, next step planned after "Pause between runs"
Stop (button, hotkey, or last run done)
  └─> StopPlaying ─> ReleaseHeld (lets go of anything still pressed)
```

**Timing without drift:** each step has a planned moment, `nextStepAt`, counted in ms **from Play** (`playClock` is a `Stopwatch`). Each tick plays every step whose moment has passed. The Timer is up to ~15 ms late each time (see [Known limit](#known-limit-the-interval-is-not-exact)), but because the moments are counted from the start and not from the last tick, the lateness **never adds up**: step 500 of a long macro is still at most ~15 ms late.

After each run, `PlayDueSteps` stops and waits for the next tick: a macro whose waits are all 0 (with no pause) would otherwise loop forever inside one tick and freeze the window.

**Repeat:** `numRepeat` times, or until stopped with **Forever**. **Pause between runs** is added after each run, before the next one's first step.

**Nothing stays stuck:** `held` lists the keys/buttons the macro pressed and has not released. Stopping in the middle (e.g. during a long key hold) releases them all (`MacroStep.CreateRelease`).

**Keys:** `Keyboard.Send` sends both the key code and the **scan code** (the physical key's number) recorded by the hook, plus the **extended** flag (right Ctrl/Alt, arrows, Insert, Delete, Home, End, Page Up/Down, numpad Enter and /). Some programs, mostly games, only read the scan code. Keys go to whatever window is active at that moment, like real typing.

**Mouse positions and display scaling:** with scaling above 100%, Windows shows "shrunk" coordinates to apps that don't declare DPI support (like this one), but the hook always reports **real pixels**. `PhysicalScreen` switches the thread to real pixels for one call (`SetThreadDpiAwarenessContext`) to move the cursor or find the window under a point, so recorded and replayed positions match.

### Editing steps

| Control | Does |
|---|---|
| Select a step in `lvSteps` | Shows its wait in `numStepDelay`. |
| `numStepDelay` | Changes the selected step's wait. |
| `btnDeleteStep` / Del key | Deletes the selected step (its wait goes to the next one, see `RemoveStep`). |
| `btnClear` | Deletes all steps (asks first). |

Editing is locked while recording or playing.

### Hotkeys: `HotkeyButton`

The big buttons show their hotkey (`WithHotkey`: "Record (F8)", "Stop (F7)"...), so the small key buttons in "Change hotkeys" are clearly only for changing them.

Record (default **F8**) and Play (default **F7**) each use a `HotkeyButton`: a `Button` subclass that does what the Auto Clicker's hotkey group does, in one control:

- shows the key as its text; click it, then press the new key or combination (Esc, clicking elsewhere, or hiding the screen cancels);
- registers it with `GlobalHotkey` on **its own handle** with its `HotkeyId` (2 = Record, 3 = Play; the Auto Clicker uses 1);
- raises `HotkeyPressed` from its `WndProc` when the hotkey is pressed anywhere, and `HotkeyChanged` when the user picked a new one (MacroView saves it).

The same key can't be used twice: registering a key another hotkey already holds fails, and the user gets the "already in use" warning.

### One place for the screen state

Like the Auto Clicker, `UpdateControls()` is the only method that enables/disables controls, reading the state from `recorder.Running`, `playTimer.Enabled` and the selection:

| State | Record | Play | Step editing | Repeat / Forever / Pause | Hotkey buttons |
|---|---|---|---|---|---|
| Idle | on, "Record" | on if there are steps | on (Wait/Delete need a selected step) | on (Repeat off with Forever) | on |
| Recording | on, "Stop recording" | off | off | on | off |
| Playing | off | on, "Stop" | off | off | off |

`UpdateStatus()` writes the line at the bottom: steps and length of one run when idle, "Recording... N steps", or "Run 2 / 10 - step 3 / 4" while playing.

---

## Feature: global hotkey

**Where:** `ClickerView.cs` (the hotkey belongs to the Auto Clicker screen).

### Who does what

| Piece | Job |
|---|---|
| `GlobalHotkey.Register` | Asks Windows: "reserve this key for my window and tell me when it is pressed". Returns `false` if another program already reserved it. |
| `GlobalHotkey.Unregister` | Gives the reservation back. |
| `WndProc` | **Where Windows' "it was pressed" notice arrives.** Calls `ToggleClicking()`. |
| `ProcessCmdKey` | Captures the new key while the user is choosing one. |

`GlobalHotkey` does **not** detect anything by itself: it only makes the reservation. Windows does the watching and notifies the window.

The hotkey is registered on `ClickerView`'s own handle (a UserControl is also a window for Windows), so the notice goes to `ClickerView.WndProc`, even while the Macro screen is the one on display.

### `WndProc`: the control's mailbox

Windows talks to a window by sending **messages**: numbers like `WM_PAINT` (redraw), `WM_LBUTTONDOWN` (mouse click), `WM_HOTKEY` (your hotkey). **Every** message passes through `WndProc`.

Normally WinForms reads these messages for us and turns them into events (`Click`, `KeyDown`...). There is no ready-made event for `WM_HOTKEY`, so we override `WndProc` to check for it ourselves:

```csharp
protected override void WndProc(ref Message m)
{
    if (m.Msg == GlobalHotkey.WM_HOTKEY && (int)m.WParam == HotkeyId)
        ToggleClicking();

    base.WndProc(ref m);   // ESSENTIAL: pass every message on, or the window stops working
}
```

- `m.Msg`: the message type.
- `m.WParam`: for `WM_HOTKEY`, the id we used when registering (`HotkeyId = 1`).

### `ProcessCmdKey`: catching the new key

Receives every key pressed inside the Auto Clicker screen **before** its controls do (including Tab, Enter, Esc and arrows, which normally get used early for navigation).

- `waitingForHotkey == false` → passes everything on, the keyboard behaves normally.
- `waitingForHotkey == true` →
  - **Esc** → cancel, keep the old hotkey.
  - **Only Ctrl / Shift / Alt / Win** → ignore, keep waiting (so combinations like `Ctrl+F6` work).
  - **Any other key** → `FinishHotkeyCapture(keyData)`.
  - Returns `true` = "handled", so the key does not reach any button.

`keyData` contains the key **plus** the modifiers. `keyData & Keys.KeyCode` keeps only the key.

### Any key is allowed

The user can pick any key or combination, including a single letter or number. Keep in mind that a global hotkey **takes the key away from every program** while MaxClicks is open: with `A` as the hotkey, pressing A anywhere toggles the clicker instead of typing "a". This was a deliberate choice: the user decides.

### No hotkey (`Keys.None`)

If registering fails and the old hotkey can't be registered either (e.g. F6 is already taken when the app opens), `hotkey` becomes `Keys.None` and the screen shows **(none)**, so it never shows a hotkey that doesn't work. `TryRegisterHotkey` skips `Keys.None`, since there is nothing to register.

### Lifecycle

```
App opens
  └─> ClickerView.OnLoad ─> LoadSettings ─> Register(saved hotkey, F6 the first time)
                              (if another program already uses it: warning, hotkey = none)
                              (all skipped inside the Visual Studio Designer)

User clicks "Change"
  └─> btnSetHotkey_Click ─> StartHotkeyCapture ─> Unregister current, waitingForHotkey = true
        (the current key must be released, or Windows would catch it before the window sees it)

User presses Ctrl+F7
  └─> ProcessCmdKey ─> FinishHotkeyCapture
        ├─ Register(Ctrl+F7) ok   → becomes the hotkey, SaveHotkey()
        └─ Register fails         → warning, Register(old one) again (fails too → none)

User presses Ctrl+F7 anywhere in Windows
  └─> Windows sends WM_HOTKEY ─> WndProc ─> ToggleClicking ─> Start / Stop

User switches to Macro while choosing a hotkey
  └─> OnVisibleChanged ─> FinishHotkeyCapture(old one)   (cancel: a hidden screen gets no keys)

User clicks Start / presses the hotkey
  └─> StartClicking ─> SaveClickSettings

App closes
  └─> ClickerView.OnHandleDestroyed ─> Unregister    (frees the key for other programs)
                                     └─> SaveClickSettings
```

---

## Saved settings

**Where:** `Properties/Settings.settings` (the list), the "Saved Settings" region of `ClickerView.cs` and `MacroView.cs` (load/save).

The Auto Clicker options and the macro (steps, playback options, hotkeys) are remembered between runs using **application settings**, a feature of .NET Framework:

1. `Settings.settings` lists each value with a **name**, a **type** and a **default**:

   | Name | Type | Default |
   |---|---|---|
   | `Hotkey` | `System.Windows.Forms.Keys` | `F6` |
   | `Interval` | `int` | `100` |
   | `ClickButton` | `System.Windows.Forms.MouseButtons` | `Left` |
   | `DoubleClick` | `bool` | `False` |
   | `LimitEnabled` | `bool` | `False` |
   | `ClickLimit` | `int` | `100` |
   | `MacroRecordHotkey` | `System.Windows.Forms.Keys` | `F8` |
   | `MacroPlayHotkey` | `System.Windows.Forms.Keys` | `F7` |
   | `MacroSteps` | `string` | (empty) |
   | `MacroRepeat` | `int` | `1` |
   | `MacroForever` | `bool` | `False` |
   | `MacroPause` | `int` | `0` |

2. Visual Studio turns that list into a class, `Properties.Settings`, with one property per value. `Settings.Default` is the single shared instance: `Settings.Default.Hotkey` reads the saved hotkey, `Settings.Default.Hotkey = X` changes it **in memory only**, and `Settings.Default.Save()` writes everything to disk.
3. Scope **User** means each Windows user has their own copy, and the app is allowed to change it. (Scope *Application* would be read-only.)

| Method | When | What |
|---|---|---|
| `LoadSettings()` | `OnLoad`, before registering the hotkey | Copies the saved hotkey into `hotkey`, the interval into `numInterval`, selects the saved button / click type in the combos, and sets the click limit box and number. |
| `SaveHotkey()` | `FinishHotkeyCapture`, only if the new hotkey was registered | Saves `hotkey`. |
| `SaveClickSettings()` | `StartClicking` and `OnHandleDestroyed` (the app is closing) | Saves the interval, the button, single/double and the click limit. Not on every change (the fields can change many times): on Start, so they survive a crash, and on close, so a choice made without starting is kept too. |

**The macro (`MacroView`):** `LoadSettings` in `OnLoad`; `SaveMacroSettings` (steps, repeat, forever, pause) after each recording, on Play and on close; the hotkeys in `hotkey_HotkeyChanged`. `MacroSteps` holds the steps as text: each step is `MacroStep.ToSaveText()` (`Delay,Kind,Key,ScanCode,Extended,Button,X,Y,WheelDelta`, all numbers) and steps are joined with `;`. If any step can't be read back (`FromSaveText` returns `null`), the macro starts empty instead of half-loaded.

**Why the hotkey is saved only on a successful change:** if the saved hotkey is taken by another program when the app opens, the screen shows "(none)". Saving at that moment would replace the user's choice with "none" for good; not saving keeps it, and the next run tries it again.

**Why `LoadSettings` clamps the interval and the limit:** `NumericUpDown.Value` throws an error if given a number outside `Minimum`..`Maximum` (15..60000 for the interval, 1..1,000,000 for the limit), and the settings file is plain text anyone can edit. `Math.Max(min, Math.Min(max, value))` forces the value into the range.

**Where the file lives:** `%LOCALAPPDATA%\AutoClicker\MaxClicks.exe_Url_<hash>\1.0.0.0\user.config`, a small XML file. (`AutoClicker` is the namespace, used because `AssemblyCompany` is empty; `MaxClicks.exe` is the exe name.) Things to know:

- `<hash>` comes from the **exe's path**, so `bin\Debug` and `bin\Release` (or a copy elsewhere) each have their **own** saved settings.
- `1.0.0.0` is the assembly version (`AssemblyInfo.cs`). If it changes, the app starts again from the defaults.

To reset to the defaults, close the app and delete that folder.

The Designer never loads or saves settings (same `inDesigner || DesignMode` check as the hotkey), otherwise it would read and write Visual Studio's own settings.

---

## One place for the screen state

`UpdateControls()` is the **only** method that enables/disables controls and changes the hotkey texts. It does not receive the state as a parameter; it **reads** it:

```csharp
bool running = clickTimer.Enabled;   // is the timer running?
// ...and the waitingForHotkey field
```

and sets every control to match:

| State | Start | Stop | Interval, click combos, "Stop after" box | Limit number | Change button | Hotkey label |
|---|---|---|---|---|---|---|
| Stopped | on | off | on | on if the box is checked | on, "Change" | current hotkey |
| Running | off | on | off | off | off | current hotkey |
| Waiting for a hotkey | off | off | on | on if the box is checked | on, "Cancel" | "Press a key..." |

Every method that changes the state (`StartClicking`, `StopClicking`, `StartHotkeyCapture`, `FinishHotkeyCapture`, `chkLimit_CheckedChanged`, `OnLoad`) just calls `UpdateControls()` at the end. When a new state or control appears, only this method changes.

---

## Code layout (regions)

Regions are grouped **by feature**, and inside each one the methods follow the order things happen, so a region reads top to bottom like a story.

### `MaxClicks.cs`

| Region | Contains (in order) |
|---|---|
| Fields | `currentView`, `nextView`. Slide: `SlideSteps`, `slideStep`, `slideDirection` |
| Constructor | `MaxClicks()` |
| Startup | `OnShown` (hides `macroView` once it exists) |
| Switching Screens | `btnSwitchView_Click` → `SlideTo` → `slideTimer_Tick` → `FinishSlide` |
| UI Updates | `UpdateHeader` |

### `ClickerView.cs`

| Region | Contains (in order) |
|---|---|
| Fields | Clicking: `count`. Hotkey: `HotkeyId`, `hotkey`, `waitingForHotkey`. Designer: `inDesigner` |
| Constructor | `ClickerView()` |
| Control Lifecycle | `OnLoad` (first shown), `OnVisibleChanged` (hidden/shown), `OnHandleDestroyed` (destroyed when the window closes) |
| Clicking | `btnStart_Click`, `btnStop_Click` → `StartClicking`, `StopClicking`, `ToggleClicking` → `clickTimer_Tick` → `LimitReached`, `OnlyOneClickLeft`, `SelectedButton`, `SelectedDoubleClick`, `chkLimit_CheckedChanged`, `numLimit_ValueChanged` |
| Hotkey: Choosing a New One | Steps 1–4: `btnSetHotkey_Click` → `StartHotkeyCapture` → `ProcessCmdKey` → `FinishHotkeyCapture` |
| Hotkey: Registering and Receiving | `TryRegisterHotkey`, `WndProc`, `ShowHotkeyTakenWarning`, `HotkeyToText` |
| Saved Settings | `LoadSettings`, `SaveHotkey`, `SaveClickSettings` |
| UI Updates | `UpdateCount`, `UpdateControls` |

### `MacroView.cs`

| Region | Contains (in order) |
|---|---|
| Fields | `steps`. Recording: `recorder`, `lastStepTime`, `MaxDelay`. Playing: `run`, `stepIndex`, `playClock`, `nextStepAt`, `held`. Designer: `inDesigner` |
| Constructor | `MacroView()` |
| Control Lifecycle | `OnLoad`, `OnHandleDestroyed` |
| Recording | `btnRecord_Click`, `hkRecord_HotkeyPressed` → `ToggleRecording` → `StartRecording`, `StopRecording` → `recorder_Captured` → `IsOwnInput`, `ProcessCmdKey`, `RemoveUnpairedSteps`, `RemoveStep` |
| Playing | `btnPlay_Click`, `hkPlay_HotkeyPressed` → `TogglePlaying` → `StartPlaying`, `StopPlaying` → `playTimer_Tick` → `PlayDueSteps` → `PlayStep`, `ReleaseHeld`, option handlers |
| Editing Steps | `lvSteps_SelectedIndexChanged`, `numStepDelay_ValueChanged`, `btnDeleteStep_Click`, `lvSteps_KeyDown` → `DeleteSelectedStep`, `btnClear_Click`, `SelectedIndex` |
| Hotkeys | `hotkey_HotkeyChanged` |
| Saved Settings | `LoadSettings`, `SaveMacroSettings` |
| UI Updates | `IsBusy`, `RefreshSteps`, `StepToItem`, `WaitText`, `UpdateStatus`, `UpdateControls`, `WithHotkey` |

Event handlers and overrides live in the region of the feature they belong to (e.g. `ProcessCmdKey` is in "Choosing a New One"), not in a separate "handlers" pile.

`#region` changes nothing in the program; it only lets the editor fold blocks.
