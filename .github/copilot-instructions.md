# Copilot Instructions for Stross Backend Development

## Project Overview
This is a .NET backend application built with Clean Architecture principles, using minimal APIs for HTTP endpoints and gRPC only for communication with the downloader service, MediatR for CQRS pattern implementation, vertical sliced, minimal api based, and Domain-Driven Design (DDD) principles.

## Architecture Guidelines

### Project Structure
- **Stross.Domain**: Core business logic, entities, value objects, and domain services
- **Stross.Application**: Application services, use cases, DTOs, and MediatR handlers
- **Stross.Infrastructure**: Data access, external services, and infrastructure concerns (including gRPC clients for downloader service)
- **Stross.API**: Minimal API endpoints only
- **Stross.Proto**: Protocol buffer definitions for gRPC communication with downloader service only
- **Stross.Config**: Configuration models and settings
- **Stross.Downloader.YT**: Specialized YouTube service for download operations and fetching metadata (uses gRPC server)
- **Stross.Exception**: Contains all the exceptions that the application can throw
- **Stross.SubsonicModels**: Auto-generated classes from the official Subsonic API schema (version 1.16.1) that provide full compliance with the Subsonic specification. Contains all response models, data structures, and API contracts required for Subsonic endpoint implementations

### Clean Code Principles
1. **Single Responsibility Principle**: Each class should have one reason to change
2. **Dependency Inversion**: Depend on abstractions, not concretions
3. **Explicit Dependencies**: Use constructor injection for dependencies
4. **Meaningful Names**: Use descriptive names for classes, methods, and variables
5. **Small Functions**: Keep methods focused and concise
6. **No Magic Numbers**: Use constants or configuration for literal values
7. **Whitespace Before Returns**: Always add a blank line before return statements when there is code above (applies to all code blocks, usings, if statements, foreach, do-while, etc.)
8. **Explicit Type Declarations**: Never use the `var` keyword - always use explicit type declarations for clarity
9. **Separate Input/Response Models**: Divide DTOs into separate Input and Response models instead of using generic DTOs
10. **Immutable Properties**: Use `init` accessors instead of `set` for DTOs and response models to ensure immutability after initialization
11. **Prefer Record Types**: Use `record` types instead of `class` for DTOs, value objects, and data-only models as they provide immutability, value equality, and better functional programming support by default

#### Vertical Slicing Guidelines
- Organize code by feature rather than by layer
- Each featur has it's own folder in the `Slices` folder in the application layer
- Each feature folder contains its own commands, queries, handlers, and DTOs
- Each feature has it own composer which looks like:
```csharp
public static class ExampleComposer
{
    public static IHostApplicationBuilder AddExampleSlice(this IHostApplicationBuilder builder)
    {
        builder.Services.RegisterExampleSliceServices();

        return builder;
    }

    private static IServiceCollection RegisterExampleSliceServices(this IServiceCollection services)
    {

        return services;
    }
}
```

### Minimal API Guidelines
- Put all the endpoints of a single slice in a single file named `SliceNameEndpoints.cs`
- Use extension methods to add endpoints to the `IEndpointRouteBuilder` which is called `SliceNameEndpoints`
- Put minimal logic in the endpoints
- Use groups to create logical groupings of endpoints

### Domain-Driven Design (DDD) Guidelines

#### Domain Layer
- Create rich domain models with behavior, not just data containers
- Use Value Objects for concepts without identity (e.g., Money, Email, Address)
- Implement Domain Events for cross-aggregate communication
- Keep domain logic free from infrastructure concerns
- Use Aggregate Roots to maintain consistency boundaries

Example domain entity structure:
```csharp
public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = new();
    
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    
    public void AddItem(Product product, int quantity)
    {
        // Domain logic here
        OrderItem item = new OrderItem(product, quantity);
        _items.Add(item);
        
        // Raise domain event if needed
        RaiseDomainEvent(new OrderItemAddedEvent(Id, item));
    }
}
```

