# TaskFlow

Полнофункциональное Kanban‑приложение для управления задачами на стеке **ASP.NET Core 8 + React 18**.

---

## Возможности

| Функция | Описание |
|---|---|
| 🔐 **Аутентификация** | JWT + ASP.NET Identity. Регистрация, вход, сброс пароля |
| 👥 **Роли** | User и Admin. Seed-администратор создаётся при старте |
| 📋 **Проекты / Доски** | Несколько проектов на пользователя, переключение в сайдбаре |
| 🗂️ **Kanban-доска** | Колонки To Do / In Progress / Review / Done с drag-and-drop |
| ✅ **Подзадачи** | Чеклист внутри задачи с прогресс-баром |
| 🏷️ **Теги** | Цветные теги, фильтрация на доске |
| 💬 **Комментарии** | Комментарии к задачам |
| 🔔 **Уведомления** | Real-time через SignalR, панель непрочитанных |
| 🗑️ **Soft Delete** | Мягкое удаление задач; администратор может восстановить |
| 📆 **Today** | Страница «Сегодня» — просроченные и запланированные на сегодня |
| 🌓 **Тема** | Dark / Light режим |
| 📎 **Вложения** | Прикрепление файлов к задачам (до 10 МБ) |
| 🔍 **Поиск и фильтры** | Поиск по названию/описанию, фильтрация по тегам и проекту |
| 📄 **Пагинация** | На всех списковых эндпоинтах |
| 🐳 **Docker** | `docker-compose up --build` — один командой |
| 📖 **Swagger** | Документация API с авторизацией Bearer |
| 🧪 **Тесты** | 72 unit-теста (xUnit + FluentAssertions) |

---

## Технологический стек

### Backend
| | |
|---|---|
| Фреймворк | ASP.NET Core 8 (Minimal API + Controllers) |
| ORM | Entity Framework Core 8 + Npgsql (PostgreSQL) |
| Архитектура | Clean Architecture, CQRS + MediatR |
| Аутентификация | ASP.NET Identity + JWT Bearer |
| Real-time | SignalR |
| Валидация | FluentValidation |
| Логирование | Serilog (консоль + rolling file) |
| Тесты | xUnit, FluentAssertions, Moq, Testcontainers |

### Frontend
| | |
|---|---|
| Фреймворк | React 18 + TypeScript |
| Сборщик | Vite |
| Стейт-менеджмент | Zustand |
| Роутинг | React Router v6 |
| UI | Tailwind CSS + DaisyUI |
| Drag & Drop | @dnd-kit |
| HTTP | Axios |
| Real-time | @microsoft/signalr |

---

## Быстрый старт

### 🐳 Docker (рекомендуется)

```bash
# 1. Создайте .env из примера
cp .env.example .env

# 2. Заполните обязательные переменные в .env:
#    JWT_KEY=<секрет минимум 32 символа>
#    POSTGRES_PASSWORD=<пароль БД>

# 3. Запуск одной командой
docker-compose up --build -d
```

| Сервис | URL |
|---|---|
| Фронтенд | http://localhost:8080 |
| Swagger API | http://localhost:5000/swagger |
| Health check | http://localhost:5000/health |

---

### 💻 Локальный запуск

**Требования:** .NET 8 SDK, Node.js 20+, PostgreSQL 15+

**Backend**
```bash
cd TaskFlow

# Скопируйте .env в папку backend или задайте переменные окружения:
# JWT_KEY=YourSecretKey...
# CONNECTION_STRING=Host=localhost;Port=5432;Database=taskflow;Username=postgres;Password=postgres

dotnet restore
dotnet ef database update
dotnet run --urls "http://localhost:5000"
```

**Frontend** (в другом терминале)
```bash
cd TaskFlow.Web
npm install
npm run dev
```

| Сервис | URL |
|---|---|
| Фронтенд | http://localhost:5173 |
| Swagger | http://localhost:5000/swagger |

---

## Переменные окружения

| Переменная | Описание | Обязательно |
|---|---|---|
| `JWT_KEY` | Секретный ключ для подписи JWT (≥ 32 символа) | ✅ |
| `CONNECTION_STRING` | Строка подключения к PostgreSQL | ✅ |
| `ADMIN_EMAIL` | Email администратора (по умолчанию `admin@taskflow.com`) | — |
| `ADMIN_PASSWORD` | Пароль администратора (по умолчанию `Admin@1234!`) | — |
| `JWT_ISSUER` | Издатель токена (по умолчанию `TaskFlow`) | — |
| `JWT_AUDIENCE` | Аудитория токена (по умолчанию `FrontendApp`) | — |
| `JWT_EXPIRATION_MINUTES` | Срок жизни токена в минутах (по умолчанию `1440`) | — |

---

## API

Полная документация доступна в Swagger: `http://localhost:5000/swagger`

### Основные эндпоинты

```
POST   /api/auth/login               — вход
POST   /api/auth/register            — регистрация
POST   /api/auth/request-password-reset
POST   /api/auth/reset-password

GET    /api/tasks                    — список задач (фильтры, пагинация)
POST   /api/tasks                    — создать задачу
PUT    /api/tasks/{id}               — обновить
PUT    /api/tasks/{id}/move          — переместить в Kanban
DELETE /api/tasks/{id}               — мягкое удаление

GET    /api/projects                 — список проектов
POST   /api/projects                 — создать проект
PUT    /api/projects/{id}
DELETE /api/projects/{id}

GET    /api/tags
POST   /api/tags
DELETE /api/tags/{id}

GET    /api/notifications
PUT    /api/notifications/{id}/read
PUT    /api/notifications/read-all

GET    /api/admin/users              — [Admin] все пользователи
GET    /api/admin/tasks              — [Admin] все задачи (включая удалённые)
POST   /api/admin/tasks/{id}/restore — [Admin] восстановить задачу
```

---

## Запуск тестов

```bash
cd tests/TaskFlow.Api.UnitTests
dotnet test
```

72 unit-теста покрывают: Auth, Tasks (CRUD, Move, Delete/Restore), Projects, Comments, Tags, Admin.

---

## Структура проекта

```
TaskFlow/
├── Domain/              # Entities, enums
├── Application/         # DTOs, Extensions
├── Features/            # CQRS — Commands, Queries, Handlers, Endpoints
│   ├── Auth/
│   ├── Tasks/
│   ├── Projects/
│   ├── Admin/
│   ├── Comments/
│   ├── Tags/
│   └── Notifications/
├── Infrastructure/      # DbContext, Services
└── Hubs/                # SignalR hubs

TaskFlow.Web/            # React 18 frontend
├── src/
│   ├── api/             # Axios client
│   ├── stores/          # Zustand stores
│   ├── services/        # SignalR services
│   ├── pages/           # AuthPage, BoardPage, TodayPage
│   └── components/      # Layout, Task, Notification

tests/
├── TaskFlow.Api.UnitTests/       # 72 unit-теста
└── TaskFlow.Api.IntegrationTests/
```

