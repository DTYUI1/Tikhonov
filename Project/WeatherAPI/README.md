# Учебный проект: CRUD‑сервис «Weather API» на ASP.NET Core (.NET 8)

## 📋 Описание проекта

Проект представляет собой полнофункциональный CRUD‑сервис для управления данными о погоде.  
API позволяет:

- управлять списком городов и типов погоды;
- сохранять и просматривать погодные записи (температура, влажность, ветер и т.д.);
- получать статистику по погоде и усреднённые значения (Dapper);
- использовать ролевую авторизацию (Admin / Manager / User) и API‑ключи;
- работать с кэшем (Redis), метриками (Prometheus), health‑checks;
- обеспечивать идемпотентность POST‑запросов и rate limiting;
- запускать интеграционный сценарий `full-test.sh` для проверки основных требований.

Архитектура проекта слоистая: **Controllers → Services → Repositories → PostgreSQL**, с использованием **EF Core**, **Dapper**, **Redis**, **Liquibase** и **ASP.NET Core middleware**.

---

## 🎯 Цели работы

1. Реализовать RESTful CRUD‑сервис с разделением слоёв:
   - Controllers (только HTTP/DTO‑логика)
   - Services (бизнес‑логика)
   - Repositories (доступ к данным)
2. Использовать PostgreSQL + миграции Liquibase для схемы БД и начальных данных.
3. Реализовать CRUD через EF Core и статистику/агрегации через Dapper.
4. Обеспечить:
   - JWT‑аутентификацию и ролевую авторизацию;
   - авторизацию по API‑ключу;
   - кэширование GET‑запросов в Redis и инвалидизацию кэша;
   - централизованную обработку ошибок и логирование;
   - Prometheus‑метрики и Health Checks;
   - пагинацию и фильтрацию на чтение;
   - unit‑тесты для репозиториев.
5. Добавить bonus‑функциональность:
   - rate limiting;
   - идемпотентность POST‑запросов через заголовок `Idempotency-Key`.

---

## 🏗️ Структура решения

```text
WeatherAPI.sln
├── docker-compose.yml          # Поднятие API, PostgreSQL, Redis, Liquibase
├── liquibase/                  # Миграции БД
│   └── changelog/...
├── full-test.sh                # Полный интеграционный тест API
├── run.sh, test-admin.sh, test-api.sh
├── WeatherAPI.API/             # Основной Web API проект
│   ├── Program.cs
│   ├── appsettings*.json
│   ├── Controllers/            # Контроллеры REST API
│   ├── Services/               # Бизнес-логика
│   ├── Repositories/           # Доступ к БД (EF Core + Dapper)
│   ├── Models/
│   │   ├── Entities/           # Доменные сущности
│   │   └── DTO/                # DTO для запросов/ответов
│   ├── Middleware/             # Глобальные middleware
│   ├── Validators/             # FluentValidation-валидаторы
│   ├── Data/ApplicationDbContext.cs
│   └── Auth/ApiKeyAuthenticationHandler.cs
└── WeatherAPI.Tests/           # Unit-тесты репозиториев (xUnit)
    └── Repositories/*.cs
```

---

## 🧩 Модель данных

### Базовый класс

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### 1. City — город

```csharp
public class City : BaseEntity
{
    public string Name { get; set; }
    public string Country { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? TimeZone { get; set; }

    public ICollection<WeatherRecord> WeatherRecords { get; set; }
    public ICollection<CityWeatherType> CityWeatherTypes { get; set; }
}
```

**Особенности:**

- Уникальный индекс по `(Name, Country)`.
- Навигация к погодным записям и many‑to‑many связи с типами погоды.

### 2. WeatherType — тип погоды

```csharp
public class WeatherType : BaseEntity
{
    public string Name { get; set; }        // Sunny, Rainy, etc.
    public string Description { get; set; }
    public string IconCode { get; set; }    // Код иконки погоды

    public ICollection<WeatherRecord> WeatherRecords { get; set; }
    public ICollection<CityWeatherType> CityWeatherTypes { get; set; }
}
```

- Уникальный индекс по `Name`.

### 3. WeatherRecord — погодная запись

