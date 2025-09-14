import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthenticationService } from '../../services/authentication.service';

@Component({
  selector: 'app-home',
  imports: [],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent {
  constructor(
    private authService: AuthenticationService,
    private router: Router
  ) { }

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }

  navigateToRegister(): void {
    this.router.navigate(['/register']);
  }

  navigateToChat(): void {
    if (this.isAuthenticated())
      this.router.navigate(['/conversations']);
    else
      this.router.navigate(['/login']);

  }
}
