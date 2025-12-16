# Лабораторная работа: Анализ производительности коллекций C#

## Описание проекта

Данный проект представляет собой сравнительный анализ производительности различных коллекций в .NET/C#. Основное приложение измеряет время выполнения операций, а модульные тесты проверяют корректность работы всех компонентов.

## Структура проекта

```
ConsoleApp1/
├── Program.cs                    # Основное приложение для тестирования производительности
├── UnitTest.cs                   # Модульные тесты (xUnit)
├── ConsoleApp1.csproj           # Файл проекта основного приложения
└── README.md                    # Документация
```

## Основное приложение

### Функциональность
Приложение выполняет сравнительный анализ производительности следующих коллекций:
- `List<int>`
- `LinkedList<int>`
- `Queue<int>`
- `Stack<int>`
- `ImmutableList<int>`

### Тестируемые операции
Для каждой коллекции измеряется время выполнения:
1. **Добавление элементов** (в конец, в начало, в середину)
2. **Удаление элементов** (из начала, из конца, из середины)
3. **Поиск элемента** по значению
4. **Доступ по индексу** (где поддерживается)

### Параметры тестирования
- **Количество элементов**: 100,000
- **Количество повторений**: 5 раз для каждой операции
- **Измерение времени**: с использованием `Stopwatch`

## Модульные тесты

### Обзор тестов
Проект содержит 12 модульных тестов, охватывающих:

#### 1. **Тесты основных методов тестирования коллекций** (5 тестов)
- `TestList_PerformanceTestsCompleteWithoutException`
- `TestLinkedList_PerformanceTestsCompleteWithoutException`
- `TestQueue_PerformanceTestsCompleteWithoutException`
- `TestStack_PerformanceTestsCompleteWithoutException`
- `TestImmutableList_PerformanceTestsCompleteWithoutException`

**Назначение**: Проверяют, что методы тестирования производительности выполняются без исключений.

#### 2. **Тесты вспомогательных методов** (4 теста)
- `MeasureTime_ReturnsPositiveTimeSpan`
- `GetMiddleNode_WithEmptyList_ReturnsNull`
- `GetMiddleNode_WithSingleElement_ReturnsFirst`
- `GetMiddleNode_WithMultipleElements_ReturnsCorrectNode`
- `PrintResults_WithValidData_DoesNotThrow`

**Назначение**: Проверяют корректность работы вспомогательных утилит.

#### 3. **Параметризованные тесты и edge cases** (3 теста)
- `TestList_WithDifferentSizes_CompletesWithoutException`
- `TestList_AddOperation_ActuallyAddsElements`

**Назначение**: Проверяют работу с различными объемами данных и граничными случаями.

### Особенности реализации тестов

#### Использование Reflection
Поскольку методы в `Program` объявлены как `private` и `static`, для тестирования используется класс-обертка `ProgramWrapper`, который с помощью Reflection вызывает приватные методы:

```csharp
public class ProgramWrapper
{
    public void TestList(int elementsCount, int repeatTests)
    {
        var method = typeof(Program).GetMethod("TestList",
            BindingFlags.NonPublic | BindingFlags.Static);
        method?.Invoke(null, new object[] { elementsCount, repeatTests });
    }
    // ... другие методы
}
```

#### Тестовые параметры
- `TestElementsCount = 1000` (уменьшено для быстрого выполнения тестов)
- `RepeatTests = 2` (уменьшено для быстрого выполнения)

## Установка и запуск

### Предварительные требования
- .NET 8.0 SDK или новее
- Любая среда разработки (Visual Studio, VS Code, Rider)

### Шаги установки

1. **Создайте структуру проекта**:
```bash
mkdir ConsoleApp1
cd ConsoleApp1
```

2. **Создайте основной проект**:
```bash
dotnet new console
```

3. **Добавьте необходимые пакеты**:
```bash
dotnet add package System.Collections.Immutable
dotnet add package xunit
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package xunit.runner.visualstudio
```

4. **Разместите файлы**:
   - `Program.cs` - основной код приложения
   - `UnitTest.cs` - модульные тесты

