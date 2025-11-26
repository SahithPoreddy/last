# Table Names Update - Lowercase Convention

## Changes Made

All database table names have been changed from PascalCase to lowercase (snake_case for multi-word tables) to follow PostgreSQL naming conventions.

## Table Name Mapping

| Old Name (PascalCase) | New Name (lowercase) |
|----------------------|---------------------|
| `Users` | `users` |
| `Products` | `products` |
| `Auctions` | `auctions` |
| `Bids` | `bids` |
| `PaymentAttempts` | `payment_attempts` |

## Files Modified

### 1. ApplicationDbContext.cs
Added `ToTable()` configuration for each entity:

```csharp
modelBuilder.Entity<User>(entity =>
{
    entity.ToTable("users");  // ? Added
    // ... rest of configuration
});

modelBuilder.Entity<Product>(entity =>
{
    entity.ToTable("products");  // ? Added
    // ... rest of configuration
});

modelBuilder.Entity<Auction>(entity =>
{
    entity.ToTable("auctions");  // ? Added
    // ... rest of configuration
});

modelBuilder.Entity<Bid>(entity =>
{
    entity.ToTable("bids");  // ? Added
    // ... rest of configuration
});

modelBuilder.Entity<PaymentAttempt>(entity =>
{
    entity.ToTable("payment_attempts");  // ? Added
    // ... rest of configuration
});
```

## Migration Created

A new migration has been created: `RenameTablesTolowercase`

This migration will:
1. Rename all tables from PascalCase to lowercase
2. Maintain all relationships and constraints
3. Preserve all existing data

## How to Apply Changes

### Option 1: Automatic (On Next Run)
The application will automatically apply this migration on the next startup.

```bash
dotnet run
```

### Option 2: Manual Migration
```bash
cd codebase
dotnet ef database update
```

### Option 3: Fresh Database
If you want to start fresh:

```bash
# Drop existing database
psql -U vectoruser -d postgres
DROP DATABASE vectordb;
CREATE DATABASE vectordb;
\q

# Run application - tables will be created with lowercase names
dotnet run
```

## SQL Queries After Change

Update your SQL queries to use lowercase table names:

### Before (PascalCase):
```sql
SELECT * FROM "Users" WHERE "Email" = 'admin@bidsphere.com';
UPDATE "Users" SET "Role" = 2 WHERE "Email" = 'admin@bidsphere.com';
SELECT * FROM "Products" WHERE "Category" = 'Electronics';
```

### After (lowercase):
```sql
SELECT * FROM users WHERE "Email" = 'admin@bidsphere.com';
UPDATE users SET "Role" = 2 WHERE "Email" = 'admin@bidsphere.com';
SELECT * FROM products WHERE "Category" = 'Electronics';
```

**Note:** Column names still use PascalCase (e.g., `Email`, `Role`) as configured in the entities.

## PostgreSQL Best Practices

### Why Lowercase?
- PostgreSQL convention: tables in lowercase, no quotes needed
- Easier to type and read
- Consistent with most PostgreSQL databases
- Avoids case-sensitivity issues

### Benefits:
? No need for double quotes in queries
? Consistent with PostgreSQL naming conventions
? Better compatibility with PostgreSQL tools
? Cleaner SQL queries

## Updated Quick Reference

### Table Names:
```
users              ? User accounts
products           ? Auction items
auctions           ? Auction state
bids               ? Bid records
payment_attempts   ? Payment tracking
```

### Common Queries:

**Check tables:**
```sql
\dt
```

**View users:**
```sql
SELECT * FROM users;
```

**View active auctions:**
```sql
SELECT * FROM auctions WHERE "Status" = 0;
```

**Update user to admin:**
```sql
UPDATE users SET "Role" = 2 WHERE "Email" = 'admin@bidsphere.com';
```

**View bids for auction:**
```sql
SELECT * FROM bids WHERE "AuctionId" = 1 ORDER BY "Timestamp" DESC;
```

**View payment attempts:**
```sql
SELECT * FROM payment_attempts WHERE "Status" = 0;
```

## Verification

After applying the migration, verify the table names:

```sql
psql -U vectoruser -d vectordb

-- List all tables
\dt

-- Should show:
-- users
-- products
-- auctions
-- bids
-- payment_attempts

-- Check data is preserved
SELECT COUNT(*) FROM users;
SELECT COUNT(*) FROM products;
SELECT COUNT(*) FROM auctions;
SELECT COUNT(*) FROM bids;
SELECT COUNT(*) FROM payment_attempts;

\q
```

## Documentation Updates

### Files to Update:
- ? ApplicationDbContext.cs - Updated with ToTable()
- ? Migration created - RenameTablesTolowercase
- ?? DATABASE_SETUP.md - Update SQL examples
- ?? QUICK_START.md - Update SQL commands
- ?? TROUBLESHOOTING.md - Update SQL queries
- ?? README.md - Update table references

## Important Notes

### Column Names
Column names are still in PascalCase and require double quotes in PostgreSQL:
```sql
-- Correct:
SELECT "UserId", "Email" FROM users;

-- Also works (case-insensitive):
SELECT userid, email FROM users;
```

### Foreign Keys and Indexes
All foreign keys, indexes, and constraints are automatically renamed by the migration.

### Existing Migrations
The old `InitialCreate` migration remains in the migration history. The new migration will rename the tables.

## Rollback (If Needed)

To revert to PascalCase table names:

```bash
cd codebase
dotnet ef migrations remove
```

Then remove the `ToTable()` calls from `ApplicationDbContext.cs` and create a new migration.

## Summary

? **Table names:** Now lowercase
? **Column names:** Still PascalCase
? **Migration:** Created and ready
? **Data:** Preserved during migration
? **Application code:** No changes needed (EF handles it)
? **SQL queries:** Use lowercase table names

## Next Steps

1. Apply migration: `dotnet run` or `dotnet ef database update`
2. Verify tables renamed: `psql -U vectoruser -d vectordb` then `\dt`
3. Test application functionality
4. Update any custom SQL queries to use lowercase table names

---

**Migration Status:** ? Ready to apply
**Data Loss Risk:** ? None (tables are renamed, data preserved)
**Application Impact:** ? None (EF Core handles mapping)
