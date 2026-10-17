# C# basics used in MaxClicks

The C# and Windows Forms concepts this project relies on, explained with examples from its own code.
For how the app itself works, see [HOW-IT-WORKS.md](HOW-IT-WORKS.md).

---

## Class, field, method

- A **class** is a blueprint (`MaxClicks`, `ClickerView`, `Mouse`, `GlobalHotkey`).
- A **field** is a variable declared directly inside the class, outside any method (e.g. `private int count`). It lives as long as the object and every method of the class can see it.
- A **local variable** is declared inside a method. It disappears when the method ends.
- A **method** is a block of code with a name (`StartClicking()`). `void` means it returns nothing.
- `private` means only this class can use it. `public` means any class can.
- `static` means it belongs to the class itself, not to an object. That is why we write `Mouse.Click(...)` without `new Mouse()`.

## `partial class`: one class, two files

`ClickerView.Designer.cs` and `ClickerView.cs` both start with `partial class ClickerView`. They are **the same class split in two files**: the Designer file holds the screen, the other holds the logic. That is why `btnStart` (declared in the Designer) can be used in `ClickerView.cs`. `MaxClicks` and `MacroView` work the same way.

`InitializeComponent()` lives in the Designer file. The constructor calls it to build the screen:

```csharp
public MaxClicks()          // constructor: runs when "new MaxClicks()" happens
{
    InitializeComponent();  // builds all the controls
}
```

## Inheritance, `override` and `base`

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

## Events and event handlers

An **event** is something that happens (a click, a timer tick). An **event handler** is the method that runs when it happens. The connection is made in the Designer with `+=`:

```csharp
this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
//   "when btnStart is clicked, call btnStart_Click"
```

Naming convention: `controlName_EventName` (`btnStart_Click`, `clickTimer_Tick`).

## Assignment reads right to left

```csharp
clickTimer.Interval = (int)numInterval.Value;
// the value typed in the field  ──>  goes into the timer
```

`(int)` is a **cast**: `numInterval.Value` is a `decimal`, `Interval` needs an `int`.

## `condition ? a : b` (the ternary operator)

A short `if/else` that gives back a value: "if the condition is true, `a`, otherwise `b`".

```csharp
uint down = button == MouseButtons.Right ? RIGHTDOWN : LEFTDOWN;
// same as:
uint down;
if (button == MouseButtons.Right) down = RIGHTDOWN; else down = LEFTDOWN;
```

## `=>` properties (computed values)

A **property** looks like a field from outside (`SelectedButton`, no parentheses) but runs code every time it is read. With `=>` the code is a single expression:

```csharp
private bool SelectedDoubleClick => cmbClickType.SelectedIndex == 1;
```

Nothing is stored: each read asks the combo box again, so the value is never out of date.

## Form vs UserControl

- A **Form** is a whole window: title bar, borders, close button. `MaxClicks` is a Form.
- A **UserControl** is a **piece of screen** you design like a form (it has its own Designer and code), but it lives **inside** a window, like a big custom control. `ClickerView` and `MacroView` are UserControls placed inside `MaxClicks`.

After building the project, both appear in the Visual Studio Toolbox and can be opened in the Designer on their own.

## Calling Windows directly (`DllImport`)

Some things .NET does not offer, so we call Windows functions from `user32.dll`:

```csharp
[DllImport("user32.dll", SetLastError = true)]
private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
```

`extern` means "the code of this method is not here, it is inside the DLL". This is called **P/Invoke**. `Mouse.cs` and `GlobalHotkey.cs` both use it.

## `struct` and `[StructLayout]`: data shaped for Windows

A **struct** is like a small class that holds values (`INPUT`, `MOUSEINPUT` in `Mouse.cs`). Unlike a class, it is copied when assigned, and an array of structs holds the values themselves, side by side in memory.

When a struct is passed to Windows, its bytes must be in exactly the order and size Windows expects:

```csharp
[StructLayout(LayoutKind.Sequential)]   // keep the fields in this order in memory
private struct MOUSEINPUT
{
    public int dx;
    public int dy;
    public uint mouseData;
    public uint dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;          // IntPtr: 4 bytes on 32-bit, 8 bytes on 64-bit
}
```

`Marshal.SizeOf(typeof(INPUT))` gives the struct's size in bytes; `SendInput` asks for it to check that both sides agree. If the size is wrong, it sends nothing.

A **union** (in C, used by Windows) is a struct whose members share the same memory: only one of them is used at a time. C# has no union keyword; it is done with `[StructLayout(LayoutKind.Explicit)]` and `[FieldOffset(0)]` on each member.

---

## Glossary

| Term | Meaning |
|---|---|
| **Control** | Anything on the window: Button, Label, NumericUpDown, CheckBox, ComboBox, GroupBox, Panel. |
| **Form** | A whole window. |
| **UserControl** | A reusable piece of screen with its own Designer and code, placed inside a Form. |
| **Panel** | An invisible container that groups controls (`pnlHeader`, `pnlViews`). |
| **Component** | Like a control but invisible (the Timer). Shown in the tray under the form in the Designer. |
| **Event / handler** | Something that happens / the method that reacts to it. |
| **Override** | Replacing a method inherited from the parent class. |
| **`base`** | The parent class (`Form`, `UserControl`); `base.X()` runs the original version. |
| **Cast** | Converting a value to another type: `(int)value`. |
| **P/Invoke** | Calling a Windows DLL function from C# (`DllImport` + `extern`). |
| **Struct** | A small value type; copied on assignment. `[StructLayout]` fixes its memory layout for Windows. |
| **Union** | A C struct whose members share the same memory; in C# done with `LayoutKind.Explicit` + `[FieldOffset]`. |
| **Handle** | The number Windows uses to identify a window or control (`this.Handle`). |
| **Message** | How Windows notifies a window that something happened (`WM_...`). |
| **Modifier** | Ctrl, Shift, Alt (and Win) combined with a key. |
| **Enum** | A type with a fixed list of named values (`MouseButtons.Left`, `Keys.F6`). |
| **Property** | Looks like a field, but runs code when read/written (`SelectedButton`). |
| **NumericUpDown** | A number field with up/down arrows; `Value` is a `decimal` between `Minimum` and `Maximum`. |
| **CheckBox** | A box that can be ticked; `Checked` is `true` or `false`, `CheckedChanged` fires when it changes. |
| **ComboBox** | A drop-down list; `SelectedIndex` is the position of the chosen item (0, 1, ...; -1 = none). |
| **Settings** | Values saved on disk between runs (`Properties.Settings.Default`). |
