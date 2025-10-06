import { AbstractControl, AsyncValidatorFn, ValidationErrors } from "@angular/forms";
import { ApplicationUserService } from "../../services/application-user.service";
import { catchError, map, Observable, of, switchMap, timer } from "rxjs";

export function usernameAvailabilityValidator(
  userService: ApplicationUserService
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || control.value.length < 3) {
      return of(null);
    }

    return timer(500).pipe(
      switchMap(() =>
        userService.checkUsernameAvailability(control.value).pipe(
          map((isAvailable: boolean) =>
            isAvailable ? null : { usernameTaken: true }
          ),
          catchError(() => of(null))
        )
      )
    );
  };
}