```csharp
public class WeatherRecord : BaseEntity
{
    public Guid CityId { get; set; }
    public Guid WeatherTypeId { get; set; }
    public DateTime RecordedAt { get; set; }
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public int? WindDirection { get; set; }
    public int? Pressure { get; set; }
    public int? Visibility { get; set; }

    public City City { get; set; }
    public WeatherType WeatherType { get; set; }
}
```

- Индекс по `(CityId, RecordedAt)` для быстрых выборок.
- Связи:
  - `City` — `WeatherRecord` (1 → many, каскадное удаление).
  - `WeatherType` — `WeatherRecord` (1 → many, `OnDelete: Restrict`).

### 4. CityWeatherType — many‑to‑many (город ↔ тип погоды)

```csharp
public class CityWeatherType
{
    public Guid CityId { get; set; }
    public Guid WeatherTypeId { get; set; }
    public int Frequency { get; set; }      // Частота появления, %
    public string? Season { get; set; }     // Winter, Spring, Summer, Autumn, All

    public City City { get; set; }
    public WeatherType WeatherType { get; set; }
}
```

- Композитный ключ `(CityId, WeatherTypeId)`.
- Обе связи сконфигурированы через Fluent API.

### 5. User — пользователь

```csharp
public class User : BaseEntity
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; } = "User"; // Admin, Manager, User
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
}
```

- Уникальный индекс по `Email`.
- Используется для JWT-аутентификации и ролевого доступа.

### 6. ApiKey — API‑ключ

```csharp
public class ApiKey : BaseEntity
{
    public string Key { get; set; }
    public string Name { get; set; }
    public Guid? UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Permissions { get; set; } // JSON c набором прав

    public User? User { get; set; }
}
```

- Уникальный индекс по `Key`.
- Может быть привязан к конкретному пользователю или быть «техническим».

### 7. IdempotencyKey — идемпотентность POST

```csharp
public class IdempotencyKey
{
    public string Key { get; set; }
    public string RequestPath { get; set; }
    public string ResponseBody { get; set; }
    public int StatusCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

- Prima ry key по `Key`.
- Используется в middleware для повторного воспроизведения ответа при повторных POST с тем же `Idempotency-Key`.

---

## 🔌 Архитектура и слои

### Контроллеры (API-слой)

Все контроллеры наследуются от `BaseController`, где реализованы вспомогательные методы:

```csharp
protected Guid GetUserId();
protected string GetUserRole();
protected string GetUserEmail();
```

**Основные контроллеры:**

- `AuthController`
  - `POST /api/auth/register` — регистрация пользователя.
  - `POST /api/auth/login` — вход и получение JWT.
  - `GET /api/auth/me` — данные текущего пользователя.

- `CitiesController`
  - `GET /api/cities` — список городов с пагинацией и фильтрацией.
  - `GET /api/cities/{id}` — детальная информация + связанные типы погоды.
  - `POST /api/cities` — создание города (Admin, Manager).
  - `PUT /api/cities/{id}` — обновление (Admin, Manager).
  - `DELETE /api/cities/{id}` — удаление (Admin).

- `WeatherRecordsController`
  - `GET /api/weatherrecords` — список записей с пагинацией и фильтрацией.
  - `GET /api/weatherrecords/{id}` — получение записи.
  - `GET /api/weatherrecords/current/{cityId}` — «текущая» погода для города.
  - `GET /api/weatherrecords/statistics/{cityId}` — агрегированная статистика (Dapper).
  - `GET /api/weatherrecords/daily-averages/{cityId}` — средние значения по дням (Dapper).
  - `POST /api/weatherrecords` — создать запись (Admin, Manager).
  - `PUT /api/weatherrecords/{id}` — обновить (Admin, Manager).
  - `DELETE /api/weatherrecords/{id}` — удалить (Admin).

- `WeatherTypesController`
  - CRUD для типов погоды; чтение — анонимно, запись — по ролям.

- `CityWeatherTypesController`
  - Операции many‑to‑many: связь город ↔ тип погоды.

- `ApiKeysController`
  - Управление API‑ключами (только Admin).

**Принцип:** контроллеры тонкие, вся бизнес‑логика вынесена в сервисы.

---

### Сервисный слой (Services)

Интерфейсы в `Services/Interfaces`:

- `IAuthService`
- `ICityService`
- `IWeatherTypeService`
- `IWeatherRecordService`
- `ICityWeatherTypeService`
- `IApiKeyService`
- `ICacheService`

Основные задачи сервисов:

- Инкапсулировать бизнес‑правила (например, ограничения по ролям, проверка уникальности, доменные проверки).
- Работать с репозиториями и кэшем.
- Маппить сущности в DTO и обратно.
- Организовывать транзакционность на уровне EF Core, где это необходимо.

Пример типичной логики сервиса города:

- проверка, что города с такой парой `(Name, Country)` ещё нет;
- создание сущности и сохранение через `ICityRepository`;
- формирование `CityResponseDto`;
- при обновлении/удалении — инвалидировать кэш соответствующих GET‑запросов.

---

### Репозитории (Data Access Layer)

Общий интерфейс:

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, ...);
    Task<IEnumerable<T>> GetAllAsync(...);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, ...);
    Task<T> AddAsync(T entity, ...);
    Task<T> UpdateAsync(T entity, ...);
    Task DeleteAsync(T entity, ...);
    Task<bool> ExistsAsync(Guid id, ...);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, ...);
}
```

