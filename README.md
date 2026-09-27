# CodeHive

**CodeHive** is a social media platform designed specifically for developers. It provides a dedicated space for engineers to connect, publish technical blogs, discuss code, and exchange thoughts. 

This project was built from the ground up to demonstrate modern backend engineering practices, focusing on scalability, observability, and robust software architecture. 

##  Key Features
- **Developer-Centric Social Feed**: Share text posts, technical blogs, and snippets.
- **Real-Time Chat & Notifications**: Instant messaging and live in-app notifications powered by SignalR.
- **Connections**: Follow other developers and curate your feed.
- **Search**: Quickly find relevant posts and users.

## 🛠 Tech Stack & Architecture
The backend is structured as a **Modular Monolith** applying **Clean Architecture** and **Domain-Driven Design (DDD)** principles to keep boundaries strict and the codebase highly maintainable.

**Core Frameworks:**
- **.NET 10 (C#)**: Core application framework.
- **Entity Framework Core**: ORM for database access.
- **PostgreSQL**: Primary relational database.

**Patterns & Practices:**
- **CQRS**: Separating reads and writes using **MediatR**.
- **Event-Driven Architecture**: Decoupling modules using domain/integration events.
- **Outbox Pattern**: Ensuring guaranteed message delivery to message brokers.

**Infrastructure & Services:**
- **RabbitMQ & MassTransit**: Asynchronous message brokering between modules.
- **Redis**: High-performance caching layer.
- **SignalR**: WebSockets for real-time communication.
- **Authentication**: JWT Bearer Tokens with OAuth integration (Google & GitHub).
- **Docker**: Containerized infrastructure services managed via Docker Compose.

**Full Observability Stack:**
- **Seq & Serilog**: Centralized structured logging.
- **OpenTelemetry**: Distributed tracing across RabbitMQ and HTTP boundaries.
- **Prometheus & Grafana**: Time-series metrics collection and visual performance dashboards.

##  Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 1. Clone the repository
```bash
git clone https://github.com/yourusername/CodeHive.git
cd CodeHive
```

### 2. Start the Infrastructure Services
Run the following command to spin up the required databases, message brokers, and observability tools (Redis, RabbitMQ, Seq, Prometheus, Grafana):

```bash
docker compose up -d
```

### 3. Setup Configuration
Update `src/CodeHive.Api/appsettings.Development.json` with your database connection strings and OAuth credentials if necessary. Local defaults are mostly provided.

*(Note: If you run your Postgres database locally outside of docker, ensure it is running on port 5432).*

### 4. Run the API
Navigate to the API project and run the application:
```bash
cd src/CodeHive.Api
dotnet run
```

### 5. Explore
- **API Documentation (Scalar / Swagger):** Available automatically when running the API locally.
- **Seq (Logs & Traces):** `http://localhost:5341`
- **Prometheus (Metrics Data):** `http://localhost:9090`
- **Grafana (Dashboards):** `http://localhost:3000` (Login: `admin` / `admin`)
