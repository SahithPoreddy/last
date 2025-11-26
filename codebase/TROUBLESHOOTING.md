# Troubleshooting Guide

Common issues and solutions for BidSphere application.

## Database Issues

### Error: "Could not connect to database"

**Symptoms:**
```
Npgsql.NpgsqlException: Connection refused
```

**Solutions:**
1. Check PostgreSQL is running:
   ```bash
   # Windows
   services.msc ? PostgreSQL service ? Start
   
   # macOS
   brew services start postgresql@14
   
   # Linux
   sudo systemctl start postgresql
   sudo systemctl status postgresql
   ```

2. Verify connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=bidsphere;Username=postgres;Password=your_password"
   }
   ```

3. Test connection manually:
   ```bash
   psql -U postgres -d bidsphere
   ```

### Error: "Database 'bidsphere' does not exist"

**Solution:**
```bash
# Create database
createdb -U postgres bidsphere

# Or via psql
psql -U postgres
CREATE DATABASE bidsphere;
\q
```

### Error: "Pending model changes"

**Symptoms:**
```
Your target database is not up to date. Run migrations.
```

**Solution:**
```bash
# Check pending migrations
dotnet ef migrations list

# Apply migrations
dotnet ef database update

# If issues persist, reset:
dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Error: "Relation does not exist"

**Symptoms:**
```
Npgsql.PostgresException: relation "Users" does not exist
```

**Solution:**
```bash
# Ensure migrations are applied
dotnet ef database update

# Check tables exist
psql -U postgres -d bidsphere
\dt
```

## Authentication Issues

### Error: "No authenticationScheme was specified"

**Solution:**
Add `[Authorize]` attribute after authentication is configured:
```csharp
[Authorize]
[HttpGet("profile")]
public async Task<ActionResult> GetProfile() { }
```

### Error: "Unauthorized" when calling protected endpoints

**Symptoms:**
```
401 Unauthorized
```

**Solutions:**
1. Ensure you're logged in and have valid token
2. Add token to request header:
   ```
   Authorization: Bearer <your-token>
   ```

3. In Swagger UI:
   - Click "Authorize" button
   - Enter: `Bearer <your-token>`
   - Click "Authorize"

4. Check token hasn't expired (24 hours default)

### Error: "Invalid token"

**Solutions:**
1. Token expired - Login again
2. Wrong secret key in appsettings.json
3. Token format incorrect - should be `Bearer <token>`

### Error: "Forbidden" (403)

**Symptoms:**
```
403 Forbidden
```

**Causes:**
- User role doesn't match endpoint requirements
- Admin endpoint accessed by User role

**Solution:**
Update user role in database:
```sql
UPDATE "Users" SET "Role" = 2 WHERE "Email" = 'admin@example.com';
-- Role: 0=Guest, 1=User, 2=Admin
```

## Build Issues

### Error: "Package restore failed"

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Rebuild
dotnet clean
dotnet build
```

### Error: "Type or namespace could not be found"

**Solutions:**
1. Check using statements
2. Restore packages: `dotnet restore`
3. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet build
   ```

### Error: "The type exists in both"

**Solution:**
Duplicate package references - check `.csproj` for duplicates and remove.

## Runtime Issues

### Error: "Port already in use"

**Symptoms:**
```
Failed to bind to address https://127.0.0.1:7xxx: address already in use
```

**Solution:**
1. Change port in `launchSettings.json` (if exists)
2. Kill process using port:
   ```bash
   # Windows
   netstat -ano | findstr :7xxx
   taskkill /PID <process-id> /F
   
   # macOS/Linux
   lsof -i :7xxx
   kill -9 <process-id>
   ```

### Error: "Unable to resolve service"

**Symptoms:**
```
InvalidOperationException: Unable to resolve service for type 'IAuthService'
```

**Solution:**
Ensure service is registered in `Program.cs`:
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

### Background Services Not Running

