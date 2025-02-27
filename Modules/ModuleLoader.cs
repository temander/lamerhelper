using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace LamerHelper.Modules
{
    public class ModuleInfo
    {
        public string ModuleName { get; set; }
        public string DisplayName { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
    }

    public class ModulesConfig
    {
        public List<ModuleInfo> Modules { get; set; }
    }

    public static class ModuleLoader
    {
        public static List<IModule> LoadModules(string configPath)
        {
            var modules = new List<IModule>();
            if (!File.Exists(configPath))
                return modules;

            string json = File.ReadAllText(configPath);
            ModulesConfig config = JsonConvert.DeserializeObject<ModulesConfig>(json);

            foreach (var moduleInfo in config.Modules)
            {
                try
                {
                    // Сначала пытаемся получить тип напрямую
                    Type moduleType = Type.GetType(moduleInfo.Type);
                    if (moduleType == null)
                    {
                        // Нету -> добавляем имя сборки
                        string assemblyName = Assembly.GetExecutingAssembly().FullName;
                        moduleType = Type.GetType($"{moduleInfo.Type}, {assemblyName}");
                    }
                    if (moduleType == null)
                        continue;

                    object instance = Activator.CreateInstance(moduleType);
                    if (instance is IModule module)
                        modules.Add(module);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading module {moduleInfo.ModuleName}: {ex.Message}");
                }
            }
            return modules;
        }
    }
}
