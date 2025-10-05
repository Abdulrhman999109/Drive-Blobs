# Drive Blobs API

Simple API to store and get files (blobs).
Supports **Local**, **Database**, **S3**, and **FTP** backends.
All routes require a **JWT token**.

---

## Run

```bash
dotnet build
dotnet ef database update
dotnet run
```

If you use Docker for MinIO or FTP, create and configure the containers manually before starting the server.
You can use SQL Server Management Studio database.

---

## API

### POST /v1/blobs

Upload a new blob.

**Body example:**

```json
{
  "id": "any_valid_string_or_identifier",
  "dataBase64": "SGVsbG8gU2ltcGxlIFN0b3JhZ2UgV29ybGQh",
  "backend": "local"    // "db" | "s3" | "ftp"
}
```

---

### GET /v1/blobs/:id

Get blob by ID.

---
### GET /v1/GetToken

Get Token.

---

## Testing


```bash
dotnet test
```

Run specific integration tests :

```bash
dotnet test --filter "FullyQualifiedName=Drive_project.Tests.Integration.BlobDbTests.CreateBlob_ShouldReturnCreated"```

---
