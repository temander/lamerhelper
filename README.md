# 📚 Lamer Helper
Программа с универстальными утилитами для упрощённого использования персонального компьютера.

| Функциональность | Наличие |
| - | :-: |
| Изменение цвета выделения курсора мыши | ✅ |
| Создание QR-кода | ✅ |
| Очистка DNS кэша | ✅ |
| Очистка временных файлов Temp | ✅ |
| Очистка кэша обновлений | ✅ |
| Смена цвета папки | ✅ |

## ⚙ Документация
### Создание модуля
1. Перейдите в директорию `Modules` и выберите нужную категорию (Feature, Optimization), в рамках которой будет модуль. В нашем случаи это "Feature"

![image](https://github.com/user-attachments/assets/146b72a8-1904-4d38-a782-7e27795c28c9)

3. Создайте новую папку, например `TestModule`

![image](https://github.com/user-attachments/assets/364538b9-dbfd-4199-905e-42751a3fe500)

4. Перейдите в созданную директорию и добавьте 2 файла: `TestModule.xaml` и `TestModule.xaml.cs`

![image](https://github.com/user-attachments/assets/96a1bc73-f70b-4645-b70b-e727f27e1228)

5. Заполните дефолтным содержанием файлы

**TestModule.xaml**
```xaml
<UserControl x:Class="LamerHelper.Modules.Feature.TestModule"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Height="Auto" Width="Auto">
    <Grid>
        <!-- Your ui components -->
    </Grid>
</UserControl>
```
**TestModule.xaml.cs**
```c#
using System.Windows.Controls;

namespace LamerHelper.Modules.Feature
{
    public partial class TestModule : UserControl, IModule
    {
        public TestModule()
        {
            InitializeComponent();
        }

        public string ModuleName => "TestModule";
        public string DisplayName => "Название модуля";
        public string Category => "Фишка";
        public string Description => "Описание модуля.";
        public UserControl GetModuleControl() => this;

        // Ниже описывается функциональность
    }
}
```
6. Готово! Остаётся внести в конфиг-файл новосозданный модуль
7. Откройте `ModuleConfig.json` и добавьте в него новый массив
```json
{
  "ModuleName": "TestModule",
  "Type": "LamerHelper.Modules.Feature.TestModule"
}
```
*В зависимости от категории модуля не забывайте поменять в файлах `Feature`*

8. Модуль добавлен

![image](https://github.com/user-attachments/assets/001773e4-1fff-450a-8158-fea79b5f51e0)
