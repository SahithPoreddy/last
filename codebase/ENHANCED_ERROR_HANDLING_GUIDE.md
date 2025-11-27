# Enhanced Exception Handling & Validation Guide

## Overview
This document describes the enhanced exception handling middleware that provides detailed, semantic error responses for various error scenarios including validation errors, missing credentials, and missing fields.

---

## ? What Was Enhanced

### 1. Exception Handling Middleware
- ? FluentValidation error handling
- ? Custom validation error handling
- ? Missing credentials detection
- ? Missing required fields handling
- ? Detailed error responses with hints
- ? Field-level validation errors
- ? Better logging for different error types

### 2. Custom Exceptions
- ? `ValidationException` - Field-level validation errors
- ? `MissingCredentialsException` - Missing authentication
- ? Enhanced all existing exceptions

---

## ?? Supported Error Scenarios

### 1. **Validation Errors** (400 Bad Request)

#### FluentValidation Errors
**Triggered by:** Invalid request data that fails FluentValidation rules

**Example Request:**
```http
POST /api/products
Content-Type: application/json

{
  "name": "",
  "startingPrice": -10,
  "auctionDuration": 1
}
```

**Response:**
```json
{
  "error": "Validation failed",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "errors": {
    "Name": ["Product name is required"],
    "StartingPrice": ["Starting price must be greater than 0"],
    "AuctionDuration": ["Auction duration must be between 2 minutes and 24 hours (1440 minutes)"]
  }
}
```

#### Custom Validation Errors
**Triggered by:** Business logic validation failures

**Example Response:**
```json
{
  "error": "Validation failed",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "errors": {
    "Amount": ["Bid amount must be greater than current highest bid of $1000"],
    "AuctionId": ["Auction is no longer active"]
  }
}
```

---

### 2. **Missing Credentials** (401 Unauthorized)

#### No Token Provided
**Triggered by:** Request without Authorization header

**Response:**
```json
{
  "error": "Authentication credentials are missing or invalid",
  "statusCode": 401,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "hint": "Please provide authentication credentials in the Authorization header"
}
```

#### Invalid/Expired Token
**Triggered by:** Invalid or expired JWT token

**Response:**
```json
{
  "error": "Authentication required",
  "statusCode": 401,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "message": "Please provide a valid authentication token",
  "hint": "Include 'Authorization: Bearer <token>' header in your request"
}
```

---

### 3. **Missing Required Fields** (400 Bad Request)

#### ArgumentNullException
**Triggered by:** Required parameter is null

**Example Response:**
```json
{
  "error": "Missing required field",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "field": "productId",
  "message": "The field 'productId' is required"
}
```

#### Custom Missing Field
**Example in code:**
```csharp
if (string.IsNullOrEmpty(request.Email))
{
    throw new ValidationException("Email", "Email address is required");
}
```

**Response:**
```json
{
  "error": "Validation failed",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "errors": {
    "Email": ["Email address is required"]
  }
}
```

---

### 4. **Invalid Input** (400 Bad Request)

#### ArgumentException
**Triggered by:** Invalid argument value

**Response:**
```json
{
  "error": "Invalid input",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "message": "Page number must be greater than 0"
}
```

---

### 5. **Resource Not Found** (404 Not Found)

#### NotFoundException
**Triggered by:** Resource doesn't exist

**Response:**
```json
{
  "error": "Product not found",
  "statusCode": 404,
  "timestamp": "2024-01-15T10:30:45.123Z"
}
```

#### KeyNotFoundException
**Triggered by:** Dictionary/collection key not found

**Response:**
```json
{
  "error": "Resource not found",
  "statusCode": 404,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "message": "The requested item does not exist"
}
```

---

### 6. **Unauthorized Access** (401 Unauthorized)

#### UnauthorizedException
**Triggered by:** Invalid credentials during login

**Response:**
```json
{
  "error": "Invalid email or password",
  "statusCode": 401,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "hint": "Please provide valid authentication credentials"
}
```

---

### 7. **Forbidden Access** (403 Forbidden)

#### ForbiddenException
**Triggered by:** Insufficient permissions

