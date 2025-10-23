import { MessageResponse } from './../models/conversations/responses/conversation-messages-response';
import { ChatHubService } from './chat-hub.service';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import { ApiResponse } from '../models/api-response';
import { BehaviorSubject, catchError, map, Observable, tap, throwError } from 'rxjs';
import { LoginRequest } from '../models/auth/requests/login-request';
import { RegisterRequest } from '../models/auth/requests/register-request';
import { RefreshTokenRequest } from '../models/auth/requests/refresh-token-request';
import { GoogleSignInRequest } from '../models/auth/requests/google-signin-request';
import { LoginResponse } from '../models/auth/responses/login-response';
import { environment } from '../../environments/environment';
import { User } from '../models/interfaces/userInterface';
import { getCookieValue } from '../helpers/cookies-reader/cookies-helper';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  currentUser: User | null = null;
  private currentUserSubject: BehaviorSubject<User | null>;
  currentUser$: Observable<User | null>;

  constructor(private httpClient: HttpClient, private chatHubService: ChatHubService) {
    const storedUser = this.getStoredCurrentUser();
    this.currentUserSubject = new BehaviorSubject<User | null>(storedUser);
    this.currentUser$ = this.currentUserSubject.asObservable();
    this.currentUser = storedUser;

    if (this.isAuthenticated()) {
      this.chatHubService.startConnection(this.getAccessToken());
    }
  }

  ngOnInit(): void { }

  login(credentials: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.httpClient
      .post<ApiResponse<LoginResponse>>(
        `${environment.apiUrl}/authentication/login`,
        credentials, { withCredentials: true }
      )
      .pipe(
        map((response: ApiResponse<LoginResponse>) => {
          if (response.succeeded && response.data) {
            localStorage.setItem('accessToken', response.data.accessToken);
            this.setAuthenticatedUser(response.data.user);
            if (this.isAuthenticated())
              this.chatHubService.startConnection(this.getAccessToken());

          }

          return response;
        }),
        catchError((error: HttpErrorResponse) => {
          const apiError = error.error as ApiResponse<null>;
          return throwError(() => new Error(apiError.message));
        })
      );
  }

  register(userData: RegisterRequest): Observable<ApiResponse<LoginResponse>> {
    return this.httpClient
      .post<ApiResponse<LoginResponse>>(
        `${environment.apiUrl}/authentication/register`,
        userData, { withCredentials: true }
      )
      .pipe(
        map((response: ApiResponse<LoginResponse>) => {
          if (response.succeeded && response.data) {
            localStorage.setItem('accessToken', response.data.accessToken);
            this.setAuthenticatedUser(response.data.user);
          }
          return response;
        }),
        catchError((error: HttpErrorResponse) => {
          const apiError = error.error as ApiResponse<null>;
          return throwError(() => new Error(apiError.message));
        })
      );
  }

  googleSignIn(googleToken: GoogleSignInRequest): Observable<ApiResponse<LoginResponse>> {
    return this.httpClient
      .post<ApiResponse<LoginResponse>>(
        `${environment.apiUrl}/authentication/google-login`,
        googleToken, { withCredentials: true }
      )
      .pipe(
        map((response: ApiResponse<LoginResponse>) => {
          if (response.succeeded && response.data) {
            localStorage.setItem('accessToken', response.data.accessToken);
            this.setAuthenticatedUser(response.data.user);
            if (this.isAuthenticated())
              this.chatHubService.startConnection(this.getAccessToken());

          }
          return response;
        }),
        catchError((error: HttpErrorResponse) => {
          const apiError = error.error as ApiResponse<null>;
          return throwError(() => new Error(apiError.message));
        })
      );
  }

  logout(): Observable<ApiResponse<void>> {
    this.chatHubService.stopConnection();
    return this.httpClient
      .post<ApiResponse<void>>(
        `${environment.apiUrl}/authentication/logout`,
        { withCredentials: true }
      )
      .pipe(
        tap(() => {
          this.clearTokens();
          this.setAuthenticatedUser(null);
        }),
        catchError((error) => {
          console.error('Logout failed:', error);
          this.clearTokens();
          this.setAuthenticatedUser(null);
          return throwError(() => error);
        })
      );
  }

  refreshToken(): Observable<ApiResponse<LoginResponse>> {
    const accessToken = this.getAccessToken();

    if (!accessToken)
      return throwError(() => new Error('No access token available for refresh'));


    const refreshRequest: RefreshTokenRequest = {
      accessToken: accessToken,
    };

    return this.httpClient
      .post<ApiResponse<LoginResponse>>(
        `${environment.apiUrl}/authentication/refresh-token`,
        refreshRequest, { withCredentials: true }
      )
      .pipe(
        map((response: ApiResponse<LoginResponse>) => {
          if (response.succeeded && response.data) {
            localStorage.setItem('accessToken', response.data.accessToken);
            this.setAuthenticatedUser(response.data.user);
          }
          return response;
        }),
        catchError((error) => {
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
    return this.currentUserSubject.value;
  }

  getCurrentUserId(): number | null {
    const user = localStorage.getItem('currentUser');
    return user ? (JSON.parse(user) as User).id : null;
  }


  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    return token !== null && token !== '';
  }

  clearTokens(): void {
    localStorage.removeItem('accessToken');
  }

  removeCurrentUser(): void {
    this.setAuthenticatedUser(null);
  }

  private getStoredCurrentUser(): User | null {
    const user = localStorage.getItem('currentUser');
    return user ? (JSON.parse(user) as User) : null;
  }

  private setAuthenticatedUser(user: User | null): void {
    this.currentUser = user;
    if (user) {
      localStorage.setItem('currentUser', JSON.stringify(user));
    } else {
      localStorage.removeItem('currentUser');
    }

    this.currentUserSubject.next(user);
  }
}