#### Application Layer
- Use MediatR for implementing CQRS pattern
- Separate Commands and Queries
- Implement validation using FluentValidation
- Use the Repository pattern for data access abstractions
- Handle cross-cutting concerns with Pipeline Behaviors

### MediatR Implementation Guidelines

#### Commands (Write Operations)
```csharp
public record CreateOrderCommand(Guid CustomerId, List<OrderItemDto> Items) : IRequest<CreateOrderResponse>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    
    public CreateOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }
    
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Implementation here
        
        return createOrderResponse;
    }
}
```

#### Queries (Read Operations)
```csharp
public record GetOrderQuery(Guid OrderId) : IRequest<GetOrderResponse>;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, GetOrderResponse>
{
    // Implementation
}
```

#### Pipeline Behaviors
- Use ValidationBehaviour for automatic validation
- Implement LoggingBehaviour for request/response logging
- Add PerformanceBehaviour for monitoring slow requests

#### DTO Models with Immutable Properties
Always use `record` types with required properties in the primary constructor for DTOs to ensure immutability after initialization:

```csharp
// Input Model Example (using record with primary constructor)
public sealed record CreateOrderInput(Guid CustomerId, List<OrderItemInput> Items)
{
    public string? Notes { get; init; }
}

// Response Model Example (using record with primary constructor)
public sealed record CreateOrderResponse(Guid OrderId, DateTime CreatedAt)
{
    public string Status { get; init; } = string.Empty;
    public List<OrderItem> Items { get; init; } = [];
}

// Simple record for DTOs with only required properties
public sealed record CreateOrderInput(Guid CustomerId, List<OrderItemInput> Items, string? Notes = null);
```

#### Subsonic API Guidelines
When creating Subsonic commands and queries, always follow these specific patterns:

- **All Subsonic Commands and Queries MUST return `SubsonicBaseResponse`**: This ensures consistency with the Subsonic API specification and allows the `SubsonicBehaviour` pipeline to properly handle responses.
- **Use the `SubsonicBehaviour` pipeline**: This behavior automatically sets the response status, version, and handles exceptions appropriately for Subsonic responses.
- **Error Handling**: Throw `StrossException` derived exceptions which will be caught by the `SubsonicBehaviour` and converted to proper Subsonic error responses.
- **ALWAYS use models from `Stross.SubsonicModels` project**: All Subsonic-related data structures, response objects, and API models MUST come from the `Stross.SubsonicModels` project. This project contains auto-generated classes from the official Subsonic API schema and ensures full compliance with the Subsonic specification. Never create custom models that duplicate or replace these official models.

Example Subsonic command pattern:
```csharp
using Stross.SubsonicModels; // Always import from SubsonicModels project

public sealed record SubsonicPingCommand() : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicPingCommandHandler : IRequestHandler<SubsonicPingCommand, SubsonicBaseResponse>
{
    public Task<SubsonicBaseResponse> Handle(SubsonicPingCommand request, CancellationToken cancellationToken)
    {
        // Use Response from Stross.SubsonicModels
        Response response = new Response
        {
            Status = ResponseStatus.Ok, // This will be set by SubsonicBehaviour
            Version = "1.16.1", // This will be set by SubsonicBehaviour
        };

        return Task.FromResult(new SubsonicBaseResponse(response));
    }
}
```

Example Subsonic query pattern:
```csharp
using Stross.SubsonicModels; // Always import from SubsonicModels project

public sealed record SubsonicSearchQuery(SubsonicSearchInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicSearchQueryHandler : IRequestHandler<SubsonicSearchQuery, SubsonicBaseResponse>
{
    public async Task<SubsonicBaseResponse> Handle(SubsonicSearchQuery request, CancellationToken cancellationToken)
    {
        // Implementation logic here
        // Use Response and other models from Stross.SubsonicModels
        Response response = new Response();
        
        // Example: Use official models like Child, Artist, Album, etc.
        List<Child> songs = new List<Child>();
        response.SearchResult = new SearchResult
        {
            Match = songs
        };
        
        return new SubsonicBaseResponse(response);
    }
}
```

