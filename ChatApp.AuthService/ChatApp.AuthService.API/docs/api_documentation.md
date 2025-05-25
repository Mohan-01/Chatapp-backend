# Authentication Microservice API Reference

This document provides detailed information about the authentication microservice API endpoints for the Real-Time Chat Application.

- **Version:** v1
- **License:** [MIT License](https://opensource.org/licenses/MIT)
- **Contact:** MVK Seshu M | [GitHub](https://github.com/mvk-seshu-m) | [Email](mailto:mmvkseshu@gmail.com)

## Security

All authenticated endpoints require JWT Bearer authentication.

Include your JWT token in the Authorization header as follows:
```
Authorization: Bearer {your_token}
```

## Endpoints

### User Registration and Authentication

#### Get Available Roles
Retrieves available user roles in the system.

- **URL:** `/api/Auth/roles`
- **Method:** GET
- **Authentication:** Required
- **Response:** 200 OK - List of available roles

---

#### Register New User
Creates a new user account.

- **URL:** `/api/Auth/register`
- **Method:** POST
- **Authentication:** Not required
- **Request Body:**
  ```json
  {
    "username": "string",     // Required
    "email": "string",        // Required
    "password": "string"      // Required
  }
  ```
- **Response:** 200 OK - User created successfully

---

#### User Login
Authenticates a user and issues a JWT token.

- **URL:** `/api/Auth/login`
- **Method:** POST
- **Authentication:** Not required
- **Request Body:**
  ```json
  {
    "username": "string",     // Required
    "password": "string"      // Required
  }
  ```
- **Response:** 200 OK - Returns authentication token and user information

---

#### User Logout
Invalidates the current user's session.

- **URL:** `/api/Auth/logout`
- **Method:** GET
- **Authentication:** Required
- **Response:** 200 OK - Successfully logged out

---

#### Authenticate User
Verifies the current user's authentication status.

- **URL:** `/api/Auth/authenticate-user`
- **Method:** GET
- **Authentication:** Required
- **Response:** 200 OK - Returns current user information

---

#### Validate Token
Validates whether a given token is valid.

- **URL:** `/api/Auth/validate-token`
- **Method:** POST
- **Authentication:** Not required
- **Request Body:**
  ```json
  {
    "token": "string"     // Required
  }
  ```
- **Response:** 200 OK - Token validation result

### Account Recovery

#### Forgot Username
Sends a username reminder to the user's registered email.

- **URL:** `/api/Auth/forgot-username`
- **Method:** POST
- **Authentication:** Not required
- **Request Body:**
  ```json
  {
    "email": "string"     // Required
  }
  ```
- **Response:** 200 OK - Username sent to the provided email

---

#### Forgot Password
Initiates the password reset process by sending a reset token to the user's email.

- **URL:** `/api/Auth/forgot-password`
- **Method:** POST
- **Authentication:** Not required
- **Request Body:**
  ```json
  {
    "email": "string"     // Required
  }
  ```
- **Response:** 200 OK - Password reset instructions sent to email

---

#### Reset Password
Resets a user's password using a valid reset token.

- **URL:** `/api/Auth/reset-password`
- **Method:** POST
- **Authentication:** Not required
- **Request Body:**
  ```json
  {
    "resetToken": "string",     // Required
    "newPassword": "string"     // Required
  }
  ```
- **Response:** 200 OK - Password reset successful

### Account Management

#### Change Username
Updates the current user's username.

- **URL:** `/api/Auth/change-username`
- **Method:** PUT
- **Authentication:** Required
- **Request Body:**
  ```json
  {
    "newUsername": "string"     // Required
  }
  ```
- **Response:** 200 OK - Username updated successfully

---

#### Update Email
Updates the current user's email address.

- **URL:** `/api/Auth/update-email`
- **Method:** PUT
- **Authentication:** Required
- **Request Body:**
  ```json
  {
    "newEmail": "string"     // Required
  }
  ```
- **Response:** 200 OK - Email updated successfully

---

#### Change Password
Updates the current user's password.

- **URL:** `/api/Auth/change-password`
- **Method:** PUT
- **Authentication:** Required
- **Request Body:**
  ```json
  {
    "newPassword": "string"     // Required
  }
  ```
- **Response:** 200 OK - Password changed successfully

---

#### Delete User
Deletes the current user's account.

- **URL:** `/api/Auth/delete-user`
- **Method:** DELETE
- **Authentication:** Required
- **Response:** 200 OK - User account deleted successfully

## Error Responses

All API endpoints may return the following error responses:

- **400 Bad Request** - Invalid request format or missing required fields
- **401 Unauthorized** - Authentication failed or token expired
- **403 Forbidden** - User does not have permission for the requested operation
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server-side error

## Models

### RegisterRequestDto
```json
{
  "username": "string",  // Required
  "email": "string",     // Required
  "password": "string"   // Required
}
```

### LoginRequestDto
```json
{
  "username": "string",  // Required
  "password": "string"   // Required
}
```

### ForgotUsernameRequestDto
```json
{
  "email": "string"      // Required
}
```

### ForgotPasswordRequestDto
```json
{
  "email": "string"      // Required
}
```

### ResetPasswordRequestDto
```json
{
  "resetToken": "string",  // Required
  "newPassword": "string"  // Required
}
```

### ChangeUsernameRequestDto
```json
{
  "newUsername": "string"  // Required
}
```

### UpdateEmailRequestDto
```json
{
  "newEmail": "string"     // Required
}
```

### ChangePasswordRequestDto
```json
{
  "newPassword": "string"  // Required
}
```

### ValidateTokenRequest
```json
{
  "token": "string"        // Required
}
```

## Integrating with Other Microservices

The Authentication Microservice is designed to work seamlessly with other microservices in the Real-Time Chat Application. Other services should use the `/api/Auth/validate-token` endpoint to verify the validity of authentication tokens received from clients.

## Rate Limiting

The API implements rate limiting to prevent abuse. Clients should implement appropriate retry logic with exponential backoff when encountering 429 Too Many Requests responses.

## API Versioning

The current API version is v1. Future versions will be accessible through updated URL paths (e.g., `/api/v2/Auth/login`).
