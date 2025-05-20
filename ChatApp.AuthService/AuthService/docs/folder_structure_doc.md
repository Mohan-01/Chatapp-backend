# Folder Structure Documentation

This document explains the organization and purpose of the main folders and files in our Real-Time Chat Application repository, following clean microservices architecture principles.

## Repository Root

```
|   .gitignore              # Git ignore file for excluding build artifacts and environment files
|   folder-structure.txt    # Text representation of the project structure
|   README.md               # Main project documentation and introduction
|   
+---.github                 # GitHub specific configuration
|   \---workflows           # CI/CD pipeline definitions for GitHub Actions
```

## Shared Library

The `ChatApp.Shared` directory contains code and contracts shared across multiple microservices:

```
+---ChatApp.Shared          # Shared libraries and contracts between services
|   |   Shared.csproj       # Project file for the shared library
|   |   Shared.sln          # Solution file for the shared library
|   +---Configurations      # Shared configuration components
|   |       IRabbitMQConnection.cs    # Interface for RabbitMQ connections
|   |       RabbitMQConnection.cs     # Implementation of RabbitMQ connection
|   +---Constants           # Shared constant values
|   |       QueueNames.cs   # Queue name constants for message broker
|   +---DTOs                # Shared Data Transfer Objects
|   |       AuthResponseDto.cs        # Authentication response object
|   |       AuthServiceResponseDto.cs # Service response wrapper
|   |       ValidateTokenRequest.cs   # Token validation request
|   |       ValidateTokenResponseDto.cs # Token validation response
|   +---Enums               # Shared enumeration types
|   |   \---User
|   |           UserRole.cs # User role definitions
|   +---EventContracts      # Event definitions for inter-service communication
|   |       EmailChangedEvent.cs      # Event when email is changed
|   |       UserDeletedEvent.cs       # Event when user is deleted
|   |       UsernameChangedEvent.cs   # Event when username is changed
|   |       UserRegisteredEvent.cs    # Event when user is registered
|   +---Middlewares         # Shared HTTP pipeline components
|   |       AuthenticationMiddleware.cs # Authentication middleware
|   +---Models              # Shared domain models
|   |   \---User
|   |           UserDto.cs  # User data transfer object
```

## Authentication Service

The `ChatApp.AuthService` handles user authentication, registration, and account management:

```
+---ChatApp.AuthService     # Authentication microservice
|   +---AuthService         # API layer
|   |   |   .dockerignore   # Docker ignore file
|   |   |   AuthService.sln # Solution file for the Auth service
|   |   |   docker-compose.yml # Docker compose for local development
|   |   |   Dockerfile      # Docker build definition
|   |   |   Program.cs      # Application entry point
|   |   |   README.md       # Auth service specific documentation
|   |   +---Controllers     # API endpoints
|   |   |       AuthController.cs # Authentication API controller
|   |   +---Logging         # Logging configurations
|   |   |       CallerInfoEnricher.cs # Enricher for structured logging
|   |   +---Middlewares     # HTTP pipeline components
|   |   |       AuthenticationMiddlewear.cs # Authentication verification
|   |   |       ExceptionHandlingMiddleware.cs # Global exception handler
|   +---AuthService.Core    # Business logic layer
|   |   |   AuthService.Core.csproj # Core project file
|   |   +---Constants       # Service-specific constants
|   |   |       RoleDescriptions.cs # Descriptions for user roles
|   |   +---DTOs            # Data Transfer Objects
|   |   |       # Various DTOs for authentication flows
|   |   +---Interfaces      # Core interfaces
|   |   |       IAuthRepository.cs    # Repository interface
|   |   |       IAuthService.cs       # Service interface
|   |   |       IEmailNotificationService.cs # Email notification interface
|   |   |       IEmailService.cs      # Email sending interface
|   |   |       IEventPublisher.cs    # Event publishing interface
|   |   |       ITokenHandler.cs      # JWT token handling interface
|   |   +---Models          # Domain models
|   |   |       AuthUser.cs # User model for authentication
|   |   +---Services        # Core business logic implementations
|   |   |       AuthService.cs # Authentication service implementation
|   |   |       EmailNotificationService.cs # Email notification service
|   |   |       EmailService.cs # Email sending service
|   |   \---Utils           # Utility classes
|   |           AuthExecption.cs # Custom exception types
|   |           PasswordHasher.cs # Password hashing utilities
|   |           TokenHandler.cs # JWT token generation and validation
|   \---AuthService.Infrastructure # Data access and external service integrations
|       |   AuthService.Infrastructure.csproj # Infrastructure project file
|       +---Configurations  # Infrastructure specific configurations
|       |       MongoDbConfig.cs # MongoDB connection configuration
|       |       RabbitMqConfig.cs # RabbitMQ connection configuration
|       +---Producers       # Message producer implementations
|       |       EventPublisher.cs # Implementation of event publishing
|       \---Repositories    # Data access implementations
|               AuthRepository.cs # User data repository
```

