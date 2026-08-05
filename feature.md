# URL Shortener Features

## Technology Baseline

This feature scope assumes:

- C#
- ASP.NET Core Web API
- React with TypeScript
- Tailwind CSS
- PostgreSQL
- Entity Framework Core
- FluentValidation
- xUnit
- Playwright
- Swagger / OpenAPI

## 1. Assignment Requirements

| ID | Feature | Description | Priority |
|---|---|---|---|
| F-001 | Web application | Provide a browser-based URL shortening application | Must |
| F-002 | Landing-page input | Allow users to enter a long URL | Must |
| F-003 | URL validation | Accept only valid HTTP and HTTPS URLs | Must |
| F-004 | Shorten button | Initiate shortening when clicked | Must |
| F-005 | Unique short code | Generate a unique Base62 short code | Must |
| F-006 | Database persistence | Store the original URL and short code in PostgreSQL | Must |
| F-007 | Feedback view | Display the generated short URL | Must |
| F-008 | Shorter result | Ensure the complete short URL is shorter than the original | Must |
| F-009 | Redirect | Redirect the short URL to the original URL | Must |
| F-010 | GitHub submission | Submit the complete code through GitHub | Must |

## 2. Assignment Bonus Features

| ID | Feature | Description | Priority |
|---|---|---|---|
| F-011 | Unit tests | Test validation and business logic with xUnit | Bonus |
| F-012 | Integration tests | Test ASP.NET Core API endpoints | Bonus |
| F-013 | End-to-end test | Test one complete flow with Playwright | Bonus |
| F-014 | UI/UX | Provide a clear and intuitive interface | Bonus |
| F-015 | Mobile responsive | Work correctly on mobile devices | Bonus |

## 3. Recommended Submission Features

| ID | Feature | Description | Recommendation |
|---|---|---|---|
| F-016 | Copy link | Copy the short URL to the clipboard | Strongly recommended |
| F-017 | Open link | Open the generated URL in a new tab | Recommended |
| F-018 | Duplicate detection | Reuse an existing short URL for the same original URL | Recommended |
| F-019 | Click count | Track redirect count | Recommended |
| F-020 | Last accessed time | Store the latest redirect time | Optional |
| F-021 | Recent links | Display recently created links | Optional |
| F-022 | QR code | Generate a QR code for the short URL | Optional |
| F-023 | CI workflow | Run build and tests in GitHub Actions | Optional |
| F-024 | Production deployment | Deploy the API, frontend, and PostgreSQL database | Recommended |

## 4. Functional Requirements

### 4.1 Create Short URL

Acceptance criteria:

- Trim the submitted URL.
- Validate it on the backend.
- Accept only HTTP and HTTPS.
- Reject input longer than 2,048 characters.
- Generate a 7-character Base62 code.
- Enforce uniqueness in PostgreSQL.
- Build the complete short URL.
- Confirm it is shorter than the original.
- Store the mapping.
- Return the result as JSON.
- Display the result in the React UI.

Example:

```text
Original:
https://www.example.com/products/category/item?id=12345

Short:
https://sho.rt/aB3xY7
```

### 4.2 Redirect

When the short code exists:

- Load the mapping.
- Increment `VisitCount`.
- Update `LastAccessedAtUtc`.
- Return HTTP 302.
- Open the original destination.

When the code does not exist:

- Return HTTP 404.
- Show a user-friendly not-found response.

### 4.3 URL Validation

Accepted:

```text
https://example.com
http://example.com/path
https://example.com/path?name=value
```

Rejected:

```text
example
www.example.com
ftp://example.com
javascript:alert(1)
```

Recommended messages:

```text
Please enter a URL.
Please enter a valid HTTP or HTTPS URL.
The URL is too long.
This URL is already too short to shorten.
Unable to generate a short URL. Please try again.
```

### 4.4 Duplicate URL Handling

Recommended behaviour:

- Normalize the submitted URL.
- Search for an existing mapping.
- Return the existing short URL.
- Avoid duplicate rows.

### 4.5 Collision Handling

- Generate a random Base62 code.
- Attempt the insert.
- Catch the PostgreSQL unique-constraint error.
- Generate another code.
- Retry a limited number of times.
- Return a controlled error if all attempts fail.

## 5. UI Features

### Landing Page

- Application name
- Clear heading
- Brief description
- URL input
- Shorten button
- Validation message
- Loading state
- Success result
- Copy button
- Open-link action

### Result View

```text
Your shortened URL is ready

https://sho.rt/aB3xY7

[Copy] [Open]
```

