# How MaxClicks works

A manual for the code: what each file does, how the pieces talk to each other, and how each feature works.
If you get lost, start with [The big picture](#the-big-picture), then jump to the part you need.

Related docs:
- [C# basics](CSHARP-BASICS.md): the C# concepts used here (classes, `partial`, `override`, events, P/Invoke...) and a glossary.
- [Decisions](DECISIONS.md): why things were done this way.
- [Testing](TESTING.md): how the app is tested.

---

## The big picture

MaxClicks is a Windows Forms app with **one window and two screens**: the **Auto Clicker** and the **Macro** (empty for now). A button in the header slides from one screen to the other. See [Screens and switching](#screens-and-switching).

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
| `MacroView.cs` / `.Designer.cs` | **The Macro screen** (a UserControl). Only a "coming soon" placeholder for now. |
| `Mouse.cs` | Sends real clicks through Windows (`user32.dll`): `Click(button)` and `DoubleClick(button)`, left or right. |
| `GlobalHotkey.cs` | Asks Windows to reserve/release a system-wide hotkey (`user32.dll`). |
| `Properties/Settings.settings` | The list of values saved between runs (hotkey, interval, click options, click limit) and their defaults. Visual Studio generates `Settings.Designer.cs` from it. See [Saved settings](#saved-settings). |
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

Both screens are created once (in the Designer) and **never destroyed**: switching only moves and hides them. That is why the clicker keeps running, and the hotkey keeps working, while the Macro screen is on display.

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
        Mouse.DoubleClick(SelectedButton);   // 2 real clicks
        count += 2;
    }
    else
    {
        Mouse.Click(SelectedButton);         // 1 real click
        count++;
    }
    UpdateCount();      // show it

    if (LimitReached)   // "Stop after N clicks" reached?
        StopClicking();
}
```

`count += 2` is short for `count = count + 2`; `count++` is short for `count = count + 1`.

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
- `Click(button)` sends "press" then "release" for that button (`mouse_event` flags: left `0x0002`/`0x0004`, right `0x0008`/`0x0010`).
- `DoubleClick(button)` just calls `Click` twice. Windows turns two clicks into a double click when they happen in the same spot within the double-click time (500 ms by default), and these happen right after each other.

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

**Where:** `Properties/Settings.settings` (the list), `ClickerView.cs` region "Saved Settings" (load/save).

The hotkey, the interval, the click options and the click limit are remembered between runs using **application settings**, a feature of .NET Framework:

1. `Settings.settings` lists each value with a **name**, a **type** and a **default**:

   | Name | Type | Default |
   |---|---|---|
   | `Hotkey` | `System.Windows.Forms.Keys` | `F6` |
   | `Interval` | `int` | `100` |
   | `ClickButton` | `System.Windows.Forms.MouseButtons` | `Left` |
   | `DoubleClick` | `bool` | `False` |
   | `LimitEnabled` | `bool` | `False` |
   | `ClickLimit` | `int` | `100` |

2. Visual Studio turns that list into a class, `Properties.Settings`, with one property per value. `Settings.Default` is the single shared instance: `Settings.Default.Hotkey` reads the saved hotkey, `Settings.Default.Hotkey = X` changes it **in memory only**, and `Settings.Default.Save()` writes everything to disk.
3. Scope **User** means each Windows user has their own copy, and the app is allowed to change it. (Scope *Application* would be read-only.)

| Method | When | What |
|---|---|---|
| `LoadSettings()` | `OnLoad`, before registering the hotkey | Copies the saved hotkey into `hotkey`, the interval into `numInterval`, selects the saved button / click type in the combos, and sets the click limit box and number. |
| `SaveHotkey()` | `FinishHotkeyCapture`, only if the new hotkey was registered | Saves `hotkey`. |
| `SaveClickSettings()` | `StartClicking` and `OnHandleDestroyed` (the app is closing) | Saves the interval, the button, single/double and the click limit. Not on every change (the fields can change many times): on Start, so they survive a crash, and on close, so a choice made without starting is kept too. |

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

Event handlers and overrides live in the region of the feature they belong to (e.g. `ProcessCmdKey` is in "Choosing a New One"), not in a separate "handlers" pile.

`#region` changes nothing in the program; it only lets the editor fold blocks.
