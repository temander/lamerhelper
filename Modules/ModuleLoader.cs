﻿using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Windows;

namespace LamerHelper.Modules
{
    [Serializable]
    public class ModuleInfo
    {
        public string ModuleName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Category { get; set; } = "";
        public string Type { get; set; } = "";
    }

    public class ModulesConfig
    {
        public List<ModuleInfo>? Modules { get; set; }
    }

    public static class ModuleLoader
    {
        public static List<IModule> LoadModules(string configPath)
        {
            var modules = new List<IModule>();
            if (!File.Exists(configPath))
                return modules;

            string json = File.ReadAllText(configPath);
            ModulesConfig? config = JsonConvert.DeserializeObject<ModulesConfig>(json);

            if (config?.Modules == null) return modules;
            foreach (var moduleInfo in config.Modules)
            {
                try
                {
                    // Сначала пытаемся получить тип напрямую
                    Type? moduleType = Type.GetType(moduleInfo.Type);
                    if (moduleType == null)
                    {
                        // Нет -> добавляем имя сборки
                        string? assemblyName = Assembly.GetExecutingAssembly().FullName;
                        moduleType = Type.GetType($"{moduleInfo.Type}, {assemblyName}");
                    }

                    if (moduleType == null)
                        continue;

                    object? instance = Activator.CreateInstance(moduleType);
                    if (instance is IModule module)
                        modules.Add(module);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки модуля {moduleInfo.ModuleName}: {ex.Message}", "Модуль",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return modules;
        }
    }
}