### Mobile Behaviour

- One-column layout
- Full-width input
- Full-width button
- Wrapping for long text
- Large touch targets
- No horizontal scrolling

### Accessibility

- Visible label
- Keyboard submission
- Focus states
- `aria-live` feedback
- Adequate contrast
- Clear error descriptions

## 6. Data Model

| Field | Type | Required | Description |
|---|---|---|---|
| Id | long | Yes | Internal primary key |
| OriginalUrl | string | Yes | Original destination |
| ShortCode | string | Yes | Unique Base62 code |
| CreatedAtUtc | DateTime | Yes | Creation time |
| VisitCount | long | Recommended | Redirect count |
| LastAccessedAtUtc | DateTime? | Optional | Latest redirect time |
| IsActive | bool | Future | Whether redirect is enabled |
| ExpiresAtUtc | DateTime? | Future | Expiry date |

Required database constraint:

```text
UNIQUE(ShortCode)
```

Recommended index:

```text
INDEX(OriginalUrl)
```

## 7. API Features

### Create URL

```http
POST /api/urls
```

Request:

```json
{
  "url": "https://www.example.com/a/very/long/path"
}
```

Response:

```json
{
  "originalUrl": "https://www.example.com/a/very/long/path",
  "shortCode": "aB3xY7",
  "shortUrl": "https://sho.rt/aB3xY7"
}
```

### Redirect

```http
GET /{shortCode}
```

Response:

```http
302 Found
Location: https://www.example.com/a/very/long/path
```

### Optional Link Details

```http
GET /api/urls/{shortCode}
```

### Optional Recent Links

```http
GET /api/urls?limit=10
```

## 8. Test Features

### xUnit Unit Tests

- Valid HTTP URL
- Valid HTTPS URL
- Empty URL
- Invalid URL
- Unsupported protocol
- Maximum URL length
- Base62 generation
- Code length
- Collision retry
- Duplicate URL reuse
- Shorter-length rule

### Integration Tests

- Valid request returns 201
- Invalid request returns 400
- Duplicate URL returns existing mapping
- Database failure returns a controlled error
- Unknown code returns 404
- Valid code returns redirect

### Playwright Test

One required flow:

```text
Open landing page
Enter a valid long URL
Submit
See the generated short URL
Open the short URL
Confirm redirect
```

Optional:

- Invalid URL
- Copy button
- Mobile viewport

## 9. Deployment Features

Recommended:

```text
ASP.NET Core API -> Azure App Service or Docker
React frontend -> Azure Static Web Apps, Vercel, or Netlify
PostgreSQL -> Managed PostgreSQL
```

Alternative:

```text
ASP.NET Core hosts the built React frontend
PostgreSQL is hosted separately
```

Deployment acceptance criteria:

- Environment variables are configured.
- EF Core migrations are applied.
- Production build succeeds.
- Production redirect works.
- No secrets are committed.

## 10. Future Bitly-Like Features

These are outside the assignment scope.

### Link Management

- Dashboard
- Search
- Filters
- Edit destination
- Delete link
- Archive
- Favorites
- Tags
- Folders
- Custom aliases

### Analytics

- Total clicks
- Click history
- Country
- Device
- Browser
- Operating system
- Referrer
- Date filters
- CSV export
- Unique visitors

### QR Codes

- Generate QR code
- Download PNG or SVG
- Custom design
- Scan analytics

### Link Controls

- Expiration
- Password protection
- One-time links
- Maximum clicks
- Scheduled activation
- Geographic restrictions
- Device restrictions

### Marketing

- UTM builder
- Campaigns
- Branded domains
- Deep links
- Geo-routing
- Device routing
- A/B routing

### Teams

- Authentication
- Workspaces
- Invitations
- Roles
- Audit logs
- Billing

### Integrations

- Public API
- API keys
- OAuth
- Webhooks
- Browser extension
- Bulk CSV shortening

### Security

- Malicious URL scanning
- Domain deny lists
- Safe Browsing integration
- Abuse reporting
- CAPTCHA
- Rate limiting
- Link suspension

## 11. Recommended Final Scope

Implement:

```text
Core assignment requirements
+ Responsive React and Tailwind UI
+ xUnit unit tests
+ ASP.NET Core integration tests
+ One Playwright end-to-end test
+ Copy button
+ Duplicate URL detection
+ Click count
```

Optional final enhancement:

```text
QR code
or
GitHub Actions
```

Avoid authentication, billing, team workspaces, advanced analytics, and bulk processing until all required features are complete.
