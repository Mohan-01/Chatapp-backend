# Real-Time Chat Application Architecture

This document outlines the architecture of our Event-Driven Microservices Real-Time Chat Application built with Angular frontend and various backend services.

## System Overview

The Real-Time Chat Application is designed using a microservices architecture pattern with event-driven communication. The system consists of:

1. **Angular Web Client** - The primary user interface
2. **Multiple Specialized Microservices** - Each handling specific business functionality
3. **WebSocket Infrastructure** - For real-time communication
4. **Message Broker** - For asynchronous inter-service communication
5. **Persistence Layer** - Various databases optimized for different services

![Architecture Diagram](architecture-diagram.png)

## Frontend Architecture

### Angular Web Client

- **Technology**: Angular 15+
- **Responsibilities**:
  - User interface rendering
  - Client-side state management
  - Real-time communication via WebSockets
  - HTTP requests to backend services
- **Key Features**:
  - Responsive design for multiple device types
  - Lazy loading of modules
  - Client-side routing
  - JWT authentication
  - Real-time notifications and chat
  - Offline capabilities

## Backend Architecture

### Microservices Overview

Each microservice is containerized, independently deployable, and responsible for a specific domain of functionality.

### 1. Authentication Service

- **Responsibilities**:
  - User registration and authentication
  - Token issuance and validation
  - Password reset workflows
  - User account management
- **Technologies**:
  - ASP.NET Core
  - Entity Framework Core
  - JWT for token-based authentication
- **Database**: SQL Server
- **API Endpoints**: As documented in [API_REFERENCE.md](./API_REFERENCE.md)

### 2. User Service

- **Responsibilities**:
  - User profile management
  - User preferences
  - User search and discovery
  - Friend/contact management
- **Technologies**:
  - ASP.NET Core
  - Entity Framework Core
- **Database**: MongoDB
- **Events Published**:
  - `UserCreated`
  - `UserUpdated`
  - `UserDeleted`
  - `FriendshipRequested`
  - `FriendshipAccepted`

### 3. Chat Service

- **Responsibilities**:
  - Managing chat rooms and channels
  - Storing chat history
  - Direct messaging
  - Group chat functionality
- **Technologies**:
  - Node.js
  - Express
  - Socket.io
- **Database**: MongoDB
- **Events Published**:
  - `MessageSent`
  - `MessageRead`
  - `ChatRoomCreated`
  - `UserJoinedChatRoom`
  - `UserLeftChatRoom`

### 4. Notification Service

- **Responsibilities**:
  - Real-time notifications
  - Push notification delivery
  - Email notifications
  - Notification preferences
- **Technologies**:
  - Node.js
  - Express
  - Firebase Cloud Messaging
- **Database**: Redis
- **Events Consumed**:
  - `MessageSent`
  - `FriendshipRequested`
  - `UserMentioned`
- **Events Published**:
  - `NotificationSent`
  - `NotificationRead`

### 5. WebSocket Gateway

- **Responsibilities**:
  - Managing WebSocket connections
  - Real-time event distribution
  - Connection state management
- **Technologies**:
  - Node.js
  - Socket.io
  - Redis Adapter (for scaling)
- **Events Consumed**: All events requiring real-time updates
- **Key Features**:
  - Authentication of WebSocket connections
  - Channel subscriptions
  - Broadcasting events to relevant clients
  - Presence detection

### 6. Media Service

- **Responsibilities**:
  - File uploads and storage
  - Image processing and optimization
  - Media sharing
  - Content delivery
- **Technologies**:
  - ASP.NET Core
  - Azure Blob Storage / AWS S3
- **Database**: MongoDB (metadata)
- **Events Published**:
  - `MediaUploaded`
  - `MediaShared`

### 7. Analytics Service

- **Responsibilities**:
  - User activity tracking
  - System usage statistics
  - Performance monitoring
  - Business intelligence
- **Technologies**:
  - Python
  - FastAPI
  - Pandas
- **Database**: 
  - InfluxDB (time-series data)
  - PostgreSQL (aggregated reports)
