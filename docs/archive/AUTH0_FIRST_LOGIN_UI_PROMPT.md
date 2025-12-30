# Auth0 First-Login Flow Integration for Angular UI

## Context

The Angular 21 SPA **already has Auth0 authentication working**. The backend API has been enhanced with a first-login flow that automatically creates/updates user records in the database.

Your task is to integrate the new `GET /api/users/me` endpoint into the existing Angular app to trigger the first-login flow.

## What Already Exists

### Backend API ✅
- **New Endpoint:** `GET /api/users/me`
  - Automatically creates user record on first login
  - Updates user profile (email, name, LastLoginAt) on every subsequent login
  - Requires JWT Bearer token (already handled by existing auth)
  - Returns `UserDto` on success (200)
  - Returns 401 for invalid tokens (already handled)
  - Returns 503 when Auth0 `/userinfo` is unavailable

### UserDto Response Model
```typescript
interface UserDto {
  id: number;              // Database ID
  authProviderId: string;  // Auth0 sub claim
  provider: string;        // "auth0"
  email: string | null;
  name: string | null;
  lastLoginAt: string;     // ISO 8601 datetime
  createdAt: string;       // ISO 8601 datetime
  updatedAt: string | null;
}
```

### Frontend (Existing) ✅
- Auth0 authentication already configured and working
- HTTP interceptor already attaches JWT tokens to API requests
- User is already authenticated with Auth0

## What Needs to be Implemented

### Goal
Call `GET /api/users/me` after successful authentication to trigger the backend first-login flow and store the user profile in application state.

### Required Changes

1. **Create UserDto Model** (`src/app/models/user.model.ts`)
   - TypeScript interface matching backend response

2. **Create or Update UserService** (`src/app/services/user.service.ts`)
   - Method to call `GET /api/users/me`
   - Store user in signal
   - Handle 503 errors gracefully

3. **Integrate with Existing Auth**
   - Call `GET /api/users/me` after successful authentication
   - Store result in application state
   - Display user info in UI (optional)

4. **Error Handling**
   - Show toast notification if 503 (Auth0 unavailable)
   - Log errors for debugging

## Implementation Steps

### Step 1: Create User Model

**File:** `src/app/models/user.model.ts`

```typescript
export interface UserDto {
  id: number;
  authProviderId: string;
  provider: string;
  email: string | null;
  name: string | null;
  lastLoginAt: string;
  createdAt: string;
  updatedAt: string | null;
}
```

### Step 2: Create User Service

**File:** `src/app/services/user.service.ts`

```typescript
import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_URL } from '../config/api.config';
import { UserDto } from '../models/user.model';
import { catchError, tap, of } from 'rxjs';
import { ToastService } from './toast.service';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(API_URL);
  private readonly toastService = inject(ToastService);

  // Current user from database (not Auth0 profile)
  private readonly _currentUser = signal<UserDto | null>(null);
  readonly currentUser = this._currentUser.asReadonly();

  /**
   * Fetches current user profile from API.
   * Triggers first-login flow on backend (creates/updates user record).
   */
  fetchCurrentUser() {
    return this.http.get<UserDto>(`${this.apiUrl}/users/me`).pipe(
      tap(user => {
        this._currentUser.set(user);
        console.log('User profile loaded:', user);
      }),
      catchError(error => {
        if (error.status === 503) {
          this.toastService.error(
            'User profile service temporarily unavailable. Please try again later.'
          );
        } else {
          console.error('Failed to fetch user profile:', error);
        }
        this._currentUser.set(null);
        return of(null);
      })
    );
  }

  /**
   * Clears current user (call on logout)
   */
  clearUser(): void {
    this._currentUser.set(null);
  }
}
```

### Step 3: Integrate with Existing Auth

**Option A: If you have an AuthService**

Add this to your existing `AuthService`:

```typescript
import { UserService } from '../services/user.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly userService = inject(UserService);

  // ... existing code ...

  constructor() {
    // ... existing auth initialization ...

    // Call /api/users/me when authenticated
    this.isAuthenticated$.pipe(
      switchMap(isAuth => {
        if (isAuth) {
          // Trigger first-login flow
          return this.userService.fetchCurrentUser();
        } else {
          this.userService.clearUser();
          return of(null);
        }
      })
    ).subscribe();
  }
}
```

**Option B: If you're using Auth0Service directly in app.component.ts**

Add to `app.component.ts`:

```typescript
import { AuthService } from '@auth0/auth0-angular';
import { UserService } from './services/user.service';
import { switchMap, of } from 'rxjs';

export class AppComponent {
  private readonly auth0 = inject(AuthService);
  private readonly userService = inject(UserService);

  constructor() {
    // Trigger first-login flow when authenticated
    this.auth0.isAuthenticated$.pipe(
      switchMap(isAuth => {
        if (isAuth) {
          return this.userService.fetchCurrentUser();
        } else {
          this.userService.clearUser();
          return of(null);
        }
      })
    ).subscribe();
  }
}
```

