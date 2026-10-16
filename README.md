# Address Book API

Source code: https://github.com/Berat-Guclu/address-book-api

A small REST API for an address book. There is no frontend. Swagger is the client. Contacts are stored in process memory.

## Run

```bash
dotnet run
```

Swagger is at http://localhost:5044/swagger. The site root redirects there. `GET /health` returns `{ "status": "ok" }`.

## Design

The project starts from the ASP.NET Core 8 Web API template and replaces the sample weather endpoint with address-book routes. Endpoints are minimal APIs in `AddressEndpoints.cs`, grouped under `/api/addresses`.

| Method | Path | Purpose |
| --- | --- | --- |
| GET | `/api/addresses` | Search, filter, and sort |
| GET | `/api/addresses/{id}` | Read one address |
| POST | `/api/addresses` | Create |
| PUT | `/api/addresses/{id}` | Replace the whole address |
| PATCH | `/api/addresses/{id}` | Edit only the fields that were sent |
| DELETE | `/api/addresses/{id}` | Delete |
| PUT | `/api/addresses/{id}/favorite` | Set the favorite flag |
| POST | `/api/addresses/{id}/favorite` | Toggle the favorite flag |

`GET /api/addresses` accepts `search`, `city`, `country`, `favorite`, `sortBy`, and `sortDirection`. Search matches name, company, street, city, region, postal code, country, phone, email, and notes. `sortBy` can be `name`, `city`, `country`, `createdAt`, `updatedAt`, or `favorite`. `sortDirection` is `asc` or `desc`.

Each address has an id, name, optional company, street, city, optional region and postal code, country, optional phone and email, notes, a favorite flag, and created/updated timestamps. Three sample contacts are loaded when the process starts.

The store is a `ConcurrentDictionary` registered as a singleton (`Data/InMemoryAddressBook.cs`). Swagger is generated with Swashbuckle and stays enabled outside Development so a hosted process can still show `/swagger`. `Program.cs` listens on the `PORT` environment variable when a host sets one. `Dockerfile` and `render.yaml` are included so the same app can be built as a container.

## Assumptions

- One process owns the data. A second instance, or a restart, starts from the three seed contacts again.
- There is no login. Anyone who can reach the API can change the book.
- Name, street, city, and country are required. Email is optional, and when it is present it must be a valid address.
- An empty string on a partial edit clears an optional field. Omitted fields stay as they are.
- Search and city/country filters are case-insensitive. Sort text uses ordinary string order.
- The API is the whole product. No web page was built besides Swagger.

## Issues

- No Issues

