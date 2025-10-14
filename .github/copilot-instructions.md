# Copilot Instructions for Stross Backend Development

## Project Overview
This is a .NET backend application built with Clean Architecture principles, using gRPC for communication, MediatR for CQRS pattern implementation, and Domain-Driven Design (DDD) principles.

## Architecture Guidelines

### Project Structure
- **Stross.Domain**: Core business logic, entities, value objects, and domain services
- **Stross.Application**: Application services, use cases, DTOs, and MediatR handlers
- **Stross.Infrastructure**: Data access, external services, and infrastructure concerns
- **Stross.API**: gRPC services and API controllers
- **Stross.Proto**: Protocol buffer definitions for gRPC
- **Stross.Config**: Configuration models and settings
- **Stross.Downloader**: Specialized service for download operations

### Clean Code Principles
1. **Single Responsibility Principle**: Each class should have one reason to change
2. **Dependency Inversion**: Depend on abstractions, not concretions
3. **Explicit Dependencies**: Use constructor injection for dependencies
4. **Meaningful Names**: Use descriptive names for classes, methods, and variables
5. **Small Functions**: Keep methods focused and concise
6. **No Magic Numbers**: Use constants or configuration for literal values
7. **Whitespace Before Returns**: Always add a blank line before return statements when there is code above (applies to all code blocks, usings, if statements, foreach, do-while, etc.)
8. **Explicit Type Declarations**: Never use the `var` keyword - always use explicit type declarations for clarity
9. **Separate Request/Response Models**: Divide DTOs into separate Request and Response models instead of using generic DTOs

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

### gRPC Service Guidelines

#### Proto File Organization
- Use semantic versioning for service definitions
- Group related operations in the same service
- Use appropriate field numbers and avoid reusing them
- Include comprehensive documentation in proto files

Example proto service:
```protobuf
syntax = "proto3";

package stross.v1;

import "google/protobuf/timestamp.proto";
import "google/protobuf/empty.proto";

service OrderService {
  rpc CreateOrder(CreateOrderRequest) returns (OrderResponse);
  rpc GetOrder(GetOrderRequest) returns (OrderResponse);
  rpc ListOrders(ListOrdersRequest) returns (ListOrdersResponse);
}

message CreateOrderRequest {
  string customer_id = 1;
  repeated OrderItem items = 2;
}
```

#### gRPC Service Implementation
```csharp
public class OrderGrpcService : OrderService.OrderServiceBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    
    public OrderGrpcService(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }
    
    public override async Task<OrderResponse> CreateOrder(
        CreateOrderRequest request, 
        ServerCallContext context)
    {
        CreateOrderCommand command = _mapper.Map<CreateOrderCommand>(request);
        CreateOrderResponse result = await _mediator.Send(command);
        
        return _mapper.Map<OrderResponse>(result);
    }
}
```

### Code Generation Guidelines

#### When creating new features:
1. **Start with the Domain**: Create entities, value objects, and domain services first
2. **Define the Contract**: Create proto files for gRPC endpoints
3. **Implement Application Layer**: Create commands/queries and their handlers
4. **Add Infrastructure**: Implement repositories and external service integrations
5. **Create gRPC Services**: Implement the gRPC service layer
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
- Use Result pattern for operation outcomes
- Implement global exception handling middleware
- Create custom domain exceptions
- Use gRPC status codes appropriately
- Log errors with structured logging

Example Result pattern:
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    
    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static Result<T> Failure(string error) => new(false, default!, error);
}
```

### Configuration Management
- Use strongly-typed configuration classes
- Implement configuration validation
- Use the Options pattern
- Store sensitive data in environment variables or key vaults

### Testing Guidelines
- Write unit tests for domain logic
- Create integration tests for MediatR handlers
- Use TestContainers for database integration tests
- Mock external dependencies
- Follow AAA pattern (Arrange, Act, Assert)

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
- Handlers: `CreateOrderCommandHandler.cs`
- Request Models: `CreateOrderRequest.cs`
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
