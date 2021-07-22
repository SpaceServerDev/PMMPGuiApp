using Microsoft.Win32;
using PMMPGuiApp.Data;
using PMMPGuiApp.Windows.PoggitWindow;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Management;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PMMPGuiApp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>
        /// Path to the folder
        /// </summary>
        private string path = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\PocketMine-Gui";

        /// <summary>
        /// execution process
        /// </summary>
        private Process process;

        /// <summary>
        /// Class that stores the ID of the process.
        /// </summary>

        private ProcessData processData = new ProcessData();

        /// <summary>
        /// True while downloading.
        /// </summary>
        private bool download;

        /// <summary>
        /// Flag to check if a file has been saved.
        /// </summary>
        private bool filesave = false;

        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Loaded(object sender, RoutedEventArgs e) {
            Directory.CreateDirectory(path);
            if (!File.Exists(path + @"\PocketMine-MP.phar")) startPMMPInstall();
        }

        private void PMMPINSTALL_Click(object sender, RoutedEventArgs e) {
            startPMMPInstall();

        }

        private void serverEngine_Click(object sender, RoutedEventArgs e) {
            if (!isOpenPMMP()) {
                OpenFileDialog open = new OpenFileDialog() {
                    Title = Properties.Resources.PleaseSelectPlugin,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Filter = Properties.Resources.PharFile + "|*.phar",
                };

                if (open.ShowDialog() != true) {
                    return;
                }

                if (Path.GetFileName(open.FileName) != "PocketMine-MP.phar") {
                    MessageBox.Show(Properties.Resources.NotMatchServerEngineFileName, "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (File.Exists(path + @"\PocketMine-MP.phar")) File.Delete(path + @"\PocketMine-MP.phar");
                File.Copy(open.FileName, path + @"\" + Path.GetFileName(open.FileName));
                textboxApeendToAddTimestamp(Properties.Resources.IntroducingTheCustomServerEngine);
            } else {
                MessageBox.Show(Properties.Resources.NotDownloadforPMMPisRunning, "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EX_Click(object sender, RoutedEventArgs e) {
            Process.Start("explorer.exe", path);
        }

        public void exit_click(object sender, RoutedEventArgs e) {
            Application.Current.MainWindow.Close();
        }


        private async void executePMMP_Click(object sender, RoutedEventArgs e) {
            if (download) {
                textboxApeendToAddTimestamp(Properties.Resources.DownloadPMMP);
                return;
            }
            if (process == null) {
                if (processData.getProcess() != -1) {
                    try {
                        if (Process.GetProcessById(processData.getProcess()).ProcessName == "php") {
                            Debug.Print((Process.GetProcessById(processData.getProcess()).ProcessName == "php").ToString());
                            textboxApeendToAddTimestamp(Properties.Resources.AlreadyExecutePMMP);
                            textboxApeendToAddTimestamp(Properties.Resources.PleaseKillPMMP);
                            return;
                        }
                    } catch { }
                }
            }
            if (File.Exists(path + @"\start.cmd")) {
                if (!isOpenPMMP()) {
                    textboxApeend("\n>>"+Properties.Resources.PMMPExecuteNow+"\n\n");
                    await Task.Run(() => {
                        process = new Process();
                        ProcessStartInfo info = new ProcessStartInfo();
                        info.FileName = "cmd.exe";
                        info.Arguments = "/C " + path + @"\start.cmd";
                        info.RedirectStandardInput = true;
                        info.UseShellExecute = false;
                        info.RedirectStandardOutput = true;
                        info.RedirectStandardError = true;
                        info.CreateNoWindow = true;
                        info.StandardOutputEncoding = Encoding.UTF8;

                        process.StartInfo = info;
                        process.OutputDataReceived += new DataReceivedEventHandler(process_DataReceived);
                        process.Start();
                        process.BeginOutputReadLine();
                        if (!File.Exists(path + @"\server.properties")) {
                            MessageBoxResult result = MessageBox.Show(Properties.Resources.AcceptLicense+"\n"+Properties.Resources.License+"\n"+Properties.Resources.Agree, "PMMPGUI", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if(result == MessageBoxResult.No) {
                                this.filesave = true;
                                useTextBoxInAsync(Properties.Resources.NotAgreeLicense,true);
                                if (Process.GetProcessById(processData.getProcess()).ProcessName == "php") {
                                    Process.GetProcessById(processData.getProcess()).Kill();
                                }
                                process.Dispose();
                                return;
                            }
                            process.StandardInput.WriteLine(Properties.Resources.SelectLanguage);
                            process.StandardInput.WriteLine("y");
                            process.StandardInput.WriteLine("y");
                            process.StandardInput.WriteLine("e");
                        }
                    });
                    this.filesave = true;
                    MenuItem_open_button.Header = Properties.Resources.StopPMMP;
                    open_button.Content = Properties.Resources.StopPMMP;
                } else {
                    await Task.Run(() => {
                        process.StandardInput.WriteLine("stop");
                        while(isOpenPMMP())
                    });
                    process.Kill();
                    MenuItem_open_button.Header = Properties.Resources.ExecutePMMP;
                    open_button.Content = Properties.Resources.ExecutePMMP;
                    textboxApeendToAddTimestamp(Properties.Resources.StoppedPMMP);
                }
            } else {
                textboxApeendToAddTimestamp(Properties.Resources.PMMPNotFound);
                textboxApeendToAddTimestamp(Properties.Resources.PleaseInstallPMMP);
            }
        }

        private void KillPMMP_Click(object sender, RoutedEventArgs e) {
            if (process == null) {
                if (processData.getProcess() == -1) {
                    textboxApeendToAddTimestamp(Properties.Resources.NotExecutePMMP);
                    return;
                }
            }
            MessageBoxResult result = MessageBox.Show(Properties.Resources.ConfirmKillPMMP, "PMMPGUI", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;
            if (process != null) {
                try {
                    Process.GetProcessById(getPMMPProcessId()).Kill();
                    MenuItem_open_button.Header = Properties.Resources.ExecutePMMP;
                    open_button.Content = Properties.Resources.ExecutePMMP;
                    textboxApeendToAddTimestamp(Properties.Resources.KillingPMMP);
                    return;
                } catch {
                    textboxApeendToAddTimestamp(Properties.Resources.NotExecutePMMP);
                    return;
                }
            }
            if (processData.getProcess() != -1) {
                try {
                    if (Process.GetProcessById(processData.getProcess()).ProcessName == "php") {
                        Process.GetProcessById(processData.getProcess()).Kill();
                        textboxApeendToAddTimestamp(Properties.Resources.KillingPMMP);
                    }
                } catch {
                    textboxApeendToAddTimestamp(Properties.Resources.NotExecutePMMP);
                }
            }
        }

        private void Propeties_Click(object sender, RoutedEventArgs e) {

        }

        private void PMMPOption_Click(object sender, RoutedEventArgs e) {

        }


        private void SelectPlugin_Click(object sender, RoutedEventArgs e) {
            if (!isOpenPMMP()) {
                OpenFileDialog open = new OpenFileDialog() {
                    Title = Properties.Resources.PleaseSelectPlugin,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Filter = Properties.Resources.PharFile+"|*.phar",
                };

                if (open.ShowDialog() != true) {
                    return;
                }

                if (File.Exists(path + @"\plugins\" + Path.GetFileName(open.FileName))) File.Delete(path + @"\plugins\" + Path.GetFileName(open.FileName));
                File.Copy(open.FileName, path + @"\plugins\" + Path.GetFileName(open.FileName));
                textboxApeendToAddTimestamp(Properties.Resources.Introduction+" >> "+ Path.GetFileNameWithoutExtension(open.FileName));
            } else {
                MessageBox.Show(Properties.Resources.NotDownloadforPMMPisRunning, "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SearchPoggit_Click(object sender, RoutedEventArgs e) {
            if (!isOpenPMMP()) {
                PoggitWindow window = new(this);
                window.Owner = this;
                window.ShowDialog();
            } else {
                MessageBox.Show(Properties.Resources.NotDownloadforPMMPisRunning, "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Poggit_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://poggit.pmmp.io") { CreateNoWindow = true });
        }

        private void Forum_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://forum.mcbe.jp/resources/categories/2/") { CreateNoWindow = true });
        }

        private void other_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("PMMPGui version 0.5.2\n\nCopylight(C)2021 yurisi\nAll rights reserved.\n\ngithub\nhttps://github.com/yurisi0212/PMMPGuiApp\n\nPocketMine-MP\ngithub\nhttps://github.com/pmmp/PocketMine-MP", "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Input_Click(object sender, RoutedEventArgs e) {
            sendPMMPCommand();
        }

        private void Input_textbox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            Key key = e.Key;
            if (key == Key.Enter && Input_textbox.Text != "") {
                sendPMMPCommand();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e) {
            if (download) {
                MessageBoxResult result = MessageBox.Show(Properties.Resources.ConfirmDownloadProgress, "PMMPGUI", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) {
                    e.Cancel = true;
                    return;
                }

            }
            if (isOpenPMMP()) {
                MessageBoxResult result = MessageBox.Show(Properties.Resources.RunningPMMP+ Properties.Resources.ConfirmKillPMMP+"\n*"+ Properties.Resources.NotBeSavedforPMMPisRunning, "PMMPGUI", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) {
                    e.Cancel = true;
                    return;
                }
            }

            if (process != null) {
                string taskkill = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "taskkill.exe");
                using (var procKiller = new System.Diagnostics.Process()) {
                    procKiller.StartInfo.FileName = taskkill;
                    procKiller.StartInfo.Arguments = string.Format("/PID {0} /T /F", process.Id);
                    procKiller.StartInfo.CreateNoWindow = true;
                    procKiller.StartInfo.UseShellExecute = false;
                    procKiller.Start();
                    procKiller.WaitForExit();
                }
            }
        }
        private async void startPMMPInstall() {
            if (isOpenPMMP()) {
                textboxApeend(Properties.Resources.NotInstall_PMMPisRunning+"\n");
                return;
            }
            textboxApeend("\n+=+="+ Properties.Resources.InstallPMMP + " ..... ("+ Properties.Resources.UpdateAndInstallTime + ")=+=+\n\n");
            download = true;
            await Task.Run(() => this.download = pmmpInstall());
        }

        private bool pmmpInstall() {
            string[] urls = new string[] {
                "https://jenkins.pmmp.io/job/PocketMine-MP/lastSuccessfulBuild/artifact/PocketMine-MP.phar",
                "https://jenkins.pmmp.io/job/PocketMine-MP/lastBuild/artifact/start.cmd",
                "https://jenkins.pmmp.io/job/PHP-7.4-Aggregate/lastBuild/artifact/PHP-7.4-Windows-x64.zip",
                "https://raw.githubusercontent.com/pmmp/PocketMine-MP/stable/composer.json",
                "https://getcomposer.org/download/latest-stable/composer.phar",
                path + @"\PHP-7.4-Windows-x64.zip",
                path + @"\vc_redist.x64.exe"
            };

            if (!Environment.Is64BitProcess) {
                MessageBox.Show(Properties.Resources.NotExecute32bit, "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            DateTime start = DateTime.Now;

            using (System.Net.WebClient wc = new()) {

                useTextBoxInAsync(Properties.Resources.DownloadPMMP + "\n", true);
                wc.DownloadFile(urls[0], path + @"\PocketMine-MP.phar");

                if (!File.Exists(path + @"\bin\php\php.exe")) {
                    using (PowerShell powerShell = PowerShell.Create()) {
                        useTextBoxInAsync(Properties.Resources.DownloadBath + "\n", true);
                        wc.DownloadFile(urls[1], path + @"\start.cmd");
                        useTextBoxInAsync(Properties.Resources.DownloadBin + "\n", true);
                        wc.DownloadFile(urls[2], path + @"\PHP-7.4-Windows-x64.zip");
                        useTextBoxInAsync(Properties.Resources.ExtractBin + "\n", true);
                        ZipFile.ExtractToDirectory(urls[5], path);
                        File.Delete(urls[5]);
                        useTextBoxInAsync(Properties.Resources.ExecuteRuntime + "\n", true);
                        Process.Start(urls[6]);
                        useTextBoxInAsync(Properties.Resources.DownloadComposer + "\n", true);
                        wc.DownloadFile(urls[3], path + @"\composer.json");
                        wc.DownloadFile(urls[4], path + @"\bin\composer.phar");
                        wc.Dispose();
                        useTextBoxInAsync(Properties.Resources.InstallComposer + "\n", true);
                        composerInstall("cd " + path, powerShell);
                        composerInstall(@"bin\php\php.exe bin\composer.phar install", powerShell);
                        powerShell.Stop();
                    }
                }
            }

            DateTime end = DateTime.Now;
            TimeSpan ts = end - start;
            useTextBoxInAsync(Properties.Resources.InstallComplete + " (" + ts.TotalSeconds + Properties.Resources.Seconds+ ") >> \"" + path + "\"" +  ")\n\n", true);
            return false;
        }

        private void composerInstall(string command, PowerShell powerShell) {
            string[] parameter;
            parameter = command.Split(" ");
            PSCommand psCommand = new();
            psCommand.AddCommand(parameter[0]);
            if (parameter.Length > 0) {
                int count = 0;
                foreach (string param in parameter) {
                    if (count == 0) {
                        count++;
                        continue;
                    }
                    psCommand.AddArgument(param);
                    count++;
                }
            }
            powerShell.Commands = psCommand;
            try {
                powerShell.Invoke();
            } catch (CommandNotFoundException) { }
        }

        private void sendPMMPCommand() {
            if (download) {
                textboxApeend(Properties.Resources.DownloadingPMMP + "\n");
                return;
            }
            if (!isOpenPMMP()) {
                if (processData.getProcess() != -1) {
                    try {
                        if (Process.GetProcessById(processData.getProcess()).ProcessName == "php") {
                            textboxApeendToAddTimestamp(Properties.Resources.AlreadyExecutePMMP);
                            textboxApeendToAddTimestamp(Properties.Resources.PleaseKillPMMP);
                            return;
                        }
                    } catch { }
                }
                if (!File.Exists(path + @"\start.cmd")) {
                    textboxApeendToAddTimestamp(Properties.Resources.PMMPNotFound);
                    textboxApeendToAddTimestamp(Properties.Resources.PleaseInstallPMMP);
                    return;
                }
                textboxApeendToAddTimestamp(Properties.Resources.NotExecutePMMP);
                textboxApeendToAddTimestamp(Properties.Resources.PleasePressExecuteButton);
                return;
            }
            if (Input_textbox.Text == "") {
                process.StandardInput.WriteLine("\n");
                return;
            }
            textboxApeend("\nCOMMAND >> " + Input_textbox.Text + "\n");
            process.StandardInput.WriteLine(Input_textbox.Text);
            Input_textbox.Text = "";
        }

        private void process_DataReceived(object sender, DataReceivedEventArgs e) {
            useTextBoxInAsync(e.Data + "\n");
        }

        private void useTextBoxInAsync(string str, bool useTimestamp = false) {
            this.Dispatcher.Invoke(() => {
                if (isOpenPMMP()) {
                    if (filesave) {
                        if (processData.getProcess() != getPMMPProcessId()) {
                            processData.setProcess(getPMMPProcessId().ToString());
                            filesave = false;
                        }
                    }

                } else {
                    MenuItem_open_button.Header = Properties.Resources.ExecutePMMP;
                    open_button.Content = Properties.Resources.ExecutePMMP;
                }
                if (useTimestamp) {
                    textboxApeendToAddTimestamp(str);
                } else {
                    textboxApeend(str);
                }
            });
        }

        public void textboxApeendToAddTimestamp(string str) {
            DateTime date = DateTime.Now;
            Output_textbox.AppendText("[" + date.ToString("HH:mm:ss") + "] [PMMPGUI]: " + str + "\n");
            Output_textbox.ScrollToEnd();
        }

        private void textboxApeend(String str) {
            Output_textbox.AppendText(str);
            Output_textbox.ScrollToEnd();
        }

        private bool isOpenPMMP() {
            if (process == null) return false;
            try {
                ManagementObjectSearcher mos = new(String.Format("Select * From Win32_Process Where ParentProcessID={0}", process.Id));

                foreach (ManagementObject mo in mos.Get()) {
                    if (Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])).ProcessName == "php") {
                        return true;
                    }
                }
            } catch { }
            /*if (Process.GetProcessById(process.Id).HasExited) return false;
            if (Process.GetProcessById(process.Id).ProcessName == "php") {
                return true;
            }*/

            return false;
        }

        private int getPMMPProcessId() {

            ManagementObjectSearcher mos = new(String.Format("Select * From Win32_Process Where ParentProcessID={0}", process.Id));

            foreach (ManagementObject mo in mos.Get()) {
                try {
                    if (Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])).ProcessName == "php") {
                        return Convert.ToInt32(mo["ProcessID"]);
                    }
                } catch (InvalidOperationException) { }
            }
            /*try {
                if (Process.GetProcessById(process.Id).ProcessName == "php") {
                    return Convert.ToInt32(process.Id);
                }
            } catch (InvalidOperationException) { }*/
            return -1;
        }
    }
}