## Chat Service

The `ChatApp.ChatService` manages chat rooms, messages, and friendships:

```
+---ChatApp.ChatService     # Chat functionality microservice
|   +---ChatApp.ChatService.API # API layer
|   |   |   ChatApp.ChatService.API.sln # Solution file
|   |   |   Program.cs      # Application entry point
|   |   +---Controllers     # API endpoints
|   |   |       ChatController.cs # Chat API controller
|   +---ChatApp.ChatService.Core # Business logic layer
|   |   |   ChatApp.ChatService.Core.csproj # Core project file
|   |   +---DTOs            # Data Transfer Objects
|   |   |   |   FriendDto.cs # Friend data transfer object
|   |   |   +---Chat        # Chat related DTOs
|   |   |   |       ChatDto.cs # Chat room DTO
|   |   |   |       PrivateChatDto.cs # Private chat DTO
|   |   |   \---Message     # Message related DTOs
|   |   |           AttachmentDto.cs # Attachment DTO
|   |   |           MessageDto.cs # Message DTO
|   |   +---Entities        # Domain entities
|   |   |   +---Chat        # Chat entities
|   |   |   |       Chat.cs # Chat room entity
|   |   |   +---Friendship  # Friendship entities
|   |   |   |       Friendship.cs # Friend relationship entity
|   |   |   \---Message     # Message entities
|   |   |           Attachment.cs # File attachment entity
|   |   |           Message.cs # Message entity
|   |   +---Enums           # Enumeration types
|   |   |   +---Chat        # Chat related enums
|   |   |   |       ChatStatus.cs # Chat status enum
|   |   |   |       ChatType.cs # Chat type enum
|   |   |   |       GroupType.cs # Group type enum
|   |   |   +---Friend      # Friend related enums
|   |   |   |       FriendRequestStatus.cs # Friend request status
|   |   |   \---Message     # Message related enums
|   |   |           AttachmentType.cs # Attachment type enum
|   |   |           MessageStatus.cs # Message status enum
|   |   |           MessageType.cs # Message type enum
|   |   +---Exceptions      # Custom exceptions
|   |   |       NotFoundException.cs # Resource not found exception
|   |   +---Interfaces      # Core interfaces
|   |   |       # Various service and repository interfaces
|   |   +---Mappings        # Object mapping configurations
|   |   |       MappingProfile.cs # AutoMapper profile
|   |   |       MappingToDtos.cs # DTO mapping extensions
|   |   +---RequestResponseModels # API request/response models
|   |   |   +---Chat        # Chat request/response models
|   |   |   |       CreateChatRequest.cs # Create chat request
|   |   |   |       ServiceResponse.cs # Generic service response
|   |   |   |       UpdateChatStatusRequest.cs # Update chat status
|   |   |   +---Friend      # Friend request/response models
|   |   |   |       # Various friend request DTOs
|   |   |   \---Message     # Message request/response models
|   |   |           ChangeMessageStatus.cs # Change message status
|   |   |           EditTextMessage.cs # Edit message request
|   |   |           SendMessageDto.cs # Send message request
|   |   \---Services        # Business logic implementations
|   |           ChatService.cs # Chat service implementation
|   |           FriendshipService.cs # Friendship service
|   |           MessageService.cs # Message service
|   \---ChatApp.ChatService.Infrastructure # Data access layer
|       |   ChatApp.ChatService.Infrastructure.csproj # Infrastructure project
|       +---HttpClients     # HTTP client implementations
|       |       ChatApiClient.cs # Chat API client
|       |       MessageApiClient.cs # Message API client
|       |       UserApiClient.cs # User API client
|       +---Repositories    # Data access implementations
|       |       ChatRepository.cs # Chat data repository
|       |       FriendshipRepository.cs # Friendship repository
|       |       MessageRepository.cs # Message repository
|       |       Pipelines.cs # Database query pipelines
|       \---Settings        # Infrastructure settings
|               MongoDbSettings.cs # MongoDB connection settings
```