**Symptoms:**
- Auctions not finalizing automatically
- Payment retries not happening

**Solutions:**
1. Check services are registered in `Program.cs`:
   ```csharp
   builder.Services.AddHostedService<AuctionExpiryMonitor>();
   builder.Services.AddHostedService<PaymentRetryService>();
   ```

2. Check logs for errors:
   ```bash
   # Enable detailed logging
   "Logging": {
     "LogLevel": {
       "Default": "Information"
     }
   }
   ```

3. Verify interval settings in `appsettings.json`:
   ```json
   "AuctionSettings": {
     "AuctionMonitorIntervalSeconds": 10,
     "PaymentMonitorIntervalSeconds": 5
   }
   ```

## API Issues

### Error: "Bad Request" when creating product

**Symptoms:**
```
400 Bad Request
{
  "error": "Validation failed",
  "errors": {
    "StartingPrice": ["Starting price must be greater than 0"]
  }
}
```

**Solution:**
Check request body matches validation rules:
- StartingPrice > 0
- AuctionDuration between 2-1440 minutes
- All required fields present

### Error: "Cannot modify product with bids"

**Symptoms:**
```
400 Bad Request
{ "error": "Cannot modify/delete product with active bids" }
```

**Solution:**
Products with bids cannot be modified. Either:
1. Wait for auction to complete
2. Use Admin force finalize: `PUT /api/products/{id}/finalize`

### Error: "Bid amount too low"

**Symptoms:**
```
400 Bad Request
{ "error": "Bid amount must be greater than current highest bid of 150" }
```

**Solution:**
1. Get current auction details: `GET /api/products/{id}`
2. Check `highestBidAmount`
3. Place bid higher than that amount

### Excel Upload Issues

**Error: "No worksheet found"**

**Solution:**
Ensure Excel file:
- Is .xlsx format (not .xls)
- Has at least one worksheet
- Has proper structure

**Error: "Invalid starting price"**

**Solution:**
Excel format requirements:
```
Column Headers (Row 1):
ProductId | Name | StartingPrice | Description | Category | DurationMinutes

Sample Data (Row 2):
1 | Product Name | 100.00 | Description here | Electronics | 60
```

## Email Issues

### Emails Not Sending

**Symptoms:**
- No email notifications received
- No errors in logs

**Solutions:**
1. Configure SMTP settings in `appsettings.json`:
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

2. For Gmail, use App Password:
   - Go to Google Account ? Security
   - Enable 2-Step Verification
   - Generate App Password
   - Use App Password in config

3. Check logs for email errors

4. Test SMTP connection:
   ```bash
   telnet smtp.gmail.com 587
   ```

### Error: "SMTP Authentication failed"

**Solutions:**
1. Verify username/password correct
2. Use App Password for Gmail
3. Enable "Less secure app access" (not recommended)
4. Check firewall/antivirus blocking SMTP

## Payment Issues

### Payment Window Expired

**Symptoms:**
- Cannot confirm payment
- Window already passed

**Explanation:**
Payment windows are 1 minute (configurable). If expired, system automatically retries with next bidder.

**Solution:**
Configure longer window in `appsettings.json`:
```json
"AuctionSettings": {
  "PaymentWindowMinutes": 5
}
```

### Amount Mismatch Error

**Symptoms:**
```
Payment amount mismatch. Expected: 500, Got: 450
```

**Solution:**
Confirmed amount must EXACTLY match highest bid amount:
1. Get auction details: `GET /api/products/{id}`
2. Note `highestBidAmount`
3. Confirm with exact amount

### Retry Not Happening

**Symptoms:**
- Payment failed but no retry initiated

**Solutions:**
1. Check PaymentRetryService is running
2. Verify max attempts not exceeded (default: 3)
3. Check logs for errors
4. Ensure next bidder exists

## Swagger Issues

### Swagger UI Not Loading

**Symptoms:**
- 404 on root URL
- Swagger page not found

