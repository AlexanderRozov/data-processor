# Data Processor Test Assignment

Распределённая система из трёх сервисов:

1. **data-generator** &mdash; .NET 8 worker, публикует случайные события в очередь RabbitMQ с заданным интервалом.
2. **data-processor** &mdash; ASP.NET Core API. Потребляет очередь, сохраняет события в PostgreSQL (FluentMigrator+Dapper), предоставляет CRUD и health endpoint. MediatR используется для CQRS.
3. **web-ui** можно отрыть по адресу  http://localhost:5173 &mdash; React + Vite SPA, отображающая таблицу событий и автоматически обновляющая данные с API. сделал для себя так как никогда не работал с таким только похожими чтобы себя проверить

## Запуск через Docker Compose

```bash
docker compose up --build
```

Сервисы и адреса:

| Сервис | Порт | Описание |
| --- | --- | --- |
| PostgreSQL | 5432 | `app/app`, БД `app_db` |
| RabbitMQ | 5672, UI на 15672 (`guest/guest`) | очередь `events` |
| data-generator | — | публикует события |
| data-processor | 8080 | API (`/events`, `/health`, Swagger в Dev) |
| web-ui | 5173 | SPA с таблицей событий |

`web-ui` общается с API через env `VITE_API_BASE_URL`. CORS в API настраивается переменной `FRONTEND_ORIGIN` (по умолчанию `http://localhost:5173`).

## API кратко

- `GET /events` – список с optional query `from`, `to` 
- `GET /events/{id}` – событие по id
- `POST /events` – ручное добавление `{ "value": 123 }`
- `DELETE /events/{id}` – удалить запись
- `GET /health` – health-check

## Frontend

`src/web-ui`  (React + TypeScript + Vite). Интерфейс показывает: можно отрыть по адресу 

- количество событий, статистика (min/max/avg)
- refresh контрол (по умолчанию 5 секунд)
- таблица ID/CreatedAt/Value
- ручная кнопка “Refresh now”

Сборка: `npm install && npm run build`. Локально `npm run dev` (порт 5173). Dockerfile выполняет prod build и отдаёт через nginx.

## Конфигурация

| Переменная | Сервис | Назначение |
| --- | --- | --- |
| `GENERATOR_INTERVAL_MS` | data-generator | интервал публикации |
| `RABBITMQ_*` | оба .NET сервиса | доступ к брокеру |
| `CONNECTION_STRING` | data-processor | строка подключения PostgreSQL |
| `FRONTEND_ORIGIN` | data-processor | CORS для веб-клиента |
| `VITE_API_BASE_URL` | web-ui | URL API (для docker-compose `http://data-processor:8080`) |

## Тесты

`dotnet test test-assignment.sln` – unit (MediatR handlers + Moq) и integration (WebApplicationFactory + Testcontainers для PostgreSQL/RabbitMQ).
