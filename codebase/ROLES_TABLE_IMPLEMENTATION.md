# Roles Table Implementation

## Overview
The application has been updated to use a separate `roles` table instead of storing roles as an enum in the `users` table. This provides better flexibility for role management and allows users to specify their role during registration.

## Changes Made

### 1. New Database Table: `roles`

| Column | Type | Description |
|--------|------|-------------|
| RoleId | int | Primary key (auto-increment) |
| UserId | int | Foreign key to users table |
| RoleName | string(50) | Role name (Admin, User, Guest) |
| AssignedAt | timestamp | When role was assigned |

**Constraints:**
- Unique index on (UserId, RoleName) - prevents duplicate role assignments
- Foreign key to users table with CASCADE delete

### 2. Updated User Entity
**Removed:**
- `Role` enum property

**Added:**
- `Roles` navigation property (collection of Role entities)

### 3. Updated Registration Process

**New Registration Request:**
```json
{
  "email": "user@example.com",
  "password": "Password123",
  "role": "User"
}
```

**Valid Roles:**
- `Admin` - Full administrative access
- `User` - Standard user access (default)
- `Guest` - Read-only access

**Validation:**
- Role field is required
- Must be one of: Admin, User, or Guest
- Invalid roles return 400 Bad Request

### 4. Updated Authentication Response

**Before:**
```json
{
  "userId": 1,
  "email": "user@example.com",
  "role": "User",
  "token": "..."
}
```

**After:**
```json
{
  "userId": 1,
  "email": "user@example.com",
  "roles": ["User"],
  "token": "..."
}
```

**Note:** Roles is now an array, allowing for future multiple role assignments.

## API Changes

