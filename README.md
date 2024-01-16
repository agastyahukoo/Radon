# Radon - Automated Mouse Clicker for Windows

### Introduction
Welcome to the GitHub repository for Radon, an open-source automated mouse clicker application for Windows. Designed with simplicity and efficiency in mind, Radon allows users to automate mouse clicks at specified screen positions, making it a handy tool for various repetitive tasks.

### Features
- **Automated Clicking:** Automate clicks at predefined screen coordinates.
- **Customizable Click Points:** Add custom click points through the application interface.
- **Adjustable Interval and Loop Count:** Set the interval between clicks and the number of times the sequence should repeat.
- **Hotkey Support:** Easily add new click points using a convenient hotkey combination.
- **User-Friendly Interface:** A simple and intuitive interface for easy operation.

### Getting Started

#### Prerequisites
- Windows Operating System
- .NET Framework

#### Installation
1. Clone the repository to your local machine.
2. Compile the code using a suitable .NET-compatible IDE (e.g., Visual Studio).
3. Run the executable to start using Radon.

### Usage
1. **Add Click Points:** Click the 'Add Point' button or use the hotkey (Ctrl + Alt + F10) to capture the current mouse position as a click point.
2. **Set Interval and Loop Count:** Enter the desired time interval (in milliseconds) between clicks and the number of loops for the click sequence.
3. **Start Automation:** Press the 'Start' button to begin the automated clicking process.

### Development
This application is written in C# and utilizes the Windows Forms framework. Key features include:
- Use of `System.Threading.Timer` for managing click intervals.
- `KeyboardHook` class for global hotkey registration.
- Interoperability with user32.dll for mouse event simulation and hotkey management.

### Contributing
Contributions to Radon are welcome! Whether it's bug reports, feature requests, or code contributions, your input is highly valued. Please read `CONTRIBUTING.md` for guidelines on how to contribute.

### License
Radon is released under the MIT License. See the `LICENSE` file for more details.

### Acknowledgments
- Contributors and maintainers of Radon.
- The .NET community for invaluable resources and support.

### Contact
For support or queries, please open an issue on the GitHub repository.

