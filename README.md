# OpenSync

Welcome to OpenSync, your all-in-one solution for PC game save backups. This robust desktop application empowers gamers with complete control over their valuable game progress. Unlike managing saves across multiple platforms like Steam, Origin, and Epic Games, OpenSync consolidates all your game saves under one roof. This means you can have them securely stored and easily accessible whenever you need to safeguard your progress, all while keeping the convenience of your favorite gaming platforms. Notably, OpenSync is designed with NAS systems in mind, providing seamless integration and secure storage options for your game saves.

## Features

- **Process Attachment:** With the "Process Selection UI," you can easily attach a process to a source (file or folder containing the save) and specify a destination for backup storage. You have the option to select a process from a list or manually type in the process name. This flexibility allows you to choose and attach processes as needed for backup.
- **Change Detection:** Receive notifications to create a backup when changes are detected in the source after the attached process is closed.
- **System Tray Running:** Operates discreetly in the background via the system tray, ensuring your gaming experience remains uninterrupted.
- **Graphical User Interface (GUI):** A user-friendly GUI that allows for easy addition of new games, management of backups, and manual restoration.
- **Network Storage Support:** Primarily designed to save games to NAS or mounted file systems. (Either locally or over the internet)
- **Auto-start Capability:** Configure the application to start on system boot using provided task scripts.
- **JSON Tracking:** All your game configurations are stored in a "TrackingApps.json" file. This file can be optionally stored on the NAS so mutiple instance of OpenSync on other devices can have the same configurations.
<img src="https://github.com/mitch344/OpenSync/blob/main/DemoImages/SystemTray.png">

<img src="https://github.com/mitch344/OpenSync/blob/main/DemoImages/MainMenu.png">

<img src="https://github.com/mitch344/OpenSync/blob/main/DemoImages/RestoreBackup.png">

## Getting Started

To get started with OpenSync, follow these steps:

1. Download the latest release from the [Releases page](https://github.com/mitch344/OpenSync/releases).
2. Follow the Wiki Guide for installation and configuration.
3. Feel free to play around with the tool and source code. The project mostly manipulates the windows file system so there are infinite possibilities.

Happy gaming!

## FAQ
Does Open Sync work with Google Cloud and One Drive?

OpenSync is compatible with any mounted file system, offering versatile backup options. It patiently waits for the TrackingApp.json file to become available before launching the application, allowing time for services or tasks to initialize. It also worth mentioning Google Drive and OneDrive use asynchronous uploading this can be a bit finicky at times. I might possibly look more into this in the future. 

## Tips
- Use enviroment variables they can help when setting up across multiple devices when user names on machines are different.

- You don't have to manually press the refresh button to reload the TrackingApp.json. When you reopen Open Sync from the system tray it will refresh automatically. This is just handy if you have both computers side by side.

## Bugs
Please report any bugs if you wish to the issues section of the page. Thank You!
