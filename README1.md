# ChatApp Backend

This repository contains the full backend codebase of a **real-time chat application** built using a microservices architecture, SignalR for WebSocket communication, and RabbitMQ for event-driven messaging.

## 🧩 Architecture Overview

The system is composed of independent services communicating over HTTP and event buses, designed for scalability, maintainability, and flexibility:

### 🧱 Microservices

- **AuthService**

  - User registration, login, JWT-based authentication
  - Google Sign-In (OAuth2)
  - Refresh token support
  - Password recovery & email service integration

- **UserService**

  - User profile management (bio, skills, contact links, status)
  - Admin panel capabilities
  - Username availability and update

- **ChatService**
  - Chat creation and message management
  - Real-time messaging via SignalR
  - WebSocket gateway for persistent communication

### 📡 Communication

- **SignalR (WebSocket):** Enables real-time bidirectional messaging between clients and ChatService.
- **RabbitMQ (Event Bus):** Powers asynchronous communication between services (e.g., Auth → UserService on user registration).

### 🗂️ Shared Library

A shared `.NET class library` is used across services for common DTOs, events, and utility classes to avoid redundancy and improve code consistency.

## 🛠️ Tech Stack

- **.NET 8** (C#)
- **MongoDB** (Database)
- **SignalR** (WebSocket layer)
- **RabbitMQ** (Messaging broker)
- **Serilog** (Structured logging)
- **Swagger** (API documentation)

## 📁 Project Structure

ChatApp/
│
├── ChatApp.AuthService/
│ ├── Core/
│ ├── Infrastructure/
│ └── AuthService/
│
├── ChatApp.UserService/
│ ├── Core/
│ ├── Infrastructure/
│ └── UserService/
│
├── ChatApp.ChatService/
│ ├── Core/
│ ├── Infrastructure/
│ └── ChatService/
│
├── Shared/
│ └── ChatApp.Shared/
│
└── docker-compose.yml

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- MongoDB instance
- RabbitMQ broker
- Node.js (for frontend if used)
- Docker (optional)

### Run Services

Each microservice can be run independently using:

```bash
dotnet run --project ChatApp.AuthService/AuthService
dotnet run --project ChatApp.UserService/UserService
dotnet run --project ChatApp.ChatService/ChatService
```