- **Events Consumed**: Most system events

## Inter-Service Communication

### Synchronous Communication

- **Protocol**: RESTful HTTP/HTTPS
- **Use Cases**:
  - User-initiated actions requiring immediate response
  - Service-to-service direct queries
  - Authentication and authorization

### Asynchronous Communication

- **Technology**: RabbitMQ
- **Implementation Pattern**: Event-driven architecture
- **Message Format**: JSON with standardized envelope structure
- **Key Patterns Used**:
  - Publish/Subscribe
  - Work Queues
  - RPC (Remote Procedure Call) when needed
- **Benefits**:
  - Decoupling services
  - Improved resilience
  - Better scaling capabilities
  - Event sourcing capabilities

## Data Storage

### Primary Databases

1. **SQL Server**
   - Used by: Authentication Service
   - Data: User credentials, roles, permissions

2. **MongoDB**
   - Used by: User Service, Chat Service, Media Service
   - Data: User profiles, chat messages, media metadata
   - Benefits: Schema flexibility, document-oriented storage

3. **Redis**
   - Used by: Notification Service, WebSocket Gateway
   - Data: Caching, pub/sub, ephemeral data
   - Benefits: High performance, in-memory storage

4. **InfluxDB**
   - Used by: Analytics Service
   - Data: Time-series metrics
   - Benefits: Optimized for time-series data

### Database Access Patterns

- Each microservice owns its database
- No cross-service direct database access
- Data replication achieved through events when necessary
- Data consistency maintained through eventual consistency patterns

## External Dependencies

1. **Firebase Cloud Messaging (FCM)**
   - Purpose: Push notifications to mobile devices
   - Integration: Via Notification Service

2. **SMTP Provider (SendGrid/Mailgun)**
   - Purpose: Email delivery
   - Integration: Via Notification Service

3. **Cloud Storage (Azure Blob/AWS S3)**
   - Purpose: Media file storage
   - Integration: Via Media Service

## Deployment and DevOps

### Containerization

- **Technology**: Docker
- **Orchestration**: Kubernetes
- **CI/CD**: GitHub Actions

### Service Discovery & Configuration

- **Service Registry**: Kubernetes services
- **Configuration Management**: Kubernetes ConfigMaps and Secrets

### Monitoring and Logging

- **Metrics**: Prometheus
- **Logging**: ELK Stack (Elasticsearch, Logstash, Kibana)
- **Tracing**: Jaeger
- **Alerts**: Grafana

## Security Architecture

### Authentication

- **Technology**: JWT (JSON Web Tokens)
- **Flow**:
  1. User authenticates via Authentication Service
  2. JWT issued to client
  3. JWT included in all subsequent requests
  4. Microservices validate JWT for protected endpoints

### Authorization

- Role-based access control (RBAC)
- Resource-level permissions
- Service-to-service authorization using API keys

### Data Protection

- TLS/SSL for all communications
- Data encryption at rest
- PII (Personally Identifiable Information) handling compliant with GDPR

## Scaling Strategy

### Horizontal Scaling

- All services designed to be stateless where possible
- WebSocket connections managed with Redis adapter for multi-instance support
- Database sharding strategy for MongoDB

### Performance Optimization

- Redis caching for frequently accessed data
- CDN integration for media delivery
- Message batching for high-volume events

## Resilience Patterns

- Circuit breakers between service communications
- Retry policies with exponential backoff
- Graceful degradation of non-critical services
- Health checks and self-healing

## Future Enhancements

1. **Voice and Video Chat**
   - WebRTC integration
   - Media server for group calls

2. **AI Features**
   - Smart message suggestions
   - Content moderation
   - Chatbots and virtual assistants

3. **Extended Platform Support**
   - Native mobile applications
   - Desktop applications

## Development Guidelines

- API-first design approach
- Event schema versioning
- Backward compatibility considerations
- Feature flags for progressive rollouts

---

This architecture document is a living document and will be updated as the system evolves.
