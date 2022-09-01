import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export function passMarkValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (control.value === '' || isNaN(control.value))
      return { mustBeANumber: true };

    const pointsValue = +control.value;
    if (pointsValue < 0)
      return { mustBePositif: true };

    return null;
  }
}

