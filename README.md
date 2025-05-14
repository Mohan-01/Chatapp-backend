# Real-Time Chat Application – Microservices Architecture

This repository contains a **scalable, event-driven real-time chat application** designed using **clean microservices architecture**. It features JWT authentication, modular services, async communication with RabbitMQ, real-time chat via WebSockets, and a highly extensible deployment strategy using Docker and Kubernetes.

> ⚙️ This project is built for scalability, clarity, and clean separation of concerns—aligned with enterprise standards such as DDD, CQRS, and Clean Architecture.

---

## 📌 Key Features

- 🔐 **Authentication & Authorization** (JWT, RBAC)
- 🧑‍🤝‍🧑 **User Profiles & Discovery**
- 💬 **1:1 & Group Chat** with real-time support (Socket.IO)
- 📩 **Notifications** (Email, Push via Firebase)
- 🗂 **Modular Microservices** with isolated domains
- 🔄 **Event-Driven Communication** (RabbitMQ)
- 🗃 **Polyglot Persistence** (SQL Server, MongoDB, Redis, InfluxDB)
- ☁️ **Containerized & Orchestrated** via Docker & Kubernetes

---

## 🧠 Architecture Overview

This system follows a **Domain-Driven Design + Clean Architecture** model. Each service owns its database and communicates asynchronously where possible.

### 🔧 Backend Services

| Service       | Responsibility                          | Tech Stack                    |
| ------------- | --------------------------------------- | ----------------------------- |
| Auth Service  | Registration, login, token validation   | ASP.NET Core, SQL Server, JWT |
| User Service  | Profiles, friend management             | ASP.NET Core, MongoDB         |
| Chat Service  | Real-time messaging, channels, DMs      | Node.js, MongoDB, Socket.IO   |
| Notification  | Email, push notifications, preferences  | Node.js, Redis, Firebase      |
| Media Service | File uploads, image optimization        | ASP.NET Core, S3/Azure Blob   |
| WebSocket GW  | Connection handling, event broadcasting | Node.js, Redis Adapter        |
| Analytics     | BI, usage tracking, system metrics      | Python, InfluxDB, PostgreSQL  |

📄 For full architectural breakdown, see [`docs/ARCHITECTURE.md`](./docs/ARCHITECTURE.md)

---

## 📁 Folder Structure

The repo is organized using **Clean Architecture layers**:

- `API Layer`: Controllers, Middleware
- `Core Layer`: Business logic, DTOs, Interfaces
- `Infrastructure`: DB access, event producers/consumers

Each service is a self-contained project with its own API/Core/Infrastructure layers.

📄 Detailed explanation: [`docs/FOLDER_STRUCTURE.md`](./docs/FOLDER_STRUCTURE.md)

---

## 🔐 API Reference

The Authentication microservice provides endpoints for:

- User Registration & Login
- Token Validation
- Forgot Username/Password flows
- Account Updates (email, username, password)
- JWT Authentication (Bearer Token)

📄 Full API Details: [`docs/API_REFERENCE.md`](./docs/API_REFERENCE.md)

🔐 Example:

```http
Authorization: Bearer {your_token}
```

---

## 🚀 Getting Started

### ✅ Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- [Node.js 18+](https://nodejs.org/)
- [Docker](https://www.docker.com/)
- \[MongoDB, SQL Server, Redis] running locally or in containers
- Optional: RabbitMQ (for events), Kubernetes (for orchestration)

---

### 🐳 Run with Docker

```bash
# Build and run all microservices
docker-compose up --build
```

> ℹ️ Make sure RabbitMQ and MongoDB containers are started

---

### ▶️ Run Locally (Development)

#### AuthService:

```bash
cd ChatApp.AuthService/AuthService
dotnet run
```

#### UserService:

```bash
cd ChatApp.UserService/ChatApp.UserService.API
dotnet run
```

#### ChatService (Node):

```bash
cd ChatApp.ChatService/ChatApp.ChatService.API
npm install
npm run dev
```

---

## ⚙️ DevOps & Monitoring

- CI/CD: GitHub Actions (`.github/workflows`)
- Logging: Serilog, Elasticsearch, Kibana
- Metrics: Prometheus, Grafana
- Tracing: Jaeger
- Config Management: Kubernetes Secrets & ConfigMaps

---

## 📦 Technologies Used

- **Backend**: ASP.NET Core, Node.js, FastAPI
- **Frontend**: Angular 15+
- **Real-Time**: Socket.IO, WebSocket
- **Messaging**: RabbitMQ
- **Auth**: JWT, Bearer Tokens
- **Storage**: MongoDB, SQL Server, Redis, InfluxDB
- **Cloud**: Docker, Kubernetes, AWS S3 / Azure Blob

---

## 📈 Future Roadmap

- 🔊 Voice & Video Chat (WebRTC)
- 🤖 AI Suggestions and Moderation
- 📱 Native Mobile Clients (iOS, Android)
- 🔐 OAuth2 Integration (Google, GitHub)

---

## 👨‍💻 Author & Contact

**MVK Seshu M**
📧 [mmvkseshu@gmail.com](mailto:mmvkseshu@gmail.com)
🔗 [GitHub](https://github.com/mvk-seshu-m)

---

## 📄 License

Licensed under the [MIT License](https://opensource.org/licenses/MIT).

---

## 📚 Related Docs

- [📘 API Reference](./docs/API_REFERENCE.md)
- [🏗 Architecture Overview](./docs/ARCHITECTURE.md)
- [🗂 Folder Structure](./docs/FOLDER_STRUCTURE.md)

---

> _This documentation is designed to be technically robust and reviewer-friendly, aligning with software engineering interview standards at companies like Microsoft._

```

---

### ✅ Next Steps

1. Create a `/docs` folder and place:
   - `API_REFERENCE.md`
   - `ARCHITECTURE.md`
   - `FOLDER_STRUCTURE.md`

2. Place the above `README.md` at the root of your GitHub repo.

3. Preview it on GitHub to ensure all links render and the layout flows cleanly.

Let me know if you want an **editable version of this `README.md` as a file**, or further refinements for your specific target audience.
```
