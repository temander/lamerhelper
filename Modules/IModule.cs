using System.Windows.Controls;

namespace LamerHelper.Modules
{
    public interface IModule
    {
        string ModuleName { get; }
        string DisplayName { get; }
        string Category { get; }
        string Description { get; }
        UserControl GetModuleControl();
    }
}