Реализация `Repository<T>` использует **EF Core** с `ApplicationDbContext` и полностью асинхронна.

Специализированные репозитории:

- `CityRepository` (`ICityRepository`)
  - пагинация и фильтрация (`GetPagedAsync(CityFilterQuery)`),
  - поиск по имени и стране,
  - загрузка города с типами погоды (`GetWithWeatherTypesAsync`).

- `WeatherRecordRepository` (`IWeatherRecordRepository`)
  - сложная фильтрация по городу, типу, датам, температуре,
  - получение последней записи по городу,
  - выборка всех записей по городу и периоду.

- `WeatherTypeRepository` (`IWeatherTypeRepository`)
  - поиск по имени.

- `CityWeatherTypeRepository`
  - работа с композитным ключом `CityId + WeatherTypeId`.

- `UserRepository` (`IUserRepository`)
  - поиск пользователя по email, проверка существования.

- `ApiKeyRepository` (`IApiKeyRepository`)
  - поиск по ключу, получение только активных и не истёкших ключей.

#### Dapper‑репозиторий статистики

```csharp
public interface IWeatherStatisticsRepository
{
    Task<WeatherStatisticsDto> GetCityStatisticsAsync(Guid cityId, DateTime from, DateTime to, ...);
    Task<IEnumerable<DailyAverageDto>> GetDailyAveragesAsync(Guid cityId, DateTime from, DateTime to, ...);
}
```

Реализация `WeatherStatisticsRepository`:

- Использует `NpgsqlConnection` и **Dapper**.
- Выполняет SQL‑запросы с CTE (`WITH`) для:
  - агрегированных статистик по температуре, влажности, ветру;
  - наиболее частого типа погоды;
  - усреднённых значений по датам.
- Все операции обёрнуты в транзакцию, при ошибке выполняется rollback, логируются ошибки через `ILogger`.

---

### Middleware

