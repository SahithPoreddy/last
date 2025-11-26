# Project File Structure

Complete list of all files in the BidSphere project.

## Root Files
```
codebase/
??? Program.cs                          # Application entry point & configuration
??? appsettings.json                    # Application configuration
??? codebase.csproj                     # Project file with dependencies
??? .gitignore                          # Git ignore rules
```

## Documentation (8 files)
```
??? README.md                           # Main project documentation
??? QUICK_START.md                      # Quick start checklist
??? API_TESTING_GUIDE.md               # Comprehensive API testing guide
??? DATABASE_SETUP.md                  # PostgreSQL setup instructions
??? IMPLEMENTATION_SUMMARY.md          # Complete feature summary
??? GIT_COMMIT_GUIDE.md                # Git workflow and commit messages
??? TROUBLESHOOTING.md                 # Common issues and solutions
??? ExcelUploadTemplate.md             # Excel file format specification
```

## Background Services (2 files)
```
BackgroundServices/
??? AuctionExpiryMonitor.cs            # Monitors and finalizes expired auctions
??? PaymentRetryService.cs             # Handles payment retry queue
```

## Common (3 files)
```
Common/
??? AppConstants.cs                     # Application-wide constants
??? Exceptions/
    ??? CustomExceptions.cs             # Custom exception classes
```

## Controllers (5 files)
```
Controllers/
??? AuthController.cs                   # Authentication endpoints (4 endpoints)
??? ProductsController.cs               # Product/Auction management (9 endpoints)
??? BidsController.cs                   # Bid management (3 endpoints)
??? TransactionsController.cs           # Transaction tracking (1 endpoint)
??? DashboardController.cs              # Analytics dashboard (1 endpoint)
```

## Data (2 files)
```
Data/
??? ApplicationDbContext.cs             # Entity Framework DbContext
??? DatabaseSeeder.cs                   # Sample data seeding (optional)
```

## Middleware (1 file)
```
Middleware/
??? ExceptionHandlingMiddleware.cs     # Global exception handling
```

## Models

### DTOs (4 files)
```
Models/DTOs/
??? AuthDTOs.cs                         # Authentication request/response models
??? ProductDTOs.cs                      # Product/Auction request/response models
??? BidDTOs.cs                          # Bid request/response models
??? PaymentDTOs.cs                      # Payment and dashboard models
```

### Entities (5 files)
```
Models/Entities/
??? User.cs                             # User entity
??? Product.cs                          # Product entity
??? Auction.cs                          # Auction entity
??? Bid.cs                              # Bid entity
??? PaymentAttempt.cs                   # Payment attempt entity
```

### Enums (3 files)
```
Models/Enums/
??? UserRole.cs                         # User role enum (Guest, User, Admin)
??? AuctionStatus.cs                    # Auction status enum
??? PaymentStatus.cs                    # Payment status enum
```

## Repositories

### Interfaces (5 files)
```
Repositories/Interfaces/
??? IUserRepository.cs                  # User repository interface
??? IProductRepository.cs               # Product repository interface
??? IAuctionRepository.cs               # Auction repository interface
??? IBidRepository.cs                   # Bid repository interface
??? IPaymentAttemptRepository.cs       # Payment repository interface
```

### Implementations (5 files)
```
Repositories/Implementations/
??? UserRepository.cs                   # User repository implementation
??? ProductRepository.cs                # Product repository implementation
??? AuctionRepository.cs                # Auction repository implementation
??? BidRepository.cs                    # Bid repository implementation
??? PaymentAttemptRepository.cs        # Payment repository implementation
```

## Services

### Interfaces (5 files)
```
Services/Interfaces/
??? IAuthService.cs                     # Authentication service interface
??? IProductService.cs                  # Product service interface
??? IBidService.cs                      # Bid service interface
??? IPaymentService.cs                  # Payment service interface
??? IEmailService.cs                    # Email service interface
```

### Implementations (5 files)
```
Services/Implementations/
??? AuthService.cs                      # Authentication & JWT service
??? ProductService.cs                   # Product/Auction business logic
??? BidService.cs                       # Bid placement & validation
??? PaymentService.cs                   # Payment processing & retry logic
??? EmailService.cs                     # Email notification service
```

## Complete File Count

| Category | Count |
|----------|-------|
| Documentation | 8 |
| Background Services | 2 |
| Common/Utilities | 2 |
| Controllers | 5 |
| Data Layer | 2 |
| Middleware | 1 |
| DTOs | 4 |
| Entities | 5 |
| Enums | 3 |
| Repository Interfaces | 5 |
| Repository Implementations | 5 |
| Service Interfaces | 5 |
| Service Implementations | 5 |
| Configuration | 3 |
| **Total** | **55** |

## Files by Type

### C# Source Files (.cs): 44
- Controllers: 5
- Services: 10
- Repositories: 10
- Entities: 5
- DTOs: 4
- Enums: 3
- Background Services: 2
- Data: 2
- Middleware: 1
- Common: 2

### Documentation Files (.md): 8
- Setup & Usage: 4
- Reference: 4

### Configuration Files: 3
- appsettings.json
- codebase.csproj
- .gitignore

## Lines of Code (Approximate)

| Category | Lines |
|----------|-------|
| Entities | ~400 |
| DTOs | ~300 |
| Repositories | ~800 |
| Services | ~1,500 |
| Controllers | ~600 |
| Background Services | ~200 |
| Other | ~300 |
| **Total** | **~4,100 lines** |

## Key Features Implemented

### Authentication (AuthService, AuthController)
- User registration
- Login with JWT
- Password hashing
- Profile management

### Product Management (ProductService, ProductsController)
- CRUD operations
- Excel bulk upload
- Auction auto-creation
- Force finalization

