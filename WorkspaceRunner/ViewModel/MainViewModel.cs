using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using WorkspaceRunner.Properties;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.ComponentModel;

namespace WorkspaceRunner.ViewModel
{

    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IHideable
    {
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SetThreadExecutionState([In] uint esFlags);

        public ExecutionStatus LastStartScriptExecutionStatus { get; private set; }
        public ExecutionStatus LastStopScriptExecutionStatus { get; private set; }

        public ProcessStatus Status
        {
            get { return Settings.Default.ProcessStatus; }
            set
            {
                if (Settings.Default.ProcessStatus == value)
                    return;

                if (value == ProcessStatus.Running)
                    SetThreadExecutionState(ES_SYSTEM_REQUIRED);
                else if (value == ProcessStatus.Stopped)
                    SetThreadExecutionState(ES_CONTINUOUS);

                Settings.Default.ProcessStatus = value;
                
                RaisePropertyChanged(nameof(Status));
            }
        }

        private string _startScript;

        public string StartScript
        {
            get { return _startScript; }
            set
            {
                if (_startScript == value)
                    return;

                _startScript = value;
                RaisePropertyChanged(nameof(StartScript));
            }
        }

        private string _stopScript;

        public event EventHandler<EventArgs> RequestHide;

        public string StopScript
        {
            get { return _stopScript; }
            set
            {
                if (_stopScript == value)
                    return;

                _stopScript = value;
                RaisePropertyChanged(nameof(StopScript));
            }
        }

        public RelayCommand<CancelEventArgs> WindowClosingCommand
        {
            get; private set;
        }

        public RelayCommand WindowLoadedCommand
        {
            get; private set;
        }


        public RelayCommand StartCommand
        {
            get; private set;
        }

        public RelayCommand StopCommand
        {
            get; private set;
        }

        public RelayCommand StartScriptLoadCommand
        {
            get; private set;
        }

        public RelayCommand StopScriptLoadCommand
        {
            get; private set;
        }

        public RelayCommand<Window> ExitCommand
        {
            get; private set;
        }

        public RelayCommand<bool> OpenLogCommand
        {
            get; private set;
        }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            InitializeCommands();

            if (!IsInDesignMode)
            {
                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
                SystemEvents.SessionEnding += SystemEvents_SessionEnding;
                Settings.Default.PropertyChanged += Default_PropertyChanged;
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 2000;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (Status == ProcessStatus.Running)
            {
                e.Cancel = true;
                var result = MessageBox.Show("You are about to log off or shutdown machine! Your workspace still running! Want stop it?", "", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    Stop();
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend && Status == ProcessStatus.Running)
            {
                Stop();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var idleTime = IdleTimeFinder.GetIdleTime();
            if (idleTime > Settings.Default.IdleTimeInMinutes * 60 * 1000 && Status == ProcessStatus.Running)
            {
                StopCommand.Execute(null);
                if (Status == ProcessStatus.Stopped)
                {
                    var result = MessageBox.Show("The StopScript was executed as ilde time was detected! Do you want to run StartScript?", "Idle time - script executed!", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        StartCommand.Execute(null);
                    }
                }
            }
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.Default.LaunchAtStartup):
                    {
                        if (Settings.Default.LaunchAtStartup)
                        {
                            StartUpManager.AddApplicationToCurrentUserStartup();
                        }
                        else
                        {
                            StartUpManager.RemoveApplicationFromCurrentUserStartup();
                        }
                        break;
                    }
            }

            Settings.Default.Save();
        }

        private void InitializeCommands()
        {
            WindowLoadedCommand = new RelayCommand(ExecuteWindowLoadedCommand);
            StartCommand = new RelayCommand(ExecuteStartCommand, CanExecuteStartCommand);
            StopCommand = new RelayCommand(ExecuteStopCommand, CanExecuteStopCommand);
            StartScriptLoadCommand = new RelayCommand(ExecuteStartScriptLoadCommand);
            StopScriptLoadCommand = new RelayCommand(ExecuteStopScriptLoadCommand);
            ExitCommand = new RelayCommand<Window>(ExecuteExitCommand);
            OpenLogCommand = new RelayCommand<bool>(ExecuteOpenLogCommand, CanExecuteOpenLogCommand);
            WindowClosingCommand = new RelayCommand<CancelEventArgs>(ExecuteWindowClosingCommand);
        }

        private void ExecuteWindowClosingCommand(CancelEventArgs args)
        {
            if (Status == ProcessStatus.Running)
            {
                var result = MessageBox.Show("You workspace is still running. You have to stop it first. Do you want minimalize this app to tray?", "Can't close!", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    RequestHide(this, EventArgs.Empty);
                }
                args.Cancel = true;
            }
        }

        private bool CanExecuteOpenLogCommand(bool isStartScript)
        {
            if (isStartScript)
                return LastStartScriptExecutionStatus != null;
            else
                return LastStopScriptExecutionStatus != null;
        }

        private bool CanExecuteStartCommand()
        {
            return Status == ProcessStatus.Stopped && !string.IsNullOrWhiteSpace(StartScript);
        }

        private bool CanExecuteStopCommand()
        {
            return Status == ProcessStatus.Running && !string.IsNullOrWhiteSpace(StopScript);
        }

        private void ExecuteExitCommand(Window window)
        {
            window.Close();
        }

        private void ExecuteOpenLogCommand(bool isStartScript)
        {
            LogViewerViewModel vm = new LogViewerViewModel();

            if (isStartScript)
            {
                vm.Log = LastStartScriptExecutionStatus.Log;
                vm.Title = "Last StartScript execution log";
                vm.WasSuccess = LastStartScriptExecutionStatus.WasSuccess;
            }
            else
            {
                vm.Log = LastStopScriptExecutionStatus.Log;
                vm.Title = "Last StopScript execution log";
                vm.WasSuccess = LastStopScriptExecutionStatus.WasSuccess;
            }

            var window = new LogViewer();
            window.DataContext = vm;
            window.ShowDialog();
        }

        private void ExecuteStopCommand()
        {
            Stop();
        }

        private void ExecuteStartCommand()
        {
            Start();
        }

        private void ExecuteStopScriptLoadCommand()
        {
            var fileName = OpenFileDialog();
            Settings.Default.StopScriptPath = fileName;
            LoadStopScript();
        }

        private void ExecuteStartScriptLoadCommand()
        {
            var fileName = OpenFileDialog();
            Settings.Default.StartScriptPath = fileName;
            LoadStartScript();
        }

        private void ExecuteWindowLoadedCommand()
        {
            LoadStartScript();
            LoadStopScript();
            if (Settings.Default.LaunchScriptAtStartup)
            {
                StartCommand.Execute(null);
            }
        }

        private void Start()
        {
            if (Status == ProcessStatus.Stopped)
            {
                Status = ProcessStatus.Starting;
                LastStartScriptExecutionStatus = RunScript(Settings.Default.StartScriptPath);
                if (LastStartScriptExecutionStatus.WasSuccess)
                {
                    Status = ProcessStatus.Running;
                    MessageBox.Show("StartScript was executed with successful!");
                }
                else
                {
                    Status = ProcessStatus.Stopped;
                    MessageBox.Show("StartScript execution failed!");
                }
            }
            else
                MessageBox.Show("StartScript was already executed!");
        }

        private void Stop()
        {
            if (Status == ProcessStatus.Running)
            {
                Status = ProcessStatus.Stopping;
                LastStopScriptExecutionStatus = RunScript(Settings.Default.StartScriptPath);
                if (LastStopScriptExecutionStatus.WasSuccess)
                {
                    Status = ProcessStatus.Stopped;
                    MessageBox.Show("StopScript was executed with successful!");
                }
                else
                {
                    Status = ProcessStatus.Running;
                    MessageBox.Show("StopScript execution failed!");
                }
            }
            else
                MessageBox.Show("StopScript was already executed!");
        }

        private ExecutionStatus RunScript(string scriptPath)
        {
            var vm = new ScriptRunnerViewModel(scriptPath);
            ScriptRunnerWindow runnerWindow = new ScriptRunnerWindow();
            runnerWindow.DataContext = vm;
            runnerWindow.ShowDialog();
            return vm.ExecutionStatus;

            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.UseShellExecute = false;
            //startInfo.FileName = "powershell";
            //startInfo.Arguments = $"-ExecutionPolicy Bypass {scriptPath}";
            //startInfo.RedirectStandardError = true;
            //startInfo.RedirectStandardOutput = true;
            //startInfo.CreateNoWindow = false;
            
            ////var process = Process.Start("powershell", $"-ExecutionPolicy Bypass {scriptPath}");
            //var process = Process.Start(startInfo);
            //var errors = process.StandardError.Read();
            //process.WaitForExit();
            //var exitCode = process.ExitCode;
            //return exitCode == 0;
        }

        private string OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        private void LoadStartScript()
        {
            try
            {
                StartScript = File.ReadAllText(Settings.Default.StartScriptPath);
            }
            catch (Exception exc)
            {
                
            }
        }

        private void LoadStopScript()
        {
            try
            {
                StopScript = File.ReadAllText(Settings.Default.StopScriptPath);
            }
            catch (Exception exc)
            {
                
            }
        }
    }
}