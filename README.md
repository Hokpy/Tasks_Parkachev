# Tasks_Parkachev

# 📋 TaskManager — Менеджер задач

## 🗂 Структура репозитория

```
TaskManager/
├── TaskManager.sln                  # Solution-файл Visual Studio
├── TaskManager.Core/                # Бизнес-логика (Class Library)
│   ├── Enums/
│   │   ├── TaskPriority.cs          # Приоритет: Low, Medium, High
│   │   └── TaskStatus.cs            # Статус: New, InProgress, Completed
│   ├── Models/
│   │   ├── TaskItem.cs              # Модель задачи
│   │   └── TaskStatistics.cs        # Модель статистики
│   └── Services/
│       └── TaskService.cs           # Вся бизнес-логика
├── TaskManager.WPF/                 # WPF-приложение
│   ├── ViewModels/
│   │   ├── BaseViewModel.cs         # INotifyPropertyChanged
│   │   ├── RelayCommand.cs          # ICommand для MVVM
│   │   └── MainViewModel.cs         # Главная ViewModel
│   └── Views/
│       ├── MainWindow.xaml/.cs      # Главное окно
│       ├── TaskDialog.xaml/.cs      # Диалог добавления/редактирования
│       └── Converters.cs            # Value Converters для DataGrid
├── TaskManager.Tests/               # Модульные тесты (xUnit)
│   └── TaskServiceTests.cs          # 25+ тестов для TaskService
└── docs/
    ├── README.md                    # Этот файл
    └── Пояснительная_записка.md     # Документация проекта
```

---

## 🚀 Запуск проекта

### Через Visual Studio 2022

1. Открыть файл `TaskManager.sln`
2. В Solution Explorer убедиться, что **TaskManager.WPF** установлен как Startup
   Project  
   (жирный шрифт в дереве; если нет — ПКМ → _Set as Startup Project_)
3. Нажать **F5** или кнопку ▶ Run

### Через командную строку

```bash
# Клонировать репозиторий
git clone <url-репозитория>
cd TaskManager

# Сборка всего решения
dotnet build TaskManager.sln

# Запуск приложения
dotnet run --project TaskManager.WPF/TaskManager.WPF.csproj
```

---

## 🧪 Запуск тестов

```bash
# Все тесты
dotnet test TaskManager.Tests/TaskManager.Tests.csproj

# С подробным выводом
dotnet test --verbosity normal

# Через Visual Studio: Test → Run All Tests (Ctrl+R, A)
```

Тесты охватывают:

- ✅ Добавление, обновление, удаление задач
- ✅ Поиск по названию и описанию
- ✅ Фильтрация по статусу
- ✅ Сортировка по приоритету и сроку
- ✅ Сохранение / загрузка JSON
- ✅ Статистика и процент выполнения
- ✅ Логика просроченности

---

## 🎯 Функционал приложения

| Функция        | Описание                                                                    |
| -------------- | --------------------------------------------------------------------------- |
| Список задач   | DataGrid с сортировкой, выделением важных (жёлтый) и просроченных (красный) |
| Добавление     | Диалоговое окно с валидацией полей                                          |
| Редактирование | Изменение всех полей выбранной задачи                                       |
| Удаление       | С подтверждением через MessageBox                                           |
| Поиск          | Поиск по названию и описанию в реальном времени                             |
| Фильтр         | По статусу (Все / Новые / В работе / Завершённые)                           |
| Сортировка     | По приоритету (убывание) или по сроку (возрастание)                         |
| Сохранение     | Экспорт задач в JSON-файл                                                   |
| Загрузка       | Импорт задач из JSON-файла                                                  |
| Статистика     | Панель: всего, завершено, просрочено, важных, % выполнения                  |

---

## 🏗 Архитектура

Проект реализован по паттерну **MVVM** (Model-View-ViewModel):

```
View (XAML)  ←→  ViewModel  ←→  Service  ←→  Model
MainWindow        MainVM         TaskSvc    TaskItem
TaskDialog                                 TaskStats
```

- **Model** (`TaskManager.Core`) — не зависит ни от чего, тестируется отдельно
- **ViewModel** — привязывает данные к View через `INotifyPropertyChanged`
- **View** — только разметка XAML, минимальный code-behind
- **RelayCommand** — реализация `ICommand` для привязки кнопок к методам
  ViewModel
