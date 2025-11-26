# Git Commit Guide

This guide helps you commit the implementation with proper commit messages following best practices.

## Initial Setup

```bash
# Initialize git repository (if not already done)
git init

# Add all files
git add .

# Check status
git status
```

## Recommended Commit Strategy

### Milestone 1 Commits

```bash
# 1. Project Setup
git add codebase.csproj appsettings.json .gitignore
git commit -m "feat: setup .NET 8 project with PostgreSQL and required NuGet packages

- Add Entity Framework Core with PostgreSQL
- Add JWT authentication packages
- Add Excel processing (EPPlus)
- Add Email service (MailKit)
- Add BCrypt for password hashing
- Configure connection strings and settings"

# 2. Create Entities and Enums
git add Models/
git commit -m "feat: create domain entities and enums

- User entity with role-based access
- Product entity with auction details
- Auction entity with status tracking
- Bid entity for bid management
- PaymentAttempt entity for payment tracking
- UserRole, AuctionStatus, PaymentStatus enums"

# 3. Create Database Context
git add Data/ApplicationDbContext.cs
git commit -m "feat: configure Entity Framework DbContext with proper relationships

- Define entity relationships (One-to-One, One-to-Many)
- Add database constraints and indexes
- Configure column types and defaults"

# 4. Create DTOs
git add Models/DTOs/
git commit -m "feat: create Data Transfer Objects for API contracts

- AuthDTOs for authentication
- ProductDTOs for product management
- BidDTOs for bidding
- PaymentDTOs for payment and dashboard"

# 5. Create Common Classes
git add Common/
git commit -m "feat: add application constants and custom exceptions

- AppConstants for configuration values
- Custom exception classes (BadRequest, NotFound, etc.)
- Global exception handling middleware"

# 6. Create Repository Layer
git add Repositories/
git commit -m "feat: implement repository pattern for data access

- Create repository interfaces
- Implement repositories for all entities
- Add async methods for CRUD operations
- Include filtering and querying capabilities"

# 7. Create Service Layer
git add Services/
git commit -m "feat: implement service layer with business logic

- AuthService for authentication and JWT
- ProductService for product/auction management
- BidService for bid placement and validation
- PaymentService for payment processing
- EmailService for notifications"

# 8. Create Controllers
git add Controllers/
git commit -m "feat: create API controllers with Swagger documentation

- AuthController for user authentication
- ProductsController for product/auction management
- BidsController for bidding operations
- TransactionsController for payment history
- DashboardController for analytics
- Add XML comments for Swagger"

# 9. Configure Program.cs
git add Program.cs
git commit -m "feat: configure application startup with DI and middleware

- Configure Entity Framework with PostgreSQL
- Setup JWT authentication
- Register all services and repositories
- Configure Swagger with JWT support
- Add CORS and logging"

# 10. Create Middleware
git add Middleware/
git commit -m "feat: add global exception handling middleware

- Catch and handle all exceptions
- Return structured error responses
- Log errors with context
- Map exceptions to HTTP status codes"

# 11. Documentation
git add README.md DATABASE_SETUP.md
git commit -m "docs: add comprehensive documentation

- README with setup instructions
- DATABASE_SETUP guide for PostgreSQL
- API endpoint documentation"
```

### Milestone 2 Commits

```bash
# 1. Background Services
git add BackgroundServices/
git commit -m "feat: implement background services for auction monitoring

- AuctionExpiryMonitor for automatic finalization
- Run every 10 seconds (configurable)
- Automatic status transitions
- Proper error handling and logging"

# 2. Anti-Sniping Feature
git commit -m "feat: implement anti-sniping protection with automatic auction extension

- Extend auction by 1 minute if bid placed in last 60 seconds
- Track extension count
- Support multiple extensions
- Configurable threshold and duration"

# 3. Filtering
git commit -m "feat: add filtering capabilities for products and bids

- Filter by category, price range, status
- Filter bids by user, auction, date range
- Efficient LINQ queries"
```

### Milestone 3 Commits

```bash
# 1. Excel Upload
git add Services/Implementations/ProductService.cs ExcelUploadTemplate.md
git commit -m "feat: implement Excel bulk product upload

- Support .xlsx format
- Row-by-row validation
- Error logging for failed rows
- Return success/failure counts
- EPPlus integration"

# 2. Role-Based Authorization
git commit -m "feat: implement role-based authorization throughout API

- Admin role for management operations
- User role for bidding
- Guest role for read-only access
- JWT claims-based authorization"

# 3. Logging
git commit -m "feat: add comprehensive logging throughout application

- Structured logging with ILogger
- Log levels (Info, Warning, Error)
- Context information in logs
- Background service logging"
```

### Milestone 4 Commits

