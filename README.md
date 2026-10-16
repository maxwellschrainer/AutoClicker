# MaxClicks

A simple auto clicker for Windows, written in C# with Windows Forms (.NET Framework 4.8).

## Features

- Left click at a configurable interval (1–60000 ms).
- Click counter.
- Global hotkey to start/stop from any program (default **F6**, changeable with Ctrl/Alt/Shift combinations).
- Window stays on top of other apps.

## Usage

1. Open `AutoClicker.sln` in Visual Studio and run it (F5).
2. Set the interval in milliseconds.
3. Put the mouse where you want to click and press the hotkey (or click **Start**).
4. Press the hotkey again (or click **Stop**) to stop.

To change the hotkey, click **Change** and press the new key or combination. Esc cancels.

## Documentation

- [How it works](docs/HOW-IT-WORKS.md): a manual of the code and the concepts behind it.
- [NOTES.md](NOTES.md): current state, to-do list and decisions.

## License

MIT, see [LICENSE.txt](LICENSE.txt).
