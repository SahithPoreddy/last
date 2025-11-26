# Real-Time Dashboard Implementation Guide

## Overview
This guide provides complete steps to implement real-time dashboard updates using SignalR (backend) and Angular (frontend).

---

## ? Backend Setup (Already Done)

### Step 1: SignalR Package Installed
```bash
? Microsoft.AspNetCore.SignalR v1.2.0 installed
```

### Step 2: DashboardHub Created
```bash
? File: codebase/Hubs/DashboardHub.cs
```

### Step 3: Program.cs Updated
```bash
? SignalR service added
? CORS configured for Angular
? Hub endpoint mapped: /hubs/dashboard
```

###

 Step 4: Background Services Updated
```bash
? AuctionExpiryMonitor - sends updates on auction finalization
? PaymentRetryService - sends updates on payment status changes
```

---

## ?? Backend Code Fixes Needed

### Fix 1: Update AuctionExpiryMonitor.cs

Replace line 65 with:
```csharp
// OLD (doesn't exist):
var expiredAuctions = await auctionRepository.GetExpiredActiveAuctionsAsync();

// NEW (use existing method):
var activeAuctions = await auctionRepository.GetAuctionsByStatusAsync(AuctionStatus.Active);
var expiredAuctions = activeAuctions.Where(a => a.ExpiryTime <= DateTime.UtcNow).ToList();
```

### Fix 2: Update PaymentRetryService.cs

Add using directive at top:
```csharp
using codebase.Services.Implementations; // Add this line
```

---

## ?? Frontend Setup (Angular)

### Part 1: Create Angular Project

```bash
# Navigate to your frontend directory
cd C:\path\to\your\frontend

# Create new Angular project
ng new bidsphere-frontend --routing --style=css

# Navigate to project
cd bidsphere-frontend

# Install SignalR client
npm install @microsoft/signalr
```

---

### Part 2: Project Structure

```
bidsphere-frontend/
??? src/
?   ??? app/
?   ?   ??? services/
?   ?   ?   ??? auth.service.ts          # Authentication
?   ?   ?   ??? dashboard.service.ts     # Dashboard API calls
?   ?   ?   ??? signalr.service.ts       # SignalR connection
?   ?   ??? models/
?   ?   ?   ??? dashboard.model.ts       # TypeScript interfaces
?   ?   ??? components/
?   ?   ?   ??? login/
?   ?   ?   ??? dashboard/
?   ?   ?   ??? navbar/
?   ?   ??? app.component.ts
?   ??? environments/
?       ??? environment.ts
?       ??? environment.prod.ts
```

---

### Part 3: Environment Configuration

**File: `src/environments/environment.ts`**
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7019/api',
  hubUrl: 'https://localhost:7019/hubs/dashboard'
};
```

**File: `src/environments/environment.prod.ts`**
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-production-api.com/api',
  hubUrl: 'https://your-production-api.com/hubs/dashboard'
};
```

---

### Part 4: Models

**File: `src/app/models/dashboard.model.ts`**
```typescript
export interface DashboardMetrics {
  activeCount: number;
  pendingPayment: number;
  completedCount: number;
  failedCount: number;
  topBidders: TopBidder[];
}

export interface TopBidder {
  userId: number;
  email: string;
  totalBids: number;
  auctionsWon: number;
  totalAmountSpent: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: number;
  email: string;
  roles: string[];
  token: string;
}
```

---

### Part 5: Auth Service

**File: `src/app/services/auth.service.ts`**
```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { LoginRequest, AuthResponse } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private tokenKey = 'auth_token';
  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadStoredUser();
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => {
          localStorage.setItem(this.tokenKey, response.token);
          localStorage.setItem('user', JSON.stringify(response));
          this.currentUserSubject.next(response);
        })
      );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    const user = this.currentUserSubject.value;
    return user?.roles.includes('Admin') ?? false;
  }

  getAuthHeaders(): HttpHeaders {
    const token = this.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  private loadStoredUser(): void {
    const userJson = localStorage.getItem('user');
    if (userJson) {
      try {
        const user = JSON.parse(userJson);
        this.currentUserSubject.next(user);
      } catch (e) {
        console.error('Error parsing stored user', e);
      }
    }
  }
}
```

---

### Part 6: Dashboard Service

