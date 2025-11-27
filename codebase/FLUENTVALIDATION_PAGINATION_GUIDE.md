# FluentValidation & Pagination Implementation Guide

## Overview
This document describes the implementation of **FluentValidation** for semantic error messages and **Pagination** for all GET endpoints in the BidSphere API.

---

## ? What Was Implemented

### 1. FluentValidation
- ? Installed `FluentValidation.AspNetCore` (v11.3.1)
- ? Created validators for all request DTOs
- ? Configured automatic validation in Program.cs
- ? Removed old Data Annotations
- ? Semantic, descriptive error messages

### 2. Pagination
- ? Created `PagedResult<T>` wrapper
- ? Created `PaginationParams` model
- ? Implemented pagination in all list endpoints
- ? Query parameter support (`?pageNumber=1&pageSize=10`)

---

## ?? FluentValidation Implementation

### Installed Package
```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
```

### Configuration (Program.cs)
```csharp
// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

### Validators Created

#### 1. **AuthValidators.cs**
- `RegisterRequestValidator` - Email, password, role validation
- `LoginRequestValidator` - Email and password validation
- `UpdateProfileRequestValidator` - Email format validation

#### 2. **ProductValidators.cs**
- `CreateProductRequestValidator` - Name, description, category, price, duration
- `UpdateProductRequestValidator` - Optional field validation

#### 3. **BidValidators.cs**
- `PlaceBidRequestValidator` - Auction ID and amount validation

#### 4. **PaymentValidators.cs**
- `ConfirmPaymentRequestValidator` - Confirmed amount validation
- `ConfirmPaymentByAuctionRequestValidator` - Auction ID and amount validation

#### 5. **PaginationValidator.cs**
- `PaginationParamsValidator` - Page number and page size validation

### Sample Validation Rules

```csharp
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(255).WithMessage("Product name cannot exceed 255 characters");

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0).WithMessage("Starting price must be greater than 0")
            .ScalePrecision(2, 18).WithMessage("Starting price must have at most 2 decimal places");

        RuleFor(x => x.AuctionDuration)
            .InclusiveBetween(2, 1440)
            .WithMessage("Auction duration must be between 2 minutes and 24 hours (1440 minutes)");
    }
}
```

### Error Response Format

**Before (Data Annotations):**
```json
{
  "errors": {
    "Name": ["The Name field is required."],
    "StartingPrice": ["The field StartingPrice must be between 0.01 and 1.79769313486232E+308."]
  }
}
```

**After (FluentValidation):**
```json
{
  "errors": {
    "Name": ["Product name is required"],
    "StartingPrice": ["Starting price must be greater than 0"]
  }
}
```

---

## ?? Pagination Implementation

### Pagination Models (PagedResult.cs)

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
```

### Endpoints with Pagination

| Endpoint | Method | Pagination Support |
|----------|--------|-------------------|
| `/api/products` | GET | ? Yes |
| `/api/products/active` | GET | ? Yes |
| `/api/bids` | GET | ? Yes |
| `/api/transactions` | GET | ? Yes |

### Query Parameters

All paginated endpoints accept:
- `pageNumber` (default: 1, min: 1)
- `pageSize` (default: 10, min: 1, max: 100)

### Example Requests

**Get first page of products (default 10 items):**
```http
GET /api/products
```

**Get second page with 20 items per page:**
```http
GET /api/products?pageNumber=2&pageSize=20
```

**Get active auctions (page 1, 5 items):**
```http
GET /api/products/active?pageNumber=1&pageSize=5
```

**Get all bids (page 3, 50 items):**
```http
GET /api/bids?pageNumber=3&pageSize=50
```

**Get transactions (page 1, 15 items):**
```http
GET /api/transactions?pageNumber=1&pageSize=15
```

### Sample Response

