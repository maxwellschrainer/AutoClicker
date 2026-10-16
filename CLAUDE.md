# CLAUDE.md — working on MaxClicks

Guide for AI assistants (and humans) continuing this project. Read this first, then `NOTES.md`.

## Start of every session

1. Read `NOTES.md` (what is done, in progress, to do, decisions).
2. Read the parts of `docs/HOW-IT-WORKS.md` related to the task (how each feature works).
3. Check `git status` / `git log -5`: the user often edits code and commits by hand between sessions. Treat the files on disk as the truth.

## End of every step

- Update `NOTES.md` (Done / In progress / To do / Decisions, and the date).
- Update `docs/HOW-IT-WORKS.md` whenever a feature or concept is added or changed.
- Update `README.md` if user-facing features change.
- Update this file (layout, Gotchas, Testing) when something non-obvious is learned.
- Keep NOTES.md "Next session" accurate: anyone opening the project must know where it stands and what comes next.
- Suggest improvements/optimizations you noticed (code, structure, docs), even outside the current step. Add the ones not done now to the To do list.
- Don't commit unless asked: the user usually commits and pushes themselves.

## The user

- Beginner learning C# through this project. Explain every change step by step (what, why, which C# concept) **in Portuguese (pt-BR)**.
- Code, comments, UI text, docs and commit messages **in English**.
- They like to review before moving on: show what changed, then wait for feedback before the next feature.

## Repository layout

```
AutoClicker/                 ← git repo root (this folder)
├─ AutoClicker.sln
├─ CLAUDE.md, NOTES.md, README.md, LICENSE.txt
├─ docs/HOW-IT-WORKS.md      ← code manual (human-oriented)
└─ AutoClicker/              ← the C# project
   ├─ AutoClicker.csproj     ← old-style csproj (see below)
   ├─ Program.cs             ← Main → Application.Run(new MaxClicks())
   ├─ MaxClicks.*            ← main Form: header + switch button + slide between screens
   ├─ ClickerView.*          ← UserControl: auto clicker screen (interval, click options, hotkey, start/stop, counter)
   ├─ MacroView.*            ← UserControl: macro screen (placeholder, not implemented)
   ├─ Mouse.cs               ← P/Invoke mouse_event: Click(button), DoubleClick(button)
   ├─ GlobalHotkey.cs        ← P/Invoke RegisterHotKey/UnregisterHotKey + WM_HOTKEY
   └─ Properties/Settings.*  ← saved settings (Hotkey, Interval, ClickButton, DoubleClick), mirrored in App.config
```

## Build

.NET Framework 4.8, Visual Studio 2022 Community. Build to the scratchpad (not `bin/`) so you don't fight Visual Studio over locked files:

```bash
"/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" AutoClicker.sln -nologo -v:minimal -p:OutDir="<scratchpad>/build/"
```

Expect zero errors and zero warnings. The output is `MaxClicks.exe` (assembly name); the project, solution and namespace are still called `AutoClicker`.

## Gotchas (learned the hard way)

- **Old-style csproj**: new `.cs` files are NOT picked up automatically. Add `<Compile Include="X.cs" />` entries (UserControls: `<SubType>UserControl</SubType>` on `X.cs` and `<DependentUpon>X.cs</DependentUpon>` on `X.Designer.cs`).
- **Designer files** (`*.Designer.cs`) are edited by hand here, but keep them in the exact format the VS Designer generates, so the user can keep editing visually. Renaming a component also requires renaming its `*.TrayLocation` metadata in the `.resx`.
- **The VS Designer runs UserControl code** (constructor, `OnLoad`) inside `devenv.exe`. Anything that touches Windows (hotkeys, clicks, files) must be skipped there: `ClickerView` checks `inDesigner || DesignMode` (`inDesigner` is set in the constructor from `LicenseManager.UsageMode`). Forgetting this made Visual Studio hold F6.
- **After changing a UserControl**, the user must rebuild in VS and close/reopen the `[Design]` tab for the Designer to load the new code.
- **Global hotkeys** are registered on `ClickerView`'s own handle, id `1`. Only one process can hold a key: if a hotkey seems "taken", check who holds it (see Testing).
- Use `System.Windows.Forms.Timer` (UI thread) for anything that updates controls. It only fires every ~15.6 ms (rounded up), and `timeBeginPeriod(1)` did not help: see "Known limit" in `docs/HOW-IT-WORKS.md`.
- **Starting the clicker in a test makes real clicks** wherever the cursor is. Don't automate Start unless the user agrees.
- Avoid non-ASCII characters in C# string literals; use escapes (`"►"`).
- **Saved settings** (`Properties.Settings`, see `docs/HOW-IT-WORKS.md`) live in `%LOCALAPPDATA%\AutoClicker\MaxClicks.exe_Url_<hash>\<version>\user.config`, and `<hash>` depends on the exe path: the scratchpad build, `bin\Debug` and `bin\Release` each have their own. When testing, always launch the exe by the same full path (a short `DEVMAS~1` path counts as a different one), and delete the test folder afterwards. `Settings.settings`, `Settings.Designer.cs` and `App.config` must stay in sync when adding a setting.

## Testing without the user

There are no unit tests. Smoke-test the built exe with PowerShell (examples were used successfully):

- Launch with `Start-Process`, wait ~2 s, check `HasExited` / `MainWindowTitle`, then `CloseMainWindow()` (Kill as fallback).
- Screenshot a window: `GetWindowRect` + `Graphics.CopyFromScreen`, then view the PNG.
- Click a button: find its child HWND by text (`EnumChildWindows` + `GetWindowText`) and `SendMessage(h, BM_CLICK=0x00F5)`. UI Automation did not find the WinForms buttons; Win32 did.
- Change a DropDownList ComboBox: they have no findable window text; find them by class name (`GetClassName` contains `COMBOBOX`, in tab order) and `SendMessage(h, CB_SETCURSEL=0x014E, index, 0)`. `SelectedIndex` reads the native selection, so saving picks it up.
- Send a key to the app: `PostMessage(buttonHwnd, WM_KEYDOWN=0x0100, vk, 0)` (goes through `ProcessCmdKey`).
- Check whether a hotkey is free: `RegisterHotKey(IntPtr.Zero, id, MOD_NOREPEAT=0x4000, vk)`; if it returns true, unregister it immediately. F6 = `0x75`, F7 = `0x76`.

Always tell the user what was tested and what was not (e.g. real mouse clicks were never automated).

## Code conventions

- Regions by feature, methods in the order things happen (see "Code layout" in `docs/HOW-IT-WORKS.md`).
- Comments explain *why*, not what the line already says.
- Control names: prefix + English (`btnStart`, `lblCount`, `numInterval`, `grpHotkey`, `pnlViews`); handlers `control_Event`.
- One method owns the screen state (`UpdateControls` in `ClickerView`, `UpdateHeader` in `MaxClicks`); state-changing methods call it at the end.
- Overrides always call `base`.
