# OpenSync

OpenSync is a tool designed to manage and automate the backup of game saves. It provides a range of features to ensure your game progress is securely stored and easily restored.

## Features

- **Process Attachment:** With the "Process Selection UI," you can attach a process to a source (file or folder containing the save) and specify a destination for backup storage. You can select a process from a list or manually type in the process name, allowing for flexible backup configurations.
- **Change Detection:** Notifications prompt you to create a backup when changes are detected in the source after the attached process is closed.
- **System Tray Running:** Runs discreetly in the background via the system tray, ensuring your gaming experience remains uninterrupted.
- **Graphical User Interface (GUI):** A user-friendly GUI allows for easy addition of new games, management of backups, and manual restoration.
- **Network Storage Support:** Primarily designed to save games to NAS or mounted file systems, whether locally or over the internet.
- **Auto-start Capability:** Configure the application to start on system boot using provided task scripts.
- **JSON Tracking:** All your game configurations are stored in a "TrackingApps.json" file. This file can optionally be stored on the NAS so multiple instances of OpenSync on other devices can share the same configurations.

![System Tray](https://github.com/mitch344/OpenSync/blob/main/DemoImages/SystemTray.png)
![Main Menu](https://github.com/mitch344/OpenSync/blob/main/DemoImages/MainMenu.png)
![Restore Backup](https://github.com/mitch344/OpenSync/blob/main/DemoImages/RestoreBackup.png)

## Getting Started

To get started with OpenSync, follow these steps:

1. Download the latest release from the [Releases page](https://github.com/mitch344/OpenSync/releases).
2. Follow the [Wiki Guide](https://github.com/mitch344/OpenSync/wiki) for installation and configuration.
3. Feel free to explore the tool and source code. The project primarily manipulates the Windows file system, offering endless possibilities.

Happy gaming!

## FAQ

### Does OpenSync work with Google Cloud and OneDrive?

OpenSync is compatible with any mounted file system, offering versatile backup options. It patiently waits for the TrackingApp.json file to become available before launching the application, allowing time for services or tasks to initialize. It is worth noting that Google Drive and OneDrive use asynchronous uploading, which can be a bit finicky at times. This may be addressed in future updates.

## Tips

- Use environment variables to simplify setup across multiple devices, especially when usernames differ on each machine.
- You don't have to manually press the refresh button to reload the TrackingApp.json. Reopening OpenSync from the system tray will refresh it automatically. This is particularly handy if you have multiple computers side by side.

## Bugs

Please report any bugs in the issues section of the GitHub page. Thank you!
