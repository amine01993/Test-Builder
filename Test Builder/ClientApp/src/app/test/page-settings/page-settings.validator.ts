import { AbstractControl, FormControl, FormGroup, ValidationErrors, ValidatorFn } from "@angular/forms";

export function limitValidator(): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const timeLimit = (group as FormGroup).get('timeLimit') as FormControl;
    const limit = (group as FormGroup).get('limit') as FormControl;

    if (timeLimit.value === false)
      return null;

    if (limit.value === '')
      return null;

    if (isNaN(limit.value))
      return { mustBeANumber: true };

    const pointsValue = +limit.value;
    if (pointsValue <= 0)
      return { mustBeStrictlyPositif: true };

    return null;
  }
}
