# CTRExplorerSD
CTRExploreSD is a file management utility for the Nintendo 3DS using the FTP protocol. This project allows you to access and manage your console's SD card content without any physical hardware manipulation.

# General Operation
The application connects to an active FTP server on your console (such as ftpd) to allow remote directory navigation and file management.

# Features
Network Navigation: Full access to SD card directories.

Risk Assessment: Automatic identification of critical files (boot.firm, Luma, system folders) using color-coding to prevent accidental manipulation.

Remote Reading: Double-clicking a file downloads it to a temporary location and opens it with your computer's default application (ideal for quick configuration edits).

Transfers: Direct download of files and folders from the console to your local machine.

# Instructions
Launch your FTP server on the 3DS console.

Enter the IP address displayed by the console into the CTRExploreSD interface.

Click the connect button to access the file system.

# Execution by Operating System
The program is provided as a "Self-Contained" version, including all necessary dependencies.

Windows: Run the .exe file. If a security alert appears, select "More info" then "Run anyway".

Linux / Steam Deck: Grant execution permissions using the command chmod +x CTRExploreSD before launching the binary.

macOS: Right-click the application and choose "Open" to bypass security restrictions for unsigned applications.

# Warning
Using this tool involves risks if you modify or delete system files. You are responsible for verifying the nature of the elements you manipulate. The author cannot be held responsible for any data loss or console malfunction.

# Technical Overview of CTRExploreSD
1. Architecture: The MVVM Pattern
The program is structured using the MVVM (Model-View-ViewModel) pattern, separating the visual interface from the business logic:

View: The XAML file defines the interface. It contains no logic and simply displays data while sending user interactions to the ViewModel.

ViewModel: The core of the program. It processes data received from the FTP server and updates the interface in real-time via observable properties.

Model: Data classes, such as FtpItem, which define file attributes (name, size, icon, risk level).

2. Communication: FTP Protocol
The program utilizes the FluentFTP library for console communication:

Connection: Establishes a TCP socket on the port defined by your FTP server (typically 5000 on 3DS).

Passive Mode: Since the 3DS is often behind a simple firewall, the program uses Auto-Passive mode. The client opens the data channels rather than the server, ensuring transfer stability.

Encoding: Forced UTF-8 usage ensures that accented characters are correctly interpreted between the console OS and your computer.

3. Risk Scanner: Metadata Analysis
The risk detection function does not perform slow binary analysis over FTP. Instead, it uses string filtering:

Extension Analysis: Checks the file suffix (e.g., .cia, .firm, .3dsx).

Dictionary Mapping: Compares names against a list of known critical 3DS folders and files (e.g., luma, nintendo 3ds).

Visual Attribution: When a match is found, the program injects a specific hex color and safety label into the object before it reaches the UI list.

4. Remote Consultation: Caching Mechanism
When double-clicking a file, the program executes the following steps:

Temporary Extraction: The file is invisibly downloaded to your system's %TEMP% folder.

System Call: The program uses the Process class to request the OS to open the file with the default associated application.

Isolation: This allows file consultation without modifying the original on the SD card, unless specifically requested by the user.

5. Cross-Platform Compilation
The program is compiled into IL (Intermediate Language) and packaged with the .NET Runtime in Self-Contained mode. All necessary libraries (network management, Skia graphics rendering, file handling) are included in the final executable, making it standalone for Windows, Linux, and macOS.
