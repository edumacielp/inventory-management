namespace InventoryManagement.API.Shared;

public sealed class DomainException(string message) : Exception(message);