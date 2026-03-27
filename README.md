# Helsi Task Lists API

Simple task lists management API.

## How to Run

1. Install MongoDB (or run via Docker):
   ```bash
   docker run -d --name mongo -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=admin -e MONGO_INITDB_ROOT_PASSWORD=admin123 mongo:latest
   ```
2. Clone this repository.
3. Run the application:
   ```bash
   cd src/Helsi.Api
   dotnet run
   ```
4. Open Swagger: **http://localhost:5118/swagger**

For testing, you can use a GUID generator: https://go.lightnode.com/resources/guid-uuid-generator

## Comments for Reviewer

- Used **.NET 10** since Oleg mentioned the team is planning to migrate to this version soon, and it's also what we use on the current project.
- **MongoDB** as the data store — I hadn't worked with it much before, but it was listed as the preferred option in the task, so I took it as a chance to pick up something new quickly. The document model turned out to be a good fit anyway: a task list with an embedded array of shared user IDs, no joins needed.
- **FluentValidation** for request validation — keeps validation rules separate from controllers and easy to unit test. I prefer it over `DataAnnotations` because the rules are more readable and composable.
- **NUnit + NSubstitute** for unit tests — added tests for domain validation, access policy rules, and all service-layer operations to make sure the core logic actually works.
- **Swashbuckle** for Swagger UI — convenient for manual testing without Postman.
- Layered architecture: **Domain** → **Application** → **Infrastructure** / **Api**. Interfaces (`ITaskListRepository`, `ITaskListService`) live in Application, so the DB or transport can be swapped without touching business logic.
- Repository pattern for data access, dedicated `TaskListAccessPolicy` for authorization rules, `GlobalExceptionMiddleware` for consistent error responses via `ProblemDetails`.
- `userId` is passed via header to simulate authorization without implementing a full auth system. In a real project this would come from a JWT token.
- Some things I'd skip for a test task but would add in production: Serilog, Mapster, Docker Compose, integration tests with Testcontainers.