В `Program.cs` конвейер запросов:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpMetrics();                  // Prometheus
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();
```

1. **ExceptionHandlingMiddleware**
   - Глобальный `try/catch` вокруг всего пайплайна.
   - Преобразует исключения в единый формат `ErrorResponse`:
     ```csharp
     public record ErrorResponse(
         string Error,
         string Message,
         string TraceId,
         Dictionary<string, string[]>? ValidationErrors = null
     );
     ```
   - Возвращает корректные HTTP‑коды (400, 401, 403, 404, 500).

2. **RequestLoggingMiddleware**
   - Логирует входящие запросы и ответы (метод, путь, статус, время обработки).
   - Логи структурированы и читаемы через встроенный `ILogger`.

3. **RateLimitingMiddleware**
   - Ограничивает количество запросов (например, по IP или по ключу) за интервал.
   - При превышении лимита возвращает соответствующий HTTP‑код (обычно `429 Too Many Requests`).

4. **IdempotencyMiddleware**
   - Читает заголовок `Idempotency-Key` для POST‑запросов.
   - Если ключ новый:
     - пропускает запрос дальше;
     - сохраняет в таблицу `idempotency_keys` пару (Key, RequestPath, ResponseBody, StatusCode, ExpiresAt).
   - Если ключ уже есть:
     - вместо повторной обработки возвращает сохранённый ответ.

---

## 🗄️ Работа с базой данных

### PostgreSQL + EF Core

В `Program.cs`:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Конфигурация сущностей выполнена через Fluent API в `ApplicationDbContext`, все таблицы явно именованы (`cities`, `weather_types`, `weather_records`, `city_weather_types`, `users`, `api_keys`, `idempotency_keys`).

### Liquibase миграции

Папка `liquibase/changelog`:

- `001-create-users-table.xml`
- `002-create-api-keys-table.xml`
- `003-create-cities-table.xml`
- ...
- `008-seed-initial-data.xml` — начальные данные.

Через сервис `liquibase` в `docker-compose.yml` миграции автоматически применяются к PostgreSQL при запуске:

```yaml
liquibase:
  image: liquibase/liquibase:4.24
  depends_on:
    postgres:
      condition: service_healthy
  volumes:
    - ./liquibase:/liquibase/changelog
  command: >
    --url=jdbc:postgresql://postgres:5432/weatherdb
    --username=postgres
    --password=postgres
    --changeLogFile=changelog/changelog-master.xml
    update
```

Seed‑данные включают, в частности:

- базовые роли/пользователей:
  - `admin@weather.api / Admin123!`
  - `manager@weather.api / Manager123!`
  - `user@weather.api / User123!`
- пример города (Moscow) и типов погоды.

---

## ⚙️ Кэширование (Redis)

В `Program.cs`:

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "WeatherAPI_";
});
```

Сервис `ICacheService` инкапсулирует работу с Redis:

- Кэшируются, в первую очередь, **GET‑запросы**:
  - Список городов / город по ID.
  - Список типов погоды.
  - Текущая погода/статистика по городу.
- При изменении данных (Create/Update/Delete в сервисах):
  - соответствующие ключи в Redis инвалидируются (удаляются),
  - чтобы следующая выборка вернула свежие данные.

---

## 🔐 Авторизация и безопасность

### JWT Bearer

Конфигурация:

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw ...))
    };
});
```

`AuthController` реализует:

- `POST /api/auth/register` — создание пользователя и выдача JWT.
- `POST /api/auth/login` — проверка пароля, выдача JWT.
- `GET /api/auth/me` — получение информации о текущем пользователе (по JWT).

### Роли и матрица доступа

Роли: **Admin**, **Manager**, **User**.

Примеры политик:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("AllUsers", policy => policy.RequireRole("Admin", "Manager", "User"));
});
```

Фактическая матрица по контроллерам:

- Анонимный доступ:
  - Логин/регистрация (`/api/auth/*`),
  - Чтение городов и типов погоды (`GET /api/cities`, `/api/weathertypes`, и т.д.),
  - Чтение погодных записей и статистики,
  - `/health`, `/metrics`, `/swagger`.

- **User**:
  - может аутентифицироваться и читать все публичные данные;
  - не может создавать/обновлять/удалять сущности.

- **Manager**:
  - `POST`/`PUT` для городов, типов погоды, погодных записей и связей город–тип погоды;
  - `DELETE` — только у Admin.

- **Admin**:
  - полный доступ ко всем CRUD‑операциям;
  - управление API‑ключами (`ApiKeysController`).

Пример:

```csharp
[HttpPost]
[Authorize(Roles = "Admin,Manager")]
public Task<ActionResult<CityResponseDto>> Create(...)

[HttpDelete("{id:guid}")]
[Authorize(Roles = "Admin")]
public Task<IActionResult> Delete(...)
```

### API Key

Добавлен отдельный схематический тип аутентификации:

```csharp
.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);
```

В `Swagger` описан как схема `ApiKey` с заголовком `X-API-KEY`.

