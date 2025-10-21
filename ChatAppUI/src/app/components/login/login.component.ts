import { Component, OnInit, AfterViewInit, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { LoginRequest } from '../../models/auth/requests/login-request';
import { GoogleSignInRequest } from '../../models/auth/requests/google-signin-request';
import { AuthenticationService } from '../../services/authentication.service';
import { environment } from '../../../environments/environment';

declare const google: any;

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent implements OnInit, AfterViewInit {
  loginForm: FormGroup;
  returnUrl: string = '/conversations';
  errorMessage: string = '';

  constructor(
    private authService: AuthenticationService,
    private formBuilder: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.loginForm = this.formBuilder.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(3)]],
    });

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/conversations';
  }

  ngOnInit(): void {
    // Check if user is already logged in
    if (this.isLoggedIn()) {
      this.router.navigateByUrl(this.returnUrl);
    }
  }

  ngAfterViewInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.initializeGoogleOneTap();
    }
  }

  initializeGoogleOneTap(): void {
    if (typeof google !== 'undefined') {
      google.accounts.id.initialize({
        client_id: environment.googleClientId,
        callback: this.handleGoogleSignIn.bind(this),
        auto_select: false,
        cancel_on_tap_outside: true,
        use_fedcm_for_prompt: true
      });

      // Display the One Tap prompt
      google.accounts.id.prompt();

      // Render the Google Sign-In button
      const googleButtonDiv = document.getElementById('google-signin-button');
      if (googleButtonDiv) {
        google.accounts.id.renderButton(
          googleButtonDiv,
          {
            theme: 'outline',
            size: 'large',
            width: googleButtonDiv.offsetWidth,
            text: 'signin_with',
            shape: 'rectangular',
            logo_alignment: 'left',
          }
        );
      }
    } else {
      console.error('Google Sign-In library not loaded');
    }
  }


  handleGoogleSignIn(response: any): void {
    const googleToken: GoogleSignInRequest = {
      idToken: response.credential,
    };

    this.authService.googleSignIn(googleToken).subscribe({
      next: (apiResponse) => {
        if (apiResponse.succeeded) {
          console.log('Google Sign-In successful');
          this.router.navigateByUrl(this.returnUrl);
        } else {
          this.errorMessage = apiResponse.message || 'Google Sign-In failed. Please try again.';
          console.error('Google Sign-In failed:', apiResponse.message);
        }
      },
      error: (error) => {
        this.errorMessage = error.message || 'An error occurred during Google Sign-In. Please try again.';
        console.error('Google Sign-In error:', error);
      },
    });
  }

  isLoggedIn(): boolean {
    return this.authService.isAuthenticated();
  }

  login(): void {
    if (this.loginForm.valid) {
      this.errorMessage = ''; // Clear previous errors
      const credentials: LoginRequest = {
        username: this.loginForm.get('username')?.value,
        password: this.loginForm.get('password')?.value,
      };

      this.authService.login(credentials).subscribe({
        next: (response) => {
          if (response.succeeded) {
            console.log('Login successful');
            this.router.navigateByUrl(this.returnUrl);
          } else {
            this.errorMessage = response.message || 'Login failed. Please try again.';
            console.error('Login failed:', response.message);
          }
        },
        error: (error) => {
          this.errorMessage = error.message || 'An error occurred during login. Please try again.';
          console.error('Login error:', error);
        },
      });
    } else {
      console.log('Form is invalid');
      this.markFormGroupTouched();
    }
  }

  // Helper method to mark all form fields as touched to show validation errors
  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach((key) => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }

  // Getter methods for easy access to form controls in template
  get username() {
    return this.loginForm.get('username');
  }

  get password() {
    return this.loginForm.get('password');
  }
}
