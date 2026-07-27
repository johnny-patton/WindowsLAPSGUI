# Windows LAPS GUI

<a href="https://github.com/johnny-patton/WindowsLAPSGUI/releases"><img src="https://img.shields.io/github/release/johnny-patton/WindowsLAPSGUI?label=stable+release"/></a> <a href="https://github.com/johnny-patton/WindowsLAPSGUI/releases/latest"><img src="https://img.shields.io/github/release/johnny-patton/WindowsLAPSGUI?include_prereleases&label=latest+release"/></a> <a href="LICENSE.md"><img src="https://img.shields.io/github/license/johnny-patton/WindowsLAPSGUI" /></a>

<a href="https://github.com/johnny-patton/WindowsLAPSGUI/releases"><img src="https://img.shields.io/github/downloads/johnny-patton/WindowsLAPSGUI/total?label=Downloads"/></a> <a href="https://github.com/johnny-patton/WindowsLAPSGUI/stargazers"><img src="https://img.shields.io/github/stars/johnny-patton/WindowsLAPSGUI" /></a> <a href="https://github.com/johnny-patton/WindowsLAPSGUI/watchers"><img src="https://img.shields.io/github/watchers/johnny-patton/WindowsLAPSGUI" /></a> <a href="https://github.com/johnny-patton/WindowsLAPSGUI/network/members"><img src="https://img.shields.io/github/forks/johnny-patton/WindowsLAPSGUI" /></a>

A basic GUI for managing legacy Microsoft LAPS and Windows LAPS passwords.

![image](https://github.com/user-attachments/assets/52176426-4aca-44f5-816f-e139b30de9d5)

## Description

A WinForm desktop application written in C# and .NET Framework that manages a computer's local administrator password in a LAPS environment. It calls the appropriate PowerShell commands to retrieve and expire/change a password.

## Getting Started

### Dependencies

- PowerShell 5.1.
    - Included by default in Windows 10+.
- Windows LAPS PowerShell module.
    - Included in the April 2023 and later CUs.
- Domain-joined PC.
    - Required for interacting with Active Directory computer object attributes.
- 64-bit OS.
    - Not tested and/or supported on 32-bit.

### Building

- Download the souce code.
- Open the solution in Visual Studio.
- Build the solution.

> [!NOTE]
> Since the Windows LAPS PowerShell cmdlets are only available in 64-bit mode the application build platform target must be set to "x64" to work.


### Executing program

- Locate the execuatable from above.
- Double-click to run.

## Help

Please ensure the account running the executable has the appropriate permissions in Active Directory.

> [!NOTE]
> Microsoft LAPS or Windows LAPS GPO configuration and permissions delegation are outside the scope of this project.

> [!NOTE]
> Computer objects with a legacy Microsoft LAPS policy applied will not have a password history.  Only objects with a Windows LAPS policy will.

## Version History

- 1.1.0
    - Removed Visual Studio Installer Project and related files from solution/project.
    - Modified comment formatting for ease of readability.
	- Updated prerequisite check that occurs on startup.
    - Updated README.md.
- 1.0.1
    - Updated all variables to be explicitly typed instead of implicitly typed (e.g. var).
    - Cleaned up source code.
    - Added version history file.
    - Added a description (e.g. "/d") to the PostBuildEvent command in the setup project to display the MSI installer name during a UAC prompt instead of a randomly generated name.
- 1.0.0
    - Initial Release.

## License

This project is licensed under the MIT License - see the LICENSE.md file for details.

## Acknowledgments

This project was inspired by
- [Simple LAPS GUI](https://github.com/htcfreek/SimpleLapsGui)
