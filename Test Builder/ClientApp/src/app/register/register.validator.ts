import { ValidatorFn, AbstractControl, ValidationErrors } from "@angular/forms";

export function passwordMatchValidator(): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const password = group.value.password;
    const confirmPassword = group.value.confirmPassword;

    if (password !== confirmPassword) {
      return { passwordMismatch: true };
    }

    return null;
  }
}
