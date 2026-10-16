using System.Collections.Concurrent;
using AddressBookApi.Models;

namespace AddressBookApi.Data;

public sealed class InMemoryAddressBook
{
    private readonly ConcurrentDictionary<Guid, Address> _addresses = new();

    public InMemoryAddressBook()
    {
        Seed(
            "Ada Lovelace",
            "Analytical Engines",
            "12 St James's Square",
            "London",
            "England",
            "SW1Y 4LB",
            "United Kingdom",
            "+44 20 7946 0958",
            "ada@example.com",
            "Notes on the analytical engine",
            true);
        Seed(
            "Grace Hopper",
            "Navy Computing",
            "1 Compiler Way",
            "Arlington",
            "Virginia",
            "22202",
            "United States",
            "+1 703 555 0148",
            "grace@example.com",
            null,
            true);
        Seed(
            "Alan Turing",
            null,
            "8 Enigma Road",
            "Manchester",
            null,
            "M1 1AE",
            "United Kingdom",
            null,
            "alan@example.com",
            "Bletchley contact",
            false);
    }

    public IReadOnlyList<Address> Query(
        string? search,
        string? city,
        string? country,
        bool? favorite,
        string sortBy,
        bool descending)
    {
        IEnumerable<Address> query = _addresses.Values;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(address =>
                Contains(address.Name, term) ||
                Contains(address.Company, term) ||
                Contains(address.Street, term) ||
                Contains(address.City, term) ||
                Contains(address.Region, term) ||
                Contains(address.PostalCode, term) ||
                Contains(address.Country, term) ||
                Contains(address.Phone, term) ||
                Contains(address.Email, term) ||
                Contains(address.Notes, term));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(address =>
                string.Equals(address.City, city.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(address =>
                string.Equals(address.Country, country.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (favorite is not null)
        {
            query = query.Where(address => address.IsFavorite == favorite);
        }

        IOrderedEnumerable<Address> ordered = sortBy switch
        {
            "city" => descending ? query.OrderByDescending(a => a.City) : query.OrderBy(a => a.City),
            "country" => descending ? query.OrderByDescending(a => a.Country) : query.OrderBy(a => a.Country),
            "createdAt" => descending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            "updatedAt" => descending ? query.OrderByDescending(a => a.UpdatedAt) : query.OrderBy(a => a.UpdatedAt),
            "favorite" => descending ? query.OrderByDescending(a => a.IsFavorite) : query.OrderBy(a => a.IsFavorite),
            _ => descending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name)
        };

        return ordered.ThenBy(address => address.Id).ToList();
    }

    public Address? Get(Guid id) =>
        _addresses.TryGetValue(id, out var address) ? address : null;

    public Address Add(CreateAddressRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        var address = new Address
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Company = Clean(request.Company),
            Street = request.Street.Trim(),
            City = request.City.Trim(),
            Region = Clean(request.Region),
            PostalCode = Clean(request.PostalCode),
            Country = request.Country.Trim(),
            Phone = Clean(request.Phone),
            Email = Clean(request.Email),
            Notes = Clean(request.Notes),
            IsFavorite = request.IsFavorite,
            CreatedAt = now,
            UpdatedAt = now
        };

        _addresses[address.Id] = address;
        return address;
    }

    public Address? Update(Guid id, UpdateAddressRequest request)
    {
        if (!_addresses.TryGetValue(id, out var address))
        {
            return null;
        }

        address.Name = request.Name.Trim();
        address.Company = Clean(request.Company);
        address.Street = request.Street.Trim();
        address.City = request.City.Trim();
        address.Region = Clean(request.Region);
        address.PostalCode = Clean(request.PostalCode);
        address.Country = request.Country.Trim();
        address.Phone = Clean(request.Phone);
        address.Email = Clean(request.Email);
        address.Notes = Clean(request.Notes);
        address.IsFavorite = request.IsFavorite;
        address.UpdatedAt = DateTimeOffset.UtcNow;
        return address;
    }

    public Address? Edit(Guid id, EditAddressRequest request)
    {
        if (!_addresses.TryGetValue(id, out var address))
        {
            return null;
        }

        if (request.Name is not null) address.Name = request.Name.Trim();
        if (request.Company is not null) address.Company = Clean(request.Company);
        if (request.Street is not null) address.Street = request.Street.Trim();
        if (request.City is not null) address.City = request.City.Trim();
        if (request.Region is not null) address.Region = Clean(request.Region);
        if (request.PostalCode is not null) address.PostalCode = Clean(request.PostalCode);
        if (request.Country is not null) address.Country = request.Country.Trim();
        if (request.Phone is not null) address.Phone = Clean(request.Phone);
        if (request.Email is not null) address.Email = Clean(request.Email);
        if (request.Notes is not null) address.Notes = Clean(request.Notes);
        if (request.IsFavorite is not null) address.IsFavorite = request.IsFavorite.Value;
        address.UpdatedAt = DateTimeOffset.UtcNow;
        return address;
    }

    public bool Delete(Guid id) => _addresses.TryRemove(id, out _);

    public Address? SetFavorite(Guid id, bool isFavorite)
    {
        if (!_addresses.TryGetValue(id, out var address))
        {
            return null;
        }

        address.IsFavorite = isFavorite;
        address.UpdatedAt = DateTimeOffset.UtcNow;
        return address;
    }

    public static bool TryNormalizeSort(string? sortBy, string? sortDirection, out string field, out bool descending, out string? error)
    {
        var requested = string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy.Trim();
        descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        error = null;

        var allowed = new[] { "name", "city", "country", "createdAt", "updatedAt", "favorite" };
        var matched = allowed.FirstOrDefault(item => item.Equals(requested, StringComparison.OrdinalIgnoreCase));
        if (matched is null)
        {
            field = requested;
            error = "sortBy must be one of: name, city, country, createdAt, updatedAt, favorite.";
            return false;
        }

        field = matched;

        if (!string.IsNullOrWhiteSpace(sortDirection) &&
            !sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
            !sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            error = "sortDirection must be asc or desc.";
            return false;
        }

        return true;
    }

    private void Seed(
        string name,
        string? company,
        string street,
        string city,
        string? region,
        string? postalCode,
        string country,
        string? phone,
        string? email,
        string? notes,
        bool isFavorite)
    {
        Add(new CreateAddressRequest(
            name, street, city, country, company, region, postalCode, phone, email, notes, isFavorite));
    }

    private static bool Contains(string? value, string term) =>
        value is not null && value.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
