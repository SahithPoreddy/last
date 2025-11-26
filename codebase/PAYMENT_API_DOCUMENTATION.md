# Payment API Endpoint Documentation

## New Payment Confirmation Endpoint

A dedicated payment API has been created to make payment confirmation easier and more intuitive using Auction ID directly.

---

## API Endpoints

### 1. **POST /api/payments/confirm** - Confirm Payment

Confirm payment for an auction by providing the auction ID and payment amount.

#### **Endpoint:**
```
POST /api/payments/confirm
```

#### **Authorization:**
- **Required:** Yes
- **Roles:** User

#### **Request Body:**
```json
{
  "auctionId": 1,
  "amount": 600.00
}
```

#### **Request Schema:**

| Field | Type | Required | Validation | Description |
|-------|------|----------|------------|-------------|
| `auctionId` | integer | Yes | >= 1 | The auction ID from the email |
| `amount` | decimal | Yes | > 0 | The exact bid amount to pay |

#### **Success Response (200 OK):**
```json
{
  "paymentId": 1,
  "auctionId": 1,
  "bidderId": 5,
  "bidderEmail": "user@example.com",
  "status": "Success",
  "attemptNumber": 1,
  "attemptTime": "2024-01-15T10:00:00Z",
  "windowExpiryTime": "2024-01-15T10:01:00Z",
  "confirmedAmount": 600.00
}
```

#### **Error Responses:**

**400 Bad Request - Amount Mismatch:**
```json
{
  "paymentId": 1,
  "auctionId": 1,
  "status": "Failed",
  "message": "Payment amount does not match the winning bid"
}
```

**400 Bad Request - Payment Window Expired:**
```json
{
  "message": "Payment window has expired"
}
```

**403 Forbidden - Not Authorized:**
```json
{
  "message": "You are not authorized to confirm payment for this auction"
}
```

**404 Not Found - Auction Not Found:**
```json
{
  "message": "Auction not found"
}
```

**404 Not Found - No Pending Payment:**
```json
{
  "message": "No pending payment attempt found for this auction"
}
```

---

### 2. **GET /api/payments/auction/{auctionId}** - Get Payment Status

Get the current payment status for a specific auction.

#### **Endpoint:**
```
GET /api/payments/auction/{auctionId}
```

#### **Authorization:**
- **Required:** Yes
- **Roles:** User, Admin, Guest

#### **Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `auctionId` | integer | The auction ID |

#### **Success Response (200 OK):**
```json
{
  "paymentId": 1,
  "auctionId": 1,
  "bidderId": 5,
  "bidderEmail": "user@example.com",
  "status": "Pending",
  "attemptNumber": 1,
  "attemptTime": "2024-01-15T10:00:00Z",
  "windowExpiryTime": "2024-01-15T10:01:00Z",
  "confirmedAmount": null
}
```

#### **Error Response:**

**404 Not Found:**
```json
{
  "message": "No payment attempt found for this auction"
}
```

---

### 3. **GET /api/payments/auction/{auctionId}/history** - Get Payment History

Get all payment attempts for a specific auction (Admin only).

#### **Endpoint:**
```
GET /api/payments/auction/{auctionId}/history
```

#### **Authorization:**
- **Required:** Yes
- **Roles:** Admin

#### **Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `auctionId` | integer | The auction ID |

#### **Success Response (200 OK):**
```json
[
  {
    "paymentId": 1,
    "auctionId": 1,
    "bidderId": 5,
    "bidderEmail": "user1@example.com",
    "status": "Failed",
    "attemptNumber": 1,
    "attemptTime": "2024-01-15T10:00:00Z",
    "windowExpiryTime": "2024-01-15T10:01:00Z",
    "confirmedAmount": 650.00
  },
  {
    "paymentId": 2,
    "auctionId": 1,
    "bidderId": 3,
    "bidderEmail": "user2@example.com",
    "status": "Success",
    "attemptNumber": 2,
    "attemptTime": "2024-01-15T10:05:00Z",
    "windowExpiryTime": "2024-01-15T10:06:00Z",
    "confirmedAmount": 600.00
  }
]
```

