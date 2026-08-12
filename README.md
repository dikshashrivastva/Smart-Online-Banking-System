# SmartBank 

## Project Structure

```
SmartBank/
├── SmartBank.sln
├── SmartBankDB_Sprint1.sql          ← Run this first in SSMS
│
├── SmartBank.Models/                ← Entities + DTOs (shared)
│   ├── Entities/
│   │   ├── Role.cs
│   │   └── User.cs
│   └── DTOs/Auth/
│       └── AuthDtos.cs
│
├── SmartBank.Data/                  ← EF Core DbContext + Repositories
│   ├── Context/
│   │   └── SmartBankDbContext.cs
│   └── Repositories/
│       ├── IAuthRepository.cs
│       └── AuthRepository.cs
│
├── SmartBank.API/                   ← ASP.NET Core Web API (JWT)
│   ├── Controllers/AuthController.cs
│   ├── Services/AuthService.cs
│   ├── Helpers/JwtHelper.cs
│   ├── Middleware/GlobalExceptionMiddleware.cs
│   └── appsettings.json
│
└── SmartBank.MVC/                   ← ASP.NET Core MVC (UI)
    ├── Controllers/AuthController.cs
    ├── Views/Auth/
    │   ├── Login.cshtml
    │   └── Register.cshtml
    ├── Views/Shared/_Layout.cshtml
    └── appsettings.json
```


| Method | Endpoint              | Auth   | Description                  |
|--------|-----------------------|--------|------------------------------|
| POST   | /api/auth/register    | Public | Register new customer        |
| POST   | /api/auth/login       | Public | Login → returns JWT token    |
| GET    | /api/auth/me          | JWT    | Test protected endpoint      |

## MVC Pages

| Page      | URL             | Description                   |
|-----------|-----------------|-------------------------------|
| Login     | /Auth/Login     | Login form → stores JWT cookie |
| Register  | /Auth/Register  | Registration form              |

## Setup Instructions

### Step 1 – Database
1. Open **SQL Server Management Studio (SSMS)**
2. Open and run `SmartBankDB_Sprint1.sql`
3. This creates `SmartBankDB`, all tables, seeds 4 roles, and an admin user

### Step 2 – API Project
1. Open `SmartBank.sln` in **Visual Studio 2022**
2. Update `SmartBank.API/appsettings.json` connection string if needed
3. Run EF Core migrations:
   ```bash
   cd SmartBank.API
   dotnet ef migrations add InitialCreate --project ../SmartBank.Data
   dotnet ef database update
   ```
   *(Or just use the SQL script from Step 1)*
4. Set **SmartBank.API** as startup project → Run (F5)
5. Swagger available at: `https://localhost:7200/swagger`

### Step 3 – MVC Project
1. Set **SmartBank.MVC** as startup project → Run (F5)
2. App opens at `https://localhost:7100`
3. Login page loads by default

### Step 4 – Test in Swagger
```json
POST /api/auth/register
{
  "firstName": "Test",
  "lastName": "User",
  "email": "test@example.com",
  "password": "Test@1234",
  "confirmPassword": "Test@1234"
}
```
```json
POST /api/auth/login
{
  "email": "test@example.com",
  "password": "Test@1234"
}
```
Copy the returned `token` → click **Authorize** in Swagger → paste `Bearer <token>`

Then test `GET /api/auth/me` — should return 200 with your user info.

## Checklist
- [x] Register API (`POST /api/auth/register`)
- [x] Login API (`POST /api/auth/login`)
- [x] JWT token generation with role claims
- [x] Role seeding (Admin, Manager, Staff, Customer)
- [x] Password hashing with BCrypt
- [x] Failed login attempt tracking (locks at 5 attempts)
- [x] Account freeze check on login
- [x] Protected endpoint (`GET /api/auth/me`)
- [x] Global exception middleware
- [x] Swagger with JWT auth header
- [x] MVC Login page
- [x] MVC Register page
- [x] JWT stored in HTTP-only cookie

## Seeded Admin Credentials
- **Email:** admin@smartbank.com
- **Password:** Admin@1234

## Security Notes
- Passwords hashed with **BCrypt** (cost factor 11)
- JWT expires in **24 hours**
- Failed login attempts reset on success, lock at **5 attempts**
- Change `JwtSettings:SecretKey` before production!