`ApiKeyAuthenticationHandler`:

- читает заголовок `X-API-KEY`,
- через `IApiKeyRepository` проверяет:
  - наличие ключа,
  - активность (`IsActive`),
  - срок действия (`ExpiresAt > now`),
- при успешной проверке создаёт `ClaimsPrincipal` с привязкой к пользователю (если `UserId` задан).

### Rate Limiting

`RateLimitingMiddleware` ограничивает количество запросов за промежуток времени (например, на IP или API‑ключ).  
В `full-test.sh` видно, что ограничение активно, но достаточно лояльное — все 10 запросов к `/api/cities` проходят успешно.

### Идемпотентность POST

`IdempotencyMiddleware` использует таблицу `idempotency_keys`:

- Для POST‑запросов с заголовком:
  ```http
  Idempotency-Key: some-unique-key
  ```
- Первый запрос обрабатывается нормально, результат сохраняется.
- Повторный запрос с тем же ключом возвращает тот же HTTP‑код и тело ответа без повторного выполнения бизнес‑логики.

---

## 📊 Метрики, Health Checks и логирование

### Prometheus Metrics

Включена интеграция `prometheus-net`:

```csharp
app.UseHttpMetrics();
app.MapMetrics(); // /metrics
```

Собираются стандартные метрики по HTTP‑запросам:

- количество запросов,
- распределение по статус‑кодам и методам,
- время обработки запросов.

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "")
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "");
```

Маршрут `/health`:

- проверяет доступность самого API;
- подключение к PostgreSQL;
- подключение к Redis.

### Логирование

`appsettings*.json` настраивают уровни логирования:

- `Default` — Information/Debug,
- `Microsoft.AspNetCore` и `Microsoft.EntityFrameworkCore` — Warning/Information.

Дополнительно `RequestLoggingMiddleware` логирует каждый запрос/ответ.

---

## 🧪 Unit‑тесты

Проект `WeatherAPI.Tests` (xUnit):

- Тесты для репозиториев:
  - `CityRepositoryTests`
  - `WeatherRecordRepositoryTests`
  - `CityWeatherTypeRepositoryTests`
  - `UserRepositoryTests`

Тесты проверяют:

- корретность CRUD‑операций;
- фильтрацию и пагинацию (для городов/погодных записей);
- поиск по email, уникальность и т.п.

В качестве БД для тестов обычно используется либо InMemory provider EF Core, либо отдельный тестовый контекст (см. `WeatherAPI.Tests.csproj` и конфигурацию).

---

## 🛠️ Технические особенности

- .NET 8, минимальный хостинг (`Program.cs` без `Startup`).
- EF Core + Npgsql для PostgreSQL.
- Dapper + Npgsql для агрегированных запросов.
- Асинхронность повсеместно (`async/await`).
- FluentValidation для валидации DTO.
- Свои middleware для ошибок, логирования, rate limiting, идемпотентности.
- Redis как распределённый кэш.
- Swagger + Swashbuckle с описанием всех эндпоинтов и HTTP‑кодов.
- Prometheus для метрик, Health Checks для мониторинга.

---
## 🚀 Запуск проекта

### Требования

- Docker и Docker Compose
- (опционально) .NET 8 SDK, если хотите запускать API локально без контейнера
- (опционально) `jq` для удобного отображения JSON в скрипте `run.sh`

### Быстрый запуск одним скриптом

В корне решения выполните:

```bash
chmod +x run.sh
./run.sh
```

Скрипт `run.sh` автоматически выполнит:
- Остановку предыдущих контейнеров
- Сборку и запуск всех сервисов в фоновом режиме
- Проверку health-эндпоинта после запуска

### Вариант 1: Всё в Docker (ручной запуск)

В корне решения:

```bash
docker compose up --build
```

Это запустит:

- контейнер `postgres` (PostgreSQL 15)
- контейнер `redis` (Redis 7)
- контейнер `liquibase` — применит миграции и seed-данные
- контейнер `api` — соберёт и запустит `WeatherAPI.API` на порту `8080`, проброшенном наружу как `http://localhost:5000`

Доступные URL:

