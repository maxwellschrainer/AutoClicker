# CLAUDE.md — working on MaxClicks

Guide for AI assistants (and humans) continuing this project. Read this first, then `NOTES.md`.

## Start of every session

1. Read `NOTES.md` (current state, next step, to do, not tested yet).
2. Read the parts of `docs/` related to the task: `HOW-IT-WORKS.md` (how each feature works), `DECISIONS.md` (why), `TESTING.md` (before testing).
3. Check `git status` / `git log -5`: the user often edits code and commits by hand between sessions. Treat the files on disk as the truth.

## End of every step

- Update the docs. Each fact lives in one place; elsewhere, link to it:

  | What changed | Where it goes |
  |---|---|
  | State, next step, to do, untested items, date | `NOTES.md` (short: no history, `git log` has it) |
  | How a feature works (flow, methods, settings) | `docs/HOW-IT-WORKS.md` |
  | A new C# / WinForms concept the user met | `docs/CSHARP-BASICS.md` (+ glossary) |
  | A choice and its reason | `docs/DECISIONS.md` |
  | A new way to test, or a testing trap | `docs/TESTING.md` |
  | User-facing features | `README.md` |
  | Workflow, layout, gotchas | this file |

- Keep NOTES.md "Next session" accurate: anyone opening the project must know where it stands and what comes next.

## Language

Code, comments, UI text, docs and commit messages **in English**. The owner's personal preferences (explanations in pt-BR, step-by-step teaching, git rules) live in their global `~/.claude/CLAUDE.md`.

## Repository layout

```
AutoClicker/                 ← git repo root (this folder)
├─ AutoClicker.sln
├─ README.md                 ← for visitors: what it is, how to use, how to build
├─ CLAUDE.md                 ← this file: workflow, build, gotchas
├─ NOTES.md                  ← current state, next step, to do
├─ LICENSE.txt
├─ docs/
│  ├─ HOW-IT-WORKS.md        ← code manual: files, features, flows
│  ├─ CSHARP-BASICS.md       ← C# concepts used + glossary (the user is learning)
│  ├─ DECISIONS.md           ← why things are the way they are
│  └─ TESTING.md             ← how to test without the user
└─ AutoClicker/              ← the C# project
   ├─ AutoClicker.csproj     ← old-style csproj (see below)
   ├─ Program.cs             ← Main → Application.Run(new MaxClicks())
   ├─ MaxClicks.*            ← main Form: header + switch button + slide between screens
   ├─ ClickerView.*          ← UserControl: auto clicker screen (interval, click options, click limit, hotkey, start/stop, counter)
   ├─ MacroView.*            ← UserControl: macro screen (placeholder, not implemented)
   ├─ Mouse.cs               ← P/Invoke mouse_event: Click(button), DoubleClick(button)
   ├─ GlobalHotkey.cs        ← P/Invoke RegisterHotKey/UnregisterHotKey + WM_HOTKEY
   └─ Properties/Settings.*  ← saved settings (Hotkey, Interval, ClickButton, DoubleClick, LimitEnabled, ClickLimit), mirrored in App.config
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
- **Global hotkeys** are registered on `ClickerView`'s own handle, id `1`. Only one process can hold a key: if a hotkey seems "taken", check who holds it (see `docs/TESTING.md`).
- Use `System.Windows.Forms.Timer` (UI thread) for anything that updates controls. It only fires every ~15.6 ms (rounded up), and `timeBeginPeriod(1)` did not help: see "Known limit" in `docs/HOW-IT-WORKS.md`.
- **Starting the clicker in a test makes real clicks** wherever the cursor is. Only automate Start with a test window under the cursor (see `docs/TESTING.md`).
- Avoid non-ASCII characters in C# string literals; use escapes (`"►"`).
- **Saved settings** (`Properties.Settings`, see `docs/HOW-IT-WORKS.md`) are stored per exe path: the scratchpad build, `bin\Debug` and `bin\Release` each have their own (testing rules in `docs/TESTING.md`). `Settings.settings`, `Settings.Designer.cs` and `App.config` must stay in sync when adding a setting.

## Testing without the user

There are no unit tests. Follow `docs/TESTING.md` (PowerShell recipes, safe real-click test, hotkey checks). Always tell the user what was tested and what was not.

## Code conventions

- Regions by feature, methods in the order things happen (see "Code layout" in `docs/HOW-IT-WORKS.md`).
- Comments explain *why*, not what the line already says.
- Control names: prefix + English (`btnStart`, `lblCount`, `numInterval`, `grpHotkey`, `pnlViews`); handlers `control_Event`.
- One method owns the screen state (`UpdateControls` in `ClickerView`, `UpdateHeader` in `MaxClicks`); state-changing methods call it at the end.
- Overrides always call `base`.
