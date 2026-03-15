using InventoryManagement.API.Shared;

namespace InventoryManagement.API.Features.Suppliers;

public sealed class Supplier
{
    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;

    private Supplier() { } // EF Core

    public static Supplier Create(string name, string email, string currency, string country)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("Valid supplier email is required.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Supplier currency is required.");

        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("Supplier country is required.");

        return new Supplier
        {
            Id = Guid.CreateVersion7().ToString(),
            Name = name,
            Email = email,
            Currency = currency,
            Country = country
        };
    }
}