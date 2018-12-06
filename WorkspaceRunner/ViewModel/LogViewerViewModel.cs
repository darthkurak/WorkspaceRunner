using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkspaceRunner.ViewModel
{
    public class LogViewerViewModel : ViewModelBase
    {
        public string Log { get; set; }
        public string Title { get; set; }

        public bool WasSuccess { get; set; }
    }
}