- **Swagger UI**: `http://localhost:5000/swagger`
- **Health Check**: `http://localhost:5000/health`
- **Prometheus metrics**: `http://localhost:5000/metrics`

### Вариант 2: БД в Docker, API локально

1. Запустить только инфраструктуру:

   ```bash
   docker compose up -d postgres redis liquibase
   ```

2. Запустить API локально:

   ```bash
   cd WeatherAPI.API
   dotnet run
   ```

   По умолчанию API будет доступен по `https://localhost:5001` и/или `http://localhost:5000` (зависит от `launchSettings.json`).

### Полное тестирование API

После запуска проекта можно выполнить комплексное тестирование:

```bash
chmod +x full-test.sh
./full-test.sh
```

Скрипт `full-test.sh` проверит:

✅ Health Check и метрики Prometheus  
✅ Аутентификацию для всех ролей (Admin, Manager, User)  
✅ CRUD операции для городов  
✅ Контроль доступа на основе ролей (RBAC)  
✅ Работу с типами погоды и погодными записями  
✅ Связи многие-ко-многим  
✅ Пагинацию и фильтрацию  
✅ Статистику через Dapper  
✅ Rate limiting  
✅ Обработку ошибок  

---

### Тестовые учетные данные

После запуска проекта доступны три предустановленные роли:

| Роль | Email | Пароль | Права доступа |
|------|-------|--------|---------------|
| **Admin** | `admin@weather.api` | `Admin123!` | Полный доступ (CRUD) |
| **Manager** | `manager@weather.api` | `Manager123!` | Чтение, создание, редактирование |
| **User** | `user@weather.api` | `User123!` | Только чтение |

### Матрица прав доступа (RBAC)

| Операция | Admin | Manager | User |
|----------|-------|---------|------|
| **Чтение** | ✅ | ✅ | ✅ |
| **Создание** | ✅ | ✅ | ❌ |
| **Редактирование** | ✅ | ✅ | ❌ |
| **Удаление** | ✅ | ❌ | ❌ |

---

### Остановка проекта

```bash
# Остановить контейнеры
docker compose down

# Остановить и удалить тома (данные БД)
docker compose down -v
```

## 🔑 Тестовые учётные данные

Seed‑данные создают трёх пользователей с ролями:

- Admin:
  - email: `admin@weather.api`
  - password: `Admin123!`
- Manager:
  - email: `manager@weather.api`
  - password: `Manager123!`
- User:
  - email: `user@weather.api`
  - password: `User123!`

Ими пользуется скрипт `full-test.sh`.

---

## 📚 Примечания

- Проект рассчитан на учебные и демо‑цели, но использует практики, близкие к продакшен‑уровню:
  - слоистая архитектура;
  - централизованная обработка ошибок и логирование;
  - кэширование и метрики;
  - разделение ответственностей между слоями.
- Все сущности и DTO используют `Guid` как идентификатор.
- Миграции и сиды полностью управляются через Liquibase, EF Core отвечает только за runtime‑модель и доступ к данным.


## 📡 Использование API

### 1. Аутентификация JWT

1. Получить токен:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@weather.api",
  "password": "Admin123!"
}
```

Ответ:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-01-08T17:00:00Z",
  "user": {
    "id": "20eebc99-...",
    "email": "admin@weather.api",
    "firstName": "Admin",
    "lastName": "User",
    "role": "Admin",
    "isActive": true,
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

2. Использовать токен в последующих запросах:

```http
GET /api/cities
Authorization: Bearer <JWT-токен>
```

### 2. Аутентификация по API Key

1. Админ создаёт ключ:

```http
POST /api/apikeys
Authorization: Bearer <Admin JWT>
Content-Type: application/json

{
  "name": "My Client",
  "userId": "20eebc99-...",
  "expiresInDays": 30,
  "permissions": [ "read:cities", "read:weather" ]
}
```

2. Клиент использует ключ:

```http
GET /api/cities
X-API-KEY: <значение ключа>
```

### 3. Пример CRUD по городам

- Создать город (Admin/Manager):

```http
POST /api/cities
Authorization: Bearer <Admin или Manager JWT>
Content-Type: application/json

