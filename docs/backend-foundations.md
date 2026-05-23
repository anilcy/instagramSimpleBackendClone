# Backend Foundations: Auth, Tokens, Sessions, Security, Global Errors

This document is a memory-friendly guide for building API backends correctly with the same patterns used in this project.

## 1) Where global error handling fits

Request flow (simplified):

1. Client sends HTTP request.
2. ASP.NET middleware pipeline runs.
3. Authentication middleware validates token and sets `HttpContext.User`.
4. Authorization checks endpoint access rules (`[Authorize]`, policies).
5. Controller/service/repository logic runs.
6. Response is returned.

Global error handling middleware wraps the whole flow and catches exceptions from any layer.

Why it helps in daily usage:

1. Clients get consistent error JSON instead of random stack traces or HTML error pages.
2. Frontend/mobile can reliably map status codes (`400`, `401`, `403`, `404`, `409`, `500`).
3. Logs include request path + trace id for faster debugging in production.

## 2) Authentication vs Authorization

- Authentication: "Who are you?"
  - Implemented with JWT validation.
  - Result: `HttpContext.User` contains claims (user id, email, roles).
- Authorization: "Are you allowed to do this?"
  - Implemented with `[Authorize]`, roles, or business checks (owner/follower/private account logic).
  - Result: request is allowed or blocked.

Mental model:

1. Token missing/invalid -> usually `401 Unauthorized`.
2. Token valid but action not allowed -> `403 Forbidden`.

## 3) Access tokens and refresh tokens (session management)

Recommended simple architecture:

1. Short-lived access token (e.g. 10-15 minutes) for API calls.
2. Long-lived refresh token stored server-side per session/device.
3. Refresh endpoint rotates refresh token (old one invalidated).
4. Logout endpoint revokes current refresh token.
5. "Logout all devices" revokes all refresh tokens for that user.

Each refresh token row should include:

1. `TokenHash` (never store raw token in DB).
2. `UserId`.
3. `CreatedAt`, `ExpiresAt`, `RevokedAt`.
4. `ReplacedByTokenHash` (for rotation chain).
5. Optional: `DeviceName`, `IpAddress`, `UserAgent`.

Benefits:

1. Real session control (per device).
2. Stolen old refresh token can be detected and revoked.
3. You can show active sessions to users.

## 4) Security baseline checklist

Apply these by default:

1. Keep JWT secret out of code and git (`.env`, secret manager, CI variables).
2. Use HTTPS everywhere outside local dev.
3. Validate input with DTO annotations and business checks.
4. Use strict CORS (allow only known frontend origins in production).
5. Limit upload file size and whitelist allowed extensions/mime types.
6. Add rate limiting for auth, comments, likes, follow actions.
7. Return generic auth errors (do not leak whether email exists).
8. Log security-sensitive events (failed login bursts, token refresh anomalies).
9. Keep packages updated and avoid unnecessary dependencies per layer.
10. Rotate secrets when compromised.

## 5) Exception-to-HTTP mapping in this project

Current middleware maps:

1. `ArgumentException` -> `400 Bad Request`
2. `UnauthorizedAccessException` -> `403 Forbidden`
3. `InvalidOperationException` -> `409 Conflict`
4. `KeyNotFoundException` -> `404 Not Found`
5. Any other exception -> `500 Internal Server Error`

Use this rule:

1. Throw meaningful exception types in services.
2. Keep controllers thin.
3. Let middleware build the final error response.

## 6) How to repeat this in another project

1. Add one global exception middleware first.
2. Configure JWT once via options object (single source of truth).
3. Add access token auth.
4. Add refresh token table + rotation (session management).
5. Add rate limits + upload hardening + strict CORS.
6. Add integration tests for auth flows and error contracts.

If you forget everything later, remember this short formula:

`AuthN -> AuthZ -> Business rules -> Global error mapping -> Consistent API contract`.
