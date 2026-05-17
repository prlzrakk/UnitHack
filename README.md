# UnitHack

![Platform](https://img.shields.io/badge/platform-Web-blue)
![Language](https://img.shields.io/badge/language-C%23-purple)
![Backend](https://img.shields.io/badge/backend-ASP.NET%20Core-green)
![Frontend](https://img.shields.io/badge/frontend-HTML%20%7C%20CSS%20%7C%20JavaScript-orange)
![Database](https://img.shields.io/badge/database-PostgreSQL-blue)
![Realtime](https://img.shields.io/badge/realtime-SignalR-red)
![Queue](https://img.shields.io/badge/queue-RabbitMQ-ff6600)
![Status](https://img.shields.io/badge/status-hackathon%20project-lightgrey)

UnitHack — система для управления задачами с автоматизацией и синхронизацией между пользователями и событиями.

Проект создан для кейса компании Victory Group в рамках хакатона UnitHack 2026. Платформа помогает командам вести проекты, управлять задачами на kanban-досках, назначать исполнителей, использовать теги и получать уведомления о важных изменениях.

# Contents

* [About](#about)
* [Возможности](#возможности)
* [Шаги для установки](#шаги-для-установки)
* [Tech Stack](#tech-stack)
* [API](#api)
* [Realtime и автоматизация](#realtime-и-автоматизация)
* [Архитектура](#архитектура)
* [Authors](#authors)

# About

UnitHack позволяет:

* регистрироваться и авторизовываться в системе
* создавать команды и управлять участниками
* менять роли участников внутри команды
* создавать проекты внутри команд
* создавать kanban-доски для проектов
* добавлять, переименовывать и удалять колонки
* создавать, обновлять, перемещать и удалять задачи
* назначать задачи пользователям
* добавлять дедлайны, приоритеты и описания задач
* создавать теги для kanban-доски
* прикреплять теги к задачам
* получать уведомления о создании, изменении и перемещении задач
* работать с интерфейсом через статический frontend

# Возможности

UnitHack построен вокруг командной работы с задачами:

* **Teams** — команды, участники и роли
* **Projects** — проекты, привязанные к командам
* **Kanban** — доски внутри проектов
* **Columns** — колонки kanban-досок
* **Tasks** — задачи с исполнителем, приоритетом, дедлайном и порядком
* **Tags** — теги для группировки задач
* **Notifications** — уведомления о событиях задач
* **Realtime** — доставка уведомлений через SignalR
* **Automation events** — публикация событий через outbox и RabbitMQ

# Шаги для установки

1. Склонируйте репозиторий.

```bash
git clone https://github.com/prlzrakk/UnitHack.git
cd UnitHack
```

2. Создайте `.env` на основе `.env.example`.

Пример переменных окружения:

```env
# ASP.NET
ASPNETCORE_URLS=http://+:8080
ASPNETCORE_ENVIRONMENT=Development

# JWT
JWT_SECRET=your_super_secret_key_32_chars_minimum

# Database
DB_HOST=db
DB_PORT=5432
DB_NAME=unithack
DB_USER=postgres
DB_PASSWORD=postgres

# RabbitMQ
RabbitMq__Host=rabbitmq
RabbitMq__Port=5672
RabbitMq__Username=guest
RabbitMq__Password=guest
RabbitMq__Exchange=kanban.events
```

3. Запустите проект через Docker Compose.

```bash
docker compose up -d --build
```

4. После запуска будут доступны:

* frontend/backend: `http://localhost:8080`
* nginx: `http://localhost`
* PostgreSQL: `localhost:5332`
* RabbitMQ AMQP: `localhost:5672`
* RabbitMQ Management UI: `http://localhost:15672`
* Swagger в Development-режиме: `http://localhost:8080/swagger`

Для запуска тестов:

```bash
dotnet test
```

# Tech Stack

## Frontend

* HTML
* CSS
* JavaScript
* SignalR client

Статический frontend находится в `Api/wwwroot`.

Основные страницы:

* `auth.html` — авторизация
* `index.html` — главная страница
* `teams.html` — команды
* `project.html` — проекты
* `kanban.html` — kanban-доска
* `user.html` — профиль пользователя

## Backend

* C#
* .NET
* ASP.NET Core Web API
* MediatR
* FluentValidation
* JWT authentication
* Swagger / OpenAPI
* SignalR

Backend реализован в проекте `Api`.

## Database

* PostgreSQL
* Entity Framework Core
* Npgsql

В базе данных есть сущности для:

* пользователей
* команд
* участников команд
* проектов
* kanban-досок
* колонок
* задач
* тегов
* уведомлений
* событий задач
* правил автоматизации
* outbox-событий

## Messaging and realtime

* RabbitMQ
* Outbox pattern
* Background workers
* SignalR

RabbitMQ используется для публикации и обработки событий задач. SignalR используется для realtime-уведомлений пользователей.

## DevOps

* Docker
* Docker Compose
* Nginx
* GitHub Actions

## Tests

* xUnit

# API

## Auth and users

* `POST /api/users` — регистрация пользователя
* `GET /api/users/me` — текущий пользователь
* `GET /api/users?query=&limit=` — поиск пользователей
* `POST /api/auth/sessions` — создание сессии
* `POST /api/auth/tokens/refresh` — обновление access/refresh token

## Teams

* `GET /api/teams` — список команд пользователя
* `GET /api/teams/{teamId}` — команда по id
* `POST /api/teams` — создание команды
* `POST /api/teams/{teamId}/members` — добавление участника
* `DELETE /api/teams/{teamId}/members/{userId}` — удаление участника
* `PATCH /api/teams/{teamId}/members/{userId}/role` — изменение роли участника

## Projects

* `POST /api/teams/{teamId}/projects` — создание проекта
* `GET /api/projects/{projectId}` — проект по id
* `GET /api/teams/{teamId}/projects` — проекты команды
* `DELETE /api/projects/{projectId}` — удаление проекта

## Kanban

* `POST /api/projects/{projectId}/kanbans` — создание kanban-доски
* `GET /api/projects/{projectId}/kanbans` — все kanban-доски проекта
* `GET /api/kanbans/{kanbanId}` — kanban-доска по id
* `PUT /api/kanbans/{kanbanId}` — переименование kanban-доски
* `DELETE /api/kanbans/{kanbanId}` — удаление kanban-доски

## Columns

* `POST /api/kanbans/{kanbanId}/columns` — создание колонки
* `PUT /api/columns/{columnId}` — переименование колонки
* `DELETE /api/columns/{columnId}` — удаление колонки

## Tasks

* `POST /api/kanbans/{kanbanId}/tasks` — создание задачи
* `PUT /api/tasks/{taskId}` — обновление задачи
* `PATCH /api/tasks/{taskId}` — перемещение задачи
* `DELETE /api/tasks/{taskId}` — удаление задачи

## Tags

* `GET /api/kanbans/{kanbanId}/tags` — теги kanban-доски
* `POST /api/kanbans/{kanbanId}/tags` — создание тега
* `PUT /api/tags/{tagId}` — переименование тега
* `DELETE /api/tags/{tagId}` — удаление тега
* `GET /api/tasks/{taskId}/tags` — теги задачи
* `POST /api/tasks/{taskId}/tags/{tagId}` — прикрепить тег к задаче
* `DELETE /api/tasks/{taskId}/tags/{tagId}` — открепить тег от задачи

## Notifications

* `GET /api/notifications` — все уведомления пользователя
* `GET /api/notifications/unread` — непрочитанные уведомления
* `GET /api/notifications/{notificationId}` — уведомление по id
* `PATCH /api/notifications/{notificationId}/read` — отметить уведомление прочитанным
* `PUT /api/notifications/read-all` — отметить все уведомления прочитанными

# Realtime и автоматизация

В проекте предусмотрена синхронизация событий задач и realtime-уведомления.

Как это работает:

1. Пользователь создает, обновляет или перемещает задачу.
2. Backend создает событие задачи.
3. Событие сохраняется в outbox.
4. `OutboxWorker` публикует pending-события в RabbitMQ.
5. `NotificationWorker` читает события из очереди.
6. Для пользователя создается уведомление.
7. Уведомление отправляется через SignalR.

SignalR hub:

```text
/hubs/notifications
```

RabbitMQ queue:

```text
notifications:events
```

# Архитектура

Основные части решения:

* `Api` — ASP.NET Core приложение, API-контроллеры, middleware, SignalR hub и статический frontend
* `Client.Models` — DTO, enum-ы и конфигурационные модели, общие для API и клиента
* `Core` — слой для доменной и прикладной логики
* `Infrastructure` — база данных, Entity Framework Core, репозитории, JWT/security, RabbitMQ, workers
* `Tests` — тестовый проект

Проект следует feature-based структуре: каждая крупная возможность вынесена в отдельную папку внутри `Api/Application/Features`.

Пример:

```text
Api/Application/Features/
├── Auth
├── Columns
├── Kanban
├── Notifications
├── Projects
├── Tags
├── TaskTags
├── Tasks
├── Teams
└── Users
```

# Authors

* [prlzrakk](https://github.com/prlzrakk)
* [Jlychee](https://github.com/Jlychee)
* [Kitiketov](https://github.com/Kitiketov)
* [FayTim](https://github.com/FayTim)
* [reqied](https://github.com/reqied)
