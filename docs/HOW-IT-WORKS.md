# How MaxClicks works

A manual for the code: what each file does, how the pieces talk to each other, and the C# / Windows concepts behind them.
If you get lost, start with [The big picture](#the-big-picture), then jump to the part you need.

---

## The big picture

MaxClicks is a Windows Forms app with **one window and two screens**: the **Auto Clicker** and the **Macro** (empty for now). A button in the header slides from one screen to the other. See [Screens and switching](#screens-and-switching).

The Auto Clicker screen does three things:

1. **Clicks the mouse** at a fixed interval while it is running.
2. **Counts** the clicks and shows the number on the screen.
3. **Listens for a global hotkey** (default F6) that starts/stops the clicking from any program.

```
 You ──clicks Start / presses hotkey──> ClickerView (the Auto Clicker screen)
                                            │
                                            ├─ starts the Timer
                                            │
 Timer ──Tick every N ms──────────────────> clickTimer_Tick
                                            ├─ Mouse.LeftClick()   → Windows clicks the mouse
                                            └─ count++ / UpdateCount() → label shows the number
```

---

## Files

| File | What it is |
|---|---|
| `Program.cs` | Entry point. `Main()` runs first and opens the window: `Application.Run(new MaxClicks())`. |
| `MaxClicks.cs` / `.Designer.cs` | **The main window** (the "frame"): header with the title and the switch button, and the area where the screens are shown. Handles the slide between screens. |
| `MaxClicks.resx` | Extra data for the Designer (e.g. where the timer icon sits in the component tray). |
| `ClickerView.cs` / `.Designer.cs` | **The Auto Clicker screen** (a UserControl): interval, hotkey, Start/Stop, counter, and all their logic. |
| `MacroView.cs` / `.Designer.cs` | **The Macro screen** (a UserControl). Only a "coming soon" placeholder for now. |

In every `X.cs` / `X.Designer.cs` pair, the **Designer** file says what is on the screen (controls, positions, texts, which method each event calls) and the other file says what happens (the logic).
| `Mouse.cs` | Sends a real left click through Windows (`user32.dll`). |
| `GlobalHotkey.cs` | Asks Windows to reserve/release a system-wide hotkey (`user32.dll`). |
| `Properties/` | Project settings and resources created by Visual Studio. |

---

## Core concepts

### Class, field, method

- A **class** is a blueprint (`MaxClicks`, `ClickerView`, `Mouse`, `GlobalHotkey`).
- A **field** is a variable declared directly inside the class, outside any method (e.g. `private int count`). It lives as long as the object and every method of the class can see it.
- A **local variable** is declared inside a method. It disappears when the method ends.
- A **method** is a block of code with a name (`StartClicking()`). `void` means it returns nothing.
- `private` means only this class can use it. `public` means any class can.
- `static` means it belongs to the class itself, not to an object. That is why we write `Mouse.LeftClick()` without `new Mouse()`.

### `partial class`: one class, two files

`ClickerView.Designer.cs` and `ClickerView.cs` both start with `partial class ClickerView`. They are **the same class split in two files**: the Designer file holds the screen, the other holds the logic. That is why `btnStart` (declared in the Designer) can be used in `ClickerView.cs`. `MaxClicks` and `MacroView` work the same way.

`InitializeComponent()` lives in the Designer file. The constructor calls it to build the screen:

```csharp
public MaxClicks()          // constructor: runs when "new MaxClicks()" happens
{
    InitializeComponent();  // builds all the controls
}
```

### Inheritance, `override` and `base`

`public partial class MaxClicks : Form` means **MaxClicks inherits from Form**. It gets everything a window already knows how to do for free: title bar, moving, closing, drawing, receiving keys...

`Form` has some methods marked `virtual`, which means "a child class may replace me". `override` is how we replace one:

```csharp
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);   // 1. run the ORIGINAL Form.OnLoad
    // 2. then our extra code
}
```

`base.X()` calls the original version from `Form`. Without it we would **replace** the behavior instead of **adding** to it. For example, the original `OnLoad` is what raises the `Load` event; skipping `base.OnLoad(e)` would silently break anything that listens to `Load`.

### Events and event handlers

An **event** is something that happens (a click, a timer tick). An **event handler** is the method that runs when it happens. The connection is made in the Designer with `+=`:

```csharp
this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
//   "when btnStart is clicked, call btnStart_Click"
```

Naming convention: `controlName_EventName` (`btnStart_Click`, `clickTimer_Tick`).

### Assignment reads right to left

```csharp
clickTimer.Interval = (int)numInterval.Value;
// the value typed in the field  ──>  goes into the timer
```

`(int)` is a **cast**: `numInterval.Value` is a `decimal`, `Interval` needs an `int`.

### Form vs UserControl

- A **Form** is a whole window: title bar, borders, close button. `MaxClicks` is a Form.
- A **UserControl** is a **piece of screen** you design like a form (it has its own Designer and code), but it lives **inside** a window, like a big custom control. `ClickerView` and `MacroView` are UserControls placed inside `MaxClicks`.

After building the project, both appear in the Visual Studio Toolbox and can be opened in the Designer on their own.

### The Designer runs your code

To draw `MaxClicks` in the Designer, Visual Studio creates **real** `ClickerView` and `MacroView` objects, so their constructor and `OnLoad` actually run **inside Visual Studio**. Anything with side effects outside the screen (registering a hotkey, clicking the mouse, opening files) must be skipped there:

```csharp
// constructor: the only place where this check is reliable
inDesigner = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

// OnLoad
if (inDesigner || DesignMode)
    return;   // don't register the hotkey inside Visual Studio
```

Without this, while the `MaxClicks.cs [Design]` tab was open, **Visual Studio itself** held F6: pressing F6 started clicking from inside Visual Studio, and the real app could not use F6 ("already used by another program").

### Calling Windows directly (`DllImport`)

Some things .NET does not offer, so we call Windows functions from `user32.dll`:

```csharp
[DllImport("user32.dll")]
private static extern void mouse_event(...);
```

`extern` means "the code of this method is not here, it is inside the DLL". This is called **P/Invoke**. `Mouse.cs` and `GlobalHotkey.cs` both use it.

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

**Controls:** `numInterval` (milliseconds), `btnStart`, `btnStop`, `lblCount`, `clickTimer`.

The **Timer** is the engine. Once started, it raises `Tick` every `Interval` milliseconds, and **each Tick is one click**:

```csharp
private void clickTimer_Tick(object sender, EventArgs e)
{
    Mouse.LeftClick();  // the real click
    count++;            // +1
    UpdateCount();      // show it
}
```

| Method | Job |
|---|---|
| `StartClicking()` | Resets `count`, copies the interval into the timer, starts the timer, updates the controls. |
| `StopClicking()` | Stops the timer, updates the controls. |
| `ToggleClicking()` | Start if stopped, stop if running (used by the hotkey). |
| `UpdateCount()` | Writes `"Clicks: N"` in the label. |
| `UpdateControls()` | See [One place for the screen state](#one-place-for-the-screen-state). |

The Start/Stop buttons and the hotkey all call these same methods, so the logic exists in one place only.

**Why `System.Windows.Forms.Timer`:** it raises `Tick` on the same thread as the window, so the label can be changed directly. A `System.Timers.Timer` would run on another thread and changing controls from there causes errors.

**Where the click goes:** `Mouse.LeftClick()` clicks wherever the cursor is right now.

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
  └─> ClickerView.OnLoad ─> Register(F6)   (if another program already uses it: warning, hotkey = none)
                                           (skipped inside the Visual Studio Designer)

User clicks "Change"
  └─> btnSetHotkey_Click ─> StartHotkeyCapture ─> Unregister current, waitingForHotkey = true
        (the current key must be released, or Windows would catch it before the window sees it)

User presses Ctrl+F7
  └─> ProcessCmdKey ─> FinishHotkeyCapture
        ├─ Register(Ctrl+F7) ok   → becomes the hotkey
        └─ Register fails         → warning, Register(old one) again (fails too → none)

User presses Ctrl+F7 anywhere in Windows
  └─> Windows sends WM_HOTKEY ─> WndProc ─> ToggleClicking ─> Start / Stop

User switches to Macro while choosing a hotkey
  └─> OnVisibleChanged ─> FinishHotkeyCapture(old one)   (cancel: a hidden screen gets no keys)

App closes
  └─> ClickerView.OnHandleDestroyed ─> Unregister    (frees the key for other programs)
```

**Not saved yet:** the chosen hotkey goes back to F6 when the app restarts.

---

## One place for the screen state

`UpdateControls()` is the **only** method that enables/disables controls and changes the hotkey texts. It does not receive the state as a parameter; it **reads** it:

```csharp
bool running = clickTimer.Enabled;   // is the timer running?
// ...and the waitingForHotkey field
```

and sets every control to match:

| State | Start | Stop | Interval | Change button | Hotkey label |
|---|---|---|---|---|---|
| Stopped | on | off | on | on, "Change" | current hotkey |
| Running | off | on | off | off | current hotkey |
| Waiting for a hotkey | off | off | on | on, "Cancel" | "Press a key..." |

Every method that changes the state (`StartClicking`, `StopClicking`, `StartHotkeyCapture`, `FinishHotkeyCapture`, `OnLoad`) just calls `UpdateControls()` at the end. When a new state or control appears, only this method changes.

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
| Fields | Clicking: `count`. Hotkey: `HotkeyId`, `hotkey`, `waitingForHotkey` |
| Constructor | `ClickerView()` |
| Control Lifecycle | `OnLoad` (first shown), `OnVisibleChanged` (hidden/shown), `OnHandleDestroyed` (destroyed when the window closes) |
| Clicking | `btnStart_Click`, `btnStop_Click` → `StartClicking`, `StopClicking`, `ToggleClicking` → `clickTimer_Tick` |
| Hotkey: Choosing a New One | Steps 1–4: `btnSetHotkey_Click` → `StartHotkeyCapture` → `ProcessCmdKey` → `FinishHotkeyCapture` |
| Hotkey: Registering and Receiving | `TryRegisterHotkey`, `WndProc`, `ShowHotkeyTakenWarning`, `HotkeyToText` |
| UI Updates | `UpdateCount`, `UpdateControls` |

Event handlers and overrides live in the region of the feature they belong to (e.g. `ProcessCmdKey` is in "Choosing a New One"), not in a separate "handlers" pile.

`#region` changes nothing in the program; it only lets the editor fold blocks.

---

## Glossary

| Term | Meaning |
|---|---|
| **Control** | Anything on the window: Button, Label, NumericUpDown, GroupBox, Panel. |
| **Form** | A whole window. |
| **UserControl** | A reusable piece of screen with its own Designer and code, placed inside a Form. |
| **Panel** | An invisible container that groups controls (`pnlHeader`, `pnlViews`). |
| **Component** | Like a control but invisible (the Timer). Shown in the tray under the form in the Designer. |
| **Event / handler** | Something that happens / the method that reacts to it. |
| **Override** | Replacing a method inherited from the parent class. |
| **`base`** | The parent class (`Form`, `UserControl`); `base.X()` runs the original version. |
| **Cast** | Converting a value to another type: `(int)value`. |
| **P/Invoke** | Calling a Windows DLL function from C# (`DllImport` + `extern`). |
| **Handle** | The number Windows uses to identify a window or control (`this.Handle`). |
| **Message** | How Windows notifies a window that something happened (`WM_...`). |
| **Modifier** | Ctrl, Shift, Alt (and Win) combined with a key. |
