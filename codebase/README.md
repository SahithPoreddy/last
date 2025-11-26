# BidSphere - Auction Management System

## Overview
BidSphere is a smart, event-driven auction management system built with .NET 8, enabling users to create, bid, and manage live auctions in real time with dynamic extensions, retries, analytics, filtering, and secure authentication.

## ?? Key Features
- ? **Automatic Database Setup** - Tables created on first run
- ? **Regular JWT Authentication** - No Identity Framework complexity
- ? **Anti-Sniping Protection** - Auto-extends auctions
- ? **Smart Payment Retry** - Cascading payment attempts
- ? **Real-time Monitoring** - Background services
- ? **Excel Bulk Import** - Upload multiple products
- ? **Role-based Access** - Admin/User/Guest roles

## Technologies
- **.NET 8** - Backend framework
- **PostgreSQL** - Database
- **Entity Framework Core** - ORM
- **JWT** - Authentication (Regular tokens, no Identity Framework)
- **Swagger** - API Documentation
- **EPPlus** - Excel file processing
- **MailKit** - Email notifications
- **BCrypt** - Password hashing

## Prerequisites
- .NET 8 SDK
- PostgreSQL 12 or higher
- Visual Studio 2022 or VS Code (optional)

## ? Quick Setup (5 Minutes)

### 1. Create Database
```bash
psql -U your_username -d postgres
CREATE DATABASE vectordb;
\q
```

### 2. Update Connection String
Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=vectordb;Username=vectoruser;Password=vectorpass"
}
```

### 3. Run Application
```bash
cd codebase
dotnet run
```

**That's it!** Tables are created automatically on first run. ??

### 4. Access Swagger UI
Open browser: `https://localhost:XXXX/` (check console for port)

## ?? What's New?

### Automatic Database Migration
- **No manual migrations needed!**
- Tables auto-create on application startup
- Logs show: `"Database is ready!"`

### Simplified JWT
- Regular JWT tokens (no Identity Framework)
- Simple secret key in configuration
- Standard claims-based authentication

### Better Startup
- Graceful error handling
- Continues even if DB temporarily unavailable
- Clear console logging

## Setup Instructions (Detailed)

### Option 1: Automatic Setup (Recommended)
```bash
# 1. Create database
createdb -U vectoruser vectordb

# 2. Run application - tables created automatically!
dotnet run
```

### Option 2: Manual Migration
```bash
# 1. Create database
createdb -U vectoruser vectordb

# 2. Restore packages
dotnet restore

# 3. Migration already exists, just update
dotnet ef database update

# 4. Run application
dotnet run
```

## Configuration

### Database Settings
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=vectordb;Username=vectoruser;Password=vectorpass"
}
```

### JWT Settings (Regular Token)
```json
"JwtSettings": {
  "SecretKey": "Your-Secret-Key-Here-Minimum-32-Characters",
  "Issuer": "BidSphere",
  "Audience": "BidSphere.Users",
  "ExpiryHours": 24
}
```

### Email Settings (Optional)
```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@gmail.com",
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "EnableSsl": true
}
```

## API Endpoints (18 Total)

### Authentication (4 endpoints)
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token
- `GET /api/auth/profile` - Get current user profile
- `PUT /api/auth/profile` - Update user profile

### Products & Auctions (9 endpoints)
- `POST /api/products` - Create product (Admin)
- `POST /api/products/upload` - Upload Excel file (Admin)
- `GET /api/products` - Get all products
- `GET /api/products/active` - Get active auctions
- `GET /api/products/{id}` - Get auction details
- `PUT /api/products/{id}` - Update product (Admin)
- `PUT /api/products/{id}/finalize` - Force finalize auction (Admin)
- `DELETE /api/products/{id}` - Delete product (Admin)
- `PUT /api/products/{id}/confirm` - Confirm payment (User)

### Bids (3 endpoints)
- `POST /api/bids` - Place bid (User)
- `GET /api/bids/{auctionId}` - Get all bids for auction
- `GET /api/bids` - Get all bids with filters

### Transactions & Dashboard (2 endpoints)
- `GET /api/transactions` - Get all transactions
- `GET /api/dashboard` - Get dashboard metrics (Admin)

## User Roles

| Role | Permissions |
|------|-------------|
| **Admin** | Full access: Create/Edit/Delete products, Upload Excel, Force finalize, Dashboard |
| **User** | Place bids, Confirm payments, View auctions |
| **Guest** | View only (read-only access) |

## Excel Upload Format

| Column | Type | Required | Validation |
|--------|------|----------|------------|
| ProductId | Integer | Yes | Unique |
| Name | String | Yes | Max 255 chars |
| StartingPrice | Decimal | Yes | > 0 |
| Description | String | Yes | Max 2000 chars |
| Category | String | Yes | Max 100 chars |
| DurationMinutes | Integer | Yes | 2-1440 |

## Core Features

### ?? Anti-Sniping Protection
- Auto-extends by 1 minute when bid placed in last 60 seconds
- Multiple extensions supported
- Tracks extension count

### ?? Payment Retry System
- 1-minute payment window
- Automatic retry with next-highest bidder
- Maximum 3 attempts per auction
- Email notifications

### ?? Real-time Monitoring
- Background service monitors auctions (every 10 seconds)
- Auto-finalizes expired auctions
- Payment retry service (every 5 seconds)

### ?? Security
- JWT-based authentication (regular tokens)
- Role-based authorization
- BCrypt password hashing
- Parameterized queries (SQL injection prevention)

## Testing with Swagger

1. Open `https://localhost:XXXX/` in browser
2. Click **"Authorize"** button (top right)
3. Register user via `POST /api/auth/register`
4. Copy the `token` from response
5. Enter: `Bearer <your-token>`
6. Click **"Authorize"** then **"Close"**
7. Green lock icon appears ?
8. Test any endpoint!

