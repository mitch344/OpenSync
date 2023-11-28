using Newtonsoft.Json;
using System.Globalization;

namespace OpenSync
{
    internal class BackupManager
    {
        public void PerformBackup(TrackingApp trackingApp)
        {
            var sourcePath = Environment.ExpandEnvironmentVariables(trackingApp.Source);
            var targetDirectory = Path.Combine(Environment.ExpandEnvironmentVariables(trackingApp.Destination), $"{trackingApp.ProcessToTrack}-{DateTime.Now:MMddhhmmsstt}");

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            if (File.Exists(sourcePath))
            {
                var targetPath = Path.Combine(targetDirectory, Path.GetFileName(sourcePath));
                File.Copy(sourcePath, targetPath, true);
            }
            else if (Directory.Exists(sourcePath))
            {
                var targetPathWithDir = Path.Combine(targetDirectory, new DirectoryInfo(sourcePath).Name);
                CopyDirectory(sourcePath, targetPathWithDir);
            }
            else
            {
                throw new InvalidOperationException($"Source path {sourcePath} does not exist");
            }
        }

        public static DateTime? ExtractDateTimeFromFolderName(string folderName)
        {
            string[] parts = folderName.Split('-');
            if (parts.Length >= 2)
            {
                string datetimePart = parts[1];
                if (DateTime.TryParseExact(datetimePart, "MMddhhmmsstt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }
            return null;
        }

        public string GetLatestBackupDirectory(TrackingApp trackingApp)
        {
            string processName = trackingApp.ProcessToTrack;
            string destinationDirectory = Environment.ExpandEnvironmentVariables(trackingApp.Destination); 

            string[] backupDirectories = Directory.GetDirectories(destinationDirectory, $"{processName}-*");
            string latestBackupDirectory = backupDirectories
                .OrderByDescending(d => Directory.GetCreationTime(d))
                .FirstOrDefault();

            return latestBackupDirectory;
        }

        protected void CopyDirectory(string source, string target)
        {
            DirectoryInfo diSource = new DirectoryInfo(source);
            DirectoryInfo diTarget = new DirectoryInfo(target);
            CopyAll(diSource, diTarget);
        }

        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public void DeleteBackup(TrackingApp trackingApp, string backupFolderName)
        {
            try
            {
                string backupFolderPath = Path.Combine(Environment.ExpandEnvironmentVariables(trackingApp.Destination), backupFolderName);

                if (Directory.Exists(backupFolderPath))
                {
                    Directory.Delete(backupFolderPath, true);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Delete failed: {ex.Message}");
            }
        }


        public void RestoreLatestVersion(TrackingApp trackingApp)
        {
            string processName = trackingApp.ProcessToTrack;
            string destinationDirectory = Environment.ExpandEnvironmentVariables(trackingApp.Destination);

            string[] backupDirectories = Directory.GetDirectories(destinationDirectory, $"{processName}-*");
            string latestBackupDirectory = backupDirectories
                .OrderByDescending(d => Directory.GetCreationTime(d))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(latestBackupDirectory))
            {
                throw new InvalidOperationException("No backups found for the selected process.");
            }

            try
            {
                string sourcePath = Environment.ExpandEnvironmentVariables(trackingApp.Source);
                string destinationPath = Environment.ExpandEnvironmentVariables(trackingApp.Destination);

                if (File.Exists(sourcePath))
                {
                    string sourceFileName = Path.GetFileName(sourcePath);
                    string backupFilePath = Path.Combine(latestBackupDirectory, sourceFileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(sourcePath));
                    File.Copy(backupFilePath, sourcePath, true);
                }
                else if (Directory.Exists(sourcePath))
                {
                    string sourceDirectoryName = new DirectoryInfo(sourcePath).Name;
                    string backupSourceDirectory = Path.Combine(latestBackupDirectory, sourceDirectoryName);
                    Directory.CreateDirectory(destinationPath);
                    CopyAll(new DirectoryInfo(backupSourceDirectory), new DirectoryInfo(sourcePath));
                }
                else
                {
                    string sourceName = Path.GetFileName(sourcePath);
                    string backupSourcePath = Path.Combine(latestBackupDirectory, sourceName);

                    if (File.Exists(backupSourcePath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath));
                        File.Copy(backupSourcePath, sourcePath, true);
                    }
                    else if (Directory.Exists(backupSourcePath))
                    {
                        Directory.CreateDirectory(sourcePath);
                        CopyAll(new DirectoryInfo(backupSourcePath), new DirectoryInfo(sourcePath));
                    }
                    else
                    {
                        throw new InvalidOperationException($"No matching file or folder found inside the backup for '{sourceName}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Restore failed: {ex.Message}");
            }
        }

        public List<string> GetBackupsForProcess(TrackingApp trackingApp)
        {
            List<string> backups = new List<string>();
            string processName = trackingApp.ProcessToTrack;
            string destinationDirectory = Environment.ExpandEnvironmentVariables(trackingApp.Destination);

            if (!Directory.Exists(destinationDirectory))
            {
                MessageBox.Show("Directory Dosen't Exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            string[] backupDirectories = Directory.GetDirectories(destinationDirectory, $"{processName}-*");

            foreach (string backupDirectory in backupDirectories)
            {
                backups.Add(Path.GetFileName(backupDirectory));
            }

            return backups;
        }


        public void RestoreBackupForProcess(TrackingApp trackingApp, string selectedBackupFolderName)
        {
            string processName = trackingApp.ProcessToTrack;
            string destinationDirectory = Environment.ExpandEnvironmentVariables(trackingApp.Destination);

            string selectedBackupDirectory = Path.Combine(destinationDirectory, selectedBackupFolderName);

            if (!Directory.Exists(selectedBackupDirectory))
            {
                throw new InvalidOperationException("Selected backup folder does not exist.");
            }

            try
            {
                string sourcePath = Environment.ExpandEnvironmentVariables(trackingApp.Source);

                if (File.Exists(sourcePath))
                {
                    string sourceFileName = Path.GetFileName(sourcePath);
                    string backupFilePath = Path.Combine(selectedBackupDirectory, sourceFileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(sourcePath));
                    File.Copy(backupFilePath, sourcePath, true);
                }
                else if (Directory.Exists(sourcePath))
                {
                    string sourceDirectoryName = new DirectoryInfo(sourcePath).Name;
                    string backupSourceDirectory = Path.Combine(selectedBackupDirectory, sourceDirectoryName);
                    Directory.CreateDirectory(sourcePath);
                    CopyAll(new DirectoryInfo(backupSourceDirectory), new DirectoryInfo(sourcePath));
                }
                else
                {
                    string sourceName = Path.GetFileName(sourcePath);
                    string backupSourcePath = Path.Combine(selectedBackupDirectory, sourceName);

                    if (File.Exists(backupSourcePath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(sourcePath));
                        File.Copy(backupSourcePath, sourcePath, true);
                    }
                    else if (Directory.Exists(backupSourcePath))
                    {
                        Directory.CreateDirectory(sourcePath);
                        CopyAll(new DirectoryInfo(backupSourcePath), new DirectoryInfo(sourcePath));
                    }
                    else
                    {
                        throw new InvalidOperationException($"No matching file or folder found inside the backup for '{sourceName}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Restore failed: {ex.Message}");
            }
        }

        public string CalculateSHA256Checksum(string path)
        {
            path = Environment.ExpandEnvironmentVariables(path);
            try
            {
                if (File.Exists(path))
                {
                    return CalculateFileHash(path);
                }
                else if (Directory.Exists(path))
                {
                    return CalculateDirectoryHash(path);
                }
                else
                {
                    throw new FileNotFoundException("The specified path does not exist.", path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        public string CalculateFileHash(string filePath)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public string CalculateDirectoryHash(string directoryPath)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                foreach (var filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).OrderBy(p => p))
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        byte[] hashBytes = sha256.ComputeHash(stream);
                        sha256.TransformBlock(hashBytes, 0, hashBytes.Length, hashBytes, 0);
                    }
                }

                sha256.TransformFinalBlock(new byte[0], 0, 0);
                return BitConverter.ToString(sha256.Hash).Replace("-", "").ToLower();
            }
        }
    }
}
