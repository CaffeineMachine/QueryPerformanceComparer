using System.Windows.Controls;
using QueryPerformance.Interfaces;
using QuerySessionSummaryControl;

namespace QueryPerformance.DefaultPlugins
{
    public class TestByCountPlugin : IPlugin
    {
        public TestByCountPlugin()
            : this(new QuerySessionControl())
        {
        }

        public TestByCountPlugin(UserControl control)
        {
            Control = control;
        }
        public UserControl Control { get; private set; }

        public string DisplayName
        {
            get { return "Run Test By Count"; }
        }
    }
}