**Option C: If you have a separate initialization service**

Call `userService.fetchCurrentUser()` wherever you handle successful authentication.

### Step 4: Display User Info (Optional)

If you want to display the database user info in your sidenav or header:

```typescript
// In your sidenav/header component
import { UserService } from '../../services/user.service';

export class SidenavComponent {
  readonly userService = inject(UserService);
}
```

```html
<!-- In template -->
@if (userService.currentUser(); as user) {
  <div class="user-info">
    <div class="user-name">{{ user.name || user.email }}</div>
    <small class="text-muted">Last login: {{ user.lastLoginAt | date:'short' }}</small>
  </div>
}
```

### Step 5: Handle Logout

Make sure to clear the user when logging out:

```typescript
logout(): void {
  this.userService.clearUser();
  this.auth0.logout({ /* ... */ });
}
```

## Files to Create/Modify

### New Files (2)
- `src/app/models/user.model.ts` - UserDto interface
- `src/app/services/user.service.ts` - User service with fetchCurrentUser()

### Modified Files (1-2)
- Your existing `AuthService` or `app.component.ts` - Call fetchCurrentUser() on auth
- Optional: `sidenav.component.ts` / `header.component.ts` - Display user info

## Testing Checklist

### First Login Flow
- [ ] Login with a new user (not in database)
- [ ] Check browser DevTools Network tab: `GET /api/users/me` returns 200
- [ ] Check browser console: "User profile loaded: {...}"
- [ ] Verify user record created in database (check with backend query)
- [ ] `userService.currentUser()` signal contains user data

### Subsequent Logins
- [ ] Login with existing user
- [ ] `GET /api/users/me` returns 200 with updated `lastLoginAt`
- [ ] Database record updated (check `last_login_at` timestamp)

### Error Handling
- [ ] Stop Auth0 service (or mock 503 response)
- [ ] Login triggers API call
- [ ] Toast error shown: "User profile service temporarily unavailable"
- [ ] App still functional (doesn't crash)

### Logout
- [ ] Logout clears `userService.currentUser()` signal
- [ ] Signal returns `null` after logout

## Key Points

### Why This is Needed
- The Auth0 user profile is stored in JWT tokens (id_token)
- The backend needs a database record for the user to link to domain entities (funds, accounts, transactions, etc.)
- The first-login flow ensures every authenticated user has a corresponding database record
- The `lastLoginAt` timestamp is updated on every login for audit purposes

### When the API Call Happens
- **Automatically** after successful Auth0 authentication
- Every time user logs in (not just first time)
- Updates user profile from Auth0 on every call

### UserDto vs Auth0 User Profile
- **Auth0 User Profile:** From `id_token`, managed by Auth0 SDK
- **UserDto:** From your database, includes `id`, `createdAt`, `lastLoginAt`
- Both should have same `email` and `name` (synced from Auth0)
- Use `UserDto` when you need the database ID or timestamps

### Error Scenarios

| Scenario | Behavior |
|----------|----------|
| 503 from API | Show toast error, continue without user profile |
| 401 from API | Existing interceptor handles (redirects to login) |
| Network error | Log error, continue without user profile |
| Successful call | Store user in signal, log success |

## Expected Network Traffic

After successful Auth0 login, you should see:

```
POST https://dev-7yj4txd3qg4xsckj.us.auth0.com/oauth/token
  → Returns access_token

GET https://localhost:7109/api/users/me
  → Request Headers: Authorization: Bearer <access_token>
  → Response 200: { id: 1, authProviderId: "auth0|...", email: "...", ... }
```

## Debugging

If the user profile isn't loading:

1. **Check Network Tab:** Is `GET /api/users/me` being called?
2. **Check Request Headers:** Does it have `Authorization: Bearer ...`?
3. **Check Response:** What status code? What error message?
4. **Check Console:** Any JavaScript errors?
5. **Check Backend Logs:** Look for correlation ID in API logs

## Success Criteria

When complete:
1. ✅ User record automatically created in database on first login
2. ✅ `userService.currentUser()` signal populated after login
3. ✅ `lastLoginAt` updated on every login
4. ✅ Profile synchronized with Auth0 (email, name)
5. ✅ 503 errors handled gracefully with toast notification
6. ✅ Signal cleared on logout

## Additional Notes

- The backend calls Auth0 `/userinfo` endpoint to get fresh profile data on every login
- This ensures the database stays synchronized with Auth0
- The access token is automatically attached by your existing HTTP interceptor
- No changes needed to authentication flow - this is purely additive

---

**Example Implementation Timeline:**
- Step 1-2: 10 minutes (create model + service)
- Step 3: 5 minutes (integrate with auth)
- Step 4: 5 minutes (optional UI display)
- Step 5: 2 minutes (handle logout)
- Testing: 10 minutes

**Total: ~30 minutes**
