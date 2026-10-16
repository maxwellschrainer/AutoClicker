# MaxClicks

An auto clicker and macro tool for Windows, two in one, written in C# with Windows Forms (.NET Framework 4.8).

## Features

- **Auto Clicker**
  - Left or right click, single or double, at a configurable interval (15–60000 ms; Windows timing makes values above 15 ms slightly slower than asked).
  - Click counter, with an optional limit: stop by itself after N clicks.
  - Global hotkey to start/stop from any program (default **F6**, any key or Ctrl/Alt/Shift combination).
  - The hotkey, interval, click options and click limit are remembered between runs.
- **Macro**: coming soon.
- One window, two screens: the button in the header slides between Auto Clicker and Macro.
- Window stays on top of other apps.

## Usage

1. Open `AutoClicker.sln` in Visual Studio and run it (F5).
2. Set the interval in milliseconds and choose the button (left / right) and single or double click. Optionally tick **Stop after** and type how many clicks.
3. Put the mouse where you want to click and press the hotkey (or click **Start**).
4. Press the hotkey again (or click **Stop**) to stop. With a click limit, it also stops by itself.

To change the hotkey, click **Change** and press the new key or combination. Esc cancels.

## Documentation

- [How it works](docs/HOW-IT-WORKS.md): a manual of the code and the concepts behind it.
- [NOTES.md](NOTES.md): current state, to-do list and decisions.

## License

MIT, see [LICENSE.txt](LICENSE.txt).
