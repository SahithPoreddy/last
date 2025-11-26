# Quick Start Guide ?

Follow this simplified checklist to get BidSphere running in minutes!

## Prerequisites
- [ ] .NET 8 SDK installed (`dotnet --version`)
- [ ] PostgreSQL 12+ installed and running
- [ ] Your database credentials ready

## ? NEW: Automatic Setup!

**Good News!** The application now automatically:
- ? Creates database tables on startup
- ? Applies all migrations automatically
- ? Uses regular JWT tokens (no Identity Framework)

## Quick Setup (5 Minutes Total!)

### 1. Ensure PostgreSQL is Running (1 minute)
- [ ] Start PostgreSQL service
  ```bash
  # Windows: Check services.msc
  # Mac: brew services start postgresql
  # Linux: sudo systemctl start postgresql
  ```

### 2. Create Database Only (1 minute)
You only need to create the database - tables will be auto-created!

```bash
# Connect to PostgreSQL
psql -U vectoruser -d postgres

# Create database
CREATE DATABASE vectordb;

# Exit
\q
```

**OR** if database already exists, skip this step!

### 3. Verify Connection Settings (1 minute)
- [ ] Check `appsettings.json` has your database info:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=vectordb;Username=vectoruser;Password=vectorpass"
  }
  ```

### 4. Run Application (1 minute)
- [ ] Open terminal in `codebase` folder
- [ ] Run:
  ```bash
  dotnet run
  ```
- [ ] Wait for these messages:
  ```
  Checking database connection...
  Database is ready!
  Now listening on: https://localhost:XXXX
  ```

### 5. Access Swagger UI (1 minute)
- [ ] Open browser
- [ ] Go to: `https://localhost:XXXX/` (use the port from step 4)
- [ ] Swagger UI loads with all endpoints! ??

## First Time Usage

### 6. Create Admin User (2 minutes)
- [ ] In Swagger, find `POST /api/auth/register`
- [ ] Click "Try it out"
- [ ] Use:
  ```json
  {
    "email": "admin@bidsphere.com",
    "password": "Admin@123"
  }
  ```
- [ ] Click "Execute"
- [ ] Copy the `token` from response ?

### 7. Update User to Admin Role (1 minute)
```bash
psql -U vectoruser -d vectordb

UPDATE users SET "Role" = 2 WHERE "Email" = 'admin@bidsphere.com';

\q
```

### 8. Authorize in Swagger (30 seconds)
- [ ] Click "Authorize" button (top right with lock icon)
- [ ] Enter: `Bearer YOUR_TOKEN_HERE`
- [ ] Click "Authorize" then "Close"
- [ ] Green lock appears ?

### 9. Create Your First Product (1 minute)
- [ ] Find `POST /api/products`
- [ ] Click "Try it out"
- [ ] Use:
  ```json
  {
    "name": "Vintage Watch",
    "description": "Beautiful vintage watch",
    "category": "Collectibles",
    "startingPrice": 500.00,
    "auctionDuration": 5
  }
  ```
- [ ] Click "Execute"
- [ ] Get `201 Created` ?

### 10. Test Bidding (2 minutes)
- [ ] Logout (click Authorize, remove token)
- [ ] Register new user: `user@example.com` / `User@123`
- [ ] Authorize with new token
- [ ] Use `POST /api/bids`:
  ```json
  {
    "auctionId": 1,
    "amount": 600.00
  }
  ```
- [ ] Success! ?

## What Changed? ??

### ? Automatic Database Setup
- **Before:** Manual migrations required
- **Now:** Tables auto-create on startup!

### ? Simplified JWT
- **Before:** Complex token in config
- **Now:** Simple secret key
- **No Identity Framework** - pure JWT implementation

### ? Better Error Handling
- Application logs database issues
- Continues to start even if DB fails
- Clear error messages

## Troubleshooting

### ? "Cannot connect to database"
**Check:**
1. PostgreSQL running? ? `pg_isready -h localhost -p 5433`
2. Database exists? ? `psql -U vectoruser -l | grep vectordb`
3. Correct password in `appsettings.json`?

**Fix:**
```bash
# Create database if missing
psql -U vectoruser -d postgres
CREATE DATABASE vectordb;
\q

# Restart application
dotnet run
```

### ? "Port already in use"
**Fix:**
```bash
# Kill process on port
# Windows: netstat -ano | findstr :7019
# Linux/Mac: lsof -ti:7019 | xargs kill -9

# Or just change port in launchSettings.json
```

### ? Still can't access Swagger?
**Try:**
1. Use `https://localhost:XXXX/` (root, not /swagger)
2. Check console for actual port number
3. Try `http://localhost:YYYY/` (HTTP version)
4. Disable HTTPS redirect temporarily

### ? Tables not creating?
**Check console logs:**
```
Looking for: "Database is ready!"
If you see errors, the migration failed
```

**Manual fix:**
```bash
cd codebase
dotnet ef database update
```

## Quick Test Scenarios

### Test Complete Flow (5 minutes)
1. ? Register admin
2. ? Update role to Admin
3. ? Create product (2 min duration)
4. ? Register 2 users
5. ? Place bids
6. ? Wait for expiry
7. ? Check payment notification

### Test Anti-Sniping (3 minutes)
1. ? Create 2-min auction
2. ? Wait 1:30
3. ? Place bid
4. ? Auction extends to 2:30 ?

## Database Schema

Tables auto-created:
```
Users             ? User accounts
Products          ? Auction items
Auctions          ? Auction state
Bids              ? Bid records
PaymentAttempts   ? Payment tracking
__EFMigrationsHistory ? Migration tracking
```

## Quick Reference

| Command | Purpose |
|---------|---------|
| `dotnet run` | Start app |
| `dotnet build` | Build only |
| `psql -U vectoruser -d vectordb` | Connect to DB |
| `dotnet ef database update` | Manual migration |
| `dotnet ef migrations list` | View migrations |

## Default Configuration

```json
Database: vectordb (Port 5433)
User: vectoruser
Password: vectorpass
JWT Expiry: 24 hours
Payment Window: 1 minute
Anti-Sniping: Last 60 seconds
```

## Success Indicators

When everything works:
- ? Console shows "Database is ready!"
- ? Console shows "Now listening on..."
- ? Browser shows Swagger UI
- ? Can register user
- ? Can login and get token
- ? Can create product (as admin)
- ? Can place bid (as user)

## Total Time: ~10 Minutes! ?

**Before:** 40 minutes (with manual migrations)
**Now:** 10 minutes (auto-setup!)

---

## Next Steps

Once everything works:
- ?? Read [API_TESTING_GUIDE.md](API_TESTING_GUIDE.md) for advanced scenarios
- ?? Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md) if issues arise
- ?? See [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) for features

## Need Help?

1. Check console logs
2. Verify PostgreSQL running
3. Confirm database exists
4. Check connection string
5. Try manual migration: `dotnet ef database update`

**Still stuck?** Check TROUBLESHOOTING.md for detailed solutions!

---

? **Automatic migration is enabled!**  
? **Regular JWT tokens (no Identity Framework)**  
? **Simple 5-minute setup**  
? **Ready to use!** ??
