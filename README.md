# SmartHomeAssistant

╔═════════════════════════════════════╗
║ S M A R T  H O M E  A S S I S T A N ║
║    — Project by Radu Pistea-Nae —   ║
╚═════════════════════════════════════╝


A terminal-based smart home assistant inspired by HAL from *2001: A Space Odyssey*.  
Built in C# using .NET 8, this project mixes practical automation features with sci-fi-inspired personality and design.

---

## What It Can Do

- Displays fire and snowfall visual effects directly in the terminal
- Simulates turning lights on and off
- Adjusts and displays temperature
- Plays a looping music track
- Validates password strength (Weak / Moderate / Strong)
- Enters Safe Mode with password challenge
- Runs a HAL-style "Termination Protocol" with interactive minigames
- Uses system text-to-speech for vocal responses (currently macOS only)

---

## Tech Overview

- Language: C#
- Framework: .NET 8
- Terminal visuals: ANSI and Unicode
- Platform-specific integrations:
  - macOS: `afplay` and `say` for audio/speech
  - Windows: Future support for `System.Media.SoundPlayer` and `System.Speech.Synthesis`

---

## Folder Structure

SmartHomeAssistant/
├── Program.cs
├── SmartHomeAssistant.csproj
├── Assets/
│ └── turbanLoop.wav
├── Audio/
│ └── (cross-platform audio logic here)
├── Voice/
│ └── (speech synthesis logic here)
├── .gitignore
├── README.md

---

## How to Run

### On Windows
1. Open the project in Visual Studio or VSCode.
2. Build and run:

### On macOS
Same steps as above. Make sure .NET 8 SDK is installed.  
macOS-specific features like `afplay` (for audio) and `say` (for speech) are used.

---

## Planned Improvements

- Cross-platform audio player abstraction
- Windows speech synthesis support
- Enhanced terminal effects and transitions
- GUI version using Avalonia or MAUI
- Logs and scheduling features

---

## Notes for Reviewers

This project was developed as part of my coursework and personal exploration of C# and interactive console applications.  
It focuses on both functionality and engagement, with attention to terminal effects, user input, and narrative elements.

---

## Author

Radu Pistea-Nae
With support from online documentation and development tools

---

## License

Free to explore and learn from. All rights to HAL's snarky attitude reserved.

---

⠀⠀⠀⠀⠀⠀⠀⢀⣠⣴⣾⣶⣶⣦⣤⣤⣄⡀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⢀⣴⣿⣿⣿⣿⣿⡿⠿⠿⢿⣿⣿⣿⣿⣦⡀⠀⠀⠀⠀
⠀⠀⠀⣼⣿⣿⣿⠋⠉⣻⣶⣶⣶⣶⣶⣤⣉⠙⠻⣿⣿⣿⣧⠀⠀⠀
⠀⣼⣿⣿⣿⣿⣿⣶⠿⠋⠁⠀⠀⠀⠀⠈⠙⠿⣷⣿⣿⣿⣿⣿⣧⠀
⠀⣿⣿⣿⣿⣿⣿⡟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠹⣿⣿⣿⣿⣿⣿⠀
⢠⣿⣿⣿⣿⣿⡟⠀⠀⠀⠀⠀⣶⣶⠀⠀⠀⠀⠀⠙⣿⣿⣿⣿⣿⡄
⢸⣿⣿⣿⣿⣿⡇⠀⠀⠀⠀⣿⣿⣿⣿⠀⠀⠀⠀⢸⣿⣿⣿⣿⣿⡇
⠘⣿⣿⣿⣿⣿⣧⠀⠀⠀⠀⠈⠛⠛⠁⠀⠀⠀⠀⣼⣿⣿⣿⣿⣿⠃
⠀⢿⣿⣿⣿⣿⣿⣧⡀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣼⣿⣿⣿⣿⣿⡿⠀
⠀⠈⢿⣿⣿⣿⣿⣿⣿⣷⣤⣀⣀⣀⣀⣤⣶⣿⣿⣿⣿⣿⣿⡿⠁⠀
⠀⠀⠀⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠀⠀⠀
⠀⠀⠀⠀⠈⠛⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠛⠁⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠈⠙⠛⠛⠿⠿⠿⠿⠛⠛⠋⠁⠀⠀⠀⠀⠀⠀⠀

