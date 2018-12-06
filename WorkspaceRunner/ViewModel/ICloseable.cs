using System;

namespace WorkspaceRunner.ViewModel
{
    interface ICloseable
    {
        event EventHandler<EventArgs> RequestClose;
    }

    interface IHideable
    {
        event EventHandler<EventArgs> RequestHide;
    }
}