**Solutions:**
1. Ensure Swagger configured in `Program.cs`:
   ```csharp
   if (app.Environment.IsDevelopment())
   {
       app.UseSwagger();
       app.UseSwaggerUI(c =>
       {
           c.SwaggerEndpoint("/swagger/v1/swagger.json", "BidSphere API v1");
           c.RoutePrefix = string.Empty;
       });
   }
   ```

2. Navigate to root URL: `https://localhost:7xxx/`

3. If not working, try: `https://localhost:7xxx/swagger`

### API Methods Not Showing in Swagger

**Solutions:**
1. Ensure controller has `[ApiController]` attribute
2. Ensure methods are public
3. Rebuild project
4. Check for compilation errors

## Performance Issues

### Slow API Response

**Solutions:**
1. Enable database query logging:
   ```json
   "Logging": {
     "LogLevel": {
       "Microsoft.EntityFrameworkCore": "Information"
     }
   }
   ```

2. Check for N+1 queries - use `.Include()` for related data

3. Add database indexes if needed

4. Enable response caching for read-heavy endpoints

### High Memory Usage

**Solutions:**
1. Check for memory leaks in background services
2. Dispose DbContext properly (using `using` statements)
3. Limit query results with pagination
4. Profile application with dotnet-counters

## Common Business Logic Issues

### Anti-Sniping Not Working

**Symptoms:**
- Auction not extending when bid placed near end

**Solutions:**
1. Check threshold setting:
   ```json
   "AuctionSettings": {
     "AntiSnipingThresholdSeconds": 60
   }
   ```

2. Verify bid timestamp vs expiry time
3. Check logs for extension events

### Dashboard Showing Wrong Counts

**Solutions:**
1. Check auction status values:
   - Active = 0
   - Completed = 1
   - Failed = 2
   - PendingPayment = 3

2. Verify database data
3. Check query logic in PaymentService

## Development Issues

### Hot Reload Not Working

**Solution:**
```bash
dotnet watch run
```

### Cannot Debug

**Solutions:**
1. Check launch settings
2. Ensure debugger attached
3. Set breakpoints in code
4. Check "Debug" configuration selected

### Environment Variables Not Loading

**Solution:**
Create `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=bidsphere_dev;Username=postgres;Password=dev_password"
  }
}
```

## Getting Help

If you're still stuck:

1. **Check Logs**
   - Console output
   - Application logs
   - Database logs

2. **Enable Detailed Errors**
   ```csharp
   app.UseDeveloperExceptionPage();
   ```

3. **Check Documentation**
   - README.md
   - API_TESTING_GUIDE.md
   - DATABASE_SETUP.md

4. **Verify Configuration**
   - appsettings.json
   - Connection strings
   - Service registration

5. **Test Components Individually**
   - Test database connection
   - Test authentication
   - Test single endpoint

## Quick Diagnostic Commands

```bash
# Check .NET version
dotnet --version

# Check if PostgreSQL running
pg_isready -h localhost -p 5432

# Test database connection
psql -U postgres -d bidsphere -c "SELECT 1;"

# View migrations
dotnet ef migrations list

# Check application is running
curl https://localhost:7xxx/api/products

# View all logs
dotnet run --verbosity detailed
```

## Reset Everything

If all else fails, complete reset:

```bash
# 1. Stop application
Ctrl + C

# 2. Drop database
psql -U postgres
DROP DATABASE bidsphere;
CREATE DATABASE bidsphere;
\q

# 3. Clean solution
dotnet clean

# 4. Remove migrations
rm -rf Migrations/

# 5. Restore packages
dotnet restore

# 6. Rebuild
dotnet build

# 7. Create fresh migration
dotnet ef migrations add InitialCreate

# 8. Apply migration
dotnet ef database update

# 9. Run application
dotnet run
```

Remember: Always check logs first - they usually contain the exact error message and stack trace!
