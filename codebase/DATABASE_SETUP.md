# Database Setup Guide

## PostgreSQL Installation

### Windows
1. Download PostgreSQL from: https://www.postgresql.org/download/windows/
2. Run the installer
3. Set password for postgres user
4. Keep default port 5432
5. Install pgAdmin 4 (included)

### macOS
```bash
brew install postgresql@14
brew services start postgresql@14
```

### Linux (Ubuntu/Debian)
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

## Database Creation

### Option 1: Using pgAdmin
1. Open pgAdmin 4
2. Connect to PostgreSQL server
3. Right-click on "Databases" → Create → Database
4. Database name: `bidsphere`
5. Owner: `postgres` (or your user)
6. Click "Save"

### Option 2: Using psql Command Line
```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE bidsphere;

# Grant privileges (if using different user)
GRANT ALL PRIVILEGES ON DATABASE bidsphere TO your_username;

# Exit
\q
```

### Option 3: Using SQL Script
Create a file `setup_database.sql`:
```sql
-- Create database
CREATE DATABASE bidsphere
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    CONNECTION LIMIT = -1;

-- Connect to database
\c bidsphere

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

Run the script:
```bash
psql -U postgres -f setup_database.sql
```

## Update Connection String

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=bidsphere;Username=postgres;Password=your_password"
  }
}
```

## Run Migrations

### Install EF Core Tools (if not already installed)
```bash
dotnet tool install --global dotnet-ef
```

### Create Initial Migration
```bash
cd codebase
dotnet ef migrations add InitialCreate
```

### Apply Migration to Database
```bash
dotnet ef database update
```

## Verify Database Setup

### Using psql
```bash
psql -U postgres -d bidsphere

# List all tables
\dt

# You should see:
# - Users
# - Products
# - Auctions
# - Bids
# - PaymentAttempts
# - __EFMigrationsHistory

# Exit
\q
```

### Using pgAdmin
1. Expand Servers → PostgreSQL → Databases → bidsphere
2. Expand Schemas → public → Tables
3. Verify all tables exist

## Seed Initial Data (Optional)

### Option 1: Manual SQL
```sql
-- Create Admin User
INSERT INTO "Users" ("Email", "PasswordHash", "Role", "CreatedAt")
VALUES (
    'admin@bidsphere.com',
    '$2a$11$YourHashedPasswordHere',
    2,
    NOW()
);

-- Create Regular User
INSERT INTO "Users" ("Email", "PasswordHash", "Role", "CreatedAt")
VALUES (
    'user@bidsphere.com',
    '$2a$11$YourHashedPasswordHere',
    1,
    NOW()
);
```

### Option 2: Via API
1. Start the application
2. Use Swagger UI to register users
3. Manually update role in database:
```sql
UPDATE "Users" 
SET "Role" = 2 
WHERE "Email" = 'admin@bidsphere.com';
```

## Database Management Commands

### Drop Database (Warning: Deletes all data)
```bash
psql -U postgres
DROP DATABASE bidsphere;
\q
```

### Backup Database
```bash
pg_dump -U postgres -d bidsphere -f bidsphere_backup.sql
```

### Restore Database
```bash
psql -U postgres -d bidsphere -f bidsphere_backup.sql
```

## Troubleshooting

### Cannot connect to PostgreSQL
1. Check if PostgreSQL is running:
   ```bash
   # Windows
   services.msc → PostgreSQL service
   
   # macOS/Linux
   sudo systemctl status postgresql
   ```

2. Check connection settings:
   - Host: localhost
   - Port: 5432 (default)
   - Username: postgres
   - Password: your_password

### Migration Errors
If you encounter migration errors:
```bash
# Remove all migrations
dotnet ef migrations remove

# Drop and recreate database
dotnet ef database drop -f

# Create fresh migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

### Permission Denied
```bash
# Grant permissions
psql -U postgres
GRANT ALL PRIVILEGES ON DATABASE bidsphere TO your_username;
GRANT ALL ON ALL TABLES IN SCHEMA public TO your_username;
GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO your_username;
```

### Port Already in Use
Edit `appsettings.json` to use different port:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5433;Database=bidsphere;Username=postgres;Password=your_password"
}
```

## Quick Reset Script

Create a file `reset_database.sh` (macOS/Linux) or `reset_database.bat` (Windows):

**Linux/macOS:**
```bash
#!/bin/bash
echo "Resetting database..."
dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
echo "Database reset complete!"
```

**Windows:**
```batch
@echo off
echo Resetting database...
dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
echo Database reset complete!
pause
```

Make executable (Linux/macOS):
```bash
chmod +x reset_database.sh
./reset_database.sh
```

## Database Schema Overview

```
Users
├── UserId (PK)
├── Email (Unique)
├── PasswordHash
├── Role
└── CreatedAt

Products
├── ProductId (PK)
├── Name
├── Description
├── Category
├── StartingPrice
├── AuctionDuration
├── OwnerId (FK → Users)
└── CreatedAt

Auctions
├── AuctionId (PK)
├── ProductId (FK → Products, Unique)
├── ExpiryTime
├── Status
├── HighestBidId (FK → Bids, Nullable)
├── ExtensionCount
├── CreatedAt
└── CompletedAt

Bids
├── BidId (PK)
├── AuctionId (FK → Auctions)
├── BidderId (FK → Users)
├── Amount
└── Timestamp

PaymentAttempts
├── PaymentId (PK)
├── AuctionId (FK → Auctions)
├── BidderId (FK → Users)
├── Status
├── AttemptNumber
├── AttemptTime
├── WindowExpiryTime
├── ConfirmedAmount
└── CompletedAt
```

## Next Steps

1. ✅ Create database
2. ✅ Update connection string
3. ✅ Run migrations
4. ✅ Verify tables created
5. ✅ Start application
6. ✅ Test with Swagger

.