---

## How It Works

### Payment Flow

1. **Auction Ends**
   - System identifies the highest bidder
   - Creates a payment attempt with status "Pending"
   - Sends email notification to the winner

2. **Winner Receives Email**
   - Email contains:
     - Auction ID
     - Winning bid amount
     - Payment deadline (1 minute by default)

3. **Winner Confirms Payment**
   - Calls `POST /api/payments/confirm`
   - Provides auction ID and exact amount from email
   
4. **System Validates Payment**
   - Checks if user is authorized
   - Checks if payment window is still valid
   - Verifies amount matches winning bid
   
5. **Payment Processed**
   - **Success:** Status changes to "Success", auction marked "Completed"
   - **Failed:** Status changes to "Failed", retry with next bidder

---

## Usage Examples

### Example 1: Successful Payment

**Email Received:**
```
Subject: Payment Required for Auction #1

You won the auction for "Vintage Watch"!
Auction ID: 1
Winning Bid: $600.00
Payment Deadline: 2024-01-15 10:01:00 UTC

Please confirm your payment within 1 minute.
```

**API Call:**
```bash
POST /api/payments/confirm
Authorization: Bearer <your-token>

{
  "auctionId": 1,
  "amount": 600.00
}
```

**Response:**
```json
{
  "paymentId": 1,
  "auctionId": 1,
  "bidderId": 5,
  "bidderEmail": "user@example.com",
  "status": "Success",
  "attemptNumber": 1,
  "attemptTime": "2024-01-15T10:00:00Z",
  "windowExpiryTime": "2024-01-15T10:01:00Z",
  "confirmedAmount": 600.00
}
```

**Result:** ? Payment confirmed! Auction completed.

---

### Example 2: Wrong Amount

**API Call:**
```bash
POST /api/payments/confirm
Authorization: Bearer <your-token>

{
  "auctionId": 1,
  "amount": 500.00  // ? Wrong amount!
}
```

**Response:**
```json
{
  "paymentId": 1,
  "auctionId": 1,
  "status": "Failed",
  "message": "Payment amount does not match the winning bid"
}
```

**Result:** ? Payment failed! System retries with next bidder.

---

### Example 3: Check Payment Status

**API Call:**
```bash
GET /api/payments/auction/1
Authorization: Bearer <your-token>
```

**Response:**
```json
{
  "paymentId": 1,
  "auctionId": 1,
  "bidderId": 5,
  "bidderEmail": "user@example.com",
  "status": "Pending",
  "attemptNumber": 1,
  "attemptTime": "2024-01-15T10:00:00Z",
  "windowExpiryTime": "2024-01-15T10:01:00Z",
  "confirmedAmount": null
}
```

---

## Payment Status Values

| Status | Description |
|--------|-------------|
| `Pending` | Payment is awaiting confirmation |
| `Success` | Payment confirmed successfully |
| `Failed` | Payment failed (wrong amount, expired, etc.) |

---

## Swagger UI Testing

### 1. Login and Get Token
```
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "User@123"
}
```

Copy the `token` from response.

### 2. Authorize in Swagger
Click "Authorize" button, enter: `Bearer <your-token>`

### 3. Confirm Payment
Go to `POST /api/payments/confirm`:

```json
{
  "auctionId": 1,
  "amount": 600.00
}
```

Click "Execute"

### 4. Check Payment Status
Go to `GET /api/payments/auction/{auctionId}`:

Enter auction ID: `1`

Click "Execute"

---

## Validation Rules

### Amount Validation
- ? Must match the exact winning bid amount
- ? Must be greater than 0
- ? Decimal precision: 2 decimal places

### Auction ID Validation
- ? Must exist in database
- ? Must be a valid integer >= 1
- ? Auction must have pending payment

### Authorization
- ? User must be logged in
- ? User must be the current pending payment bidder
- ? Other users cannot confirm payment

### Timing
- ? Must be within payment window (1 minute by default)
- ? Expired payments automatically failed

---

## Error Handling

### Common Errors

