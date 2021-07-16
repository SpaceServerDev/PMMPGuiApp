using Microsoft.Win32;
using PMMPGuiApp.Data;
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
        /// フォルダまでのパス
        /// </summary>
        private string path = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\PocketMine-Gui";

        /// <summary>
        /// 実行プロセス
        /// </summary>
        private Process process;

        /// <summary>
        /// 
        /// </summary>

        private ProcessData processData = new ProcessData();

        /// <summary>
        /// ダウンロード中はtrue
        /// </summary>
        private bool download;

        /// <summary>
        /// ファイルがセーブされてるか確認するフラグ
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
                    Title = "プラグインを選択してください",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Filter = "Pharファイル|*.phar",
                };

                if (open.ShowDialog() != true) {
                    return;
                }

                if (Path.GetFileName(open.FileName) != "PocketMine-MP.phar") {
                    MessageBox.Show("サーバーエンジンの名前は\"PocketMine-MP.phar\"のみ有効です。", "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (File.Exists(path + @"\PocketMine-MP.phar")) File.Delete(path + @"\PocketMine-MP.phar");
                File.Copy(open.FileName, path + @"\" + Path.GetFileName(open.FileName));
                textboxApeendToAddTimestamp("カスタムサーバーエンジンを導入しました");
            } else {
                MessageBox.Show("PMMP実行中にはダウンロードできません。PMMPを停止してください。", "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                textboxApeendToAddTimestamp("\n現在PMMPダウンロード中です。しばらくお待ち下さい。\n");
                return;
            }
            if (process == null) {
                if (processData.getProcess() != -1) {
                    try {
                        if (Process.GetProcessById(processData.getProcess()).ProcessName == "php") {
                            Debug.Print((Process.GetProcessById(processData.getProcess()).ProcessName == "php").ToString());
                            textboxApeendToAddTimestamp("すでにPMMPが起動しているようです。\n");
                            textboxApeendToAddTimestamp("[実行(D)]から[サーバーを強制終了する]をクリックしてください。\n");
                            return;
                        }
                    } catch { }
                }
            }
            if (File.Exists(path + @"\start.cmd")) {
                if (!isOpenPMMP()) {
                    textboxApeend("\n>>PMMP実行中...\n\n");
                    await Task.Run(() => {
                        process = new Process();
                        ProcessStartInfo info = new ProcessStartInfo();
                        info.FileName = "cmd.exe";
                        info.Arguments = "/C " + path + @"\start.cmd";
                        //info.FileName = path + @"\bin\php\php.exe";
                        //info.Arguments = "-c " + path + @"\bin\php " + path + "\\PocketMine-MP.phar";
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
                            process.StandardInput.WriteLine("jpn");
                            process.StandardInput.WriteLine("y");
                            process.StandardInput.WriteLine("y");
                            process.StandardInput.WriteLine("e");
                        }
                    });
                    this.filesave = true;
                    MenuItem_open_button.Header = "PMMPを停止する";
                    open_button.Content = "PMMPを停止する";
                } else {
                    process.StandardInput.WriteLine("stop");
                    process.Kill();
                    MenuItem_open_button.Header = "PMMPを起動する";
                    open_button.Content = "PMMPを起動する";
                }
            } else {
                textboxApeendToAddTimestamp("pmmpが存在しません。\n");
                textboxApeendToAddTimestamp("[ファイル(F)] から [PMMPをインストールする] を選択してください\n");
            }
        }

        private void KillPMMP_Click(object sender, RoutedEventArgs e) {
            if (process == null) {
                if (processData.getProcess() == -1) {
                    textboxApeendToAddTimestamp("PMMPは起動していないようです。");
                    return;
                }
            }
            MessageBoxResult result = MessageBox.Show("PMMPを強制終了しますか？", "PMMPGUI", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;
            if (process != null) {
                try {
                    Process.GetProcessById(getPMMPProcessId()).Kill();
                    MenuItem_open_button.Header = "PMMPを起動する";
                    open_button.Content = "PMMPを起動する";
                    textboxApeendToAddTimestamp("PMMPを強制終了します。\n");
                    return;
                } catch {
                    textboxApeendToAddTimestamp("PMMPは起動していませんでした。\n");
                    return;
                }
            }
            if (processData.getProcess() != -1) {
                try {
                    if (Process.GetProcessById(processData.getProcess()).ProcessName == "php") {
                        Process.GetProcessById(processData.getProcess()).Kill();
                        textboxApeendToAddTimestamp("PMMPを強制終了します。\n");
                    }
                } catch {
                    textboxApeendToAddTimestamp("PMMPは起動していませんでした。\n");
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
                    Title = "プラグインを選択してください",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Filter = "Pharファイル|*.phar",
                };

                if (open.ShowDialog() != true) {
                    return;
                }

                if (File.Exists(path + @"\plugins\" + Path.GetFileName(open.FileName))) File.Delete(path + @"\plugins\" + Path.GetFileName(open.FileName));
                File.Copy(open.FileName, path + @"\plugins\" + Path.GetFileName(open.FileName));
                textboxApeendToAddTimestamp(Path.GetFileNameWithoutExtension(open.FileName) + "を導入しました");
            } else {
                MessageBox.Show("PMMP実行中にはダウンロードできません。PMMPを停止してください。", "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SearchPoggit_Click(object sender, RoutedEventArgs e) {
            if (!isOpenPMMP()) {
                PoggitWindow.PoggitWindow window = new(this);
                window.Owner = this;
                window.ShowDialog();
            } else {
                MessageBox.Show("PMMP実行中にはダウンロードできません。PMMPを停止してください。", "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Poggit_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://poggit.pmmp.io") { CreateNoWindow = true });
        }

        private void Forum_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://forum.mcbe.jp/resources/categories/2/") { CreateNoWindow = true });
        }

        private void other_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("PMMPGui version 0.5.2\n\nCopylight(C)2021 yurisi\nAll rights reserved.\n\ngithub\nhttps://github.com/yurisi0212/PMMPGuiApp", "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBoxResult result = MessageBox.Show("ダウンロードが進行中です。強制終了しますか？", "PMMPGUI", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) {
                    e.Cancel = true;
                    return;
                }

            }
            if (isOpenPMMP()) {
                MessageBoxResult result = MessageBox.Show("PMMPが実行中です。強制終了しますか？\n*[PMMPを停止する]を押さないと一部データが保存されません。", "PMMPGUI", MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
                textboxApeend("PMMP実行時にはインストールできません。\n");
                return;
            }
            textboxApeend("\n+=+=PocketMine-MPをインストールします。 ..... (アップデートは10秒ほど、インストールは5分程かかります)=+=+\n\n");
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
                MessageBox.Show("32bitのwindowsではPMMPを動かすことができません！", "PMMPGUI", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            DateTime start = DateTime.Now;

            using (System.Net.WebClient wc = new()) {

                useTextBoxInAsync("PocketMine-MPダウンロード中..." + "\n", true);
                wc.DownloadFile(urls[0], path + @"\PocketMine-MP.phar");

                if (!File.Exists(path + @"\bin\php\php.exe")) {
                    using (PowerShell powerShell = PowerShell.Create()) {
                        useTextBoxInAsync("バッチファイルダウンロード中..." + "\n", true);
                        wc.DownloadFile(urls[1], path + @"\start.cmd");
                        useTextBoxInAsync("バイナリダウンロード中..." + "\n", true);
                        wc.DownloadFile(urls[2], path + @"\PHP-7.4-Windows-x64.zip");
                        useTextBoxInAsync("バイナリの解凍中..." + "\n", true);
                        ZipFile.ExtractToDirectory(urls[5], path);
                        File.Delete(urls[5]);
                        useTextBoxInAsync("Windowsランタイムインストーラーが起動します。手動でのインストールをお願いします。" + "\n", true);
                        Process.Start(urls[6]);
                        useTextBoxInAsync("composer関連ファイルダウンロード中..." + "\n", true);
                        wc.DownloadFile(urls[3], path + @"\composer.json");
                        wc.DownloadFile(urls[4], path + @"\bin\composer.phar");
                        wc.Dispose();
                        useTextBoxInAsync("composerインストール中......" + "\n", true);
                        composerInstall("cd " + path, powerShell);
                        composerInstall(@"bin\php\php.exe bin\composer.phar install", powerShell);
                        powerShell.Stop();
                    }
                }
            }

            DateTime end = DateTime.Now;
            TimeSpan ts = end - start;
            useTextBoxInAsync("\"" + path + "\"に" + "インストールが完了しました。(" + ts.TotalSeconds + "秒)\n\n", true);
            return false;
        }

        public void addInstallMessage(string plugin) {
            textboxApeendToAddTimestamp(plugin + "をインストールしました。");
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
                textboxApeend("\n現在PMMPダウンロード中です。しばらくお待ち下さい。\n");
                return;
            }
            if (!isOpenPMMP()) {
                if (processData.getProcess() != -1) {
                    try {
                        if (Process.GetProcessById(processData.getProcess()).ProcessName == "php") {
                            textboxApeendToAddTimestamp("すでにPMMPが起動しているようです。\n");
                            textboxApeendToAddTimestamp("[実行(D)]から[サーバーを強制終了する]をクリックしてください。\n");
                            return;
                        }
                    } catch { }
                }
                if (!File.Exists(path + @"\start.cmd")) {
                    textboxApeendToAddTimestamp("PMMPが存在していません\n");
                    textboxApeendToAddTimestamp("[ファイル(F)] から [PMMPをインストールする] を選択してください\n");
                    return;
                }
                textboxApeendToAddTimestamp("PMMPが起動していません。\n");
                textboxApeendToAddTimestamp("起動ボタンを押して下さい。\n");
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
                    MenuItem_open_button.Header = "PMMPを起動する";
                    open_button.Content = "PMMPを起動する";
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
            Output_textbox.AppendText("[" + date.ToString("HH:mm:ss") + "] [PMMPGUI]: " + str);
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
            return -1;
        }
    }
}

