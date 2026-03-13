# TaskFlow - Smart Task Manager

TaskFlow is a modern full-stack task management application featuring a Kanban board, drag-and-drop support, real-time updates, and a Clean Architecture backend.

## Architecture

- **Backend**: ASP.NET Core 8 Web API, Vertical Slice Architecture, CQRS (MediatR), Entity Framework Core (PostgreSQL).
- **Frontend**: Vue 3 (Composition API), Vite, Pinia, TailwindCSS.
- **Real-time**: SignalR for live task updates and comments.
- **Infrastructure**: Docker & Docker Compose for easy local setup. GitHub Actions for CI pipeline.

## Features

- [x] Board with drag-and-drop columns
- [x] Task creation, editing, status tracking
- [x] User Authentication & Authorization (JWT)
- [x] Prioritization and Deadlines
- [x] Tags and Filtering
- [x] Real-time updates via SignalR
- [x] Comments & Attachments (Backend Support)
- [x] Notifications system

## Quick Start (Docker)

1. Ensure Docker Desktop is running.
2. Run `docker-compose up --build -d` from the root directory.
3. Access Frontend at `http://localhost:8080`
4. Access Backend Swagger at `http://localhost:5000/swagger`

## Local Development (Without Docker)

### Backend
1. Navigate to `TaskFlow` directory.
2. Update `appsettings.Development.json` with your PostgreSQL string.
3. Run `dotnet ef database update`
4. Run `dotnet run` 

### Frontend
1. Navigate to `TaskFlow.Web` directory.
2. Run `npm install`
3. Run `npm run dev`

## CI/CD Workflow

GitHub Actions runs on every push to main to:
1. Build and Test the .NET Backend.
2. Build the Vue Frontend.
3. Prepare Docker images for deployment to environments like Render/Railway and Vercel.

## Roadmap

- [ ] Implement team projects & collaboration
- [ ] Implement robust sub-tasks 
- [ ] Calendar view with sync
- [ ] Email reports and reminders