**1. Amount Mismatch**
```
Winning bid: $600.00
Your payment: $599.99
? Result: Failed - amounts must match exactly
```

**2. Payment Window Expired**
```
Window: 10:00:00 - 10:01:00
Your payment: 10:01:30
? Result: Failed - payment window expired
```

**3. Wrong User**
```
Pending payment for: user1@example.com
You are: user2@example.com
? Result: Forbidden - not authorized
```

**4. No Pending Payment**
```
Auction status: Completed
? Result: No pending payment found
```

---

## Database Changes

### Payment Status Storage

After the enum-to-string conversion, statuses are now stored as:

**Before:**
```sql
SELECT "Status" FROM payment_attempts;
-- 0, 1, 2
```

**After:**
```sql
SELECT "Status" FROM payment_attempts;
-- 'Pending', 'Success', 'Failed'
```

### Query Example:
```sql
-- Get all pending payments
SELECT * FROM payment_attempts WHERE "Status" = 'Pending';

-- Get successful payments
SELECT * FROM payment_attempts WHERE "Status" = 'Success';

-- Get failed payments
SELECT * FROM payment_attempts WHERE "Status" = 'Failed';
```

---

## Configuration

### Payment Window (appsettings.json)

```json
{
  "AuctionSettings": {
    "PaymentWindowMinutes": 1
  }
}
```

Change to 5 minutes:
```json
"PaymentWindowMinutes": 5
```

### Max Retry Attempts

```json
{
  "AuctionSettings": {
    "MaxPaymentAttempts": 3
  }
}
```

---

## Testing Scenarios

### Scenario 1: Happy Path
1. Create product and auction
2. Place bid and win
3. Receive email with auction ID and amount
4. Call payment API with correct details
5. ? Payment succeeds, auction completes

### Scenario 2: Wrong Amount
1. Receive email: Amount = $600.00
2. Call API with: Amount = $500.00
3. ? Payment fails
4. System retries with next bidder

### Scenario 3: Late Payment
1. Receive email at 10:00:00
2. Payment window: 1 minute (until 10:01:00)
3. Call API at 10:01:30
4. ? Payment fails (window expired)
5. System retries with next bidder

### Scenario 4: Check Status
1. Payment pending
2. Call `GET /api/payments/auction/1`
3. See current payment status
4. Confirm payment
5. Call status endpoint again
6. ? Status now "Success"

---

## Comparison: Old vs New Endpoint

### Old Endpoint (Still Works)
```
PUT /api/products/{productId}/confirm
{
  "confirmedAmount": 600.00
}
```
- Uses Product ID
- Less intuitive
- Still functional

### New Endpoint (Recommended)
```
POST /api/payments/confirm
{
  "auctionId": 1,
  "amount": 600.00
}
```
- Uses Auction ID (from email)
- More intuitive
- Clearer purpose
- Better naming

**Recommendation:** Use the new `/api/payments/confirm` endpoint.

---

## Summary

### ? What's New
- Dedicated payment controller
- Auction ID-based payment confirmation
- Payment status checking endpoint
- Payment history endpoint (admin)

### ? Benefits
- Easier to use with email information
- Direct auction ID usage
- Clear API structure
- Better separation of concerns

### ? Endpoints
1. `POST /api/payments/confirm` - Confirm payment
2. `GET /api/payments/auction/{id}` - Check status
3. `GET /api/payments/auction/{id}/history` - View history (admin)

---

## Quick Reference

### Confirm Payment
```bash
POST /api/payments/confirm
Authorization: Bearer <token>

{
  "auctionId": 1,
  "amount": 600.00
}
```

### Check Status
```bash
GET /api/payments/auction/1
Authorization: Bearer <token>
```

### View History (Admin)
```bash
GET /api/payments/auction/1/history
Authorization: Bearer <admin-token>
```

---

**Status:** ? Ready to use
**Files Created:** 
- `Controllers/PaymentsController.cs`
- `Models/DTOs/PaymentDTOs.cs` (updated)

**Testing:** Available in Swagger UI at `/api/payments`
