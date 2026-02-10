## Scope
### In Scope
1. Users can upload CSV files up to 10MB
2. Retry failed uploads 3 times with exponential backoff (1s, 2s, 4s)
3. Return HTTP 413 if file exceeds size limit
4. Admin users can upload up to 50MB
5. Rate limit: max 100 uploads per hour per user

## Entities
- Upload (id, userId, fileName, fileSize, status, createdAt)
- UploadError (id, uploadId, errorMessage, retryCount)

## User Flows
### Upload CSV
1. User selects CSV file
2. System validates file size and format
3. System uploads and processes file
4. System shows success/error message
