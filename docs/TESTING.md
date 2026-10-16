# Testing MaxClicks

There are no unit tests yet. Changes are checked by building the app and smoke-testing the real exe from PowerShell, plus a manual check in Visual Studio.
What is still untested is tracked in [NOTES.md](../NOTES.md).

---

## Build

.NET Framework 4.8, Visual Studio 2022 Community. Either press **F5** in Visual Studio, or build from a terminal:

```bash
"/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" AutoClicker.sln -nologo -v:minimal -p:OutDir="<output folder>/"
```

Expect zero errors and zero warnings. The output is `MaxClicks.exe`. When Visual Studio is open, build to a folder outside `bin/` so the two don't fight over locked files.

## Safety first

- **Starting the clicker makes real clicks** wherever the cursor is. Only automate Start with a test window under the cursor (see [Real clicks](#real-clicks)).
- **Settings are saved per exe path** (`%LOCALAPPDATA%\AutoClicker\MaxClicks.exe_Url_<hash>\...`, see [Saved settings](HOW-IT-WORKS.md#saved-settings)). Always launch the exe by the same full path (a short `DEVMAS~1` path counts as a different one), and delete the test folder afterwards.
- **A hotkey can only be held by one process.** Check that it is free before and after a test (see [Hotkeys](#hotkeys)).

## Driving the app from PowerShell

These recipes were all used successfully.

| Goal | How |
|---|---|
| Launch / close | `Start-Process`, wait ~2 s, check `HasExited` / `MainWindowTitle`, then `CloseMainWindow()` (Kill as fallback). |
| Screenshot a window | `GetWindowRect` + `Graphics.CopyFromScreen`, then view the PNG. |
| Click a button or toggle a CheckBox | Find its child HWND by text (`EnumChildWindows` + `GetWindowText`) and `SendMessage(h, BM_CLICK=0x00F5)`. UI Automation did not find the WinForms buttons; Win32 did. |
| Change a DropDownList ComboBox | They have no findable window text: find them by class name (`GetClassName` contains `COMBOBOX`, in tab order) and `SendMessage(h, CB_SETCURSEL=0x014E, index, 0)`. `SelectedIndex` reads the native selection, so saving picks it up. |
| Send a key to the app | `PostMessage(buttonHwnd, WM_KEYDOWN=0x0100, vk, 0)` (goes through `ProcessCmdKey`). |
| Check saved settings | Read or edit `user.config` (XML) between runs. |

## Real clicks

Open a small TopMost logging Form right under the cursor (PowerShell + WinForms, counting `MouseDown` / `MouseDoubleClick`), move MaxClicks away from the cursor with `SetWindowPos`, press Start with `BM_CLICK`, and read the counter label with `GetWindowText`. The clicks only land on the test window.

To call `Mouse` directly, load the built exe with `[Reflection.Assembly]::LoadFrom` (the class is public).

## Hotkeys

Check whether a hotkey is free: `RegisterHotKey(IntPtr.Zero, id, MOD_NOREPEAT=0x4000, vk)`; if it returns true, unregister it immediately. F6 = `0x75`, F7 = `0x76`.

If a hotkey seems "taken", another process holds it. A common culprit is Visual Studio itself when the Designer runs UserControl code (see [The Designer runs your code](HOW-IT-WORKS.md#the-designer-runs-your-code)).

## After testing

Always report what was tested and what was not, and add anything left untested to [NOTES.md](../NOTES.md).
