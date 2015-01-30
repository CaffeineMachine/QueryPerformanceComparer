using System.Windows.Controls;
using QueryPerformance.Interfaces;

namespace BillProcessorServicePlugin
{
    public class BillProcessorServicePlugin : IPlugin
    {
        public BillProcessorServicePlugin() : this(new BillProcessorSessionControl())
        {
            
        }

        public BillProcessorServicePlugin(UserControl control)
        {
            Control = control;
        }

        public UserControl Control { get; private set; }

        public string DisplayName
        {
            get { return "Bill Processor Service Tests"; }
        }

        public void Run(Grid hostPanel)
        {
            hostPanel.Children.Clear();
            hostPanel.Children.Add(Control);
        }
    }
}