```json
{
  "items": [
    {
      "productId": 1,
      "name": "iPhone 15 Pro",
      "description": "Latest iPhone model",
      "category": "Electronics",
      "startingPrice": 999.99,
      "auctionDuration": 60,
      "ownerId": 1,
      "createdAt": "2024-01-15T10:00:00Z",
      "auction": {
        "auctionId": 1,
        "productId": 1,
        "expiryTime": "2024-01-15T11:00:00Z",
        "status": "Active",
        "highestBidAmount": 1200.00,
        "highestBidderId": 3,
        "extensionCount": 0,
        "timeRemaining": "00:45:30",
        "createdAt": "2024-01-15T10:00:00Z"
      }
    }
    // ... more items
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 45,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Pagination Metadata

The response includes:
- `items` - Array of results for current page
- `pageNumber` - Current page number (1-based)
- `pageSize` - Number of items per page
- `totalCount` - Total number of items across all pages
- `totalPages` - Total number of pages
- `hasPreviousPage` - Boolean indicating if previous page exists
- `hasNextPage` - Boolean indicating if next page exists

---

## ?? Updated Services

### ProductService
- `GetAllProductsAsync(PaginationParams)` - Returns `PagedResult<ProductResponse>`
- `GetActiveAuctionsAsync(PaginationParams)` - Returns `PagedResult<ProductResponse>`

### BidService
- `GetAllBidsAsync(PaginationParams)` - Returns `PagedResult<BidResponse>`

### PaymentService
- `GetTransactionsAsync(PaginationParams)` - Returns `PagedResult<PaymentAttemptResponse>`

---

## ?? Benefits

### FluentValidation Benefits
1. **Semantic Error Messages** - Clear, user-friendly messages
2. **Centralized Validation** - All rules in one place
3. **Testable** - Easy to unit test validators
4. **Flexible** - Conditional validation with `When()` clauses
5. **Type-Safe** - Compile-time checking
6. **Extensible** - Easy to add custom validators

### Pagination Benefits
1. **Performance** - Reduces payload size
2. **Scalability** - Handles large datasets efficiently
3. **User Experience** - Faster page loads
4. **Standardized** - Consistent across all endpoints
5. **Flexible** - Client controls page size

---

## ?? Testing Pagination

### Using Swagger UI

1. Navigate to `https://localhost:XXXX/`
2. Authenticate with Bearer token
3. Expand `GET /api/products`
4. Click **Try it out**
5. Enter pagination parameters:
   - `pageNumber`: 2
   - `pageSize`: 5
6. Click **Execute**
7. View paginated response

### Using cURL

```bash
# Get page 1 with 10 items
curl -X GET "https://localhost:7019/api/products?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_TOKEN"

# Get page 2 with 20 items
curl -X GET "https://localhost:7019/api/products?pageNumber=2&pageSize=20" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Using Postman

1. Create GET request: `{{baseUrl}}/api/products`
2. Add Query Params:
   - Key: `pageNumber`, Value: `1`
   - Key: `pageSize`, Value: `10`
3. Add Authorization header: `Bearer {{token}}`
4. Send request

---

## ?? Testing FluentValidation

### Valid Request (Passes)

```json
POST /api/products
{
  "name": "iPhone 15 Pro",
  "description": "Latest Apple smartphone",
  "category": "Electronics",
  "startingPrice": 999.99,
  "auctionDuration": 60
}
```

**Response:** `201 Created`

### Invalid Request (Fails)

```json
POST /api/products
{
  "name": "",
  "description": "Short",
  "category": "Electronics",
  "startingPrice": -10,
  "auctionDuration": 1
}
```

**Response:** `400 Bad Request`
```json
{
  "errors": {
    "Name": ["Product name is required"],
    "StartingPrice": ["Starting price must be greater than 0"],
    "AuctionDuration": ["Auction duration must be between 2 minutes and 24 hours (1440 minutes)"]
  }
}
```

---

## ?? Integration with Existing Code

### No Breaking Changes
- Existing non-paginated endpoints remain functional
- Controllers accept optional query parameters
- Default values ensure backward compatibility
- Response format includes metadata

### Swagger Documentation
- Pagination parameters appear automatically
- Type descriptions included
- Default values shown
- Try it out feature works seamlessly

---

## ?? Configuration Options

### Pagination Limits

To change max page size, edit `PagedResult.cs`:

```csharp
private const int MaxPageSize = 100; // Change to desired max
```

### Default Page Size

```csharp
private int _pageSize = 10; // Change default here
```

### FluentValidation Options

To disable auto-validation (not recommended):
```csharp
// Remove this line from Program.cs
builder.Services.AddFluentValidationAutoValidation();
```

---

## ?? Example: Building a Paginated UI

### Frontend Pagination Logic (Pseudocode)

```typescript
async function loadProducts(page: number, pageSize: number) {
  const response = await fetch(
    `/api/products?pageNumber=${page}&pageSize=${pageSize}`
  );
  const data = await response.json();
  
  // Display items
  displayProducts(data.items);
  
  // Update pagination controls
  updatePagination({
    currentPage: data.pageNumber,
    totalPages: data.totalPages,
    hasPrevious: data.hasPreviousPage,
    hasNext: data.hasNextPage
  });
}

