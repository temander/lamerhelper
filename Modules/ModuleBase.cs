using System.Windows.Controls;

namespace LamerHelper.Modules
{
    public abstract class ModuleBase : UserControl, IModule
    {
        public abstract string ModuleName { get; }
        public abstract string DisplayName { get; }
        public abstract string Category { get; }
        public virtual UserControl GetModuleControl() => this;
    }
}
