# Habit Scheduler API

A backend REST API for a constraint-based habit scheduling application.  
Built with **ASP.NET Core**, **Entity Framework Core**, and **PostgreSQL**.

This service is responsible for:
- Managing habits
- Generating weekly schedules based on constraints
- Tracking scheduled, completed, and missed habit slots

## Tech Stack

- **ASP.NET Core Web API**
- **C#**
- **Entity Framework Core**
- **PostgreSQL**
- **Swagger / OpenAPI**
- **Render** (deployment)

---

## Core Features

- CRUD operations for habits
- Constraint-based weekly scheduling
- Slot status tracking (`Scheduled`, `Completed`, `Missed`)
- Delete / update scheduled slots
- Automatic database migrations on startup
- CORS configured for Angular frontend

---

## Architecture Overview

- **Controllers**  
  Handle HTTP requests and responses

- **Services**  
  Business logic (e.g., scheduling algorithm)

- **Data Layer**
  - EF Core DbContext
  - Entity models
  - Migrations

- **DTOs**
  - Clean API contracts
  - Prevent over-fetching / circular references

---

## Local Development

### Prerequisites
- .NET 10 SDK
- PostgreSQL
- EF Core CLI

### Setup

```bash
git clone https://github.com/your-username/habit-scheduler-backend.git
cd habit-scheduler-backend
dotnet restore
```
Configure appsettings.Development.json:
```bash
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=habits;Username=postgres;Password=yourpassword"
  }
}
```
Run the API:
```bash
dotnet run
```
Swagger UI will be available at:
```bash
http://localhost:5000/swagger
```
---

## Deployed API
- Deployed on Render
- Uses a managed PostgreSQL instance

API accessible at:
```bash
https://habitscheduler.onrender.com
```

---

## API Endpoints

All endpoints are prefixed with:

```bash
/api
```
### Habits
Get all habits
```bash
GET /api/habit
```

Response
```bash
[
  {
    "id": 1,
    "name": "Workout",
    "frequencyPerWeek": 3,
    "minDurationMinutes": 30,
    "startHour": 6,
    "endHour": 22,
    "isActive": true
  }
]
```
Create a new habit
```bash
POST /api/habit
```

Request Body
```bash
{
  "name": "Read",
  "frequencyPerWeek": 5,
  "minDurationMinutes": 20,
  "startHour": 8,
  "endHour": 23
}
```

Response
```bash
201 Created
```
Update an existing habit
```bash
PUT /api/habit/{id}
```

Request Body
```bash
{
  "name": "Read",
  "frequencyPerWeek": 4,
  "minDurationMinutes": 30,
  "startHour": 9,
  "endHour": 22,
  "isActive": true
}
```

Response
```bash
204 No Content
```
### Scheduling
Generate weekly schedule

Creates schedule slots for all active habits for the current week based on constraints.
```bash
POST /api/schedule/create
```

Response
```bash
200 OK
```
Get schedule for current week

Returns all scheduled slots for the current week.
```bash
GET /api/schedule/week
```

Response
```bash
[
  {
    "id": 12,
    "habitId": 1,
    "habitName": "Workout",
    "date": "2026-02-02",
    "startTime": "18:00",
    "durationMinutes": 30,
    "status": "Scheduled"
  }
]
```
Mark a slot as missed

Marks a scheduled slot as missed and updates its status.
```bash
POST /api/schedule/{slotId}/miss
```

Response
```bash
200 OK
```
Delete a scheduled slot

Removes a scheduled slot from the calendar.
```bash
DELETE /api/schedule/{slotId}
```

Response
```bash
204 No Content
```
