import {
  AbstractControl,
  AsyncValidatorFn,
  ValidationErrors,
} from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { switchMap, map, catchError } from 'rxjs/operators';
import { ApplicationUserService } from '../../services/application-user.service';

export function emailAvailabilityValidator(
  userService: ApplicationUserService
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    const email = control.value?.trim();
    if (!email || email.length < 5) {
      return of(null);
    }

    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(email)) {
      return of(null);
    }

    return timer(500).pipe(
      switchMap(() =>
        userService.checkEmailAvailability(email).pipe(
          map((isAvailable: boolean) =>
            isAvailable ? null : { emailTaken: true }
          ),
          catchError(() => of(null))
        )
      )
    );
  };
}
