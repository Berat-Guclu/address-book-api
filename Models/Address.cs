namespace AddressBookApi.Models;

public sealed class Address
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Company { get; set; }
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public bool IsFavorite { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed record CreateAddressRequest(
    string Name,
    string Street,
    string City,
    string Country,
    string? Company,
    string? Region,
    string? PostalCode,
    string? Phone,
    string? Email,
    string? Notes,
    bool IsFavorite = false);

public sealed record UpdateAddressRequest(
    string Name,
    string Street,
    string City,
    string Country,
    string? Company,
    string? Region,
    string? PostalCode,
    string? Phone,
    string? Email,
    string? Notes,
    bool IsFavorite);

public sealed record EditAddressRequest(
    string? Name,
    string? Street,
    string? City,
    string? Country,
    string? Company,
    string? Region,
    string? PostalCode,
    string? Phone,
    string? Email,
    string? Notes,
    bool? IsFavorite);

public sealed record FavoriteRequest(bool IsFavorite);