## User Service

The `ChatApp.UserService` manages user profiles and processes user-related events:

```
\---ChatApp.UserService     # User management microservice
    +---ChatApp.UserService.API # API layer
    |   |   ChatApp.UserService.API.sln # Solution file
    |   |   Program.cs      # Application entry point
    |   +---Controllers     # API endpoints
    |   |       UserController.cs # User API controller
    |   +---Mappings        # Object mapping configurations
    |   |       MappingProfile.cs # AutoMapper profile
    |   |       MappingToDtos.cs # DTO mapping extensions
    |   +---Middlewares     # HTTP pipeline components
    |   |       AuthenticationMiddleware.cs # Authentication verification
    |   |       ExceptionHandlingMiddleware.cs # Exception handling
    +---ChatApp.UserService.Core # Business logic layer
    |   |   ChatApp.UserService.Core.csproj # Core project file
    |   +---Entities        # Domain entities
    |   |       User2.cs    # User entity
    |   +---Enums           # Enumeration types
    |   |       UserStatus.cs # User status enum
    |   +---Extensions      # Extension methods
    |   |       StringExtensions.cs # String utility extensions
    |   +---Interfaces      # Core interfaces
    |   |       # Various service and consumer interfaces
    |   +---Mappings        # Object mapping configurations
    |   |       MappingProfile.cs # AutoMapper profile
    |   |       MappingToDtos.cs # DTO mapping extensions
    |   +---RequestDTOs     # API request models
    |   |       BatchUserRequest.cs # Batch user request
    |   |       SearchUsersRequest.cs # User search request
    |   |       UpdateUserRequest.cs # User update request
    |   |       UserByEmailRequest.cs # Get user by email
    |   |       ValidateTokenRequest.cs # Token validation
    |   +---ResponseDTOs    # API response models
    |   |       AuthResponseDto.cs # Auth response
    |   |       AuthServiceResponseDto.cs # Service response
    |   |       ServiceResponse.cs # Generic response
    |   |       ValidateTokenResponseDto.cs # Token validation
    |   \---Services        # Business logic implementations
    |           UserEventsService.cs # User events handler
    |           UserService.cs # User service implementation
    \---ChatApp.UserService.Infrastructure # Data access layer
        |   ChatApp.UserService.Infrastructure.csproj # Infrastructure project
        +---BackgroundServices # Background processing services
        |       EmailChangedConsumerService.cs # Email change processor
        |       UserDeletedConsumerService.cs # User deletion processor
        |       UsernameChangedConsumerService.cs # Username change processor
        |       UserRegisteredConsumerService.cs # User registration processor
        +---Consumers        # Event consumer implementations
        |       EmailChangedConsumer.cs # Email change event consumer
        |       EventConsumer.cs # Base event consumer
        |       UserDeletedConsumer.cs # User deletion event consumer
        |       UsernameChangedConsumer.cs # Username change consumer
        |       UserRegisteredConsumer.cs # User registration consumer
        +---Repositories     # Data access implementations
        |       UserRepository.cs # User data repository
        \---Settings         # Infrastructure settings
                MongoDbSettings.cs # MongoDB connection settings
```

## Architecture Pattern

This project follows the Clean Architecture pattern with a clear separation of concerns:

1. **API Layer**: Contains controllers, middleware, and API-specific mappings
2. **Core Layer**: Contains business logic, domain entities, interfaces, and DTOs
3. **Infrastructure Layer**: Contains implementations of repositories, external service integrations, and data access

Each microservice follows the same architectural pattern, promoting consistency and maintainability across the system.

## Key Architectural Components

### Domain-Driven Design (DDD)
- Clear entity models representing business concepts
- Rich domain model with business rules encapsulated within the domain layer
- Separation of domain models from DTOs for external communication

### Dependency Inversion
- Core business logic depends on abstractions (interfaces)
- Infrastructure implementations depend on these interfaces
- Flow of control points inward toward the domain

### Event-Driven Communication
- Event contracts for inter-service communication
- Event consumers and producers for asynchronous processing
- RabbitMQ configured for message brokering

### Repository Pattern
- Abstracting data access behind repository interfaces
- Repository implementations in the infrastructure layer
- Clean separation of data access concerns from business logic

### Microservices Principles
- Each service has its own database
- Services communicate via events and HTTP when necessary
- Shared code minimized and placed in the Shared project

This structure ensures the application is maintainable, testable, and follows the principles of clean architecture in a microservices context.
