# Excel Upload Template

Create an Excel file (.xlsx) with the following structure:

## Column Headers (Row 1)
| ProductId | Name | StartingPrice | Description | Category | DurationMinutes |

## Sample Data (Rows 2+)
| 1 | Vintage Watch | 500.00 | Beautiful vintage watch from 1920s | Collectibles | 60 |
| 2 | Gaming Laptop | 1200.00 | High-performance gaming laptop | Electronics | 120 |
| 3 | Oil Painting | 800.00 | Original oil painting by local artist | Art | 180 |
| 4 | Designer Handbag | 350.00 | Authentic designer handbag | Fashion | 90 |
| 5 | Rare Book | 250.00 | First edition rare book | Books | 60 |

## Validation Rules
- **ProductId**: Must be unique (integer)
- **Name**: Required, max 255 characters
- **StartingPrice**: Required, must be greater than 0 (decimal)
- **Description**: Required, max 2000 characters
- **Category**: Required, max 100 characters
- **DurationMinutes**: Required, must be between 2 and 1440 (2 minutes to 24 hours)

## Notes
- All columns are required
- Invalid rows will be skipped and logged in the response
- Successful and failed counts will be returned