### POST /api/auth/register

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "SecurePass123",
  "role": "User"
}
```

**Response:**
```json
{
  "userId": 5,
  "email": "newuser@example.com",
  "roles": ["User"],
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Validation Errors:**
```json
// Invalid role
{
  "error": "Role must be Admin, User, or Guest",
  "statusCode": 400
}

// Missing role
{
  "error": "The Role field is required.",
  "statusCode": 400
}
```

### POST /api/auth/login

**Response (unchanged structure but roles is now array):**
```json
{
  "userId": 1,
  "email": "user@example.com",
  "roles": ["User"],
  "token": "..."
}
```

### GET /api/auth/profile

**Response:**
```json
{
  "userId": 1,
  "email": "user@example.com",
  "roles": ["Admin"],
  "createdAt": "2024-01-15T10:30:00Z"
}
```

## Database Migration

### Migration: `AddRolesTable`

**Creates:**
- `roles` table
- Unique index on (UserId, RoleName)
- Foreign key relationship

**Migrates:**
- Moves existing role data from users table to roles table
- Drops Role column from users table

### How to Apply

**Option 1: Automatic (Recommended)**
```bash
dotnet run
```
Migration applies automatically on startup.

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

# Run app
dotnet run
```

## SQL Queries

### View User Roles
```sql
SELECT u."Email", r."RoleName", r."AssignedAt"
FROM users u
JOIN roles r ON u."UserId" = r."UserId";
```

### Check User's Roles
```sql
SELECT "RoleName" 
FROM roles 
WHERE "UserId" = 1;
```

### Assign Additional Role (Manual)
```sql
INSERT INTO roles ("UserId", "RoleName", "AssignedAt")
VALUES (1, 'Admin', NOW());
```

### Remove Role (Manual)
```sql
DELETE FROM roles 
WHERE "UserId" = 1 AND "RoleName" = 'Guest';
```

### Count Users by Role
```sql
SELECT "RoleName", COUNT(DISTINCT "UserId") as UserCount
FROM roles
GROUP BY "RoleName";
```

## JWT Token Changes

### Token Claims

**Before:**
```
{
  "nameid": "1",
  "email": "user@example.com",
  "role": "User",
  "jti": "..."
}
```

**After:**
```
{
  "nameid": "1",
  "email": "user@example.com",
  "role": ["User"],
  "jti": "..."
}
```

**Note:** Role claim can now contain multiple values if user has multiple roles.

### Authorization

Role-based authorization still works the same:
```csharp
[Authorize(Roles = "Admin")]
[Authorize(Roles = "User")]
[Authorize(Roles = "Admin,User")] // Either role
```

## Testing

### Test Registration with Role

**1. Register as Admin:**
```bash
POST /api/auth/register
{
  "email": "admin@test.com",
  "password": "Admin@123",
  "role": "Admin"
}
```

**2. Register as User (default):**
```bash
POST /api/auth/register
{
  "email": "user@test.com",
  "password": "User@123",
  "role": "User"
}
```

**3. Register as Guest:**
```bash
POST /api/auth/register
{
  "email": "guest@test.com",
  "password": "Guest@123",
  "role": "Guest"
}
```

**4. Test Invalid Role:**
```bash
POST /api/auth/register
{
  "email": "invalid@test.com",
  "password": "Pass@123",
  "role": "SuperAdmin"
}
# Returns: 400 Bad Request
```

### Verify in Database
```sql
-- Check user and their role
SELECT u."UserId", u."Email", r."RoleName"
FROM users u
LEFT JOIN roles r ON u."UserId" = r."UserId"
WHERE u."Email" = 'admin@test.com';
```

### Test Authorization
```bash
# 1. Login as admin
POST /api/auth/login
{ "email": "admin@test.com", "password": "Admin@123" }

# 2. Copy token

# 3. Try admin-only endpoint
POST /api/products
Authorization: Bearer <admin-token>
# Should work ?

# 4. Try with user token
POST /api/products
Authorization: Bearer <user-token>
# Should fail with 403 Forbidden ?
```

## Benefits

### ? Flexibility
- Users can specify role during registration
- No need to manually update database
- Future support for multiple roles per user

### ? Better Design
- Normalized database structure
- Follows relational database best practices
- Easier to audit role changes

### ? Scalability
- Easy to add new roles
- Can track when roles were assigned
- Can implement role expiration if needed

### ? Security
- Role validation during registration
- Roles stored separately from user credentials
- Can revoke roles without affecting user account

## Migration Considerations

### Data Migration
The migration automatically:
1. Creates roles table
2. Migrates existing roles from users table
3. Maintains referential integrity

### Rollback
If needed, you can rollback:
```bash
cd codebase
dotnet ef migrations remove
```

Then restore the old User entity with Role enum.

## Updated Documentation

Files updated:
- ? User.cs - Removed Role enum, added Roles collection
- ? Role.cs - New entity created
- ? ApplicationDbContext.cs - Added roles table configuration
- ? AuthDTOs.cs - Updated with role field and roles array
- ? AuthService.cs - Updated to use roles table
- ? IAuthService.cs - Updated interface
- ? RoleRepository.cs - New repository created
- ? IRoleRepository.cs - New interface created
- ? Program.cs - Registered IRoleRepository
- ? DatabaseSeeder.cs - Updated to use roles table

## Quick Reference

### Register User
```json
POST /api/auth/register
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "role": "User"
}
```

### Default Role
If role is not specified (though it's required now), it would default to "User".

### Valid Roles
- `Admin` - Administrative access
- `User` - Standard user (default)
- `Guest` - Read-only access

### Check User Roles
```sql
SELECT * FROM roles WHERE "UserId" = 1;
```

### View All Roles
```sql
SELECT * FROM roles ORDER BY "UserId", "RoleName";
```

## Next Steps

1. ? Apply migration: `dotnet run`
2. ? Test registration with different roles
3. ? Verify role-based authorization works
4. ? Update any custom SQL queries

## Common Issues

### Issue: "Role field is required"
**Solution:** Always include role in registration request.

### Issue: "Invalid role"
**Solution:** Use only: Admin, User, or Guest (case-sensitive).

### Issue: Roles not showing after migration
**Solution:** Check migration applied correctly:
```bash
dotnet ef migrations list
```

---

**Migration Status:** ? Ready to apply
**Breaking Changes:** ?? Yes - Registration API now requires role field
**Data Loss Risk:** ? None - Data migrated automatically
**Backward Compatibility:** ?? Client apps must update registration requests
