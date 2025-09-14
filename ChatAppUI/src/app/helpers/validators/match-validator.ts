import {
  AbstractControl,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';

export function matchValidator(
  controlName: string,
  matchingControlName: string
): ValidatorFn {
  return (formGroup: AbstractControl): ValidationErrors | null => {
    if (!(formGroup instanceof FormGroup)) {
      return null;
    }

    const control = formGroup.get(controlName);
    const matchingControl = formGroup.get(matchingControlName);

    if (!control || !matchingControl) {
      return null;
    }

    // If the matching control already has other errors, don't override them
    if (matchingControl.errors && !matchingControl.errors['mismatch']) {
      return null;
    }

    // Set error on the matching control if values don't match
    if (control.value !== matchingControl.value) {
      matchingControl.setErrors({ mismatch: true });
      return { mismatch: true };
    } else {
      // Remove the mismatch error if values match
      if (matchingControl.errors?.['mismatch']) {
        delete matchingControl.errors['mismatch'];
        if (Object.keys(matchingControl.errors).length === 0) {
          matchingControl.setErrors(null);
        }
      }
      return null;
    }
  };
}
