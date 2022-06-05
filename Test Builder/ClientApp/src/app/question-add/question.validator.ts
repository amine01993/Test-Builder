import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export function pointsValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (control.value === '' || isNaN(control.value))
      return { mustBeANumber: true };

    const pointsValue = +control.value;
    if (pointsValue <= 0)
      return { mustBeStrictlyPositif: true };

    return null;
  }
}

export function answerValidator(): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {

    if (group.value.points === '' || isNaN(group.value.points))
      return { pointsMustBeANumber: true};
    if (group.value.penalty === '' || isNaN(group.value.penalty))
      return { penaltyMustBeANumber: true };

    const pointsValue = +group.value.points;
    const penaltyValue = +group.value.penalty;

    if (pointsValue <= 0)
      return { pointsMustBeStrictlySuperiorThanZero: true };
    if (penaltyValue < 0)
      return { penaltyMustBeSuperiorThanZero: true };

    if (penaltyValue > pointsValue)
      return { penaltyMustBeInferiorThanPoints: true };

    return null;
  }
}

export function questionValidator(): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {

    //const pointsValue = +group.value.points;
    //let pointsSum = 0;

    //for (let answer of group.value.answers) {
    //  if (answer.points === '' || isNaN(answer.points)) // points must be a valid number, validation is already done for each answer
    //    return null;

    //  pointsSum += +answer.points;
    //}

    //if (pointsSum !== pointsValue)
    //  return { incorrectPointsDistribution: true };

    return null;
  }
}
