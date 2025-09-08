# disk-spy

A small Windows utility that watches for removable volume arrival events and automatically copies files to/from the inserted volume.

> **Note:** This tool uses WMI (`System.Management`) and is intended for **Windows (desktop)** environments. Build and run on a Windows machine with .NET (see Requirements).

---

## Features

* Listens for `Win32_VolumeChangeEvent` (volume arrival) events.
* When a removable volume is inserted, the app either:

  * Copies a configured local directory onto the inserted drive (if the inserted drive's DeviceId matches a configured ID), or
  * Copies the contents of the inserted drive into a local directory organized by the device ID.
* Simple, single-file console app suitable as a starting point for automated USB sync or backup utilities.

---

## How it works (high level)

On a volume-arrival event the program:

1. Reads the event's `DeviceId` and `DriveName` properties.
2. Normalizes `DeviceId` (the GUID in braces is extracted).
3. If the extracted `deviceId` equals the first command-line argument (`args[0]`), it copies a local source directory (`args[1]`) to the new drive under a subfolder name given by `args[2]`.
4. Otherwise, it copies the inserted drive's root contents into a folder under the local base directory (`args[1]`) named after the inserted device's `deviceId`.

This behavior allows you to either push a directory to a known device or pull contents from unknown/other devices into a structured local archive.

---

## Requirements

* Windows 10 / Windows 11 (or other modern Windows desktop OS that supports WMI).
* .NET 8.0 SDK installed.
* The `System.Management` assembly/package available. On .NET 6+ (including .NET 8.0) you need to add the NuGet package `System.Management`.
* File system permissions to read from and write to involved directories and drives.

---

## Build

1. Create a console project (if you don't already have one):

```bash
dotnet new console -o disk-spy
cd disk-spy
```

2. Add the `System.Management` package:

```bash
dotnet add package System.Management
```

3. Update your project file to target Windows with .NET 8.0:

```xml
<!-- Example in your .csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

4. Place the provided code into `Program.cs` and build:

```bash
dotnet build -c Release
```

---

## Usage

Run the executable from a command line and provide three arguments:

```text
disk-spy.exe <deviceIdToMatch> <localBaseDirectory> <folderNameOnDrive>
```

* `deviceIdToMatch` — the device ID (GUID) to compare against the inserted volume. Example format (without braces): `12345678-90AB-CDEF-1234-567890ABCDEF`.
* `localBaseDirectory` — the local path where incoming drive contents will be saved (when deviceId does not match) or the source directory to copy to the drive (when deviceId matches).
* `folderNameOnDrive` — the name of the target folder to create on the inserted drive when `deviceIdToMatch` matches.

### Examples

* Push a folder to a known device when it is inserted:

```text
disk-spy.exe 12345678-90AB-CDEF-1234-567890ABCDEF C:\MyBackup DataToCopy
```

If a drive with `DeviceId` `12345678-90AB-CDEF-1234-567890ABCDEF` is inserted and mounted as `E:\`, the app copies `C:\MyBackup` → `E:\DataToCopy`.

* Pull contents of any other inserted drive into a local archive directory:

```text
disk-spy.exe 12345678-90AB-CDEF-1234-567890ABCDEF C:\UsbArchives DataToCopy
```

If a drive with a different DeviceId is inserted and mounted as `F:\`, the app copies `F:\` → `C:\UsbArchives\<deviceId>\`.

---

## Permissions and Security

* The app requires file read/write access to the source and destination paths. Run it with an account that has appropriate permissions.
* If you expect to copy system-protected files, run the program with elevated privileges (Administrator). Elevated privileges are not always required but may be necessary depending on the source/destination.
* Be careful when automatically copying unknown removable drives — this can expose your machine to malicious files. Consider adding file filters, scanning, or stricter validation before copying.

---

## Limitations & To-Do / Improvements

* No conflict handling: the code will call `File.CopyTo` without overwrite options; existing files with the same name will cause exceptions. Consider using `file.CopyTo(target, overwrite: true)` or adding more robust conflict resolution.
* No logging: add structured logging to track what was copied, errors, timestamps, etc.
* No file filters: add white/black lists, file size limits, or file type filters to avoid copying large or unwanted files.
* No concurrency control: copying large trees is synchronous and may block the event loop. Consider copying in a background worker and reporting progress.
* No graceful shutdown handling (the app uses `Console.ReadKey()` and `watcher.Stop()` after that; consider cancellation tokens for cleaner shutdown in services).

---

## Troubleshooting

* **DeviceId is null or empty**: WMI event may not contain the expected property (rare). The code already checks and logs that case.
* **Access denied exceptions**: check file system permissions and run as Administrator if necessary.
* **Long copy times / app unresponsive**: copying large volumes may take time. Consider running the copy in a separate thread/task with progress reporting.

---

## Contributing

Feel free to open issues or pull requests to:

* Add logging and configuration via a JSON or YAML file.
* Add exclusion rules and virus-scan hooks.
* Improve error handling and add retry logic.

---

## License

This repository does not include a license by default. Add a suitable open-source license (e.g. MIT) if you plan to publish.

---

## Acknowledgements

Uses Windows Management Instrumentation (`System.Management`) for event subscription and the .NET `System.IO` APIs for filesystem operations.
