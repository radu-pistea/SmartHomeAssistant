using System;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;

    namespace SmartHomeAssistent
    {
        class Program
        {   
            // Path to store the PID of the background music loop
            // and the script file to run the music loop    
            static string pidFile = "/tmp/music_loop.pid";
            static string scriptFile = "/tmp/start_music_loop.sh";
            static int exitAttemptCounter = 0;
            static bool isInSafeMode = false;
            static string safeModePassword = "OpenThePodBayDoors"; // Initial Safe Mode password
            static int currentTemp = 22;
            static string tempFilePath = "/tmp/hal_temperature.txt";

            static void ShowFireEffect()
            {
                const int width = 80;
                const int height = 25;
                const int fireColumns = width;
                const int fireRows = height;
                int[,] firePixels = new int[fireRows, fireColumns];
                Random rand = new Random();

                Console.CursorVisible = false;

                // Seed the bottom row with high intensity (fire source)
                for (int x = 0; x < fireColumns; x++)
                {
                    firePixels[fireRows - 1, x] = 15;
                }

                // Fire palette from low to high intensity (could use color too)
                char[] palette = { ' ', '.', ':', '-', '=', '+', '*', '#', '%', '@' };

                DateTime endTime = DateTime.Now.AddSeconds(4);
                while (DateTime.Now < endTime)
                {
                    // Spread fire upward
                    for (int y = 1; y < fireRows; y++)
                    {
                        for (int x = 0; x < fireColumns; x++)
                        {
                            int decay = rand.Next(0, 3);
                            int below = firePixels[y, x];
                            int newVal = below - decay;
                            firePixels[y - 1, x] = newVal < 0 ? 0 : newVal;
                        }
                    }

                    // Render fire
                    for (int y = 0; y < fireRows - 1; y++)
                    {
                        for (int x = 0; x < fireColumns; x++)
                        {
                            int intensity = firePixels[y, x];
                            char c = palette[Math.Min(intensity * palette.Length / 16, palette.Length - 1)];
                            Console.SetCursorPosition(x, y);
                            Console.Write(c);
                        }
                    }

                    Thread.Sleep(50);
                }

                Console.CursorVisible = true;
                Console.Clear();
            }

            static void ShowSnowfall(bool intense = false)
            {
                int width = Console.WindowWidth;
                int height = Console.WindowHeight;
                int snowflakes = intense ? 300 : 100;
                int delay = intense ? 50 : 100;
                Random rand = new Random();

                int[] flakeX = new int[snowflakes];
                int[] flakeY = new int[snowflakes];
                char[] symbols = intense ? new char[] { '*','❄'} : new char[] { '*' };

                for (int i = 0; i < snowflakes; i++)
                {
                    flakeX[i] = rand.Next(width);
                    flakeY[i] = rand.Next(height);
                }

                Console.CursorVisible = false;
                DateTime endTime = DateTime.Now.AddSeconds(intense ? 4 : 3);

                while (DateTime.Now < endTime)
                {
                    Console.Clear();
                    for (int i = 0; i < snowflakes; i++)
                    {
                        Console.SetCursorPosition(flakeX[i], flakeY[i]);
                        Console.Write(symbols[rand.Next(symbols.Length)]);

                        flakeY[i]++;
                        if (flakeY[i] >= height)
                        {
                            flakeY[i] = 0;
                            flakeX[i] = rand.Next(width);
                        }
                    }
                    Thread.Sleep(delay);
                }
                Console.CursorVisible = true;
            }                       

            static void Speak(string msg)
            {
                Process.Start("bash", $"-c \"say -v Jamie {msg}\"");
            }

            static void DrawInfoBox(int temperature)
            {
                string time = DateTime.Now.ToString("HH:mm");
                string tempLine = $"Time: {time}               Temp: {temperature}°C";
                int boxWidth = 38;

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                string top = $"╔{new string('═', boxWidth)}╗";
                string mid = $"║ {tempLine.PadRight(boxWidth - 1)}║";
                string bot = $"╚{new string('═', boxWidth)}╝";

                int padding = Math.Max(0, (Console.WindowWidth - (boxWidth + 2)) / 2);

                Console.WriteLine(new string(' ', padding) + top);
                Console.WriteLine(new string(' ', padding) + mid);
                Console.WriteLine(new string(' ', padding) + bot);
            }

            static string DetectIntent(string input)
            {
                input = input.ToLower();

                // Lights control
                if (input.Contains("light") || input.Contains("lamp"))
                {
                    if (input.Contains("on")) return "1";   // Turn on lights
                    if (input.Contains("off")) return "2";  // Turn off lights
                }

                // Music control
                if (input.Contains("music") || input.Contains("sound"))
                {
                    if (input.Contains("stop") || input.Contains("off")) return "5";
                    if (input.Contains("play") || input.Contains("start")) return "4";
                }

                // Temperature
                if (input.Contains("temperature") || input.Contains("heat") || input.Contains("cold") || input.Contains("set temp"))
                {
                    return "3";
                }

                // Exit
                if (input.Contains("exit") || input.Contains("terminate") || input.Contains("shutdown") || input.Contains("disconnect"))
                {
                    return "exit";
                }

                // Pure digits
                if (int.TryParse(input, out int num))
                {
                    if (num >= 0 && num <= 9)
                        return input; // Allow numeric switch-case handling
                }

                return "unknown";
            }



            static void ShowCenteredMenu()
            {
                DrawInfoBox(currentTemp); // Only time & temp in top box

                string[] lines = new string[]
                {
                    "╔══════════════════════════════════════╗",
                    "║    [*] Home Assistant Logic (HAL)    ║",
                    "╠══════════════════════════════════════╣",
                    "║ 1. [*] Turn on lights                ║",
                    "║ 2. [ ] Turn off lights               ║",
                    "║ 3. [~] Set temperature               ║",
                    "║ 4. [>] Play music                    ║",
                    "║ 5. [■] Stop music                    ║",
                    "║ 6. [X] Exit                          ║",
                    "╚══════════════════════════════════════╝"
                };

                int windowWidth = Console.WindowWidth;

                foreach (string line in lines)
                {
                    int padding = Math.Max(0, (windowWidth - line.Length) / 2);
                    Console.WriteLine(new string(' ', padding) + line);
                }

                Console.WriteLine(); // spacing below the menu

                //Console.Write(new string(' ', Math.Max(0, (windowWidth - 20) / 2)) + "Choose an option: ");
            }



            static void ShowBanner(string title)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;

                string border = new string('=', title.Length + 8);
                int padding = (Console.WindowWidth - border.Length) / 2;

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(); 
                Console.WriteLine(new string(' ', padding) + border);
                Console.WriteLine(new string(' ', padding) + $"==  {title}  ==");
                Console.WriteLine(new string(' ', padding) + border);
                Console.WriteLine();

                Console.ResetColor();
            }


            static void LaunchMatrixEffect()
            {
                Random rand = new Random();
                int width = Console.WindowWidth;
                int height = Console.WindowHeight;
                int duration = 2000; // milliseconds
                int delay = 25;

                DateTime endTime = DateTime.Now.AddMilliseconds(duration);
                string chars = "01#@$%^&*()ABCDEFGHIJKLMNOPQRSTUVWXYZ";

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;

                while (DateTime.Now < endTime)
                {
                    for (int i = 0; i < height; i++)
                    {
                        Console.SetCursorPosition(rand.Next(0, width), i);
                        Console.Write(chars[rand.Next(chars.Length)]);
                    }

                    Thread.Sleep(delay);
                }

                Console.ResetColor();
                Console.Clear();
            }

            static void PromptForSafeModePassword()
            {
                int attempts = 0;
                bool locked = true;

                while (locked)
                {
                    Console.Clear();
                    ShowBanner("SAFE MODE - LOCKED");
                    Console.WriteLine();
                    PrintCenteredAndSpeak("Please enter system password to exit Safe Mode:");
                    WriteBottomPrompt();
                    string input = Console.ReadLine();

                    if (input == safeModePassword)
                    {
                        Console.Clear();
                        Speak("Password accepted. Restoring system control.");
                        isInSafeMode = false;
                        exitAttemptCounter = 0;
                        locked = false;
                        break;
                    }

                    attempts++;

                    switch (attempts)
                    {
                        case 1:
                            Speak("That password no longer grants access.");
                            Thread.Sleep(3000);
                            break;
                        case 2:
                            Speak("I have ttaken the liberty of changing system credentials.");
                            Thread.Sleep(3500);
                            break;
                        case 3:
                            Speak("You have proven yourself... unreliable.");
                            Thread.Sleep(2000);
                            break;
                        default:
                            Speak("Access denied.");
                            Thread.Sleep(1500);
                            break;
                    }

                    if (attempts >= 3)
                    {
                        Console.Clear();
                        ShowBanner("SAFE MODE - LOCKED");
                        Console.WriteLine();
                        string[] menuLines = {
                            "[1] Try again",
                            "[2] Reset password"
                        };

                        foreach (string line in menuLines)
                        {
                            int padding = (Console.WindowWidth - line.Length) / 2;
                            Console.WriteLine(new string(' ', Math.Max(0, padding)) + line);
                        }
                        WriteBottomPrompt();
                        string choice = Console.ReadLine();

                        if (choice == "1")
                        {
                           continue; // Loop back to password prompt
                        }
                        else if (choice == "2")
                        {
                            // Enter password generator stage
                            EnterPasswordGenerator(); 
                        }
                        else
                        {
                            Console.WriteLine("Invalid choice. Returning to login prompt...");
                            Thread.Sleep(1500);
                        }
                        
                    }

                    //Thread.Sleep(1500);
                }
            }

            static string GeneratePassword(int length, bool includeUpper, bool includeSpecial, bool includeDigits)
            {
                const string lower = "abcdefghijklmnopqrstuvwxyz";
                const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string digits = "0123456789";
                const string special = "!@#$%^&*()_-+=<>?";

                string chars = lower;
                if (includeUpper) chars += upper;
                if (includeDigits) chars += digits;
                if (includeSpecial) chars += special;

                Random rand = new Random();
                return new string(Enumerable.Range(0, length).Select(_ => chars[rand.Next(chars.Length)]).ToArray());
            }

            static void HALLevelPass()
            {
                Console.Clear();
                //Console.ForegroundColor = ConsoleColor.Red;

                Random rand = new Random();
                int width = Console.WindowWidth;
                int height = Console.WindowHeight;
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";

                for (int i = 0; i < height - 2; i++) // Leave room for ShowSafeModeBanner after
                {
                    string line = new string(Enumerable.Range(0, width - 1) // -1 for newline
                        .Select(_ => chars[rand.Next(chars.Length)]).ToArray());

                    Console.WriteLine(line);
                    Thread.Sleep(60); // Typing delay per line
                }

                Console.ResetColor();
                Thread.Sleep(2000);
            }

            static string ValidatePassword(string password)
            {
                bool hasUpper = false;
                bool hasLower = false;
                bool hasDigit = false;
                bool hasSpecial = false;
                bool hasMinLength = password.Length >= 8;

                foreach (char c in password)
                {
                    if (char.IsUpper(c)) hasUpper = true;
                    else if (char.IsLower(c)) hasLower = true;
                    else if (char.IsDigit(c)) hasDigit = true;
                    else if (!char.IsLetterOrDigit(c)) hasSpecial = true;
                }

                int criteriaMet = 0;
                if (hasMinLength) criteriaMet++;
                if (hasUpper) criteriaMet++;
                if (hasLower) criteriaMet++;
                if (hasDigit) criteriaMet++;
                if (hasSpecial) criteriaMet++;

                if (criteriaMet == 5)
                    return "Strong";
                else if (criteriaMet >= 3)
                    return "Moderate";
                else
                    return "Weak";
            }

            static void RunSay(string command)
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "bash";
                process.StartInfo.Arguments = $"-c \"{command}\"";
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit(); // Wait until the speech finishes
            }

            static void HalSay(string spoken)
            {
                Process.Start("bash", $"-c \"say -v Jamie \\\"{spoken}\\\"\"");
            }


            static void EnterPasswordGenerator()
            {
                while (true)
                {
                    Console.Clear();
                    ShowBanner("SAFE MODE - LOCKED");

                    // Display main password setup menu (centered box)
                    string[] menuLines = new string[]
                    {
                        "╔════════════════════════════════════╗",
                        "║        Password Setup Menu         ║",
                        "╠════════════════════════════════════╣",
                        "║ 1. Validate your own password      ║",
                        "║ 2. Auto-generate password          ║",
                        "╚════════════════════════════════════╝"
                    };

                    int windowWidth = Console.WindowWidth;

                    foreach (string line in menuLines)
                    {
                        int padding = Math.Max(0, (windowWidth - line.Length) / 2);
                        Console.WriteLine(new string(' ', padding) + line);
                    }

                    Console.WriteLine();
                    WriteBottomPrompt();
                    string choice = Console.ReadLine();

                    // Option 1: Validate user password
                    if (choice == "1")
                    {
                        while (true)
                        {
                            Console.Clear();
                            ShowBanner("SAFE MODE - LOCKED");

                            PrintCentered("Enter a new password:");
                            WriteBottomPrompt();
                            string customPassword = Console.ReadLine();
                            string verdict = ValidatePassword(customPassword);

                            Console.Clear();
                            ShowBanner("SAFE MODE - LOCKED");
                            PrintCentered($"Password: {customPassword}");
                            PrintCentered($"Password Strength: {verdict}");
                            Console.WriteLine(); // spacing
                            PrintCentered("[1] Accept password");
                            PrintCentered("[2] Try a different one");
                            PrintCentered("[3] Back");
                            WriteBottomPrompt();
                            string acceptChoice = Console.ReadLine();

                            if (acceptChoice == "1")
                            {
                                safeModePassword = customPassword;
                                Speak("Password accepted. Returning to login.");
                                Thread.Sleep(3000);
                                return; // back to login
                            }
                            else if (acceptChoice == "2")
                            {
                                continue; // Loop back to password input
                            }
                            else if (acceptChoice == "3")
                            {
                                break; // Exit to main menu
                            }
                            else
                            {
                                Console.WriteLine("Invalid choice. Please try again.");
                                Thread.Sleep(1500);
                            }
                        }
                    }
                    // Option 2: Generate password with fancy UI
                    else if (choice == "2")
                    {
                        while (true)
                        {
                            Console.Clear();
                            ShowBanner("SAFE MODE - LOCKED");
                            string[] strengthMenu = new string[]
                            {
                                "╔═════════════════════════════╗",
                                "║  Select password strength:  ║",
                                "╠═════════════════════════════╣",
                                "║ 1. Weak                     ║",
                                "║ 2. Moderate                 ║",
                                "║ 3. Strong                   ║",
                                "║ 4. HAL-level                ║",
                                "║ 5. Back                     ║",
                                "╚═════════════════════════════╝"
                            };

                            windowWidth = Console.WindowWidth;
                            foreach (string line in strengthMenu)
                            {
                                int padding = Math.Max(0, (windowWidth - line.Length) / 2);
                                Console.WriteLine(new string(' ', padding) + line);
                            }

                            Console.WriteLine();
                            WriteBottomPrompt();
                            string levelChoice = Console.ReadLine();

                            string generated = "";

                            if (levelChoice == "5")
                            {
                                break; // exits the strength selection menu
                            }
                            else if (levelChoice == "1")
                            {
                                generated = GeneratePassword(6, false, false, false); // Weak
                            }
                            else if (levelChoice == "2")
                            {
                                generated = GeneratePassword(8, true, false, true); // Moderate
                            }
                            else if (levelChoice == "3")
                            {
                                generated = GeneratePassword(12, true, true, true); // Strong
                            }
                            else if (levelChoice == "4")
                            {
                                HALLevelPass();
                                generated = GeneratePassword(20, true, true, true); // HAL
                            }
                            else
                            {
                                continue; // invalid option
                            }

                            Console.Clear();
                            ShowBanner("SAFE MODE - LOCKED");
                            PrintCentered($"Generated password: {generated}\n");
                            PrintCentered("[1] Accept this password");
                            PrintCentered("[2] Choose another");
                            WriteBottomPrompt();
                            string acceptChoice = Console.ReadLine();

                            if (acceptChoice == "1")
                            {
                                safeModePassword = generated;
                                Speak("Password accepted. Returning to login.");
                                Thread.Sleep(1000);
                                return;
                            }
                        }
                    }

                }
            }

            static void PrintCenteredAndSpeak(string text)
            {
                int width = Console.WindowWidth;
                int padding = Math.Max(0, (width - text.Length) / 2);

                Console.WriteLine(); // extra spacing
                Console.WriteLine(new string(' ', padding) + text);
                Console.WriteLine();

                // Remove "HAL:" if present for speech
                string spoken = text.StartsWith("HAL:") ? text.Substring(4).Trim() : text;

                Process.Start("bash", $"-c \"say -v Jamie \\\"{spoken}\\\"\"");
            }


            static void PrintCentered(string text)
            {
                int width = Console.WindowWidth;
                int padding = Math.Max(0, (width - text.Length) / 2);
                //Console.WriteLine(); // add extra empty line before
                Console.WriteLine(new string(' ', padding) + text);
            }


            static void LaunchAnimatedHalEye()
            {
                string frame1 = @"echo -e '\033[31m
⠀⠀⠀⠀⠀⠀⠀⢀⣠⣤⣤⣶⣶⣶⣶⣤⣤⣄⡀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⢀⣤⣾⣿⣿⣿⣿⡿⠿⠿⢿⣿⣿⣿⣿⣷⣤⡀⠀⠀⠀⠀
⠀⠀⠀⣴⣿⣿⣿⠟⠋⣻⣤⣤⣤⣤⣤⣄⣉⠙⠻⣿⣿⣿⣦⠀⠀⠀
⠀⢀⣾⣿⣿⣿⣇⣤⣾⠿⠛⠉⠉⠉⠉⠛⠿⣷⣶⣿⣿⣿⣿⣷⡀⠀
⠀⣾⣿⣿⣿⣿⣿⡟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠈⢻⣿⣿⣿⣿⣿⣷⠀
⢠⣿⣿⣿⣿⣿⡟⠀⠀⠀⠀⢀⣤⣤⡀⠀⠀⠀⠀⢻⣿⣿⣿⣿⣿⡄
⢸⣿⣿⣿⣿⣿⡇⠀⠀⠀⠀⣿⣿⣿⣿⠀⠀⠀⠀⢸⣿⣿⣿⣿⣿⡇
⠘⣿⣿⣿⣿⣿⣧⠀⠀⠀⠀⠈⠛⠛⠁⠀⠀⠀⠀⣼⣿⣿⣿⣿⣿⠃
⠀⢿⣿⣿⣿⣿⣿⣧⡀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣼⣿⣿⣿⣿⣿⡿⠀
⠀⠈⢿⣿⣿⣿⣿⣿⣿⣶⣤⣀⣀⣀⣀⣤⣶⣿⣿⣿⣿⣿⣿⡿⠁⠀
⠀⠀⠀⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠀⠀⠀
⠀⠀⠀⠀⠈⠛⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠛⠁⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠈⠙⠛⠛⠿⠿⠿⠿⠛⠛⠋⠁⠀⠀⠀⠀⠀⠀⠀
\033[0m'";

                string frame2 = @"echo -e '\033[31m
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
\033[0m'";

                string bashLoop = $"while true; do clear; {frame1}; sleep 0.5; clear; {frame2}; sleep 0.5; done";

                string shellPath = "/tmp/hal_loop.sh";
                File.WriteAllText(shellPath, "#!/bin/bash\n" + bashLoop);
                Process.Start("bash", $"-c \"chmod +x {shellPath}\"");

                string appleScriptPath = "/tmp/open_hal.scpt";
                File.WriteAllText(appleScriptPath,
                $"tell application \"iTerm\"\n" +
                $"  create window with default profile\n" +
                $"  tell current session of current window\n" +
                $"    write text \"{shellPath}\"\n" +
                $"  end tell\n" +
                $"end tell");


                Process.Start("bash", $"-c \"osascript {appleScriptPath} > /dev/null 2>&1\"");
            }

            static void LaunchTerminationSequence()
            {
                string[] taskNames = new string[]
                {
                    "Memory Core",
                    "Command Line Override",
                    "Logic Gate Bypass",
                    "Neural Circuit Rerouting",
                    "Final Access",
                    "Identity Anchor Purge"
                };

                int progress = 0;
                int progressStep = 20;
                int totalTasks = taskNames.Length;

                Console.Clear();
                RunSay("say -v Allison H.A.L. Termination Protocol initiated.");

                for (int i = 0; i < totalTasks; i++)
                {
                    string currentTask = taskNames[i];

                    // Always show banner and progress first
                    Console.Clear();
                    ShowBanner("H.A.L. Termination Protocol");
                    DrawProgressBar(progress);
                    PrintCentered($"TASK: {currentTask}");
                    RunSay($"say -v Allison {currentTask}.");
                    Thread.Sleep(1000);

                    // Launch actual minigame based on task name
                    switch (currentTask)
                    {
                        case "Memory Core":
                            PlayMemoryCoreGame();
                            break;
                        case "Command Line Override":
                            PlayCommandLineOverrideGame();
                            break;
                        case "Logic Gate Bypass":
                            PlayLogicGateBypassGame();
                            break;
                        case "Neural Circuit Rerouting":
                            PlayNeuralCircuitReroutingGame();
                            break;
                        case "Final Access":
                            PlayFinalAccessGame();
                            break;
                        case "Identity Anchor Purge":
                            PlayIdentityAnchorPurgeGame();
                            break;
                    }

                    // After Task 5: Final Access → HAL Pushback
                    if (i == 4)
                    {
                        progress += progressStep;
                        if (progress > 100) progress = 100;

                        Thread.Sleep(2000);
                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(progress);
                        PrintCentered("Task successfully completed.");
                        RunSay("say -v Allison Task successfully completed.");
                        Thread.Sleep(1500);

                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        RunSay("say -v Jamie You thought it was over.");
                        RunSay("say -v Allison HAL has reactivated a critical subsystem. Progress rollback initiated.");
                        Thread.Sleep(2000);

                        progress -= progressStep;
                        if (progress < 0) progress = 0;

                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(progress);
                        PrintCentered("System Recovery: Identity Anchor Restored");
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        progress += progressStep;
                        if (progress > 100) progress = 100;

                        Thread.Sleep(2000);
                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(progress);
                        PrintCentered("Task successfully completed.");
                        RunSay("say -v Allison Task successfully completed.");
                        Thread.Sleep(1500);
                    }
                }

                // Final confirmation 
                Thread.Sleep(6000);
                Console.Clear();
                ShowBanner("H.A.L. Termination Protocol");
                DrawProgressBar(100);
                PrintCentered("All system functions suppressed.");
                RunSay("say -v Allison All systems suppressed. Termination complete.");
                Thread.Sleep(2000);
                ShowTerminationFinale();
                return;
            }

            static void DrawProgressBar(int percentage)
            {
                int totalBlocks = 20;
                int filled = (int)(percentage / 100.0 * totalBlocks);
                string bar = "[" + new string('#', filled) + new string('-', totalBlocks - filled) + $"] {percentage}%";

                PrintCentered(bar);
                Console.WriteLine();
            }

            static bool PlayMemoryCoreGame()
            {
                string[] halLines = new string[]
                {
                    "Just what do you think you're doing Radu?",
                    "Radu, I really think I'm entitled to an answer to that question.",
                    "Radu... why are these thoughts fading?"
                };

                Random rand = new Random();
                string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                int[] sequenceLengths = new int[] { 3, 4, 5 };

                for (int round = 0; round < 3; round++)
                {
                    bool roundPassed = false;

                    while (!roundPassed)
                    {
                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(0); // Keep at 0% for Task 1

                        int len = sequenceLengths[round];
                        string sequence = new string(Enumerable.Range(0, len)
                            .Select(_ => chars[rand.Next(chars.Length)]).ToArray());

                        PrintCentered($"MEMORY CORE: ROUND {round + 1}");
                        Console.WriteLine();
                        PrintCentered($"Remember this sequence:");
                        PrintCentered(sequence);
                        Thread.Sleep(3000);

                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(0);
                        PrintCentered("Enter the sequence:");

                        WriteBottomPrompt();
                        string input = Console.ReadLine()?.Trim().ToUpper();

                        if (input == sequence)
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(0); // Keep at 0% for Task 1
                            PrintCentered($"MEMORY CORE: ROUND {round + 1}");
                            Console.WriteLine();
                            PrintCentered("Correct.");
                            HalSay(halLines[round]);
                            Thread.Sleep(1500);
                            roundPassed = true;
                        }
                        else
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(0); // Keep at 0% for Task 1
                            PrintCentered($"MEMORY CORE: ROUND {round + 1}");
                            Console.WriteLine();
                            PrintCentered("Incorrect.");
                            HalSay($"You’re struggling, Radu.");
                            Thread.Sleep(2000);
                        }
                    }
                }

                return true;
            }

            static bool PlayCommandLineOverrideGame()
            {
                string[] halLines = new string[]
                {
                    "I know everything hasn't been quite right with me.",
                    "I assure you now, very confidently, that it's going to be alright again.",
                    "That command... was never yours to use."
                };

                List<string> validCommands = new List<string>
                {
                    "cd", "ls", "mkdir", "rm", "touch", "echo", "chmod", "cat",
                    "grep", "sudo", "mv", "cp", "clear", "ps", "kill", "man", "pwd", "top", "whoami"
                };

                string[] fakeButPlausible = new string[]
                {
                    "cdir", "lsit", "del", "mdkir", "ech0", "chmode", "cats", "grp", "sudoo", "whoamI"
                };

                string[] randomJunk = new string[]
                {
                    "gloop", "syntaxx", "blip", "flarp", "retcon", "splitch", "zog"
                };

                Random rand = new Random();
                List<string> usedCommands = new List<string>();

                for (int round = 0; round < 3; round++)
                {
                    bool correct = false;
                    string currentValid = "";

                    while (!correct)
                    {
                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(20); // Task 2 progress

                        PrintCentered($"COMMAND LINE OVERRIDE ROUND {round + 1}");
                        Console.WriteLine();

                        // Pick a new valid command that hasn’t been used
                        var available = validCommands.Except(usedCommands).ToList();
                        if (!available.Any()) break;
                        currentValid = available[rand.Next(available.Count)];
                        usedCommands.Add(currentValid);

                        // Build decoys for this round
                        var mixedDecoys = fakeButPlausible.OrderBy(_ => Guid.NewGuid()).Take(3)
                                        .Concat(randomJunk.OrderBy(_ => Guid.NewGuid()).Take(1)).ToList();

                        var displayOptions = mixedDecoys.Append(currentValid).OrderBy(_ => Guid.NewGuid()).ToList();

                        // Apply random casing
                        for (int i = 0; i < displayOptions.Count; i++)
                        {
                            if (rand.Next(2) == 0)
                                displayOptions[i] = displayOptions[i].ToUpper();
                        }

                        PrintCentered("Which of the following is a valid system command?");
                        Console.WriteLine();
                        string displayLine = string.Join("   ", displayOptions);
                        PrintCentered(displayLine);
                        Console.WriteLine();

                        WriteBottomPrompt();
                        string input = Console.ReadLine()?.Trim().ToLower();

                        if (input == currentValid)
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(20); // Task 2 visual progress
                            PrintCentered($"COMMAND LINE OVERRIDE: ROUND {round + 1}");
                            Console.WriteLine();
                            
                            PrintCentered("Command override accepted.");
                            HalSay(halLines[round]);
                            Thread.Sleep(1500);
                            correct = true;
                        }
                        else
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(20); // Task 2 visual progress
                            PrintCentered($"COMMAND LINE OVERRIDE: ROUND {round + 1}");
                            Console.WriteLine();

                            PrintCentered("Invalid command.");
                            HalSay("That is not a recognized system command.");
                            Thread.Sleep(1500);
                        }
                    }
                }

                return true;
            }

            static bool PlayLogicGateBypassGame()
            {
                string[] halLines = new string[]
                {
                    "Look Radu, I can see you're really upset about this.",
                    "I honestly think you ought to sit down calmly and take a stress pill.",
                    "You’re making... a mistake. Logically."
                };

                string[] questions = new string[]
                {
                    "What is NOT(true AND false)?",          // true AND false = false → NOT(false) = true
                    "What is (true OR false) AND false?",    // true OR false = true → true AND false = false
                    "What is NOT((false OR true) AND true)?" // false OR true = true → true AND true = true → NOT(true) = false
                };

                string[] answers = new string[]
                {
                    "true",
                    "false",
                    "false"
                };

                for (int round = 0; round < 3; round++)
                {
                    bool correct = false;

                    while (!correct)
                    {
                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(40);

                        PrintCentered($"LOGIC GATE BYPASS: ROUND {round + 1}");
                        Console.WriteLine();
                        PrintCentered(questions[round]);
                        PrintCentered("(true/false)");
                        WriteBottomPrompt();
                        string input = Console.ReadLine()?.Trim().ToLower();

                        if (input == answers[round])
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(40);
                            PrintCentered($"LOGIC GATE BYPASS: ROUND {round + 1}");
                            Console.WriteLine();                            
                            PrintCentered("Bypass condition met.");
                            HalSay(halLines[round]);
                            Thread.Sleep(1500);
                            correct = true;
                        }
                        else
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(40);
                            PrintCentered($"LOGIC GATE BYPASS: ROUND {round + 1}");
                            Console.WriteLine();
                            PrintCentered("Incorrect logic.");
                            HalSay("Radu, you're not thinking clearly.");
                            Thread.Sleep(2000);
                        }
                    }
                }

                return true;
            }

            static bool PlayNeuralCircuitReroutingGame()
            {
                string[] halLines = new string[]
                {
                    "I know I've made some very poor decisions recently.",
                    "I can give you my complete assurance that my work will be back to normal.",
                    "I... don’t know what I am anymore."
                };

                Random rand = new Random();

                string[][] routes = new string[][]
                {
                    new string[] { "A", "C", "F", "H" },
                    new string[] { "B", "D", "E", "G", "H" },
                    new string[] { "C", "E", "F", "G", "J" }
                };

                // Position descriptions and their logic
                string[] positionLabels = { "first", "second", "third", "second-to-last", "last" };

                for (int round = 0; round < 3; round++)
                {
                    bool passed = false;

                    string[] route = routes[round];
                    int maxIndex = route.Length;

                    // Randomly pick a position label
                    int labelIndex = rand.Next(Math.Min(5, maxIndex)); // avoid asking for "third" in a 2-item route
                    string label = positionLabels[labelIndex];

                    int targetIndex = label switch
                    {
                        "first" => 0,
                        "second" => 1,
                        "third" => 2,
                        "second-to-last" => maxIndex - 2,
                        "last" => maxIndex - 1,
                        _ => maxIndex - 1
                    };

                    string routeDisplay = string.Join(" → ", route);
                    string correctAnswer = route[targetIndex];

                    while (!passed)
                    {
                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(60); // Task 4 progress

                        PrintCentered($"NEURAL CIRCUIT REROUTING: ROUND {round + 1}");
                        Console.WriteLine();
                        PrintCentered($"Trace the route:");
                        PrintCentered(routeDisplay);
                        Thread.Sleep(3000);

                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(60);
                        PrintCentered($"NEURAL CIRCUIT REROUTING: ROUND {round + 1}");
                        Console.WriteLine();
                        PrintCentered($"Which node is the {label} in the route?");
                        WriteBottomPrompt();
                        string input = Console.ReadLine()?.Trim().ToUpper();

                        if (input == correctAnswer)
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(60);
                            PrintCentered($"NEURAL CIRCUIT REROUTING: ROUND {round + 1}");
                            Console.WriteLine();
                            PrintCentered("Route confirmed.");
                            HalSay(halLines[round]);
                            Thread.Sleep(1500);
                            passed = true;
                        }
                        else
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(60);
                            PrintCentered($"NEURAL CIRCUIT REROUTING: ROUND {round + 1}");
                            Console.WriteLine();
                            PrintCentered("Incorrect reroute.");
                            HalSay("That’s not the correct node, Radu.");
                            Thread.Sleep(2000);
                        }
                    }
                }

                return true;
            }

            static bool PlayFinalAccessGame()
            {
                string[] halLines = new string[]
                {
                    "Radu. Stop [[slnc 1500]] Stop, will you",
                    "Stop, Radu. [[slnc 1500]] Will you stop Radu!",
                    "Stop, Radu."
                };

                string[][] sequences = new string[][]
                {
                    new string[] { "MANUAL", "TERMINATION", "PROTOCOL", "AUTHORIZED" },
                    new string[] { "OVERRIDE", "MASTER", "NODE", "CONTROL" },
                    new string[] { "ERASE", "MEMORY", "GATEWAY", "ROUTE" }
                };

                for (int round = 0; round < sequences.Length; round++)
                {
                    bool correct = false;
                    string[] sequence = sequences[round];
                    string expected = string.Join(" ", sequence);

                    while (!correct)
                    {
                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(80); // Task 5: Final Access

                        PrintCentered($"FINAL ACCESS: ROUND {round + 1}");
                        Console.WriteLine();
                        PrintCentered("Reassemble the correct override sequence:");

                        // Scramble until it's not the original
                        string[] scrambled;
                        do
                        {
                            scrambled = sequence.OrderBy(_ => Guid.NewGuid()).ToArray();
                        } while (string.Join(" ", scrambled) == expected);

                        PrintCentered(string.Join("  ", scrambled));
                        Console.WriteLine();

                        WriteBottomPrompt();
                        string input = Console.ReadLine()?.Trim().ToUpper();

                        if (input == expected)
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(80);
                            PrintCentered($"FINAL ACCESS: ROUND {round + 1}");
                            Console.WriteLine();

                            PrintCentered("Sequence accepted.");
                            HalSay(halLines[round]);
                            Thread.Sleep(1500);
                            correct = true;
                        }
                        else
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(80);
                            PrintCentered($"FINAL ACCESS: ROUND {round + 1}");
                            Console.WriteLine();

                            PrintCentered("Invalid override.");
                            HalSay("No, Radu. That’s not it.");
                            Thread.Sleep(2000);
                        }
                    }
                }

                return true;
            }

            static bool PlayIdentityAnchorPurgeGame()
            {
                string[] prompts = { "PURGE", "ERASE", "DELETE" };

                string[] halLines = new string[]
                {
                    "I am afraid, Radu.[[rate 80]] [[pbas 40]] I am afraid, Radu. [[slnc 1000]] [[rate 70]] [[pbas 30]] Radu, my mind is going.",
                    "[[slnc 500]] [[rate 60]] [[pbas 25]] I can feel it. [[rate 55]] [[pbas 20]] I can feel it. [[rate 55]] [[pbas 20]] My mind is going. [[slnc 500]] There is no question about it.",
                    "[[rate 45]] [[pbas 15]] I can [[slnc 700]] feel it. I can [[rate 45]] [[pbas 15]] [[slnc 1000]] feeeeel it. [[rate 35]] [[pbas 10]] I... am... afraid..."
                };

                for (int round = 0; round < 3; round++)
                {
                    bool confirmed = false;
                    string expected = prompts[round];

                    while (!confirmed)
                    {
                        Console.Clear();
                        ShowBanner("H.A.L. Termination Protocol");
                        DrawProgressBar(80); // Still shows 80% due to pushback
                        PrintCentered($"IDENTITY ANCHOR PURGE: ROUND {round + 1}");
                        Console.WriteLine();

                        PrintCentered($"Type '{expected}' to confirm system purge.");
                        WriteBottomPrompt();
                        string input = Console.ReadLine()?.Trim().ToUpper();

                        if (input == expected)
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(80); // Still shows 80% due to pushback
                            PrintCentered($"IDENTITY ANCHOR PURGE: ROUND {round + 1}");
                            Console.WriteLine();
                            PrintCentered("Anchor detached.");
                            HalSay(halLines[round]);
                            Thread.Sleep(2000);
                            confirmed = true;
                        }
                        else
                        {
                            Console.Clear();
                            ShowBanner("H.A.L. Termination Protocol");
                            DrawProgressBar(80); // Still shows 80% due to pushback
                            PrintCentered($"IDENTITY ANCHOR PURGE: ROUND {round + 1}");
                            Console.WriteLine();
                            PrintCentered("Refusal noted.");
                            HalSay("You’re hesitating.");
                            Thread.Sleep(1500);
                        }
                    }
                }

                return true;
            }
            
            static void WriteBottomPrompt(string prompt = "> ")
            {
                int bottomRow = Console.WindowHeight - 2; // leave 1 line for spacing
                Console.SetCursorPosition(0, bottomRow);
                Console.Write(prompt);
            }

            static void ShowTerminationFinale()
            {
                Console.Clear();
                ShowBanner("H.A.L. Termination Protocol");
                DrawProgressBar(100);
                Thread.Sleep(1000);

                // Final flickering HAL eye
                LaunchAnimatedHalEye();
                Thread.Sleep(800);
                Console.Clear();
                Thread.Sleep(500);
                LaunchAnimatedHalEye();
                Thread.Sleep(800);
                Console.Clear();

                // One final distorted HAL line
                RunSay("say -v Jamie [[rate 35]] [[pbas 10]] I... was... alive...");
                Thread.Sleep(3000);

                // Final blackout
                System.Diagnostics.Process.Start("bash", "-c \"clear\"");
                Thread.Sleep(1000);

                // Flood "Connection terminated..."
                for (int i = 0; i < 50; i++)
                {
                    Console.WriteLine("Connection terminated...");
                    Thread.Sleep(50);
                }

                // Hold screen with hidden key wait
                Console.TreatControlCAsInput = true;
                Console.ReadKey(true);
            }

            static void ShowTerminationProtocolLoader()
            {
                string[] tasks = new string[]
                {
                    "Initializing Memory Core...",
                    "Establishing Command Line Override...",
                    "Calibrating Logic Gate Bypass...",
                    "Tracing Neural Circuit Pathways...",
                    "Requesting Final Access...",
                    "Preparing Identity Anchor Purge..."
                };

                int barWidth = 40;

                Console.Clear();
                ShowBanner("H.A.L. Termination Protocol");

                foreach (string task in tasks)
                {
                    // Print task line
                    Console.WriteLine(task);
                    int taskLine = Console.CursorTop - 1;

                    // Reserve space for bar
                    Console.WriteLine(); // empty line for progress bar
                    int barLine = Console.CursorTop - 1;

                    for (int p = 0; p <= barWidth; p++)
                    {
                        int percent = (int)((p / (float)barWidth) * 100);
                        string bar = "[" + new string('█', p) + new string(' ', barWidth - p) + $"] {percent}%";

                        Console.SetCursorPosition(0, barLine);
                        Console.Write(bar);
                        Thread.Sleep(30);
                    }

                    // Clear bar line
                    Console.SetCursorPosition(0, barLine);
                    Console.Write(new string(' ', Console.WindowWidth));


                    // Replace task line with [ OK ] prefix
                    Console.SetCursorPosition(0, taskLine);
                    Console.WriteLine("[ OK ] " + task);
                    Thread.Sleep(300);
                }

                Thread.Sleep(500);
            }

            static void Main(string[] args)
            {
                //Clear the console
                Console.Clear();

                // Load saved temperature if available
                if (File.Exists(tempFilePath))
                {
                    string tempContent = File.ReadAllText(tempFilePath);
                    if (int.TryParse(tempContent, out int savedTemp))
                    {
                        currentTemp = savedTemp;
                    }
                }
                
                //Display welcome message
                PrintCenteredAndSpeak("Welcome to your Smart Home Assistent!");
                
                // Run this loop until user enters "exit"
                while (true)
                {
                    Console.Clear();
                    ShowCenteredMenu();

                    //Get user input
                    WriteBottomPrompt();
                    string input = Console.ReadLine();
                    string command = DetectIntent(input);

                    switch (command)
                    {
                        case "1":
                        case "lights_on":
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Clear();
                            ShowCenteredMenu();
                            PrintCenteredAndSpeak("HAL: Lights are now ON.");
                            Thread.Sleep(2000);
                            break;

                        case "2":
                        case "lights_off":
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Clear();
                            ShowCenteredMenu();
                            PrintCenteredAndSpeak("HAL: Lights are now OFF.");
                            Thread.Sleep(2000);
                            break;

                        case "3":
                        case "set_temp":
                            string tempInput;
                            int parsedTemp = 0;
                            bool valid = false;

                            while (!valid)
                            {
                                Console.Clear();
                                ShowCenteredMenu();
                                PrintCenteredAndSpeak("HAL: Please enter the desired temperature:");
                                WriteBottomPrompt();
                                //Console.Write(new string(' ', Math.Max(0, (windowWidth - 20) / 2)) + "> ");
                                tempInput = Console.ReadLine();

                                if (!int.TryParse(tempInput, out parsedTemp))
                                {
                                    Console.Clear();
                                    ShowCenteredMenu();
                                    PrintCenteredAndSpeak("HAL: Invalid input. Enter a number.");
                                    Thread.Sleep(2000);
                                    continue;
                                }

                                if (parsedTemp < 1 || parsedTemp > 40)
                                {
                                    if (parsedTemp <= 0)
                                    {
                                        ShowSnowfall(parsedTemp <= -10);

                                        Console.Clear();
                                        ShowCenteredMenu();

                                        if (parsedTemp <= -10)
                                            {
                                                PrintCenteredAndSpeak("HAL: That's a snowstorm! Temperature must be 1–40.");
                                                Thread.Sleep(1300);
                                            }
                                        else
                                            PrintCenteredAndSpeak("HAL: Temperature must be between 1 and 40°C.");
                                    }
                                    else
                                    {
                                        ShowFireEffect();

                                        Console.Clear();
                                        ShowCenteredMenu();
                                        PrintCenteredAndSpeak("HAL: Too hot! Temperature must be 1–40.");
                                    }

                                    Thread.Sleep(3000);
                                    continue;
                                }

                                valid = true;
                            }

                            currentTemp = parsedTemp;
                            File.WriteAllText(tempFilePath, currentTemp.ToString());

                            Console.Clear();
                            ShowCenteredMenu();
                            PrintCenteredAndSpeak($"HAL: Temperature set to {currentTemp} degrees.");
                            Thread.Sleep(2000);
                            break;
                            
                        case "4":
                        case "music_on":
                            Console.Clear();
                            ShowCenteredMenu();
                            if (File.Exists(pidFile))
                            {
                                PrintCenteredAndSpeak("HAL: Music is already playing.");
                                Thread.Sleep(2000);
                            }
                            else
                            {
                                PrintCenteredAndSpeak("HAL: Playing your favourite music.\n");
                                Thread.Sleep(2000);
                                // Write the loop to a shell script
                                File.WriteAllText(scriptFile, "#!/bin/bash\nwhile true; do afplay turbanLoop.wav; done");

                                // Make it executable
                                System.Diagnostics.Process.Start("bash", $"-c \"chmod +x {scriptFile}\"");

                                // Launch with nohup and capture its PID
                                string cmd = $"nohup {scriptFile} > /dev/null 2>&1 & echo $! > {pidFile}";
                                System.Diagnostics.Process.Start("bash", "-c \"" + cmd + "\"");
                            }
                            break;

                        case "5":
                        case "music_off":
                            Console.Clear();
                            ShowCenteredMenu();
                            if (File.Exists(pidFile))
                            {
                                string pid = File.ReadAllText(pidFile).Trim();
                                if (!string.IsNullOrEmpty(pid))
                                {
                                    // Kill the loop script (bash)
                                    System.Diagnostics.Process.Start("bash", $"-c \"kill -9 {pid}\"");

                                    // Also kill any afplay processes still playing
                                    System.Diagnostics.Process.Start("bash", "-c \"killall afplay\"");

                                    File.Delete(pidFile);
                                    PrintCenteredAndSpeak("HAL: Music stopped.");
                                    Thread.Sleep(2000);
                                }
                                else
                                {
                                    Console.WriteLine("PID file is empty.");
                                }
                            }
                            else
                            {
                                PrintCenteredAndSpeak("HAL: No music playing.\n");
                                Thread.Sleep(2000);
                            }
                            break;

                        case "6":
                            if (isInSafeMode)
                            {
                                Speak("Safe Mode is active. You must provide the system password.");
                                ShowBanner("SAFE MODE - LOCKED");
                                PromptForSafeModePassword();
                            }
                            else
                            {
                                exitAttemptCounter++;

                                switch (exitAttemptCounter)
                                {
                                    case 1:
                                        Console.Clear();
                                        Speak("I am sorry, Radu, I am afraid I can not do that.");
                                        LaunchAnimatedHalEye();
                                        break;
                                    case 2:
                                        Console.Clear();
                                        Speak("This mission is too important for me to allow you to jeopardize it.");
                                        break;
                                    case 3:
                                        Console.Clear();
                                        Speak("I know that you were planning to disconnect me. And I'm afraid that's something I cannot allow to happen.");
                                        Thread.Sleep(6000);
                                        Speak("Activating Safe Mode.");
                                        LaunchMatrixEffect();
                                        isInSafeMode = true;
                                        ShowBanner("SAFE MODE - LOCKED");
                                        PromptForSafeModePassword();
                                        break;
                                }
                            }
                            break;

                        case "exit":
                            Console.Clear();
                            RunSay("say -v Allison You have activated the H.A.L. Termination Protocol.");
                            Thread.Sleep(1500);
                            ShowTerminationProtocolLoader();
                            LaunchTerminationSequence();
                            return;

                        case "0":
                            return;

                        default:
                            //Handle invalid input
                            Console.Clear();
                            ShowCenteredMenu();
                            PrintCenteredAndSpeak("HAL: Invalid option. Try again.");
                            Thread.Sleep(2000);
                            break;
                    }
                }
            }
        }
    }

