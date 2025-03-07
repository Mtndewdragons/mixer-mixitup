﻿using MixItUp.Base.Model.API;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MixItUp.Installer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public const string InstallerLogFileName = "MixItUp-Installer-Log.txt";
        public const string ShortcutFileName = "Mix It Up.lnk";

        public const string ApplicationSettingsFileName = "ApplicationSettings.xml";

        public const string MixItUpProcessName = "MixItUp";
        public const string AutoHosterProcessName = "MixItUp.AutoHoster";

        private static readonly Version minimumOSVersion = new Version(6, 2, 0, 0);

        public static readonly string DefaultInstallDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MixItUp");
        public static readonly string StartMenuDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Mix It Up");

        public static string InstallSettingsDirectory { get { return Path.Combine(MainWindowViewModel.InstallSettingsDirectory, "Settings"); } }

        public static byte[] ZipArchiveData { get; set; }

        public static string StartMenuShortCutFilePath { get { return Path.Combine(StartMenuDirectory, ShortcutFileName); } }
        public static string DesktopShortCutFilePath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ShortcutFileName); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsUpdate
        {
            get { return this.isUpdate; }
            private set
            {
                this.isUpdate = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("IsInstall");
            }
        }
        private bool isUpdate;

        public bool IsInstall { get { return !this.IsUpdate; } }

        public bool IsPreview
        {
            get { return this.isPreview; }
            private set
            {
                this.isPreview = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isPreview;

        public bool IsOperationBeingPerformed
        {
            get { return this.isOperationBeingPerformed; }
            private set
            {
                this.isOperationBeingPerformed = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isOperationBeingPerformed;

        public bool IsOperationIndeterminate
        {
            get { return this.isOperationIndeterminate; }
            private set
            {
                this.isOperationIndeterminate = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isOperationIndeterminate;

        public int OperationProgress
        {
            get { return this.operationProgress; }
            private set
            {
                this.operationProgress = value;
                this.NotifyPropertyChanged();
            }
        }
        private int operationProgress;

        public string DisplayText1
        {
            get { return this.displayText1; }
            private set
            {
                this.displayText1 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string displayText1;

        public bool ErrorOccurred
        {
            get { return this.errorOccurred; }
            private set
            {
                this.errorOccurred = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool errorOccurred;

        public string DisplayText2
        {
            get { return this.displayText2; }
            private set
            {
                this.displayText2 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string displayText2;

        private string installDirectory;

        public MainWindowViewModel()
        {
            this.installDirectory = DefaultInstallDirectory;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                this.installDirectory = args[1];
            }

            if (Directory.Exists(this.installDirectory))
            {
                this.IsUpdate = true;
                string applicationSettingsFilePath = Path.Combine(this.installDirectory, ApplicationSettingsFileName);
                if (File.Exists(applicationSettingsFilePath))
                {
                    using (StreamReader reader = new StreamReader(File.OpenRead(applicationSettingsFilePath)))
                    {
                        JObject jobj = JObject.Parse(reader.ReadToEnd());
                        if (jobj != null && jobj.ContainsKey("PreviewProgram"))
                        {
                            this.IsPreview = jobj["PreviewProgram"].ToObject<bool>();
                        }
                    }
                }
            }

            this.DisplayText1 = "Preparing installation...";
            this.isOperationBeingPerformed = true;
            this.IsOperationIndeterminate = true;
        }

        public bool CheckCompatability()
        {
            if (Environment.OSVersion.Version < minimumOSVersion)
            {
                this.ShowError("Mix It Up only runs on Windows 8 & higher.", "If incorrect, please contact support@mixitupapp.com");
                return false;
            }
            return true;
        }

        public async Task<bool> Run()
        {
            bool result = false;

            await Task.Run(async () =>
            {
                try
                {
                    if (!this.IsUpdate || await this.WaitForMixItUpToClose())
                    {
                        MixItUpUpdateModel update = await this.GetUpdateData();
                        if (this.IsPreview)
                        {
                            MixItUpUpdateModel preview = await this.GetUpdateData(preview: true);
                            if (preview != null && preview.SystemVersion > update.SystemVersion)
                            {
                                update = preview;
                            }
                        }

                        if (update != null)
                        {
                            if (await this.DownloadZipArchive(update))
                            {
                                if (this.InstallMixItUp() && this.CreateMixItUpShortcut())
                                {
                                    result = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.WriteToLogFile(ex.ToString());
                }
            });

            if (!result && !this.ErrorOccurred)
            {
                this.ShowError(string.Format("{0} file created in folder.", InstallerLogFileName), "Email support@mixitupapp.com with this file to help diagnose this issue.");
            }
            return result;
        }

        public void Launch()
        {
            if (Path.Equals(this.installDirectory, DefaultInstallDirectory))
            {
                if (File.Exists(StartMenuShortCutFilePath))
                {
                    Process.Start(StartMenuShortCutFilePath);
                }
                else if (File.Exists(DesktopShortCutFilePath))
                {
                    Process.Start(DesktopShortCutFilePath);
                }
            }
            else
            {
                Process.Start(Path.Combine(this.installDirectory, "MixItUp.exe"));
            }
        }

        protected void NotifyPropertyChanged([CallerMemberName]string name = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async Task<bool> WaitForMixItUpToClose()
        {
            this.DisplayText1 = "Waiting for Mix It Up to close...";
            this.IsOperationIndeterminate = true;
            this.OperationProgress = 0;

            for (int i = 0; i < 10; i++)
            {
                bool isRunning = false;
                foreach (Process clsProcess in Process.GetProcesses())
                {
                    if (clsProcess.ProcessName.Equals(MixItUpProcessName) || clsProcess.ProcessName.Equals(AutoHosterProcessName))
                    {
                        isRunning = true;
                        if (i == 5)
                        {
                            clsProcess.CloseMainWindow();
                        }
                    }
                }

                if (!isRunning)
                {
                    return true;
                }
                await Task.Delay(1000);
            }
            return false;
        }

        private async Task<MixItUpUpdateModel> GetUpdateData(bool preview = false)
        {
            this.DisplayText1 = "Finding latest version...";
            this.IsOperationIndeterminate = true;
            this.OperationProgress = 0;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync((preview) ? "https://mixitupapi.azurewebsites.net/api/updates/preview"
                    : "https://mixitupapi.azurewebsites.net/api/updates");
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    JObject jobj = JObject.Parse(responseString);
                    return jobj.ToObject<MixItUpUpdateModel>();
                }
            }
            return null;
        }

        private async Task<bool> DownloadZipArchive(MixItUpUpdateModel update)
        {
            this.DisplayText1 = "Downloading files...";
            this.IsOperationIndeterminate = false;
            this.OperationProgress = 0;

            bool downloadComplete = false;

            WebClient client = new WebClient();
            client.DownloadProgressChanged += (s, e) =>
            {
                this.OperationProgress = e.ProgressPercentage;
            };

            client.DownloadDataCompleted += (s, e) =>
            {
                downloadComplete = true;
                this.OperationProgress = 100;
                if (e.Error == null && !e.Cancelled)
                {
                    ZipArchiveData = e.Result;
                }
                else if (e.Error != null)
                {
                    this.WriteToLogFile(e.Error.ToString());
                }
            };

            client.DownloadDataAsync(new Uri(update.ZipArchiveLink));

            while (!downloadComplete)
            {
                await Task.Delay(1000);
            }

            client.Dispose();

            return (ZipArchiveData != null && ZipArchiveData.Length > 0);
        }

        private bool InstallMixItUp()
        {
            this.DisplayText1 = "Installing files...";
            this.IsOperationIndeterminate = false;
            this.OperationProgress = 0;

            try
            {
                if (ZipArchiveData != null && ZipArchiveData.Length > 0)
                {
                    Directory.CreateDirectory(this.installDirectory);
                    if (Directory.Exists(this.installDirectory))
                    {
                        using (MemoryStream zipStream = new MemoryStream(ZipArchiveData))
                        {
                            ZipArchive archive = new ZipArchive(zipStream);
                            double current = 0;
                            double total = archive.Entries.Count;
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                string filePath = Path.Combine(this.installDirectory, entry.FullName);
                                string directoryPath = Path.GetDirectoryName(filePath);
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }

                                if (Path.HasExtension(filePath))
                                {
                                    entry.ExtractToFile(filePath, overwrite: true);
                                }

                                current++;
                                this.OperationProgress = (int)((current / total) * 100);
                            }
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteToLogFile(ex.ToString());
            }
            return false;
        }

        private bool CreateMixItUpShortcut()
        {
            try
            {
                this.DisplayText1 = "Creating Start Menu & Desktop shortcuts...";
                this.IsOperationIndeterminate = true;
                this.OperationProgress = 0;

                if (!Directory.Exists(StartMenuDirectory))
                {
                    Directory.CreateDirectory(StartMenuDirectory);
                }

                if (Directory.Exists(StartMenuDirectory))
                {
                    string tempLinkFilePath = Path.Combine(DefaultInstallDirectory, "Mix It Up.link");
                    if (File.Exists(tempLinkFilePath))
                    {
                        File.Copy(tempLinkFilePath, StartMenuShortCutFilePath, overwrite: true);
                        if (File.Exists(StartMenuShortCutFilePath))
                        {
                            return true;
                        }
                        else
                        {
                            File.Copy(tempLinkFilePath, DesktopShortCutFilePath, overwrite: true);
                            if (File.Exists(DesktopShortCutFilePath))
                            {
                                this.ShowError("We were unable to create the Start Menu shortcut.", "You can instead use the Desktop shortcut to launch Mix It Up");
                            }
                            else
                            {
                                this.ShowError("We were unable to create the Start Menu & Desktop shortcuts.", "Email support@mixitupapp.com to help diagnose this issue further.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteToLogFile(ex.ToString());
            }
            return false;
        }

        private void ShowError(string message1, string message2)
        {
            this.IsOperationBeingPerformed = false;
            this.ErrorOccurred = true;
            this.DisplayText1 = message1;
            this.DisplayText2 = message2;
        }

        private void WriteToLogFile(string text)
        {
            File.WriteAllText(InstallerLogFileName, text);
        }
    }
}
