using System.Management;

var watcher = new ManagementEventWatcher(
    new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2"));
watcher.EventArrived += async (sender, e) =>
{
    var deviceId = string.Empty;
    var driveName = e.NewEvent.Properties["DriveName"]?.Value?.ToString();
    if (string.IsNullOrEmpty(driveName))
    {
        Console.WriteLine("DriveName is null or empty.");
        return;
    }
    else
    {
        var searcher = new ManagementObjectSearcher(
            $"SELECT DeviceID FROM Win32_Volume WHERE DriveLetter = '{driveName}'");
        foreach (var volume in searcher.Get())
        {
            deviceId = volume["DeviceID"]?.ToString();
        }
        if (string.IsNullOrEmpty(deviceId)) return;
        deviceId = deviceId.Split('{', '}')[1];
    }
    try
    {
        if (deviceId == args[0])
        {
            await CopyDirectory(args[1], Path.Combine(driveName, args[2]), true);
        }
        else
        {
            await CopyDirectory(driveName, Path.Combine(args[1], deviceId), true);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error copying contents: {ex.Message}");
    }
};
watcher.Start();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
watcher.Stop();

static async Task CopyDirectory(string sourceDir, string destinationDir, bool recursive)
{
    // Get information about the source directory
    var dir = new DirectoryInfo(sourceDir);

    // Check if the source directory exists
    if (!dir.Exists)
        throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

    // Cache directories before we start copying
    DirectoryInfo[] dirs = dir.GetDirectories();

    // Create the destination directory
    Directory.CreateDirectory(destinationDir);

    // Get the files in the source directory and copy to the destination directory
    foreach (FileInfo file in dir.GetFiles())
    {
        string targetFilePath = Path.Combine(destinationDir, file.Name);
        file.CopyTo(targetFilePath);
    }

    // If recursive and copying subdirectories, recursively call this method
    if (recursive)
    {
        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            await CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }
}