### Bidding (BidService, BidsController)
- Bid placement
- Validation
- Anti-sniping
- History tracking

### Payment Processing (PaymentService)
- Payment confirmation
- Retry logic
- Email notifications
- Window management

### Background Processing
- Auction expiry monitoring
- Payment retry queue
- Status transitions

### Analytics (DashboardController)
- Auction statistics
- Top bidders
- Performance metrics

### Data Access (Repositories)
- Repository pattern
- Async operations
- LINQ queries
- Entity relationships

### Infrastructure
- Global exception handling
- Logging throughout
- Dependency injection
- Configuration management

## Dependencies (NuGet Packages)

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.3.1" />
<PackageReference Include="EPPlus" Version="7.0.5" />
<PackageReference Include="MailKit" Version="4.3.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

## API Endpoints Summary

Total: **18 endpoints** across 5 controllers

### AuthController (4 endpoints)
- POST /api/auth/register
- POST /api/auth/login
- GET /api/auth/profile
- PUT /api/auth/profile

### ProductsController (9 endpoints)
- POST /api/products
- POST /api/products/upload
- GET /api/products
- GET /api/products/active
- GET /api/products/{id}
- PUT /api/products/{id}
- PUT /api/products/{id}/finalize
- DELETE /api/products/{id}
- PUT /api/products/{id}/confirm

### BidsController (3 endpoints)
- POST /api/bids
- GET /api/bids/{auctionId}
- GET /api/bids

### TransactionsController (1 endpoint)
- GET /api/transactions

### DashboardController (1 endpoint)
- GET /api/dashboard

## Database Schema

**5 Tables:**
1. Users - User accounts and roles
2. Products - Auction products
3. Auctions - Auction state management
4. Bids - Bid records
5. PaymentAttempts - Payment tracking

**Relationships:**
- User ? Products (1:N)
- User ? Bids (1:N)
- User ? PaymentAttempts (1:N)
- Product ? Auction (1:1)
- Auction ? Bids (1:N)
- Auction ? PaymentAttempts (1:N)
- Auction ? HighestBid (1:1)

## Architecture Patterns

1. **Repository Pattern** - Data access abstraction
2. **Service Layer Pattern** - Business logic separation
3. **Dependency Injection** - Loose coupling
4. **Background Services** - Async processing
5. **Middleware Pipeline** - Request processing
6. **DTO Pattern** - API contracts
7. **Entity Framework** - ORM

## Testing Support

- Swagger UI integration
- Test headers (X-Test-Instant-Fail)
- Comprehensive logging
- Detailed error messages
- Sample data seeder

## Documentation Coverage

- ? Setup instructions (DATABASE_SETUP.md)
- ? Quick start guide (QUICK_START.md)
- ? API testing guide (API_TESTING_GUIDE.md)
- ? Troubleshooting (TROUBLESHOOTING.md)
- ? Implementation details (IMPLEMENTATION_SUMMARY.md)
- ? Git workflow (GIT_COMMIT_GUIDE.md)
- ? Excel format (ExcelUploadTemplate.md)
- ? Main README (README.md)

## Project Statistics

- **Files:** 55
- **Lines of Code:** ~4,100
- **Controllers:** 5
- **API Endpoints:** 18
- **Services:** 5
- **Repositories:** 5
- **Entities:** 5
- **Background Services:** 2
- **Documentation Files:** 8
- **Dependencies:** 8 NuGet packages

## All Files Checklist

Core Application:
- [x] Program.cs
- [x] appsettings.json
- [x] codebase.csproj
- [x] .gitignore

Documentation:
- [x] README.md
- [x] QUICK_START.md
- [x] API_TESTING_GUIDE.md
- [x] DATABASE_SETUP.md
- [x] IMPLEMENTATION_SUMMARY.md
- [x] GIT_COMMIT_GUIDE.md
- [x] TROUBLESHOOTING.md
- [x] ExcelUploadTemplate.md

Background Services:
- [x] AuctionExpiryMonitor.cs
- [x] PaymentRetryService.cs

Common:
- [x] AppConstants.cs
- [x] CustomExceptions.cs

Controllers:
- [x] AuthController.cs
- [x] ProductsController.cs
- [x] BidsController.cs
- [x] TransactionsController.cs
- [x] DashboardController.cs

Data:
- [x] ApplicationDbContext.cs
- [x] DatabaseSeeder.cs

Middleware:
- [x] ExceptionHandlingMiddleware.cs

Models (DTOs):
- [x] AuthDTOs.cs
- [x] ProductDTOs.cs
- [x] BidDTOs.cs
- [x] PaymentDTOs.cs

Models (Entities):
- [x] User.cs
- [x] Product.cs
- [x] Auction.cs
- [x] Bid.cs
- [x] PaymentAttempt.cs

Models (Enums):
- [x] UserRole.cs
- [x] AuctionStatus.cs
- [x] PaymentStatus.cs

Repositories (Interfaces):
- [x] IUserRepository.cs
- [x] IProductRepository.cs
- [x] IAuctionRepository.cs
- [x] IBidRepository.cs
- [x] IPaymentAttemptRepository.cs

Repositories (Implementations):
- [x] UserRepository.cs
- [x] ProductRepository.cs
- [x] AuctionRepository.cs
- [x] BidRepository.cs
- [x] PaymentAttemptRepository.cs

Services (Interfaces):
- [x] IAuthService.cs
- [x] IProductService.cs
- [x] IBidService.cs
- [x] IPaymentService.cs
- [x] IEmailService.cs

Services (Implementations):
- [x] AuthService.cs
- [x] ProductService.cs
- [x] BidService.cs
- [x] PaymentService.cs
- [x] EmailService.cs

**All 55 Files Created! ?**
