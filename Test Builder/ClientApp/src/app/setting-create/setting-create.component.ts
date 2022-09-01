import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormArray, FormControl, FormGroup } from '@angular/forms';
import { AngularEditorConfig } from '@kolkov/angular-editor';
import { editorConfig } from '../editor.config';
import { passMarkValidator } from './setting.validator';

@Component({
  selector: 'app-setting-create',
  templateUrl: './setting-create.component.html',
  styleUrls: ['./setting-create.component.scss']
})
export class SettingCreateComponent implements OnInit {

  _editorConfig: AngularEditorConfig = editorConfig;

  hourOptions: { key: number , val: string }[] = [];
  minuteOptions: { key: number, val: string }[] = [];

  settingsForm = new FormGroup({
    availability: new FormControl(true),
    availableFrom: this.getAvailabilityDateControl(),
    availableUntil: this.getAvailabilityDateControl(),
    attempts: new FormControl(''), // must be an integerpassword
    password: new FormControl(''),
    userInfoFirstname: new FormControl(false),
    userInfoLastname: new FormControl(false),
    userInfoEmail: new FormControl(false),
    extraQuestions: new FormArray([this.getExtraQuestionControl()]),
    resumeLater: new FormControl(false),
    displayPoints: new FormControl(false),
    displayCategory: new FormControl(false),
    passMark: new FormControl('', [passMarkValidator()]),
    showOnPass: new FormControl(''),
    showOnFail: new FormControl(''),
    allowPrinting: new FormControl(false),
    allowHighlightAndCopy: new FormControl(false),
    allowPasting: new FormControl(false),
  });

  constructor() { }

  get availability(): FormControl {
    return this.settingsForm.get('availability') as FormControl;
  }

  get extraQuestions(): FormArray {
    return this.settingsForm.get('extraQuestions') as FormArray;
  }

  get passMark(): FormControl {
    return this.settingsForm.get('passMark') as FormControl;
  }

  private getExtraQuestionControl(): FormGroup {
    return new FormGroup({
      mandatory: new FormControl(false),
      question: new FormControl(''),
      answerOption: new FormControl(0),
      dropDownOption: new FormGroup({
        preview: new FormControl(false),
        answers: new FormArray([]),
        answersText: new FormControl(''),
      }),
      checkBoxesOption: new FormGroup({
        preview: new FormControl(false),
        answers: new FormArray([]),
        answersText: new FormControl(''),
      }),
    });
  }

  private getAvailabilityDateControl(): FormGroup {
    return new FormGroup({
      date: new FormControl(''),
      hour: new FormControl(12),
      minute: new FormControl(0),
      a: new FormControl('am'),
    })
  }

  ngOnInit(): void {
    for (let i = 0; i < 12; i++) {
      let key = i === 0 ? 12 : i;
      let val = key.toString().padStart(2, '0');
      this.hourOptions.push({key, val});
    }

    for (let i = 0; i < 60; i++) {
      let val = i.toString().padStart(2, '0');
      this.minuteOptions.push({ key: i, val });
    }
  }

  getAnswersFormArray(extraQuestion: any): FormArray {
    const answerOption = extraQuestion.value.answerOption;

    const formGroup = answerOption === 2 ? extraQuestion.controls.dropDownOption as FormGroup
      : extraQuestion.controls.checkBoxesOption as FormGroup;

    const answersFormArray = formGroup.controls.answers as FormArray;
    //console.log('getAnswersFormArray', answersFormArray);
    return answersFormArray;
  }

  OnAvailabilityChange() {
    //console.log(this.availability);
  }

  OnExtraQuestionPreview(extraQuestion: any) {
    //extraQuestion = extraQuestion as FormGroup;
    //console.log();
    const answerOption = extraQuestion.value.answerOption;
    const formGroup = answerOption === 2 ? extraQuestion.controls.dropDownOption as FormGroup
      : extraQuestion.controls.checkBoxesOption as FormGroup;

    const answersText = formGroup.value.answersText;
    let answers = answersText.split('\n');
    for (let i = 0; i < answers.length; i++) {
      if (answers[i].trim() === '')
        answers.splice(i--, 1);
    }
    const answersFormArray = formGroup.controls.answers as FormArray;
    for (let ans of answers)
      answersFormArray.push(new FormControl(ans));
    //answersFormArray.setValue(answers);
    formGroup.patchValue({
      preview: true,
    });
    //console.log(extraQuestion, answers);
  }

  OnExtraQuestionEdit(extraQuestion: any) {
    //console.log(extraQuestion);
    const answerOption = extraQuestion.value.answerOption;
    const formGroup = answerOption === 2 ? extraQuestion.controls.dropDownOption as FormGroup
      : extraQuestion.controls.checkBoxesOption as FormGroup;

    formGroup.patchValue({
      preview: false,
    });
  }

  OnAddExtraQuestion() {
    this.extraQuestions.push(this.getExtraQuestionControl());
  }

  OnDeleteExtraQuestion(i: number) {
    this.extraQuestions.removeAt(i);
  }

  OnSubmit() {
    console.log('OnSubmit', this.settingsForm);
  }

}
