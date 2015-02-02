using System.Windows.Controls;
using QueryPerformance.Interfaces;
using QuerySessionSummaryControl;

namespace QueryPerformance.DefaultPlugins
{
    public class TestByDurationPlugin : IPlugin
    {
        public TestByDurationPlugin()
            : this(new TimeDurationView())
        {
        }

        public TestByDurationPlugin(UserControl control)
        {
            Control = control;
        }
        public UserControl Control { get; private set; }

        public string DisplayName
        {
            get { return "Run Test For Duration"; }
        }
    }
}
