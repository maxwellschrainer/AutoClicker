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
| Type in a NumericUpDown | Find its inner edit (`GetClassName` contains `EDIT`; tell interval and limit apart by `GetWindowRect` position), select all with `EM_SETSEL=0x00B1 (0, -1)`, then `SendMessage(h, WM_CHAR=0x0102, char, 0)` per character. |
| Read a control's text | Use `SendMessage(h, WM_GETTEXT=0x000D, ...)`. `GetWindowText` from another process does NOT ask the control: for edits it returns stale text. |
| Send a key to the app | `PostMessage(buttonHwnd, WM_KEYDOWN=0x0100, vk, 0)` (goes through `ProcessCmdKey`). |
| Check saved settings | Read or edit `user.config` (XML) between runs. |

## Real clicks

Open a small TopMost logging Form right under the cursor (PowerShell + WinForms, counting `MouseDown` / `MouseDoubleClick`), move MaxClicks away from the cursor with `SetWindowPos`, press Start with `BM_CLICK`, and read the counter label with `GetWindowText`. The clicks only land on the test window.

To call `Mouse` directly, load the built exe with `[Reflection.Assembly]::LoadFrom` (the class is public).

## Macro

- **Recording ignores injected input** on purpose, and everything a script sends (`SendInput`, `keybd_event`, `mouse_event`) is injected. To test it anyway, copy the project to the scratchpad, remove the two `INJECTED` checks in the copy's `InputRecorder.cs`, build the copy and drive it: press Record with `BM_CLICK`, send keys that do nothing in other apps (F13 = `0x7C`) and clicks only on a TopMost test window, press F8, then read the status label and `MacroSteps` in `user.config`. Never change the real `InputRecorder.cs` for this. Delete the copy and its settings folder afterwards.
- **Playback can be**: close the app, write a macro into `MacroSteps` in `user.config` (format in [Saved settings](HOW-IT-WORKS.md#saved-settings); `MouseButtons.Left` = `1048576`, key `A` = `65`, its scan code `30`), relaunch, open the Macro screen and press Play with `BM_CLICK`. Point the mouse steps at a test window (TopMost, logging `MouseDown`/`MouseUp` and the text of a TextBox) and check the counts.
- **Start the test window with `-WindowStyle Normal`**: with `Hidden` or `Minimized`, Windows applies that to the Form too, and the clicks land on whatever is behind it.
- **Keep the test window open longer than the whole test** (no short auto-close), and remember that **keys go to the active window**: if the user is working on the PC at the same time, clicks and keys can land in their apps. Prefer to let the user test playback, or ask them not to touch the PC during it.
- Hotkeys at startup: F6, F7 and F8 must all be taken while the app runs, without opening the Macro screen, and free again after it closes.
- **Load / Save dialogs**: click the button with `PostMessage(BM_CLICK)` (`SendMessage` would wait until the dialog closes). Wait until a `#32770` window with the dialog's title belongs to the process (it can take more than 1 s), put the path in its first visible `Edit` with `WM_SETTEXT`, then `PostMessage(dialog, WM_COMMAND, IDOK=1)`. A warning `MessageBox` is also a `#32770`, titled "MaxClicks".
- **Changing a hotkey**: `BM_CLICK` on the key button, then `PostMessage(button, WM_KEYDOWN, vk)`; Esc = `0x1B`.
- **Screenshots**: crop with `DwmGetWindowAttribute(DWMWA_EXTENDED_FRAME_BOUNDS = 9)`, not `GetWindowRect`: on Windows 11 the window rect includes an invisible border, and the picture would show a strip of whatever is behind the window.
- PowerShell's inline runner can fail to parse some long command lines; put the test in a `.ps1` file in the scratchpad and run it.

## Hotkeys

Check whether a hotkey is free: `RegisterHotKey(IntPtr.Zero, id, MOD_NOREPEAT=0x4000, vk)`; if it returns true, unregister it immediately. F6 = `0x75`, F7 = `0x76`.

If a hotkey seems "taken", another process holds it. A common culprit is Visual Studio itself when the Designer runs UserControl code (see [The Designer runs your code](HOW-IT-WORKS.md#the-designer-runs-your-code)).

## After testing

Always report what was tested and what was not, and add anything left untested to [NOTES.md](../NOTES.md).
