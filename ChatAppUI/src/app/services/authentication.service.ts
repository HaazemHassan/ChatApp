import { ChatHubService } from './chat-hub.service';
import { HttpClient } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import { ApiResponse } from '../models/api-response';
import { catchError, map, Observable, tap, throwError } from 'rxjs';
import { LoginRequest } from '../models/auth/requests/login-request';
import { RegisterRequest } from '../models/auth/requests/register-request';
import { RefreshTokenRequest } from '../models/auth/requests/refresh-token-request';
import { LoginResponse } from '../models/auth/responses/login-response';
import { environment } from '../../environments/environment';
import { User } from '../models/interfaces/userInterface';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  currentUser: User | null = null;

  constructor(private httpClient: HttpClient, private chatHubService: ChatHubService) {
    this.currentUser = this.getCurrentUser();

    if (this.isAuthenticated()) {
      this.chatHubService.startConnection(this.getAccessToken());
    }
  }

  ngOnInit(): void { }

  login(credentials: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.httpClient
      .post<ApiResponse<LoginResponse>>(
        `${environment.apiUrl}/authentication/login`,
        credentials
      )
      .pipe(
        map((response: ApiResponse<LoginResponse>) => {
          if (response.succeeded && response.data) {
            localStorage.setItem('accessToken', response.data.accessToken);
            if (response.data.refreshToken) {
              document.cookie = `refreshToken=${response.data.refreshToken.token}; path=/`;
            }
            localStorage.setItem(
              'currentUser',
              JSON.stringify(response.data.user)
            );
            if (this.isAuthenticated()) {
              this.chatHubService.startConnection(this.getAccessToken());
            }
          }

          return response;
        }),
        catchError((error) => {
          console.error('Login failed:', error);
          return throwError(() => error);
        })
      );
  }

  register(userData: RegisterRequest): Observable<ApiResponse<LoginResponse>> {
    return this.httpClient
      .post<ApiResponse<LoginResponse>>(
        `${environment.apiUrl}/authentication/register`,
        userData
      )
      .pipe(
        map((response: ApiResponse<LoginResponse>) => {
          if (response.succeeded && response.data) {
            // Automatically log in the user after successful registration
            localStorage.setItem('accessToken', response.data.accessToken);
            if (response.data.refreshToken) {
              document.cookie = `refreshToken=${response.data.refreshToken.token}; path=/`;
            }
            localStorage.setItem(
              'currentUser',
              JSON.stringify(response.data.user)
            );
          }
          return response;
        }),
        catchError((error) => {
          console.error('Registration failed:', error);
          return throwError(() => error);
        })
      );
  }

  logout(): Observable<ApiResponse<void>> {
    this.chatHubService.stopConnection();
    return this.httpClient
      .post<ApiResponse<void>>(
        `${environment.apiUrl}/authentication/logout`,
        {}
      )
      .pipe(
        tap(() => {
          this.clearTokens();
          this.removeCurrentUser();
        }),
        catchError((error) => {
          console.error('Logout failed:', error);
          this.clearTokens();
          this.removeCurrentUser();
          return throwError(() => error);
        })
      );
  }

  refreshToken(): Observable<ApiResponse<LoginResponse>> {
    const accessToken = this.getAccessToken();
    const refreshToken = this.getRefreshTokenFromCookie();

    if (!accessToken || !refreshToken) {
      return throwError(() => new Error('No tokens available for refresh'));
    }

    const refreshRequest: RefreshTokenRequest = {
      accessToken: accessToken,
      refreshToken: refreshToken,
    };

    return this.httpClient
      .post<ApiResponse<LoginResponse>>(
        `${environment.apiUrl}/authentication/refresh-token`,
        refreshRequest
      )
      .pipe(
        map((response: ApiResponse<LoginResponse>) => {
          if (response.succeeded && response.data) {
            // Update stored tokens with new ones
            localStorage.setItem('accessToken', response.data.accessToken);
            if (response.data.refreshToken) {
              document.cookie = `refreshToken=${response.data.refreshToken.token}; path=/`;
            }
          }
          return response;
        }),
        catchError((error) => {
          console.error('Token refresh failed:', error);
          this.clearTokens();
          return throwError(() => error);
        })
      );
  }

  // Helper methods for token management
  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getCurrentUser(): User | null {
    const user = localStorage.getItem('currentUser');
    return user ? (JSON.parse(user) as User) : null;
  }

  getCurrentUserId(): number | null {
    const user = localStorage.getItem('currentUser');
    return user ? (JSON.parse(user) as User).id : null;
  }

  getRefreshToken(): string | null {
    return this.getRefreshTokenFromCookie();
  }

  getRefreshTokenFromCookie(): string | null {
    const cookies = document.cookie.split(';');
    for (let cookie of cookies) {
      const [name, value] = cookie.trim().split('=');
      if (name === 'refreshToken') {
        return value;
      }
    }
    return null;
  }

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    return token !== null && token !== '';
  }

  clearTokens(): void {
    localStorage.removeItem('accessToken');
    document.cookie =
      'refreshToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
  }

  removeCurrentUser(): void {
    localStorage.removeItem('currentUser');
  }
}
