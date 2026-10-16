using System.Net.Mail;
using AddressBookApi.Data;
using AddressBookApi.Models;

namespace AddressBookApi;

public static class AddressEndpoints
{
    public static void MapAddressEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/addresses")
            .WithTags("Addresses");

        group.MapGet("/", (
            InMemoryAddressBook book,
            string? search,
            string? city,
            string? country,
            bool? favorite,
            string? sortBy,
            string? sortDirection) =>
        {
            if (!InMemoryAddressBook.TryNormalizeSort(sortBy, sortDirection, out var field, out var descending, out var error))
            {
                return Results.BadRequest(new { error });
            }

            return Results.Ok(book.Query(search, city, country, favorite, field, descending));
        })
        .WithName("SearchAddresses")
        .WithSummary("Search, filter, and sort addresses")
        .WithDescription("Filter by free-text search, city, country, and favorite. Sort by name, city, country, createdAt, updatedAt, or favorite.")
        .WithOpenApi();

        group.MapGet("/{id:guid}", (Guid id, InMemoryAddressBook book) =>
        {
            var address = book.Get(id);
            return address is null ? Results.NotFound() : Results.Ok(address);
        })
        .WithName("GetAddress")
        .WithSummary("Get one address")
        .WithOpenApi();

        group.MapPost("/", (CreateAddressRequest request, InMemoryAddressBook book) =>
        {
            var validation = ValidateRequired(request.Name, request.Street, request.City, request.Country, request.Email);
            if (validation is not null)
            {
                return validation;
            }

            var created = book.Add(request);
            return Results.Created($"/api/addresses/{created.Id}", created);
        })
        .WithName("CreateAddress")
        .WithSummary("Create an address")
        .WithOpenApi();

        group.MapPut("/{id:guid}", (Guid id, UpdateAddressRequest request, InMemoryAddressBook book) =>
        {
            var validation = ValidateRequired(request.Name, request.Street, request.City, request.Country, request.Email);
            if (validation is not null)
            {
                return validation;
            }

            var updated = book.Update(id, request);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .WithName("UpdateAddress")
        .WithSummary("Replace an address")
        .WithOpenApi();

        group.MapPatch("/{id:guid}", (Guid id, EditAddressRequest request, InMemoryAddressBook book) =>
        {
            if (request.Email is not null && !IsValidEmail(request.Email))
            {
                return Results.BadRequest(new { error = "Email is not valid." });
            }

            if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new { error = "Name cannot be empty." });
            }

            var edited = book.Edit(id, request);
            return edited is null ? Results.NotFound() : Results.Ok(edited);
        })
        .WithName("EditAddress")
        .WithSummary("Edit fields on an address")
        .WithDescription("Send only the fields to change. Omitted fields stay as they are. An empty string clears an optional field.")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", (Guid id, InMemoryAddressBook book) =>
            book.Delete(id) ? Results.NoContent() : Results.NotFound())
        .WithName("DeleteAddress")
        .WithSummary("Delete an address")
        .WithOpenApi();

        group.MapPut("/{id:guid}/favorite", (Guid id, FavoriteRequest request, InMemoryAddressBook book) =>
        {
            var updated = book.SetFavorite(id, request.IsFavorite);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        })
        .WithName("SetFavorite")
        .WithSummary("Mark or unmark an address as a favorite")
        .WithOpenApi();

        group.MapPost("/{id:guid}/favorite", (Guid id, InMemoryAddressBook book) =>
        {
            var existing = book.Get(id);
            if (existing is null)
            {
                return Results.NotFound();
            }

            var updated = book.SetFavorite(id, !existing.IsFavorite);
            return Results.Ok(updated);
        })
        .WithName("ToggleFavorite")
        .WithSummary("Toggle the favorite flag")
        .WithOpenApi();
    }

    private static IResult? ValidateRequired(string? name, string? street, string? city, string? country, string? email)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(street) ||
            string.IsNullOrWhiteSpace(city) ||
            string.IsNullOrWhiteSpace(country))
        {
            return Results.BadRequest(new { error = "Name, street, city, and country are required." });
        }

        if (!IsValidEmail(email))
        {
            return Results.BadRequest(new { error = "Email is not valid." });
        }

        return null;
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return true;
        }

        return MailAddress.TryCreate(email.Trim(), out _);
    }
}