### Quick Test Flow
```
1. Register ? 2. Login ? 3. Create Product (Admin) ? 
4. Place Bid (User) ? 5. Wait for Expiry ? 6. Confirm Payment
```

### Test Instant Failure
Add header to payment confirmation:
```
X-Test-Instant-Fail: true
```

## Project Structure
```
codebase/
??? BackgroundServices/     # Auction monitoring, Payment retry
??? Common/                 # Constants, Custom exceptions
??? Controllers/            # API endpoints (5 controllers)
??? Data/                   # DbContext, Auto-migration
??? Middleware/             # Global exception handling
??? Models/
?   ??? DTOs/              # Request/Response models
?   ??? Entities/          # Database entities
?   ??? Enums/             # UserRole, AuctionStatus, etc.
??? Repositories/          # Data access layer
?   ??? Interfaces/
?   ??? Implementations/
??? Services/              # Business logic layer
?   ??? Interfaces/
?   ??? Implementations/
??? Migrations/            # Auto-generated EF migrations
```

## Database Schema

Auto-created tables:
- `Users` - User accounts and roles
- `Products` - Auction items
- `Auctions` - Auction state management
- `Bids` - Bid records
- `PaymentAttempts` - Payment tracking

## Quick Start

### Create Admin User
```bash
# 1. Register via Swagger
POST /api/auth/register
{
  "email": "admin@bidsphere.com",
  "password": "Admin@123"
}

# 2. Update role in database
psql -U vectoruser -d vectordb
UPDATE users SET "Role" = 2 WHERE "Email" = 'admin@bidsphere.com';
\q
```

### Create Sample Product
```json
POST /api/products
{
  "name": "Vintage Watch",
  "description": "1960s classic timepiece",
  "category": "Collectibles",
  "startingPrice": 500.00,
  "auctionDuration": 60
}
```

### Place Bid
```json
POST /api/bids
{
  "auctionId": 1,
  "amount": 600.00
}
```

## Troubleshooting

### Database Connection Failed
```bash
# Check PostgreSQL is running
pg_isready -h localhost -p 5433

# Create database if missing
createdb -U vectoruser vectordb

# Restart application
dotnet run
```

### Tables Not Created
Check console logs:
```
? "Database is ready!" - Success
? Error message - Check connection string
```

Manual fix:
```bash
dotnet ef database update
```

### Swagger Not Loading
- Try root URL: `https://localhost:XXXX/` (not /swagger)
- Check console for actual port
- Verify application started successfully

### Port Already in Use
```bash
# Windows
netstat -ano | findstr :7019
taskkill /PID <process-id> /F

# Linux/Mac
lsof -ti:7019 | xargs kill -9
```

## Useful Commands

| Command | Purpose |
|---------|---------|
| `dotnet run` | Start application |
| `dotnet build` | Build only |
| `dotnet ef database update` | Manual migration |
| `dotnet ef migrations list` | View migrations |
| `psql -U vectoruser -d vectordb` | Connect to database |

## Documentation

- ?? **QUICK_START.md** - Fast 5-minute setup guide
- ?? **API_TESTING_GUIDE.md** - Complete API examples
- ?? **DATABASE_SETUP.md** - PostgreSQL setup details
- ?? **TROUBLESHOOTING.md** - Common issues & solutions
- ?? **IMPLEMENTATION_SUMMARY.md** - Feature documentation

## Best Practices

? **Applied in this project:**
- Repository Pattern
- Service Layer Pattern
- Dependency Injection
- Async/Await
- SOLID Principles
- Clean Architecture
- Exception Handling
- Comprehensive Logging

## Performance

- ? Async operations throughout
- ? Efficient LINQ queries
- ? Background services for long operations
- ? Proper database indexing
- ? Connection pooling

## License
Educational project for learning .NET 8 and PostgreSQL.

## Support
For issues, check **TROUBLESHOOTING.md** or review console logs.

---

**?? Ready to use! Just `dotnet run` and go!**

? Auto-migration enabled  
? Regular JWT (no Identity Framework)  
? 18 API endpoints  
? Production-ready code  
