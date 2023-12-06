namespace OpenSync
{
    internal class BackupManager
    {
        public void PerformBackup(TrackingApp trackingApp)
        {
            var sourcePath = Environment.ExpandEnvironmentVariables(trackingApp.Source);
            var processName = trackingApp.ProcessToTrack;
            var destinationDirectory = Environment.ExpandEnvironmentVariables(trackingApp.Destination);

            var processFolder = Path.Combine(destinationDirectory, processName);
            if (!Directory.Exists(processFolder))
            {
                Directory.CreateDirectory(processFolder);
            }

            var timestamp = DateTime.Now.ToString("MMddyyyyhhmmsstt");
            var backupDirectory = Path.Combine(processFolder, timestamp);

            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            if (File.Exists(sourcePath))
            {
                var targetPath = Path.Combine(backupDirectory, Path.GetFileName(sourcePath));
                File.Copy(sourcePath, targetPath, true);
            }
            else if (Directory.Exists(sourcePath))
            {
                var targetPathWithDir = Path.Combine(backupDirectory, new DirectoryInfo(sourcePath).Name);
                CopyDirectory(sourcePath, targetPathWithDir);
            }
            else
            {
                throw new InvalidOperationException($"Source path {sourcePath} does not exist");
            }
        }


        public static DateTime? ExtractDateTimeFromFolderName(string folderName)
        {
            if (folderName.Length == 16)
            {
                string month = folderName.Substring(0, 2);
                string day = folderName.Substring(2, 2);
                string year = folderName.Substring(4, 4);
                string hour = folderName.Substring(8, 2);
                string minute = folderName.Substring(10, 2);
                string second = folderName.Substring(12, 2);
                string amPm = folderName.Substring(14, 2);

                if (int.TryParse(year, out int yyyy) && int.TryParse(month, out int MM) &&
                    int.TryParse(day, out int dd) && int.TryParse(hour, out int hh) &&
                    int.TryParse(minute, out int mm) && int.TryParse(second, out int ss))
                {
                    if (amPm == "PM" && hh < 12)
                    {
                        hh += 12;
                    }
                    else if (amPm == "AM" && hh == 12)
                    {
                        hh = 0;
                    }

                    return new DateTime(yyyy, MM, dd, hh, mm, ss);
                }
            }

            return null;
        }


        public string GetLatestBackupDirectory(TrackingApp trackingApp)
        {
            string processName = trackingApp.ProcessToTrack;
            string destinationDirectory = Environment.ExpandEnvironmentVariables(trackingApp.Destination);
            string processFolder = Path.Combine(destinationDirectory, processName);

            if (!Directory.Exists(processFolder))
            {
                return null;
            }

            string[] backupDirectories = Directory.GetDirectories(processFolder);

            string latestTimestamp = backupDirectories
                .Select(folder => ExtractDateTimeFromFolderName(Path.GetFileName(folder)))
                .Where(date => date.HasValue)
                .OrderByDescending(date => date)
                .FirstOrDefault()?.ToString("MMddhhmmsstt");

            if (!string.IsNullOrEmpty(latestTimestamp))
            {
                return Path.Combine(processFolder, latestTimestamp);
            }

            return null;
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
                string processName = trackingApp.ProcessToTrack;
                string destinationDirectory = Environment.ExpandEnvironmentVariables(trackingApp.Destination);
                string processFolder = Path.Combine(destinationDirectory, processName);
                string backupFolderPath = Path.Combine(processFolder, backupFolderName);

                if (Directory.Exists(backupFolderPath))
                {
                    Directory.Delete(backupFolderPath, true);
                }
                else
                {
                    throw new InvalidOperationException($"Backup folder '{backupFolderPath}' does not exist.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"UnauthorizedAccessException: {ex.Message}. Check permissions.");
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"IOException: {ex.Message}. Ensure that no files are locked.");
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

            string processFolder = Path.Combine(destinationDirectory, processName);

            string[] backupDirectories = Directory.GetDirectories(processFolder);

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
                MessageBox.Show("Directory Doesn't Exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return backups;
            }

            string processFolder = Path.Combine(destinationDirectory, processName);

            if (!Directory.Exists(processFolder))
            {
                return backups;
            }

            string[] backupDirectories = Directory.GetDirectories(processFolder);

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

            string processFolder = Path.Combine(destinationDirectory, processName);
            string selectedBackupDirectory = Path.Combine(processFolder, selectedBackupFolderName);

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
