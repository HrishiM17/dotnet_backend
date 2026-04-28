# UserManagementAPI — TechHive Solutions

## Folder Structure

```
UserManagementAPI/
│
├── Controllers/
│   └── UsersController.cs        # All CRUD endpoints (GET, POST, PUT, DELETE)
│
├── DTOs/
│   ├── CreateUserDto.cs          # Input model for POST — includes validation
│   └── UpdateUserDto.cs          # Input model for PUT  — includes validation
│
├── Extensions/
│   └── MiddlewareExtensions.cs   # Clean extension methods for middleware registration
│
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs    # Catches all unhandled exceptions → JSON response
│   ├── AuthenticationMiddleware.cs   # Validates Bearer token → 401 if invalid
│   └── LoggingMiddleware.cs          # Logs method, path, status code, elapsed time
│
├── Models/
│   ├── User.cs                   # Core User entity
│   └── ApiResponse.cs            # Generic response wrapper for all endpoints
│
├── Services/
│   ├── IUserService.cs           # Interface (contract) for user operations
│   └── UserService.cs            # In-memory implementation (replace with DB in production)
│
├── Program.cs                    # App entry point — DI registration + middleware pipeline
├── appsettings.json              # Configuration (auth token, logging levels)
├── appsettings.Development.json  # Development overrides
└── UserManagementAPI.csproj      # Project file (targets .NET 8)
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Running the API

```bash
# 1. Navigate to the project folder
cd UserManagementAPI

# 2. Restore packages
dotnet restore

# 3. Run the project
dotnet run
```

The API will start on:
- `http://localhost:5000`
- `https://localhost:5001`

Open Swagger UI at: **http://localhost:5000/swagger**

---

## Authentication

All `/api/*` endpoints require a Bearer token.

Add this header to every request:

```
Authorization: Bearer demo-secret-token
```

In Swagger UI, click **Authorize** and enter `demo-secret-token`.

To change the token, update `appsettings.json`:
```json
"Auth": { "Token": "your-new-secret-token" }
```

---

## API Endpoints

| Method | Endpoint           | Description              |
|--------|--------------------|--------------------------|
| GET    | /api/users         | Retrieve all users       |
| GET    | /api/users/{id}    | Retrieve user by ID      |
| POST   | /api/users         | Create a new user        |
| PUT    | /api/users/{id}    | Update an existing user  |
| DELETE | /api/users/{id}    | Delete a user            |

---

## Testing with Postman

### 1. GET all users
```
GET http://localhost:5000/api/users
Authorization: Bearer demo-secret-token
```

### 2. GET user by ID
```
GET http://localhost:5000/api/users/1
Authorization: Bearer demo-secret-token
```

### 3. POST — Create a user
```
POST http://localhost:5000/api/users
Authorization: Bearer demo-secret-token
Content-Type: application/json

{
  "name": "Diana Prince",
  "email": "diana@techhive.com",
  "role": "Manager"
}
```

### 4. PUT — Update a user
```
PUT http://localhost:5000/api/users/1
Authorization: Bearer demo-secret-token
Content-Type: application/json

{
  "name": "Alice Smith",
  "email": "alice.smith@techhive.com",
  "role": "Admin"
}
```

### 5. DELETE — Delete a user
```
DELETE http://localhost:5000/api/users/3
Authorization: Bearer demo-secret-token
```

### 6. Test 401 Unauthorized (no token)
```
GET http://localhost:5000/api/users
(no Authorization header)
```

### 7. Test validation (bad data)
```
POST http://localhost:5000/api/users
Authorization: Bearer demo-secret-token
Content-Type: application/json

{
  "name": "",
  "email": "not-an-email",
  "role": ""
}
```
Expected: 400 Bad Request with validation errors.

---

## Middleware Pipeline Order

Middleware is registered in this order in `Program.cs` (outermost → innermost):

```
1. ErrorHandlingMiddleware   ← wraps everything, catches unhandled exceptions
2. AuthenticationMiddleware  ← validates Bearer token
3. LoggingMiddleware         ← logs request/response details
4. Controllers               ← your actual endpoint logic
```

---