5. **Обновите файл проекта** `ConsoleApp1.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Collections.Immutable" Version="8.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.5">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

</Project>
```

### Запуск приложения

```bash
# Запуск основного приложения
dotnet run

# Запуск модульных тестов
dotnet test

# Запуск конкретного теста
dotnet test --filter "TestList_PerformanceTestsCompleteWithoutException"
```

## Пример вывода основного приложения

```
=== Лабораторная работа: Анализ производительности коллекций ===

1. Тестирование List<int>:
  Добавление в конец (100000):
    Среднее: 0.54 мс
    Минимум: 0.48 мс
    Максимум: 0.62 мс
    Тестов: 5
  ... (другие операции)

2. Тестирование LinkedList<int>:
  ... (аналогично)

=== Тестирование завершено ===
```

## Пример вывода тестов

```
Test Run Successful.
Total tests: 12
     Passed: 12
 Total time: 1.2345 seconds
```

## Технические детали

### Архитектура тестирования

```
Основное приложение (Program.cs)
├── Публичный интерфейс:
│   └── Main() - точка входа
│
├── Приватные методы тестирования (через Reflection):
│   ├── TestList() - тесты List<int>
│   ├── TestLinkedList() - тесты LinkedList<int>
│   ├── TestQueue() - тесты Queue<int>
│   ├── TestStack() - тесты Stack<int>
│   └── TestImmutableList() - тесты ImmutableList<int>
│
├── Вспомогательные методы:
│   ├── MeasureTime() - измерение времени
│   ├── PrintResults() - вывод результатов
│   └── GetMiddleNode() - поиск среднего узла
│
└── Модульные тесты (UnitTest.cs)
    ├── ProgramWrapper - обертка для доступа к приватным методам
    ├── Тесты производительности - проверка выполнения
    ├── Тесты утилит - проверка вспомогательных методов
    └── Тесты граничных случаев
```

### Ключевые классы и методы

1. **Program.Main()** - точка входа, запускает все тесты производительности
2. **ProgramWrapper** - обеспечивает доступ к приватным методам для тестирования
3. **CollectionsPerformanceTests** - содержит все модульные тесты

## Результаты тестирования

### Производительность коллекций (основные выводы)

1. **List<T>** - оптимален для большинства сценариев, быстрый доступ по индексу
2. **LinkedList<T>** - эффективен для операций с началом/концом коллекции
3. **Queue<T>/Stack<T>** - специализированы для FIFO/LIFO операций
4. **ImmutableList<T>** - потокобезопасен, но медленнее при модификациях

### Модульное тестирование

Все 12 тестов успешно проходят, проверяя:
- ✅ Корректность выполнения методов тестирования
- ✅ Правильность работы вспомогательных утилит
- ✅ Обработку граничных случаев
- ✅ Параметризованные сценарии

## Расширение проекта

### Добавление новых тестов
1. Создайте новый метод с атрибутом `[Fact]` или `[Theory]`
2. Используйте `ProgramWrapper` для доступа к приватным методам
3. Проверяйте ожидаемое поведение с помощью утверждений Xunit

### Пример добавления теста:
```csharp
[Fact]
public void NewFeature_Test()
{
    var wrapper = new ProgramWrapper();
    var exception = Record.Exception(() => wrapper.TestList(500, 1));
    Assert.Null(exception);
}
```

## Устранение неполадок

### Проблема: "Method not found" при запуске тестов
**Решение**: Убедитесь, что:
- Имена методов в `ProgramWrapper` совпадают с именами в `Program`
- Методы в `Program` объявлены как `static`

### Проблема: Медленное выполнение тестов
**Решение**: Уменьшите `TestElementsCount` в тестах или используйте `[Fact(Skip = "Причина")]` для временного отключения

### Проблема: Отсутствуют пакеты
**Решение**: Выполните:
```bash
dotnet restore
dotnet add package <имя-пакета>
```

## Лицензия и использование

Проект создан в учебных целях. Вы можете свободно использовать, модифицировать и распространять код с указанием авторства.