// Navigate to next page
function nextPage() {
  if (currentData.hasNextPage) {
    loadProducts(currentData.pageNumber + 1, pageSize);
  }
}

// Navigate to previous page
function previousPage() {
  if (currentData.hasPreviousPage) {
    loadProducts(currentData.pageNumber - 1, pageSize);
  }
}
```

---

## ? Verification Checklist

After implementation, verify:

- [ ] FluentValidation package installed
- [ ] All validators created and registered
- [ ] Data annotations removed from DTOs
- [ ] Pagination models created
- [ ] All GET list endpoints updated
- [ ] Controllers accept pagination params
- [ ] Services return `PagedResult<T>`
- [ ] Build succeeds without errors
- [ ] Swagger shows pagination parameters
- [ ] API returns paginated responses
- [ ] Error messages are semantic and clear
- [ ] Validation works for all request types

---

## ?? Summary

### What Changed

**FluentValidation:**
- ? Installed FluentValidation.AspNetCore
- ? Created 5 validator files with 9 validators
- ? Configured auto-validation
- ? Removed old Data Annotations
- ? Semantic error messages

**Pagination:**
- ? Created `PagedResult<T>` and `PaginationParams`
- ? Updated 4 GET endpoints with pagination
- ? Modified 3 services and 3 controllers
- ? Query parameter support

### Files Created
1. `codebase/Models/Common/PagedResult.cs`
2. `codebase/Validators/AuthValidators.cs`
3. `codebase/Validators/ProductValidators.cs`
4. `codebase/Validators/BidValidators.cs`
5. `codebase/Validators/PaymentValidators.cs`
6. `codebase/Validators/PaginationValidator.cs`

### Files Modified
1. `codebase/Program.cs` - Added FluentValidation configuration
2. `codebase/Models/DTOs/ProductDTOs.cs` - Removed Data Annotations
3. `codebase/Models/DTOs/AuthDTOs.cs` - Removed Data Annotations
4. `codebase/Models/DTOs/BidDTOs.cs` - Removed Data Annotations
5. `codebase/Models/DTOs/PaymentDTOs.cs` - Removed Data Annotations
6. `codebase/Services/Interfaces/IProductService.cs` - Added pagination
7. `codebase/Services/Interfaces/IBidService.cs` - Added pagination
8. `codebase/Services/Interfaces/IPaymentService.cs` - Added pagination
9. `codebase/Services/Implementations/ProductService.cs` - Implemented pagination
10. `codebase/Services/Implementations/BidService.cs` - Implemented pagination
11. `codebase/Services/Implementations/PaymentService.cs` - Implemented pagination
12. `codebase/Controllers/ProductsController.cs` - Added pagination params
13. `codebase/Controllers/BidsController.cs` - Added pagination params
14. `codebase/Controllers/TransactionsController.cs` - Added pagination params

---

## ?? API Usage Examples

### Paginated Products
```bash
GET /api/products?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

### Paginated Active Auctions
```bash
GET /api/products/active?pageNumber=2&pageSize=5
Authorization: Bearer {token}
```

### Paginated Bids
```bash
GET /api/bids?pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

### Paginated Transactions
```bash
GET /api/transactions?pageNumber=3&pageSize=15
Authorization: Bearer {token}
```

---

**Implementation Complete! Your API now has professional-grade validation and efficient pagination.** ?
