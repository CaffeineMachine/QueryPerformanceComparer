using System.Windows.Controls;
using QueryPerformance.Interfaces;

namespace QueryPerformance.ThreadedPerformanceSessionPlugin
{
    public class ThreadedPerformancePlugin : IPlugin
    {
        public ThreadedPerformancePlugin() : this(new ThreadedPerformanceSessionControl())
        {
            
        }

        public ThreadedPerformancePlugin(UserControl control)
        {
            Control = control;
        }

        public UserControl Control { get; private set; }

        public string DisplayName
        {
            get { return "Threaded Url Performance Tests"; }
        }
    }
}
