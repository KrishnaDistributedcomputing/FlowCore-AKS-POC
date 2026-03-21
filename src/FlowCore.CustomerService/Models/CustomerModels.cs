namespace FlowCore.CustomerService.Models;

public record CreateCustomerRequest(string FirstName, string LastName, string Email);
public record UpdateCustomerRequest(string? FirstName, string? LastName, string? Email);
public record CustomerResponse(Guid Id, string FirstName, string LastName, string Email, DateTime CreatedAtUtc);
