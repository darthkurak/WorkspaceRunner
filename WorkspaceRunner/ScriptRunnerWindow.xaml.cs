using WorkspaceRunner.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WorkspaceRunner
{
    /// <summary>
    /// Interaction logic for ScriptRunnerWindow.xaml
    /// </summary>
    public partial class ScriptRunnerWindow : Window
    {
        public ScriptRunnerWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (DataContext is ICloseable)
                {
                    (DataContext as ICloseable).RequestClose += (_, __) => this.Dispatcher.Invoke(() => this.Close());
                }
            };
        }

    }
}
