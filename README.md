# ITrade

## React app notes:
- All authenticated endpoints automatically include the JWT token in the Authorization header
- Tokens are refreshed automatically when they expire
- On authentication failure, the app redirects to `/login`
- Date fields should be ISO 8601 strings
- API requests use relative paths (`/api`) which are proxied to the backend in both development (Vite) and production (Nginx)
- To bypass the proxy and connect directly to backend, set `VITE_API_BASE_URL` to an absolute URL like `https://localhost:7254/api`