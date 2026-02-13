# Disk Spy

Disk Spy is a small Windows console utility that watches for newly mounted volumes (such as USB drives) and automatically mirrors data when a drive is inserted.

It listens for `Win32_VolumeChangeEvent` events (`EventType = 2`, device arrival) and then performs one of two copy actions based on the inserted volume ID:

- **Matched device ID**: copy from a local source folder to a target folder on the inserted drive.
- **Other device IDs**: copy the inserted drive contents to a local destination folder, grouped by the drive's volume ID.

## Features

- Detects removable/storage volume insertions in real time.
- Resolves the inserted drive letter to a stable volume `DeviceID`.
- Recursively copies files and directories.
- Supports bidirectional behavior through startup arguments.

## Requirements

- Windows (uses WMI via `System.Management`)
- .NET SDK compatible with `net10.0-windows`

## Project Structure

- `disk-spy/Program.cs` – watcher setup, event handling, and recursive copy logic.
- `disk-spy/disk-spy.csproj` – project settings and package references.

## Build

From the repository root:

```bash
dotnet build disk-spy/disk-spy.csproj
```

## Run

```bash
dotnet run --project disk-spy/disk-spy.csproj -- <specialDeviceId> <localPath> <driveTargetSubPath>
```

### Arguments

1. `specialDeviceId`
   - The volume ID (GUID portion of `DeviceID`) that triggers **push mode**.
2. `localPath`
   - In push mode: source directory on the local machine.
   - In pull mode: destination root directory on the local machine.
3. `driveTargetSubPath`
   - Subfolder path on the inserted drive used in push mode.

## Behavior Details

When a new volume is inserted:

1. The app reads the drive letter from the event (`DriveName`).
2. It queries WMI for `Win32_Volume.DeviceID` of that drive.
3. It extracts the GUID section from the `DeviceID`.
4. It compares the extracted ID to `specialDeviceId`:
   - **Equal**: copies `localPath` -> `<DriveLetter>\<driveTargetSubPath>`
   - **Not equal**: copies `<DriveLetter>\` -> `<localPath>\<deviceGuid>`

The copy operation is recursive and creates destination directories as needed.

## Notes & Limitations

- Existing files with the same name at destination may cause copy exceptions (current implementation uses `FileInfo.CopyTo` without overwrite).
- Copy operations start immediately on insertion and run within event handling.
- Be careful with large drives and sensitive data: this tool can copy entire volumes.

## License

This project is licensed under the GNU GPL v3 License. See [![license](https://img.shields.io/github/license/rbxluau/disk-spy)](https://github.com/rbxluau/disk-spy/blob/main/LICENSE).
