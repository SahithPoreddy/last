# API Testing Guide

## Getting Started

1. Run the application: `dotnet run`
2. Navigate to Swagger UI: `https://localhost:7xxx/`
3. All endpoints are documented with Swagger

## Authentication Flow

### 1. Register a New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "testuser@example.com",
  "password": "Test@123"
}
```

**Response:**
```json
{
  "userId": 1,
  "email": "testuser@example.com",
  "role": "User",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "testuser@example.com",
  "password": "Test@123"
}
```

### 3. Use JWT Token
Add the token to all authenticated requests:
```
Authorization: Bearer <your-token-here>
```

## Product Management (Admin Only)

### Create Product
```http
POST /api/products
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "Vintage Camera",
  "description": "Classic 35mm film camera from the 1980s",
  "category": "Electronics",
  "startingPrice": 250.50,
  "auctionDuration": 60
}
```

### Upload Products via Excel
```http
POST /api/products/upload
Authorization: Bearer <admin-token>
Content-Type: multipart/form-data

file: products.xlsx
```

### Get All Products
```http
GET /api/products
```

### Get Active Auctions
```http
GET /api/products/active
```

### Get Auction Details
```http
GET /api/products/1
```

### Update Product
```http
PUT /api/products/1
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "Updated Product Name",
  "startingPrice": 300.00
}
```

### Force Finalize Auction
```http
PUT /api/products/1/finalize
Authorization: Bearer <admin-token>
```

### Delete Product
```http
DELETE /api/products/1
Authorization: Bearer <admin-token>
```

## Bidding (User Only)

### Place a Bid
```http
POST /api/bids
Authorization: Bearer <user-token>
Content-Type: application/json

{
  "auctionId": 1,
  "amount": 300.00
}
```

### Get Bids for Auction
```http
GET /api/bids/1
```

### Get All Bids (Authenticated)
```http
GET /api/bids
Authorization: Bearer <token>
```

## Payment Confirmation

### Confirm Payment (Normal)
```http
PUT /api/products/1/confirm
Authorization: Bearer <winner-token>
Content-Type: application/json

{
  "confirmedAmount": 500.00
}
```

### Confirm Payment with Test Instant Fail
```http
PUT /api/products/1/confirm
Authorization: Bearer <winner-token>
Content-Type: application/json
X-Test-Instant-Fail: true

{
  "confirmedAmount": 500.00
}
```

## Transactions

### Get All Transactions
```http
GET /api/transactions
Authorization: Bearer <token>
```

## Dashboard (Admin Only)

### Get Dashboard Metrics
```http
GET /api/dashboard
Authorization: Bearer <admin-token>
```

**Response:**
```json
{
  "activeCount": 5,
  "pendingPayment": 2,
  "completedCount": 10,
  "failedCount": 1,
  "topBidders": [
    {
      "userId": 2,
      "email": "user1@example.com",
      "totalBids": 15,
      "auctionsWon": 5,
      "totalAmountSpent": 5000.00
    }
  ]
}
```

## Testing Scenarios

### Scenario 1: Complete Auction Flow

1. **Register Admin**
   ```json
   POST /api/auth/register
   { "email": "admin@test.com", "password": "Admin@123" }
   ```
   *Note: Manually update role to Admin in database*

2. **Create Product**
   ```json
   POST /api/products
   { "name": "Test Item", "description": "Test", "category": "Test", 
     "startingPrice": 100, "auctionDuration": 5 }
   ```

3. **Register User 1**
   ```json
   POST /api/auth/register
   { "email": "user1@test.com", "password": "User@123" }
   ```

4. **Register User 2**
   ```json
   POST /api/auth/register
   { "email": "user2@test.com", "password": "User@123" }
   ```

5. **User 1 Places Bid**
   ```json
   POST /api/bids
   { "auctionId": 1, "amount": 150 }
   ```

6. **User 2 Places Higher Bid**
   ```json
   POST /api/bids
   { "auctionId": 1, "amount": 200 }
   ```

7. **Wait for Auction to Expire** (5 minutes)

8. **Winner Receives Email Notification**

9. **Winner Confirms Payment**
   ```json
   PUT /api/products/1/confirm
   { "confirmedAmount": 200 }
   ```

### Scenario 2: Anti-Sniping Test

1. Create auction with 2-minute duration
2. Place bid at 1:30 remaining
3. Verify auction extends to 2:30
4. Place another bid at 1:55
5. Verify auction extends again

### Scenario 3: Payment Retry Test

1. Create auction and let it expire with bids
2. Winner gets 1-minute window
3. **Option A:** Winner confirms with wrong amount ? Retry next bidder
4. **Option B:** Use `X-Test-Instant-Fail: true` ? Instant retry
5. **Option C:** Wait for window to expire ? Automatic retry
6. After 3 failed attempts ? Auction marked as Failed

## Common Status Codes

- `200 OK` - Success
- `201 Created` - Resource created
- `204 No Content` - Successful deletion
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Duplicate resource

## Tips

1. **Use Swagger UI** for easy testing - it handles authentication automatically
2. **Copy JWT tokens** from login response to use in other requests
3. **Check logs** in console for background service activities
4. **Monitor database** to see real-time changes during auctions
5. **Test anti-sniping** with short auction durations (2-5 minutes)
6. **Test payment retry** with instant fail header for quick testing

## Troubleshooting

### "Unauthorized" Error
- Ensure you're logged in and using valid JWT token
- Check token hasn't expired (24 hours)
- Verify role matches endpoint requirements

### "Bid amount too low" Error
- Get current auction details first
- Bid must be higher than current highest bid

### "Cannot modify product with bids" Error
- Products can only be updated/deleted if no bids exist
- Use force finalize to complete auction first

### Payment Window Expired
- Payment must be confirmed within 1 minute
- System automatically retries with next bidder