**Response:**
```json
{
  "error": "You are not authorized to perform this action",
  "statusCode": 403,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "hint": "You do not have permission to access this resource"
}
```

---

### 8. **Conflict** (409 Conflict)

#### ConflictException
**Triggered by:** Duplicate or conflicting data

**Response:**
```json
{
  "error": "User with this email already exists",
  "statusCode": 409,
  "timestamp": "2024-01-15T10:30:45.123Z"
}
```

---

### 9. **Invalid Operation** (400 Bad Request)

#### InvalidOperationException
**Triggered by:** Operation not allowed in current state

**Response:**
```json
{
  "error": "Invalid operation",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "message": "Cannot update product with active bids"
}
```

---

### 10. **Server Error** (500 Internal Server Error)

#### Unhandled Exceptions
**Triggered by:** Unexpected server errors

**Response:**
```json
{
  "error": "An unexpected error occurred",
  "statusCode": 500,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "message": "Please contact support if the problem persists"
}
```

---

## ?? How to Use Custom Exceptions in Code

### 1. ValidationException with Multiple Fields

```csharp
var errors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required", "Email format is invalid" } },
    { "Password", new[] { "Password must be at least 6 characters" } }
};
throw new ValidationException(errors);
```

### 2. ValidationException with Single Field

```csharp
throw new ValidationException("ProductName", "Product name cannot be empty");
```

### 3. MissingCredentialsException

```csharp
if (string.IsNullOrEmpty(token))
{
    throw new MissingCredentialsException("Authorization token is missing");
}
```

### 4. BadRequestException

```csharp
if (amount <= 0)
{
    throw new BadRequestException("Bid amount must be greater than 0");
}
```

### 5. NotFoundException

```csharp
var product = await _productRepository.GetByIdAsync(id);
if (product == null)
{
    throw new NotFoundException($"Product with ID {id} not found");
}
```

### 6. UnauthorizedException

```csharp
if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
{
    throw new UnauthorizedException("Invalid email or password");
}
```

### 7. ForbiddenException

```csharp
if (auction.Product.OwnerId != userId)
{
    throw new ForbiddenException("You are not authorized to modify this product");
}
```

### 8. ConflictException

```csharp
var existingUser = await _userRepository.GetByEmailAsync(email);
if (existingUser != null)
{
    throw new ConflictException("User with this email already exists");
}
```

---

## ?? Error Response Structure

### Standard Error Response
All error responses follow this structure:
```json
{
  "error": "Error summary",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:45.123Z"
}
```

### Validation Error Response
Validation errors include field-level details:
```json
{
  "error": "Validation failed",
  "statusCode": 400,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "errors": {
    "FieldName1": ["Error message 1", "Error message 2"],
    "FieldName2": ["Error message 3"]
  }
}
```

### Error Response with Hints
Some errors include helpful hints:
```json
{
  "error": "Authentication required",
  "statusCode": 401,
  "timestamp": "2024-01-15T10:30:45.123Z",
  "message": "Additional context",
  "hint": "Helpful suggestion for the user"
}
```

---

## ?? Testing Error Scenarios

### 1. Test Validation Errors

**Invalid Product Creation:**
```bash
curl -X POST https://localhost:7019/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "",
    "startingPrice": -10
  }'
```

**Expected Response:** `400 Bad Request` with field-level errors

---

### 2. Test Missing Credentials

**Request Without Token:**
```bash
curl -X GET https://localhost:7019/api/dashboard
```

**Expected Response:** `401 Unauthorized` with authentication hint

---

### 3. Test Missing Fields

**Incomplete Request:**
```bash
curl -X POST https://localhost:7019/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "iPhone 15"
  }'
```

**Expected Response:** `400 Bad Request` listing missing required fields

---

### 4. Test Resource Not Found

**Non-existent Product:**
```bash
curl -X GET https://localhost:7019/api/products/99999 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Expected Response:** `404 Not Found`

---

### 5. Test Forbidden Access

**User tries Admin endpoint:**
```bash
curl -X DELETE https://localhost:7019/api/products/1 \
  -H "Authorization: Bearer USER_TOKEN"