**File: `src/app/services/dashboard.service.ts`**
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DashboardMetrics } from '../models/dashboard.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getDashboardMetrics(): Observable<DashboardMetrics> {
    return this.http.get<DashboardMetrics>(
      `${this.apiUrl}/dashboard`,
      { headers: this.authService.getAuthHeaders() }
    );
  }
}
```

---

### Part 7: SignalR Service (MOST IMPORTANT!)

**File: `src/app/services/signalr.service.ts`**
```typescript
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private dashboardUpdateSubject = new BehaviorSubject<any>(null);
  public dashboardUpdate$ = this.dashboardUpdateSubject.asObservable();
  
  private connectionStateSubject = new BehaviorSubject<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected
  );
  public connectionState$ = this.connectionStateSubject.asObservable();

  constructor(private authService: AuthService) {}

  /**
   * Start SignalR connection
   */
  public async startConnection(): Promise<void> {
    const token = this.authService.getToken();
    
    if (!token) {
      console.warn('No authentication token available for SignalR');
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.hubUrl}`, {
        accessTokenFactory: () => token,
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry intervals
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Connection state handlers
    this.hubConnection.onclose((error) => {
      console.log('SignalR connection closed', error);
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
    });

    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR reconnecting', error);
      this.connectionStateSubject.next(signalR.HubConnectionState.Reconnecting);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected', connectionId);
      this.connectionStateSubject.next(signalR.HubConnectionState.Connected);
      this.subscribeToDashboard(); // Re-subscribe after reconnection
    });

    // Dashboard update handler
    this.hubConnection.on('DashboardUpdate', (data) => {
      console.log('Dashboard update received:', data);
      this.dashboardUpdateSubject.next(data);
    });

    try {
      await this.hubConnection.start();
      console.log('SignalR connection established');
      this.connectionStateSubject.next(signalR.HubConnectionState.Connected);
      
      // Subscribe to dashboard updates
      await this.subscribeToDashboard();
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
    }
  }

  /**
   * Subscribe to dashboard updates
   */
  private async subscribeToDashboard(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('SubscribeToDashboard');
        console.log('Subscribed to dashboard updates');
      } catch (error) {
        console.error('Error subscribing to dashboard:', error);
      }
    }
  }

  /**
   * Unsubscribe from dashboard updates
   */
  private async unsubscribeFromDashboard(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('UnsubscribeFromDashboard');
        console.log('Unsubscribed from dashboard updates');
      } catch (error) {
        console.error('Error unsubscribing from dashboard:', error);
      }
    }
  }

  /**
   * Stop SignalR connection
   */
  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.unsubscribeFromDashboard();
      await this.hubConnection.stop();
      console.log('SignalR connection stopped');
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
    }
  }

  /**
   * Check if connected
   */
  public isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }

  /**
   * Get connection state
   */
  public getConnectionState(): signalR.HubConnectionState {
    return this.hubConnection?.state ?? signalR.HubConnectionState.Disconnected;
  }
}
```

---

### Part 8: Dashboard Component

**File: `src/app/components/dashboard/dashboard.component.ts`**
```typescript
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription, interval } from 'rxjs';
import { switchMap, startWith } from 'rxjs/operators';
import { DashboardService } from '../../services/dashboard.service';
import { SignalRService } from '../../services/signalr.service';
import { DashboardMetrics } from '../../models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {
  metrics: DashboardMetrics | null = null;
  loading = true;
  error: string | null = null;
  lastUpdateTime: Date | null = null;
  isConnected = false;

  private signalrSubscription?: Subscription;
  private connectionStateSubscription?: Subscription;

  constructor(
    private dashboardService: DashboardService,
    private signalrService: SignalRService
  ) {}

  async ngOnInit(): Promise<void> {
    // Load initial data
    this.loadDashboardData();

    // Start SignalR connection
    await this.signalrService.startConnection();

    // Subscribe to connection state
    this.connectionStateSubscription = this.signalrService.connectionState$.subscribe(
      state => {
        this.isConnected = this.signalrService.isConnected();
        console.log('Connection state:', state, 'Connected:', this.isConnected);
      }
    );

    // Subscribe to real-time updates
    this.signalrSubscription = this.signalrService.dashboardUpdate$.subscribe(
      update => {
        if (update) {
          console.log('Dashboard update triggered, refreshing data...');
          this.loadDashboardData();
        }
      }
    );
  }

  ngOnDestroy(): void {
    this.signalrSubscription?.unsubscribe();
    this.connectionStateSubscription?.unsubscribe();
    this.signalrService.stopConnection();
  }

  loadDashboardData(): void {
    this.loading = true;
    this.error = null;

    this.dashboardService.getDashboardMetrics().subscribe({
      next: (data) => {
        this.metrics = data;
        this.lastUpdateTime = new Date();
        this.loading = false;
        console.log('Dashboard data loaded:', data);
      },
      error: (err) => {
        this.error = 'Failed to load dashboard data';
        this.loading = false;
        console.error('Error loading dashboard:', err);
      }
    });
  }

  refreshManually(): void {
    this.loadDashboardData();
  }
}
```

**File: `src/app/components/dashboard/dashboard.component.html`**
```html
<div class="dashboard-container">
  <!-- Header -->
  <div class="dashboard-header">
    <h1>Dashboard</h1>
    <div class="header-actions">
      <!-- Connection Status -->
      <div class="connection-status" [class.connected]="isConnected">
        <span class="status-indicator"></span>
        <span>{{ isConnected ? 'Connected' : 'Disconnected' }}</span>
      </div>
      
      <!-- Manual Refresh Button -->
      <button class="btn-refresh" (click)="refreshManually()" [disabled]="loading">
        <span *ngIf="!loading">?? Refresh</span>
        <span *ngIf="loading">? Loading...</span>
      </button>
      
      <!-- Last Update Time -->
      <div class="last-update" *ngIf="lastUpdateTime">
        Last updated: {{ lastUpdateTime | date:'medium' }}
      </div>
    </div>
  </div>

  <!-- Loading State -->
  <div *ngIf="loading && !metrics" class="loading">
    <p>Loading dashboard...</p>
  </div>

  <!-- Error State -->
  <div *ngIf="error" class="error">
    <p>{{ error }}</p>
    <button (click)="loadDashboardData()">Retry</button>
  </div>

  <!-- Dashboard Content -->
  <div *ngIf="metrics && !loading" class="dashboard-content">
    <!-- Metrics Cards -->
    <div class="metrics-grid">
      <div class="metric-card active">
        <h3>Active Auctions</h3>
        <div class="metric-value">{{ metrics.activeCount }}</div>
      </div>

      <div class="metric-card pending">
        <h3>Pending Payment</h3>
        <div class="metric-value">{{ metrics.pendingPayment }}</div>
      </div>

      <div class="metric-card completed">
        <h3>Completed</h3>
        <div class="metric-value">{{ metrics.completedCount }}</div>
      </div>

      <div class="metric-card failed">
        <h3>Failed</h3>
        <div class="metric-value">{{ metrics.failedCount }}</div>
      </div>
    </div>

    <!-- Top Bidders Table -->
    <div class="top-bidders-section">
      <h2>Top Bidders</h2>
      <table class="bidders-table" *ngIf="metrics.topBidders.length > 0">
        <thead>
          <tr>
            <th>Rank</th>
            <th>Email</th>
            <th>Total Bids</th>
            <th>Auctions Won</th>
            <th>Total Spent</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let bidder of metrics.topBidders; let i = index">
            <td>{{ i + 1 }}</td>
            <td>{{ bidder.email }}</td>
            <td>{{ bidder.totalBids }}</td>
            <td>{{ bidder.auctionsWon }}</td>
            <td>${{ bidder.totalAmountSpent | number:'1.2-2' }}</td>
          </tr>
        </tbody>
      </table>
      <p *ngIf="metrics.topBidders.length === 0" class="no-data">
        No bidder data available yet.
      </p>
    </div>
  </div>
</div>
```

**File: `src/app/components/dashboard/dashboard.component.css`**
```css
.dashboard-container {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.header-actions {
  display: flex;
  gap: 15px;
  align-items: center;
}

.connection-status {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 15px;
  border-radius: 20px;
  background-color: #f44336;
  color: white;
  font-size: 14px;
}

.connection-status.connected {
  background-color: #4caf50;
}

.status-indicator {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background-color: white;
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.btn-refresh {
  padding: 10px 20px;
  background-color: #2196f3;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 14px;
}

.btn-refresh:hover {
  background-color: #1976d2;
}

.btn-refresh:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

.last-update {
  font-size: 12px;
  color: #666;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.metric-card {
  padding: 25px;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  transition: transform 0.2s;
}

.metric-card:hover {
  transform: translateY(-5px);
}

.metric-card.active {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.metric-card.pending {
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
  color: white;
}

.metric-card.completed {
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
  color: white;
}

.metric-card.failed {
  background: linear-gradient(135deg, #fa709a 0%, #fee140 100%);
  color: white;
}

.metric-card h3 {
  margin: 0 0 15px 0;
  font-size: 16px;
  font-weight: 500;
}

.metric-value {
  font-size: 48px;
  font-weight: bold;
}

.top-bidders-section {
  background: white;
  padding: 25px;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.top-bidders-section h2 {
  margin: 0 0 20px 0;
}

.bidders-table {
  width: 100%;
  border-collapse: collapse;
}

.bidders-table th,
.bidders-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #e0e0e0;
}

.bidders-table th {
  background-color: #f5f5f5;
  font-weight: 600;
}

.bidders-table tr:hover {
  background-color: #f9f9f9;
}

.loading, .error, .no-data {
  text-align: center;
  padding: 40px;
  color: #666;
}

.error {
  color: #f44336;
}

.error button {
  margin-top: 15px;
  padding: 10px 20px;
  background-color: #f44336;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
}

.error button:hover {
  background-color: #d32f2f;
}
```

---

### Part 9: App Module Configuration

**File: `src/app/app.module.ts`**
```typescript
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { LoginComponent } from './components/login/login.component';

// Services
import { AuthService } from './services/auth.service';
import { DashboardService } from './services/dashboard.service';
import { SignalRService } from './services/signalr.service';

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    LoginComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [
    AuthService,
    DashboardService,
    SignalRService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

---

### Part 10: Routing Configuration

**File: `src/app/app-routing.module.ts`**
```typescript
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: '**', redirectTo: '/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
```

---

## ?? Testing

### Step 1: Start Backend
```bash
cd codebase
dotnet run
```

Expected output:
```
Now listening on: https://localhost:7019
SignalR Hub available at: /hubs/dashboard
```

### Step 2: Start Frontend
```bash
cd bidsphere-frontend
ng serve
```

Expected output:
```
Angular Live Development Server is listening on localhost:4200
```

### Step 3: Test Real-Time Updates

1. **Login as Admin**
   - Go to `http://localhost:4200`
   - Login with admin credentials

2. **Open Dashboard**
   - Navigate to `/dashboard`
   - Should see metrics and "Connected" status

3. **Trigger Updates**
   - Create a new auction (use Swagger or Postman)
   - Place bids
   - Let auction expire
   - **Dashboard updates automatically!** ?

---

## ?? How It Works

### Flow Diagram:
```
1. Angular app connects to SignalR Hub
   ?
2. Hub subscribes client to "DashboardSubscribers" group
   ?
3. Backend events occur:
   - Auction expires
   - Payment status changes
   ?
4. Background services call:
   hubContext.Clients.Group("DashboardSubscribers")
     .SendAsync("DashboardUpdate", {})
   ?
5. Angular SignalR service receives update
   ?
6. Dashboard component reloads data
   ?
7. UI updates WITHOUT page refresh! ?
```

---

## ?? Debugging

### Backend Console Logs:
```
SignalR connection established
Client subscribed to dashboard updates
Dashboard update sent to group
```

### Frontend Browser Console:
```
SignalR connection established
Subscribed to dashboard updates
Dashboard update received: {timestamp: ...}
Dashboard data loaded: {activeCount: 5, ...}
```

---

## ?? Features Implemented

? Real-time connection status indicator  
? Automatic dashboard updates  
? Manual refresh button  
? Last update timestamp  
? Auto-reconnection on connection loss  
? Metrics cards with live data  
? Top bidders table  
? Loading states  
? Error handling  

---

## ?? Common Issues

### Issue 1: CORS Error
```
Access to XMLHttpRequest has been blocked by CORS policy
```

**Solution:** Backend already configured for `http://localhost:4200`

### Issue 2: SignalR Connection Failed
```
Error: Failed to start the connection
```

**Solutions:**
- Check backend is running
- Verify token is valid
- Check firewall settings
- Try WebSockets explicitly

### Issue 3: Dashboard Not Updating
```
Connected but no updates received
```

**Solutions:**
- Check browser console for errors
- Verify subscription to "DashboardSubscribers"
- Trigger backend events (create auction, place bid)
- Check backend console logs

---

## ?? Summary

### Backend ?
- SignalR Hub created
- Background services send updates
- CORS configured for Angular
- Hub endpoint: `/hubs/dashboard`

### Frontend Steps:
1. Create Angular project
2. Install SignalR client
3. Create services (Auth, Dashboard, SignalR)
4. Create dashboard component
5. Configure routing
6. Test real-time updates

### Result:
?? **Real-time dashboard that updates without refreshing!**

---

## ?? Next Steps

1. Run `dotnet run` (backend)
2. Run `ng serve` (frontend)
3. Login as admin
4. View live dashboard
5. Trigger events and watch updates! ?

---

**Status:** ? Complete Implementation Guide  
**Real-Time:** ? SignalR + Angular  
**Updates:** ? Automatic, no refresh needed  
**Ready to implement!** ??