{
  "name": "TestCity",
  "country": "TestCountry",
  "latitude": 50.0,
  "longitude": 10.0,
  "timeZone": "Europe/Moscow"
}
```

- Получить список с пагинацией и фильтрацией:

```http
GET /api/cities?page=1&pageSize=10&search=Mos&country=Russia
```

- Обновить город:

```http
PUT /api/cities/{id}
Authorization: Bearer <Admin или Manager JWT>
Content-Type: application/json

{
  "name": "TestCityUpdated",
  "country": "TestCountry",
  "latitude": 51.0,
  "longitude": 11.0,
  "timeZone": "Europe/Moscow"
}
```

- Удалить город (только Admin):

```http
DELETE /api/cities/{id}
Authorization: Bearer <Admin JWT>
```

### 4. Пример получения статистики (Dapper)

```http
GET /api/weatherrecords/statistics/{cityId}?from=2026-01-01&to=2026-12-31
```

Ответ (пример):

```json
{
  "cityId": "20eebc99-...",
  "cityName": "Moscow",
  "avgTemperature": 12.3,
  "minTemperature": -15.0,
  "maxTemperature": 32.5,
  "avgHumidity": 65.0,
  "avgWindSpeed": 3.5,
  "recordCount": 365,
  "mostCommonWeatherType": "Cloudy"
}
```

---

## 🔁 Идемпотентность POST-запросов

Для избежания дубликатов при повторной отправке одного и того же POST (например, при сетевых сбоях):

1. Клиент генерирует уникальный ключ (UUID, hash и т.п.).
2. Передаёт его в заголовке `Idempotency-Key`.

Пример:

```http
POST /api/cities
Authorization: Bearer <Admin JWT>
Idempotency-Key: 123e4567-e89b-12d3-a456-426614174000
Content-Type: application/json

{
  "name": "IdempTest",
  "country": "Test",
  "latitude": 0,
  "longitude": 0
}
```

- Первый запрос создаёт город, ответ и статус код сохраняются в `idempotency_keys`.
- Повторный запрос с тем же `Idempotency-Key` вернёт тот же ответ и HTTP‑код, без повторного создания сущности.

---

## ⏱️ Rate Limiting

`RateLimitingMiddleware` ограничивает частоту запросов к API:

- При нормальной нагрузке все запросы проходят.
- При превышении лимита клиент получает ошибку (обычно `429 Too Many Requests`).
- В интеграционном тесте отправляется 10 запросов к `/api/cities` подряд, что демонстрирует работу лимитера.

---

## 🧪 Запуск тестов

### Unit‑тесты репозиториев

В директории решения:

```bash
dotnet test
```

Покрытие:

- `CityRepositoryTests` — CRUD, фильтрация и пагинация городов.
- `WeatherRecordRepositoryTests` — выборки по городу, периоду, фильтры по температуре и т.п.
- `CityWeatherTypeRepositoryTests` — операции many‑to‑many.
- `UserRepositoryTests` — поиск по email, проверка уникальности.

### Интеграционный сценарий `full-test.sh`

После запуска `docker compose up`:

```bash
./full-test.sh
```

Сценарий:

- логин всех ролей;
- CRUD операций с проверкой прав;
- статистика (Dapper);
- идемпотентность;
- rate limiting;
- проверка формата ошибок;
- финальная проверка метрик Prometheus.

---

## ✅ Итоги

В рамках работы реализован учебный Weather API, соответствующий большинству промышленных требований к CRUD‑сервисам:

- Чистая архитектура с разделением на контроллеры, сервисы и репозитории.
- EF Core + PostgreSQL для CRUD, Dapper для тяжёлых агрегирующих запросов.
- JWT и API‑ключи, ролевой доступ и матрица авторизации.
- Кэширование в Redis с инвалидизацией.
- Prometheus‑метрики, health‑checks, централизованное логирование и обработка ошибок.
- Пагинация и фильтрация.
- Unit‑тесты для репозиториев.
- Бонус: rate limiting

Проект может использоваться как основа для более сложных микросервисов и как учебный пример построения API‑сервиса 