using System.Windows.Controls;

namespace LamerHelper.Modules
{
    public interface IModule
    {
        string ModuleName { get; }
        string DisplayName { get; }
        string Category { get; }

        UserControl GetModuleControl();
    }
}
