using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO.Compression;
using Hardcodet.Wpf.TaskbarNotification;
using System.ComponentModel;
namespace Backup_Automator
{
    public partial class BackupAutomator : Window
    {
        // UI Buttons
        private BitmapImage discordD;
        private BitmapImage discordH;
        private BitmapImage patreonD;
        private BitmapImage patreonH;
        private BitmapImage githubD;
        private BitmapImage githubH;
        private BitmapImage webD;
        private BitmapImage webH;
        private BitmapImage deviantArtD;
        private BitmapImage deviantArtH;
        private BitmapImage artstationD;
        private BitmapImage artstationH;
        private BitmapImage button2X;
        private BitmapImage button2D;
        private BitmapImage button2H;
        private BitmapImage button3D;
        private BitmapImage button3H;
        private BitmapImage button4D;
        private BitmapImage button4H;
        private BitmapImage online;
        private BitmapImage offline;
        // Function
        private CancellationTokenSource cancellationTokenSource;
        private int totalFilesShredded = 0;
        private int totalFilesSkipped = 0;
        private int multiPassLevel = 1;
        private bool taskRunning;

        private TaskbarIcon tbi;

        public BackupAutomator()
        {
            InitializeComponent();
            InitializeTrayIcon();

            // UI Buttons
            button2X = new BitmapImage(new Uri("/Images/Button2X.png", UriKind.Relative));
            button2D = new BitmapImage(new Uri("/Images/Button2D.png", UriKind.Relative));
            button2H = new BitmapImage(new Uri("/Images/Button2H.png", UriKind.Relative));
            button3D = new BitmapImage(new Uri("/Images/Button3D.png", UriKind.Relative));
            button3H = new BitmapImage(new Uri("/Images/Button3H.png", UriKind.Relative));
            button4D = new BitmapImage(new Uri("/Images/Button4D.png", UriKind.Relative));
            button4H = new BitmapImage(new Uri("/Images/Button4H.png", UriKind.Relative));

            online = new BitmapImage(new Uri("/Images/Online.png", UriKind.Relative));
            offline = new BitmapImage(new Uri("/Images/Offline.png", UriKind.Relative));

            discordD = new BitmapImage(new Uri("/Images/Discord.png", UriKind.Relative));
            discordH = new BitmapImage(new Uri("/Images/Discord_H.png", UriKind.Relative));
            patreonD = new BitmapImage(new Uri("/Images/Patreon.png", UriKind.Relative));
            patreonH = new BitmapImage(new Uri("/Images/Patreon_H.png", UriKind.Relative));
            githubD = new BitmapImage(new Uri("/Images/Github.png", UriKind.Relative));
            githubH = new BitmapImage(new Uri("/Images/Github_H.png", UriKind.Relative));
            webD = new BitmapImage(new Uri("/Images/Website.png", UriKind.Relative));
            webH = new BitmapImage(new Uri("/Images/Website_H.png", UriKind.Relative));
            deviantArtD = new BitmapImage(new Uri("/Images/DeviantArt.png", UriKind.Relative));
            deviantArtH = new BitmapImage(new Uri("/Images/DeviantArt_H.png", UriKind.Relative));
            artstationD = new BitmapImage(new Uri("/Images/Artstation.png", UriKind.Relative));
            artstationH = new BitmapImage(new Uri("/Images/Artstation_H.png", UriKind.Relative));

        }

        private void InitializeTrayIcon()
        {
            tbi = new TaskbarIcon();
            var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Images/Icon.ico", UriKind.Absolute)).Stream;
            tbi.Icon = new System.Drawing.Icon(iconStream);

            tbi.TrayMouseDoubleClick += Tbi_TrayMouseDoubleClick;
        }

        private void Tbi_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void MinimizeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (tbi != null)
            {
                e.Cancel = true;
                this.Hide();
            }

            base.OnClosing(e);
        }

