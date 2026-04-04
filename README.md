# ITrade

This project is structured using **.NET Aspire** for local orchestration, deploying and managing the API, frontend app, and database as a unified distributed application.

## Architecture
- **Frontend**: React 19 application (`ITrade.UserClient/itrade`) using React Router and Tailwind CSS v4. Managed via Vite in development and Nginx in production/containers.
- **Backend**: .NET 9+ API (`ITrade.ApiServices`).
- **Database**: PostgreSQL (provisioned automatically via .NET Aspire with Azure PostgreSQL Flexible Server support; includes PgWeb in local run mode).
- **AppHost**: .NET Aspire orchestration project (`ITrade.AppHost`).

## Key Backend Features
- **User Authentication**: Secure JWT-based access with Identity `PasswordHasher`, and automatic token refresh logic.
- **MailJet Integration**: Outbound email service powered by MailJet, utilizing scoped template configurations.
- **Database Initializer**: Built-in logic to execute migrations and seed developmental data seamlessly upon startup in Dev mode.
- **Matching & Searching Modules**: Advanced built-in services for handling requests, projects, user tags matching, and targeted discovery.
- **Global Error Handling**: Custom exception middleware integrated into the .NET Problem Details framework.
- **API Documentation**: Built-in OpenAPI specification with Scalar UI integration (accessible in Development).

## Configuration & Secrets
When running locally via `ITrade.AppHost`, Aspire orchestrates the environment variables. You can configure required secrets via `dotnet user-secrets` in the `ITrade.AppHost` project directory:
- `pg-user` / `pg-pass`: PostgreSQL admin credentials.
- `Database:MigrateOnStartup` (or `Database__MigrateOnStartup`): Set to run migrations automatically.
- `Urls:ApiBase` (or `Urls__ApiBase`): Base URL for the API.
- `Jwt:Secret` (or `Jwt__Secret`): Secret key for JWT token generation.
- `MailJet:Key` (or `MailJet__Key`): MailJet API Key.
- `MailJet:Secret` (or `MailJet__Secret`): MailJet API Secret.

## React app notes:
- Uses `axios` for HTTP client handling and built-in interceptors.
- All authenticated endpoints automatically include the JWT token in the Authorization header.
- Tokens are refreshed automatically when they expire.
- On authentication failure, the app redirects to `/login`.
- Date fields should be ISO 8601 strings.
- API requests use relative paths (`/api`) which are proxied to the backend in both development (Vite) and production (Nginx). The backend URL is automatically injected by Aspire via the `BACKEND_URL` environment variable.