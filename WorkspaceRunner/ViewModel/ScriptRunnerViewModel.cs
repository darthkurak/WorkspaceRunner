using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkspaceRunner.ViewModel
{
    public class ScriptRunnerViewModel : ViewModelBase, ICloseable
    {
        Process _process;
        
        public RelayCommand WindowLoadedCommand
        {
            get; private set;
        }

        private bool _wasErrorOccured = false;

        public ExecutionStatus ExecutionStatus { get; private set; }

        public ScriptRunnerViewModel(string scriptPath)
        {
            ScriptPath = scriptPath;
            WindowLoadedCommand = new RelayCommand(ExecuteWindowLoadedCommand);
        }

        public string ScriptPath { get; set; }

        private string _output;

        public event EventHandler<EventArgs> RequestClose;

        public string Output
        {
            get { return _output; }
            set
            {
                if (_output == value)
                    return;

                _output = value;
                RaisePropertyChanged(nameof(Output));
            }
        }

        private void ExecuteWindowLoadedCommand()
        {
            Run();
        }

        public void Run()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.FileName = "powershell";
            startInfo.Arguments = $"-ExecutionPolicy Bypass {ScriptPath}";
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            _process = new Process();
            //var process = Process.Start("powershell", $"-ExecutionPolicy Bypass {scriptPath}");
            _process.EnableRaisingEvents = true;
            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.ErrorDataReceived += Process_ErrorDataReceived;
            _process.Exited += Process_Exited;
            _process.StartInfo = startInfo;
            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            //var output = process.StandardOutput.ReadToEnd();
            //var error = process.StandardError.ReadToEnd();
           // process.WaitForExit();
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            _process.WaitForExit();
            _process.Close();
            ExecutionStatus = new ExecutionStatus() { WasSuccess = !_wasErrorOccured, Log = Output };
            if (ExecutionStatus.WasSuccess)
                RequestClose(this, EventArgs.Empty);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                _wasErrorOccured = true;
                Output += e.Data + Environment.NewLine;
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Output += e.Data + Environment.NewLine;
            }
        }
    }
}
