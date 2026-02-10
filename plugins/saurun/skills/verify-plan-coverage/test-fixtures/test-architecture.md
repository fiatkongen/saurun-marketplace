## Entity Model
- Upload (Aggregate Root, Rich) — behaviors: Create(), Validate(), Process(), MarkFailed()
- UploadError (Owned, Anemic)

## API Contract
- POST /uploads — CreateUploadRequest → UploadDto (201)
- GET /uploads/{id} — → UploadDto (200)
- DELETE /uploads/{id} — → 204

## Component Tree
- UploadPage
- UploadForm
- UploadList
- UploadStatus
