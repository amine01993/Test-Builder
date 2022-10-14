import { ValidatorFn, AbstractControl, ValidationErrors } from "@angular/forms";

export function requiredFileType(type: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const file = control.value;

    if (file) {
      const extension = file.type.toLowerCase();
      if (type.toLowerCase() !== extension.toLowerCase()) {
        return { requiredFileType: true };
      }
      return null;
    }
    return null;
  }
}