        private void TargetFolder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
            folderDialog.IsFolderPicker = true;
            folderDialog.Title = "Select the target folder for the process";

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                BackupTarget.Text = folderDialog.FileName;
            }
        }
        private void TargetFiles_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CommonOpenFileDialog fileDialog = new CommonOpenFileDialog();
            fileDialog.IsFolderPicker = false;
            fileDialog.Multiselect = true;
            fileDialog.Title = "Select the target files for the process";

            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string[] selectedFiles = fileDialog.FileNames.ToArray();
                string filesText = string.Join(", ", selectedFiles);
                BackupFileTarget.Text = filesText;
            }
        }
        private void PickSaveDirectory_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
            folderDialog.IsFolderPicker = true;
            folderDialog.Title = "Select a directory for backups to be saved in";

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SaveTarget.Text = folderDialog.FileName;
            }
        }

        // Basic Buttons
        private void btnInfo_Click(object sender, RoutedEventArgs e)
        { ShowInformation(); }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        { System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized; }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        { System.Windows.Application.Current.Shutdown(); }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        { if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); } }

        // Button Highlight
        private void Return_MouseEnter(object sender, MouseEventArgs e)
        { Return.Source = button2H; }
        private void Return_MouseLeave(object sender, MouseEventArgs e)
        { Return.Source = button2D; }
        private void Discord_MouseEnter(object sender, MouseEventArgs e)
        { Discord.Source = discordH; }
        private void Discord_MouseLeave(object sender, MouseEventArgs e)
        { Discord.Source = discordD; }
        private void Patreon_MouseEnter(object sender, MouseEventArgs e)
        { Patreon.Source = patreonH; }
        private void Patreon_MouseLeave(object sender, MouseEventArgs e)
        { Patreon.Source = patreonD; }
        private void Github_MouseEnter(object sender, MouseEventArgs e)
        { Github.Source = githubH; }
        private void Github_MouseLeave(object sender, MouseEventArgs e)
        { Github.Source = githubD; }
        private void Website_MouseEnter(object sender, MouseEventArgs e)
        { Website.Source = webH; }
        private void Website_MouseLeave(object sender, MouseEventArgs e)
        { Website.Source = webD; }
        private void DeviantArt_MouseEnter(object sender, MouseEventArgs e)
        { DeviantArt.Source = deviantArtH; }
        private void DeviantArt_MouseLeave(object sender, MouseEventArgs e)
        { DeviantArt.Source = deviantArtD; }
        private void Artstation_MouseEnter(object sender, MouseEventArgs e)
        { Artstation.Source = artstationH; }
        private void Artstation_MouseLeave(object sender, MouseEventArgs e)
        { Artstation.Source = artstationD; }
        private void OpenLastLogsBtn_MouseEnter(object sender, MouseEventArgs e)
        { OpenLastLogsBtn.Source = button3H; }
        private void OpenLastLogsBtn_MouseLeave(object sender, MouseEventArgs e)
        { OpenLastLogsBtn.Source = button3D; }
        private void BackupBtn_MouseEnter(object sender, MouseEventArgs e)
        { ShredBtn.Source = button2H; }
        private void BackupBtn_MouseLeave(object sender, MouseEventArgs e)
        { ShredBtn.Source = button2D; }
        private void PickDirectory_MouseEnter(object sender, MouseEventArgs e)
        { PickDirectory.Source = button4H; }
        private void PickDirectory_MouseLeave(object sender, MouseEventArgs e)
        { PickDirectory.Source = button4D; }
        private void PickSaveDirectory_MouseEnter(object sender, MouseEventArgs e)
        { PickSaveDirectory.Source = button4H; }
        private void PickSaveDirectory_MouseLeave(object sender, MouseEventArgs e)
        { PickSaveDirectory.Source = button4D; }

        // Links
        private void Patreon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://www.patreon.com/msdysphoria";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void Website_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://msdysphoria.shop/";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void DeviantArt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://www.deviantart.com/msdysphoria";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void Artstation_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://www.artstation.com/msdysphoria";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void Discord_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://discord.gg/uQDPFt6WKn";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void Github_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string patreonLink = "https://github.com/MsDysphoria";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = patreonLink,
                UseShellExecute = true
            });
        }

        private void Return_MouseDown(object sender, MouseButtonEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            Storyboard fadeOutStoryboard = this.FindResource("FadeOut_Info") as Storyboard;
            fadeOutStoryboard?.Begin();
        }
        private void ShowInformation()
        {

            cancellationTokenSource = new CancellationTokenSource();
            _ = Typewriter(0, cancellationTokenSource.Token);

            Storyboard fadeInStoryboard = this.FindResource("FadeIn_Info") as Storyboard;
            fadeInStoryboard.Begin();
        }
        public async Task Typewriter(int message, CancellationToken cancellationToken)
        {
            Author.Text = "";
            string msg;
            if (message == 0) { msg = ""; }
            else if (message == 1) { msg = "Created by Ms. Dysphoria"; }
            else { msg = "Discord: msdysphoria"; }

            Random randomDelay = new Random();

            for (int i = 0; i < msg.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                Author.Text += msg[i].ToString();
                int delay = randomDelay.Next(35, 55);
                await Task.Delay(delay, cancellationToken);
            }

            if (message == 0)
            {
                await Task.Delay(2500, cancellationToken);
                await Typewriter(1, cancellationToken);
            }
            else if (message == 1)
            {
                Storyboard fadeInStoryboard = this.FindResource("GlowAuthor") as Storyboard;
                fadeInStoryboard.Begin();
                await Task.Delay(5000, cancellationToken);
                await Typewriter(2, cancellationToken);
            }
            else
            {

                await Task.Delay(5000, cancellationToken);
                await Typewriter(1, cancellationToken);
            }
        }

        private void WriteToLog()
        {
            string logFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Latest.log");
            string backupName = BackupName.Text;
            string backupFolder = BackupTarget.Text;

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - {backupFolder} - {backupName}");
            }
        }


        private void ConsoleSuccess()
        {
            ResultConsole.Document.Blocks.Clear();

            Paragraph p = new Paragraph();
            p.TextAlignment = TextAlignment.Center;

            Run r = new Run($"▰▰▰▰▰▰▰▰▰▰ Backup Created ▰▰▰▰▰▰▰▰▰▰");
            r.Foreground = new SolidColorBrush(Colors.Green);

            p.Inlines.Add(r);
            ResultConsole.Document.Blocks.Add(p);

            Paragraph pt = new Paragraph();
            pt.TextAlignment = TextAlignment.Center;

            Run rt = new Run($"Last Backup: {DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
            rt.Foreground = new SolidColorBrush(Colors.White);
            pt.Inlines.Add(rt);

            ResultConsole.Document.Blocks.Add(pt);

            Paragraph ps = new Paragraph();
            ps.TextAlignment = TextAlignment.Center;

            Run rs = new Run($"Open the latest log to view the details");
            rs.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x79, 0xD3, 0xF1));
            ps.Inlines.Add(rs);
            ResultConsole.Document.Blocks.Add(ps);
        }

        private void ConsoleError(string text, Color color)
        {
            ResultConsole.Document.Blocks.Clear();

            Paragraph p = new Paragraph();
            Run r = new Run($"");
            p.Inlines.Add(r);
            ResultConsole.Document.Blocks.Add(p);

            Paragraph pt = new Paragraph
            {
                TextAlignment = TextAlignment.Center
            };

            Run rt = new Run(text)
            {
                Foreground = new SolidColorBrush(color)
            };
            pt.Inlines.Add(rt);

            ResultConsole.Document.Blocks.Add(pt);
        }

        private void OpenLastLogsBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string rootDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string logFilePath = System.IO.Path.Combine(rootDirectory, "Latest.log");

                Process.Start("notepad.exe", logFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log file: {ex.Message}");
            }
        }

        private async void PerformBackup()
        {
            taskRunning = true;
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            int backupInterval = int.Parse(BackupInterval.Text);
            int selectedType = BackupType.SelectedIndex;
            string backupName = BackupName.Text;

            string[] targetFiles = null;
            string targetFolder = null;
            bool includeSubfolders = SubfolderCheckbox.IsChecked ?? false;



            try
            {
                if (selectedType == 0)
                {
                    targetFiles = BackupFileTarget.Text.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    targetFolder = BackupTarget.Text;
                }

                string backupFolder = System.IO.Path.Combine(SaveTarget.Text, $"{backupName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
                System.IO.Directory.CreateDirectory(backupFolder);

                if (selectedType == 0)
                {
                    foreach (string file in targetFiles)
                    {
                        token.ThrowIfCancellationRequested();
                        string destFile = System.IO.Path.Combine(backupFolder, System.IO.Path.GetFileName(file));
                        File.Copy(file, destFile);
                    }
                }
                else
                {
                    IEnumerable<string> files = Directory.GetFiles(targetFolder, "*.*", includeSubfolders ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        token.ThrowIfCancellationRequested();
                        string destFile = System.IO.Path.Combine(backupFolder, System.IO.Path.GetRelativePath(targetFolder, file));
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destFile));
                        File.Copy(file, destFile);
                    }
                }

                if (CompressCheckBox.IsChecked ?? false)
                {
                    string zipFile = System.IO.Path.Combine(SaveTarget.Text, $"{backupName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.zip");
                    ZipFile.CreateFromDirectory(backupFolder, zipFile, CompressionLevel.SmallestSize, true);
                    Directory.Delete(backupFolder, true);
                }

                ConsoleSuccess();
                WriteToLog();

                await Task.Delay(TimeSpan.FromMinutes(backupInterval), token);
                PerformBackup();
            }
            catch (OperationCanceledException)
            {
                taskRunning = false;
                ConsoleError("Backup operation is aborted", Colors.IndianRed);
            }
        }


        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DirectoryGrid != null && FileGrid != null)
            {
                int selectedType = BackupType.SelectedIndex;
                if (selectedType == 0)
                {
                    FileGrid.Visibility = Visibility.Visible;
                    DirectoryGrid.Visibility = Visibility.Hidden;
                    SubfolderCheckbox.IsEnabled = false;

                    SubfolderCheckbox.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x90, 0x90, 0x90));
                    SubfolderCheckbox.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x6F, 0x6F, 0x6F));
                }
                else
                {
                    FileGrid.Visibility = Visibility.Hidden;
                    DirectoryGrid.Visibility = Visibility.Visible;
                    SubfolderCheckbox.IsEnabled = true;

                    SubfolderCheckbox.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                    SubfolderCheckbox.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xA9, 0xA9, 0xA9));
                }
            }
        }

        private void StartBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int selectedType = BackupType.SelectedIndex;

            if (selectedType == 0)
            {
                if (BackupFileTarget.Text == "Pick the target files to backup")
                {
                    ConsoleError("Please select the target files to backup", Colors.IndianRed);
                    return;
                }
            }
            else if (selectedType == 1)
            {
                if (BackupTarget.Text == "Pick the target directory to backup")
                {
                    ConsoleError("Please select the target directory to backup", Colors.IndianRed);
                    return;
                }
            }

            if (BackupName.Text == "Specify a name to save the backup")
            {
                ConsoleError("Please specify a backup name.", Colors.IndianRed);
                return;
            }
            if (SaveTarget.Text == "Pick a directory in which backups will be saved")
            {
                ConsoleError("Please select a directory in which backup will be saved.", Colors.IndianRed);
                return;
            }
            if (BackupInterval.Text == "" || !int.TryParse(BackupInterval.Text, out _))
            {
                ConsoleError("Please enter a valid numerical interval.", Colors.IndianRed);
                return;
            }

            if (!taskRunning)
            {
                taskRunning = true;
                PerformBackup();
                Status.Source = online;
                ProcessHeader.Text = "Stop Backup";
            }
            else
            {
                cancellationTokenSource.Cancel();
                taskRunning = false;
                Status.Source = offline;
                ProcessHeader.Text = "Start Backup";
            }
        }

        private void BackupInterval_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text[0]))
            {
                e.Handled = true;
            }
        }

        private void BackupInterval_GotFocus(object sender, RoutedEventArgs e)
        {
            BackupInterval.Text = string.Empty;
        }

        private void MinimizeBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            MinimizeBtn.Source = button3H;
        }

        private void MinimizeBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            MinimizeBtn.Source = button3D;
        }
    }
}
