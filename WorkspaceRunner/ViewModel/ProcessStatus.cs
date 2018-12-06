using System;
using System.Configuration;

namespace WorkspaceRunner.ViewModel
{
    [Serializable]
    public enum ProcessStatus
    {
        Starting,
        Running,
        Stopping,
        Stopped
    }
}