```

**Expected Response:** `403 Forbidden`

---

## ?? Logging Levels

Different errors are logged at appropriate levels:

| Error Type | Log Level | Reason |
|------------|-----------|--------|
| Validation Errors | Warning | Expected user input errors |
| Missing Credentials | Warning | Authentication issues |
| Not Found | Warning | Resource doesn't exist |
| Unauthorized | Warning | Invalid credentials |
| Forbidden | Warning | Permission issues |
| Conflict | Warning | Duplicate data |
| Missing Fields | Error | Potential code issue |
| Invalid Arguments | Error | Potential code issue |
| Unhandled Exceptions | Error | Unexpected server errors |

---

## ?? Frontend Integration

### TypeScript Error Handling

```typescript
interface ApiError {
  error: string;
  statusCode: number;
  timestamp: string;
  errors?: { [field: string]: string[] };
  message?: string;
  hint?: string;
  field?: string;
}

async function createProduct(data: CreateProductRequest) {
  try {
    const response = await fetch('/api/products', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      const error: ApiError = await response.json();
      
      if (error.errors) {
        // Handle validation errors
        Object.keys(error.errors).forEach(field => {
          console.error(`${field}: ${error.errors[field].join(', ')}`);
        });
      } else if (error.hint) {
        // Show hint to user
        console.error(`${error.error}: ${error.hint}`);
      } else {
        // Generic error
        console.error(error.error);
      }
    }
    
    return await response.json();
  } catch (error) {
    console.error('Network error:', error);
  }
}
```

### Angular Error Handling

```typescript
import { HttpErrorResponse } from '@angular/common/http';

handleError(error: HttpErrorResponse) {
  if (error.error?.errors) {
    // Validation errors
    const validationErrors = error.error.errors;
    Object.keys(validationErrors).forEach(field => {
      this.form.get(field)?.setErrors({
        serverError: validationErrors[field].join(', ')
      });
    });
  } else if (error.status === 401) {
    // Unauthorized
    this.router.navigate(['/login']);
    this.snackBar.open(error.error?.message || 'Authentication required', 'Close');
  } else if (error.error?.hint) {
    // Error with hint
    this.snackBar.open(`${error.error.error}: ${error.error.hint}`, 'Close');
  } else {
    // Generic error
    this.snackBar.open(error.error?.error || 'An error occurred', 'Close');
  }
}
```

---

## ? Benefits of Enhanced Error Handling

1. **Clear Error Messages** - Users know exactly what went wrong
2. **Field-Level Validation** - Frontend can highlight specific fields
3. **Helpful Hints** - Suggestions for resolving errors
4. **Better Logging** - Different log levels for different error types
5. **Structured Responses** - Consistent JSON format
6. **Developer Friendly** - Easy to parse and handle in code
7. **Security** - No sensitive information leaked in errors
8. **User Experience** - Actionable error messages

---

## ?? Debugging Tips

### Check Logs
```bash
# Backend logs show detailed error information
[Warning] Validation failed: Product name is required, Starting price must be greater than 0
[Error] Missing required argument: productId
[Error] An unhandled error occurred: Object reference not set to an instance
```

### Swagger Testing
1. Open Swagger UI
2. Try invalid requests
3. See formatted error responses
4. Check response status codes

### Postman Collection
Create tests for different error scenarios:
- Missing required fields
- Invalid data types
- Missing authentication
- Invalid permissions
- Non-existent resources

---

## ?? Summary

### Enhanced Exception Types
- ? FluentValidation errors
- ? Custom validation errors
- ? Missing credentials
- ? Missing required fields
- ? Invalid arguments
- ? Resource not found
- ? Unauthorized access
- ? Forbidden access
- ? Conflicts
- ? Invalid operations
- ? Server errors

### Response Features
- ? Semantic error messages
- ? Field-level validation details
- ? Helpful hints and suggestions
- ? Consistent JSON structure
- ? Timestamps
- ? Status codes
- ? Detailed logging

---

**Your API now provides professional-grade error handling with clear, actionable error messages!** ?
