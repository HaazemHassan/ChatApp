import { Component } from '@angular/core';
import { Router } from '@angular/router';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { AuthenticationService } from '../../services/authentication.service';
import { RegisterRequest } from '../../models/auth/requests/register-request';
import { matchValidator } from '../../helpers/validators';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  registerForm: FormGroup;

  constructor(
    private authService: AuthenticationService,
    private router: Router,
    private formBuilder: FormBuilder
  ) {
    this.registerForm = this.formBuilder.group(
      {
        fullName: ['', [Validators.required, Validators.minLength(3)]],
        userName: ['', [Validators.required, Validators.minLength(3)]],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(3)]],
        confirmPassword: ['', [Validators.required]],
        address: ['', [Validators.required]],
        country: ['', [Validators.required]],
        phoneNumber: [
          '',
          [Validators.required, Validators.pattern(/^\+?[1-9]\d{1,14}$/)],
        ],
      },
      { validators: matchValidator('password', 'confirmPassword') }
    );
  }

  register(): void {
    if (this.registerForm.valid) {
      const registerData: RegisterRequest = {
        fullName: this.registerForm.get('fullName')?.value,
        userName: this.registerForm.get('userName')?.value,
        email: this.registerForm.get('email')?.value,
        password: this.registerForm.get('password')?.value,
        confirmPassword: this.registerForm.get('confirmPassword')?.value,
        address: this.registerForm.get('address')?.value,
        country: this.registerForm.get('country')?.value,
        phoneNumber: this.registerForm.get('phoneNumber')?.value,
      };

      this.authService.register(registerData).subscribe({
        next: (response) => {
          if (response.succeeded) {
            console.log('Registration successful');
            this.router.navigate(['/']);
          } else {
            console.error('Registration failed:', response.message);
          }
        },
        error: (error) => {
          console.error('Registration error:', error);
        },
      });
    } else {
      console.log('Form is invalid');
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.registerForm.controls).forEach((key) => {
      const control = this.registerForm.get(key);
      control?.markAsTouched();
    });
  }

  get fullName() {
    return this.registerForm.get('fullName');
  }
  get userName() {
    return this.registerForm.get('userName');
  }
  get email() {
    return this.registerForm.get('email');
  }
  get password() {
    return this.registerForm.get('password');
  }
  get confirmPassword() {
    return this.registerForm.get('confirmPassword');
  }
  get address() {
    return this.registerForm.get('address');
  }
  get country() {
    return this.registerForm.get('country');
  }
  get phoneNumber() {
    return this.registerForm.get('phoneNumber');
  }
}