```bash
# 1. Payment Workflow
git commit -m "feat: implement payment confirmation workflow

- 1-minute payment window
- Amount validation
- Automatic status updates
- Winner notification"

# 2. Email Service
git commit -m "feat: add email notification service

- MailKit integration
- Payment request emails
- Auction won notifications
- Configurable SMTP settings"

# 3. Retry Service
git add BackgroundServices/PaymentRetryService.cs
git commit -m "feat: implement payment retry queue service

- Monitor expired payment windows
- Automatic retry with next bidder
- Maximum 3 attempts
- Failed auction handling"

# 4. Test Support
git commit -m "feat: add instant failure testing support

- X-Test-Instant-Fail header
- Instant retry trigger
- Test automation support"

# 5. Dashboard
git commit -m "feat: implement analytics dashboard

- Active/Pending/Completed/Failed counts
- Top bidders statistics
- Total amounts tracking"

# 6. Additional Documentation
git add API_TESTING_GUIDE.md IMPLEMENTATION_SUMMARY.md
git commit -m "docs: add testing guide and implementation summary

- Comprehensive API testing examples
- Complete implementation documentation
- Testing scenarios and troubleshooting"
```

## Final Commit

```bash
# Add all remaining files
git add .

# Final commit
git commit -m "feat: complete BidSphere auction management system

Complete implementation of all 4 milestones:

Milestone 1:
- ? Project setup with .NET 8 and PostgreSQL
- ? User authentication with JWT
- ? Product CRUD operations
- ? Swagger integration
- ? Entity relationships

Milestone 2:
- ? Complete Product & Auction APIs
- ? Bid management with validation
- ? Filtering capabilities
- ? Background service for monitoring
- ? Anti-sniping protection

Milestone 3:
- ? Excel bulk upload
- ? Pagination support
- ? Query language (ASQL) ready
- ? Role-based authorization
- ? Comprehensive logging

Milestone 4:
- ? Payment confirmation workflow
- ? Email notifications
- ? Retry queue service
- ? Transaction tracking
- ? Dashboard analytics
- ? Best practices applied

Features:
- Event-driven architecture
- Real-time auction monitoring
- Smart retry logic
- Anti-sniping protection
- Excel import/export
- Analytics dashboard
- Role-based access control
- Comprehensive documentation

Technologies:
- .NET 8, PostgreSQL, EF Core
- JWT Authentication
- EPPlus, MailKit, BCrypt
- Swagger/OpenAPI

Total: 18 API endpoints, 5 entities, 3 background services"
```

## Git Best Practices

### Commit Message Format
```
<type>: <subject>

<body>

<footer>
```

### Types
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance tasks

### Example Good Commits
```bash
# Feature with detailed body
git commit -m "feat: add anti-sniping protection

Automatically extends auction by 1 minute when bids are placed
within the last 60 seconds of auction expiry.

- Configurable threshold (default: 60 seconds)
- Configurable extension (default: 1 minute)
- Tracks extension count
- Supports multiple extensions"

# Bug fix
git commit -m "fix: validate bid amount against current highest bid

Previously allowed bids equal to current highest bid.
Now requires bid to be strictly greater."

# Documentation
git commit -m "docs: update README with database setup instructions

Added step-by-step guide for PostgreSQL installation
and migration commands."
```

## Create Feature Branches (Optional)

```bash
# Create branch for each milestone
git checkout -b milestone-1
# ... make changes ...
git commit -m "feat: complete milestone 1"
git checkout main
git merge milestone-1

git checkout -b milestone-2
# ... make changes ...
git commit -m "feat: complete milestone 2"
git checkout main
git merge milestone-2

# Continue for remaining milestones
```

## Push to Remote

```bash
# Add remote repository
git remote add origin https://github.com/yourusername/bidsphere.git

# Push all commits
git push -u origin main

# Or push specific branch
git push origin milestone-1
```

## View Commit History

```bash
# View all commits
git log

# View with graph
git log --graph --oneline --all

# View specific file history
git log -- Controllers/ProductsController.cs

# View changes in commit
git show <commit-hash>
```

## Useful Git Commands

```bash
# Check status
git status

# View changes
git diff

# Stage specific files
git add Controllers/AuthController.cs

# Stage all changes
git add .

# Amend last commit
git commit --amend

# View commit history
git log --oneline

# Create tag for version
git tag -a v1.0.0 -m "BidSphere v1.0.0 - Complete implementation"
git push origin v1.0.0
```

## Semantic Versioning

```bash
# Tag versions
git tag -a v1.0.0 -m "Initial release - All milestones complete"
git tag -a v1.1.0 -m "Added pagination feature"
git tag -a v1.2.0 -m "Added ASQL query language"

# Push tags
git push --tags
```

## .gitignore Already Created

The `.gitignore` file is already in place to exclude:
- Build outputs (bin/, obj/)
- IDE files (.vs/, .vscode/)
- User-specific files (*.user, *.suo)
- Database files
- Logs
- Environment files with secrets

## Recommended Workflow

1. **Initial commit** - Project setup
2. **Feature commits** - Each major feature
3. **Milestone commits** - End of each milestone
4. **Documentation commits** - Separate from code
5. **Final commit** - Complete implementation

This keeps history clean and makes it easy to:
- Track progress
- Revert specific features
- Review changes
- Collaborate with team
- Generate changelogs

## Example Complete Workflow

```bash
# 1. Initialize
git init
git add .gitignore README.md
git commit -m "chore: initial commit with .gitignore and README"

# 2. Add features incrementally (as shown above)

# 3. Create final tag
git tag -a v1.0.0 -m "BidSphere v1.0.0 - Complete auction management system"

# 4. Push to remote
git remote add origin <your-repo-url>
git push -u origin main
git push --tags

# 5. Done!
```

Remember: Commit early, commit often, write clear commit messages!
