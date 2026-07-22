import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// Backend password rules from RegisterRequestValidator.cs:
// - Minimum length 8
// - Maximum length 128
// - At least one uppercase letter
// - At least one lowercase letter
// - At least one non-alphanumeric character
export function passwordValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string | null;

    if (!value) {
      return null;
    }

    const errors: ValidationErrors = {};

    if (value.length < 8) {
      errors['minLength'] = true;
    }

    if (value.length > 128) {
      errors['maxLength'] = true;
    }

    if (!/[A-Z]/.test(value)) {
      errors['uppercase'] = true;
    }

    if (!/[a-z]/.test(value)) {
      errors['lowercase'] = true;
    }

    if (!/[^a-zA-Z0-9]/.test(value)) {
      errors['special'] = true;
    }

    return Object.keys(errors).length > 0 ? errors : null;
  };
}

export function confirmPasswordValidator(passwordControlName: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const confirmPassword = control.value as string | null;
    const password = control.parent?.get(passwordControlName)?.value as string | null;

    if (!confirmPassword || !password) {
      return null;
    }

    return confirmPassword === password ? null : { mismatch: true };
  };
}
