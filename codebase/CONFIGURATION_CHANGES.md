# Configuration Changes Summary

## Changes Implemented

### 1. ? Default URL Configuration

**Changed:** Default application URL from `localhost:7019/swagger` to `localhost:7019`

**Files Modified:**
- `Properties/launchSettings.json`

**Before:**
```json
"launchUrl": "swagger"
```

**After:**
```json
"launchUrl": ""
```

**Impact:**
- When you run the application, browser opens to `https://localhost:7019/`
- Swagger UI is still at root, so you'll see Swagger immediately
- Cleaner URL without the `/swagger` suffix

### 2. ? Enum Storage as Strings

**Changed:** Database now stores enum values as descriptive strings instead of integers

**Enums Affected:**
1. **AuctionStatus** (in `auctions` table)
   - Before: `0, 1, 2, 3`
   - After: `Active, Completed, Failed, PendingPayment`

2. **PaymentStatus** (in `payment_attempts` table)
   - Before: `0, 1, 2`
   - After: `Pending, Success, Failed`

**Files Modified:**
- `Data/ApplicationDbContext.cs`

**Database Configuration:**
```csharp
// Auction Status
entity.Property(e => e.Status)
      .IsRequired()
      .HasConversion<string>()  // ? Stores as string
      .HasMaxLength(20);

// Payment Status
entity.Property(e => e.Status)
      .IsRequired()
      .HasConversion<string>()  // ? Stores as string
      .HasMaxLength(20);
```

---

## Detailed Changes

### Change 1: URL Configuration

#### What Changed
The `launchSettings.json` file controls what URL opens when you run the application.

#### Previous Behavior
```
dotnet run
? Browser opens: https://localhost:7019/swagger
```

#### New Behavior
```
dotnet run
? Browser opens: https://localhost:7019/
```

**Note:** Since Swagger UI is configured at the root (`RoutePrefix = string.Empty` in Program.cs), you'll still see Swagger immediately, just with a cleaner URL.

#### All Profiles Updated
- ? `http` profile
- ? `https` profile  
- ? `IIS Express` profile

---

### Change 2: Enum String Storage

#### Why This Change?

**Benefits of String Storage:**
1. **? Readable Database** - Can directly read status in SQL queries
2. **? Better Debugging** - Clear values when viewing database
3. **? Database Independence** - Not tied to enum integer values
4. **? Migration Safety** - Adding new enum values won't break existing data
5. **? API Clarity** - JSON responses show descriptive names

**Before (Integer Storage):**
```sql
SELECT * FROM auctions;

 AuctionId | Status | ExpiryTime
-----------|--------|------------
     1     |   0    | 2024-01-15 10:00:00  -- What does 0 mean?
     2     |   3    | 2024-01-15 11:00:00  -- What does 3 mean?
```

**After (String Storage):**
```sql
SELECT * FROM auctions;

 AuctionId |    Status      | ExpiryTime
-----------|----------------|------------
     1     |    Active      | 2024-01-15 10:00:00  -- Clear!
     2     | PendingPayment | 2024-01-15 11:00:00  -- Clear!
```

#### Enum Values Mapping

##### AuctionStatus
| Integer (Old) | String (New) | Description |
|--------------|--------------|-------------|
| 0 | `Active` | Auction is currently active |
| 1 | `Completed` | Auction finished successfully |
| 2 | `Failed` | Auction failed (no successful payment) |
| 3 | `PendingPayment` | Auction ended, awaiting payment |

##### PaymentStatus
| Integer (Old) | String (New) | Description |
|--------------|--------------|-------------|
| 0 | `Pending` | Payment pending confirmation |
| 1 | `Success` | Payment confirmed successfully |
| 2 | `Failed` | Payment failed or rejected |

---

## Migration

### Migration Created: `ConvertEnumsToStrings`

**What This Migration Does:**
1. Alters `Status` column in `auctions` table to VARCHAR(20)
2. Alters `Status` column in `payment_attempts` table to VARCHAR(20)
3. Converts existing integer values to string equivalents:
   - `0` ? `Active` / `Pending`
   - `1` ? `Completed` / `Success`
   - `2` ? `Failed` / `Failed`
   - `3` ? `PendingPayment`

### How to Apply

**Option 1: Automatic (Recommended)**
```bash
dotnet run
```
The migration applies automatically on startup.

**Option 2: Manual**
```bash
cd codebase
dotnet ef database update
```

**Option 3: Fresh Database**
```bash
# Drop and recreate
psql -U vectoruser -d postgres
DROP DATABASE vectordb;
CREATE DATABASE vectordb;
\q

# Run app - tables created with string enums
dotnet run
```

---

## SQL Query Updates

### Before (Integer Queries)
```sql
-- Get active auctions
SELECT * FROM auctions WHERE "Status" = 0;

-- Get pending payments
SELECT * FROM payment_attempts WHERE "Status" = 0;

-- Get completed auctions
SELECT * FROM auctions WHERE "Status" = 1;
```

### After (String Queries)
```sql
-- Get active auctions
SELECT * FROM auctions WHERE "Status" = 'Active';

-- Get pending payments
SELECT * FROM payment_attempts WHERE "Status" = 'Pending';

-- Get completed auctions
SELECT * FROM auctions WHERE "Status" = 'Completed';
```

