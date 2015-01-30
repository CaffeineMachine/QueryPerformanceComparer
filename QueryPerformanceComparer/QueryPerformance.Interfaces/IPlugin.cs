using System.Windows.Controls;

namespace QueryPerformance.Interfaces
{
    public interface IPlugin
    {
        UserControl Control { get; }
        string DisplayName { get; }
        void Run(Grid hostPanel);
    }
}
