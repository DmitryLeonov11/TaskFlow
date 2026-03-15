## TaskFlow

Простое Kanban‑приложение для управления задачами (аналог Todoist + Trello) на стеке ASP.NET Core + Vue 3.

### Стек

- **Backend**: ASP.NET Core 8, EF Core (PostgreSQL), CQRS + MediatR, ASP.NET Identity + JWT, SignalR.
- **Frontend**: Vue 3 (Composition API), Vite, Pinia, TailwindCSS, daisyUI.
- **Инфраструктура**: Docker + docker‑compose, GitHub Actions.

### Быстрый старт

1. Склонируйте репозиторий.
2. Создайте файл `.env` в корне проекта (скопируйте `.env.example`):
   ```bash
   cp .env.example .env
   ```
3. Настройте переменные в `.env`:
   - `POSTGRES_USER`, `POSTGRES_PASSWORD` — учётные данные PostgreSQL
   - `CONNECTION_STRING` — строка подключения к БД
   - `JWT_KEY` — секретный ключ для JWT (минимум 32 символа)

### Запуск с Docker

```bash
docker-compose up --build -d
```

- Фронтенд: `http://localhost:8080`
- Swagger API: `http://localhost:5000/swagger`

### Локальный запуск

**Backend**
```bash
cd TaskFlow
dotnet restore
dotnet ef database update
dotnet run --urls "http://localhost:5000"
```

**Frontend** (в новом терминале)
```bash
cd TaskFlow.Web
npm install
npm run dev
```

- Фронтенд: `http://localhost:5173`
- Swagger API: `http://localhost:5000/swagger`
- Health check: `http://localhost:5000/health`