---

## Testing the Changes

### Test 1: URL Change

1. **Run the application:**
```bash
dotnet run
```

2. **Verify console output:**
```
Now listening on: https://localhost:7019
Now listening on: http://localhost:5097
```

3. **Browser should auto-open to:**
```
https://localhost:7019/
```

4. **Swagger UI should be visible immediately**

### Test 2: Enum String Storage

1. **Create a product and auction:**
```json
POST /api/products
{
  "name": "Test Product",
  "description": "Test",
  "category": "Test",
  "startingPrice": 100,
  "auctionDuration": 60
}
```

2. **Check database:**
```sql
psql -U vectoruser -d vectordb

SELECT "AuctionId", "Status" FROM auctions;

-- Should show:
 AuctionId |  Status
-----------|--------
     1     | Active
```

3. **Place a bid and let auction expire:**

4. **Check payment status:**
```sql
SELECT "PaymentId", "Status", "AttemptNumber" 
FROM payment_attempts;

-- Should show:
 PaymentId |  Status  | AttemptNumber
-----------|----------|---------------
     1     | Pending  |       1
```

5. **Verify string values (not integers):**
```sql
-- This should work
SELECT * FROM auctions WHERE "Status" = 'Active';

-- This should return nothing
SELECT * FROM auctions WHERE "Status" = '0';
```

---

## Application Behavior

### API Responses (Unchanged)

The API JSON responses **remain the same** because:
- C# enums still work the same way
- `.ToString()` converts enums to strings
- Only **database storage** changed

**Example Response:**
```json
{
  "auctionId": 1,
  "productId": 1,
  "status": "Active",        // ? Still string in JSON
  "expiryTime": "2024-01-15T10:00:00Z"
}
```

### Code Behavior (Unchanged)

Your application code works **exactly the same**:

```csharp
// This still works
if (auction.Status == AuctionStatus.Active)
{
    // do something
}

// This still works
auction.Status = AuctionStatus.Completed;
await _repository.UpdateAsync(auction);
```

**EF Core handles the conversion automatically!**

---

## Benefits Summary

### URL Change Benefits
? **Cleaner URLs** - No `/swagger` suffix  
? **Professional** - Root URL looks better  
? **Consistent** - Swagger is at root, URL matches  

### Enum String Storage Benefits
? **Readable Database** - Direct SQL queries easier  
? **Better Debugging** - See actual status values  
? **Safer Migrations** - Less risk of data corruption  
? **Database Tools** - Easier to use pgAdmin, DBeaver, etc.  
? **Reports/Analytics** - No need to decode integers  

---

## Rollback (If Needed)

### Rollback URL Change
Edit `Properties/launchSettings.json`:
```json
"launchUrl": "swagger"
```

### Rollback Enum Storage
```bash
cd codebase
dotnet ef migrations remove
```

Then remove `.HasConversion<string>()` from `ApplicationDbContext.cs`.

---

## Verification Checklist

After applying changes:

- [ ] Application runs: `dotnet run`
- [ ] Browser opens to `https://localhost:7019/`
- [ ] Swagger UI loads immediately
- [ ] Can create product/auction
- [ ] Database shows string values:
  ```sql
  SELECT "Status" FROM auctions;  -- Shows "Active"
  SELECT "Status" FROM payment_attempts;  -- Shows "Pending"
  ```
- [ ] SQL queries work with strings:
  ```sql
  SELECT * FROM auctions WHERE "Status" = 'Active';
  ```
- [ ] Application functionality unchanged
- [ ] No errors in console logs

---

## Common Issues

### Issue: Migration Warning
```
An operation was scaffolded that may result in the loss of data.
```

**Explanation:** This is normal. EF Core warns when changing column types. The migration handles data conversion safely.

### Issue: Can't find old integer values
```sql
SELECT * FROM auctions WHERE "Status" = 0;  -- Returns nothing
```

**Solution:** Use string values:
```sql
SELECT * FROM auctions WHERE "Status" = 'Active';
```

### Issue: Browser still opens to /swagger
**Solution:** Restart Visual Studio or clear browser cache.

---

## Summary

### Changes Made
1. ? **URL:** Default is now `localhost:7019` (without /swagger)
2. ? **Enums:** Stored as strings (`Active`, `Pending`, etc.) in database

### Files Modified
1. ? `Properties/launchSettings.json` - Updated launchUrl
2. ? `Data/ApplicationDbContext.cs` - Added string conversion for enums

### Migration Created
? `ConvertEnumsToStrings` - Converts enum columns to strings

### Breaking Changes
?? **SQL Queries:** Must use string values instead of integers

**Old SQL:**
```sql
WHERE "Status" = 0
```

**New SQL:**
```sql
WHERE "Status" = 'Active'
```

### Non-Breaking
? Application code unchanged  
? API responses unchanged  
? C# enum usage unchanged  
? Only database storage changed  

---

**Status:** ? Complete and ready to deploy  
**Migration:** ? Created and ready to apply  
**Testing:** ? Recommended before production  
**Rollback:** ? Possible if needed