### gRPC Guidelines (Infrastructure Layer Only)

**Important**: gRPC should ONLY be used in the Infrastructure layer for communication with the downloader service. The API layer should only expose minimal HTTP endpoints and remain agnostic of gRPC implementation details.

#### Proto File Organization (Downloader Service Only)
- Use semantic versioning for service definitions
- Group related download operations in the same service
- Use appropriate field numbers and avoid reusing them
- Include comprehensive documentation in proto files
- Focus on download-specific operations like metadata retrieval and download requests

Example proto service for downloader communication:
```protobuf
syntax = "proto3";

package stross.downloader.v1;

import "google/protobuf/timestamp.proto";
import "google/protobuf/empty.proto";

service DownloaderService {
  rpc GetMetadata(GetMetadataRequest) returns (MetadataResponse);
  rpc DownloadTrack(DownloadRequest) returns (DownloadResponse);
  rpc GetDownloadStatus(StatusRequest) returns (StatusResponse);
}

message GetMetadataRequest {
  string url = 1;
  string provider = 2;
}
```

#### gRPC Client Implementation (Infrastructure Layer)
```csharp
public class DownloaderGrpcClient : IDownloaderService
{
    private readonly DownloaderService.DownloaderServiceClient _client;
    private readonly IMapper _mapper;
    
    public DownloaderGrpcClient(DownloaderService.DownloaderServiceClient client, IMapper mapper)
    {
        _client = client;
        _mapper = mapper;
    }
    
    public async Task<MetadataResponse> GetMetadataAsync(
        GetMetadataRequest request, 
        CancellationToken cancellationToken = default)
    {
        GetMetadataRequest grpcRequest = _mapper.Map<GetMetadataRequest>(request);
        MetadataResponse response = await _client.GetMetadataAsync(grpcRequest, cancellationToken: cancellationToken);
        
        return _mapper.Map<MetadataResponse>(response);
    }
}
```

### Code Generation Guidelines

#### When creating new features:
1. **Start with the Domain**: Create entities, value objects, and domain services first
2. **Define the Contract**: Create minimal API endpoints for HTTP communication (gRPC only for downloader service)
3. **Implement Application Layer**: Create commands/queries and their handlers
4. **Add Infrastructure**: Implement repositories and external service integrations
5. **Create API Endpoints**: Implement minimal API endpoints for the feature
6. **Add Validation**: Implement FluentValidation rules
7. **Write Tests**: Create unit tests for domain logic and integration tests for handlers

#### Repository Pattern
```csharp
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(OrderId id, CancellationToken cancellationToken = default);
}
```

### Error Handling
- Throw errors provided by the Stross.Exception project

### Configuration Management
- Use strongly-typed configuration classes
- Implement configuration validation
- Use the Options pattern
- Store sensitive data in environment variables or key vaults

### Performance Considerations
- Use async/await consistently
- Implement caching where appropriate
- Use pagination for large result sets
- Consider using streaming for large data transfers
- Monitor and log performance metrics

### Security Guidelines
- Validate all inputs
- Use authentication and authorization
- Sanitize data before persistence
- Use HTTPS for all communications

## File Naming Conventions
- Commands: `CreateOrderCommand.cs`
- Queries: `GetOrderQuery.cs`
- Input Models: `CreateOrderInput.cs`
- Response Models: `CreateOrderResponse.cs`
- Entities: `Order.cs`
- Value Objects: `OrderId.cs`
- Services: `OrderService.cs`
- Repositories: `IOrderRepository.cs`, `OrderRepository.cs`

## Common Code Patterns to Follow
1. Always use CancellationToken in async methods
2. Implement proper disposal of resources
3. Use guard clauses for parameter validation
4. Prefer composition over inheritance
5. Use readonly fields where possible
6. Implement proper equality for value objects
7. Use factory methods for complex object creation

Remember to maintain consistency with the existing codebase and follow the established patterns in the Stross project.
