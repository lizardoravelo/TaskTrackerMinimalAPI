# TaskTracker Minimal API

A clean and modern REST API built using **.NET 8 Minimal APIs**, featuring:

- 🔐 Auth0 JWT Authentication
- 🧭 Swagger/OpenAPI with endpoint tagging
- 💡 Modular route design via extension methods
- 🚀 EF Core 8 with MS SQL Server
- ⚙️ AutoMapper-based DTO mapping
- 🛡️ Rate limiting with token bucket algorithm
- 🌍 CORS configured for frontend access

> Designed for scalability, clarity, and real-world production readiness.

---

## 📁 Project Structure

```
TaskTrackerMinimalAPI/
├── Program.cs
├── Extensions/
├── Models/
├── DTOs/
├── Data/
├── Mappings/
└── README.md
```

---

## 🛠️ Technologies

- **.NET 8**
- **Minimal API**
- **Entity Framework Core 8**
- **Auth0** for authentication
- **SQL Server**
- **Swagger (Swashbuckle)**
- **AutoMapper**
- **Rate Limiting** (Token Bucket)
- **CORS**

---

## 🔐 Authentication

All endpoints are protected using **Auth0 JWT Bearer tokens**.

- The JWT must be included in the `Authorization: Bearer <token>` header.
- Token configuration is located in `appsettings.json`.

---

## 🧪 API Endpoints

See [Swagger UI](https://localhost:port/swagger) for full interactive documentation.

> Endpoints are grouped using `.WithTags("Projects")`, `.WithTags("Tasks")` for readability.

Example:
```
GET     /api/projects
POST    /api/projects
GET     /api/projects/{id}
DELETE  /api/projects/{id}

GET     /api/projects/{projectId}/tasks
POST    /api/projects/{projectId}/tasks
```

---

## 🚦 Rate Limiting

Implemented using `TokenBucketRateLimiter` to prevent abuse.

- Configured per IP
- Limits are customizable via `RateLimiterOptions`

---

## 🔄 How to Run Locally

1. **Clone the repository**
2. Update `appsettings.json` with your SQL Server and Auth0 settings
3. Run migrations:
   ```bash
   dotnet ef database update
   ```
4. Start the API:
   ```bash
   dotnet run
   ```

---

## 📌 TODO (Next Improvements)

- ✅ CORS
- ✅ Swagger Tags & Summaries
- ✅ Auth0 Middleware
- ✅ Rate Limiting
- ⬜ FluentValidation (Coming)
- ⬜ Global Error Handling
- ⬜ Service Layer Abstraction
- ⬜ Unit & Integration Tests

---

## 👨‍💻 Author
**Julio Lizardo**  