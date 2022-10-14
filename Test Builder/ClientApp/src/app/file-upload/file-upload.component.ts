import { Component, OnInit } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      multi: true,
      useExisting: FileUploadComponent
    }
  ]
})
export class FileUploadComponent implements OnInit, ControlValueAccessor {

  file: File | undefined;

  disabled = false;
  touched = false;
  onChange = (file: File) => { };
  onTouched = () => { };

  constructor() { }

  writeValue(file: File): void {
    console.log('writeValue', file);
    this.file = file;
  }

  registerOnChange(onChange: any): void {
    console.log('registerOnChange', onChange);
    this.onChange = onChange;
  }

  registerOnTouched(onTouched: any): void {
    console.log('registerOnTouched', onTouched);
    this.onTouched = onTouched;
  }

  markAsTouched() {
    if (!this.touched) {
      this.onTouched();
      this.touched = true;
    }
  }

  setDisabledState(disabled: boolean) {
    this.disabled = disabled;
  }

  ngOnInit(): void {
  }

  OnFileSelected(event: any) {
    console.log('OnFileSelected', event);

    if (event.target.files.length > 0) {
      this.file = event.target.files[0];

      this.markAsTouched();
      if (!this.disabled) {
        this.onChange(this.file!);
      }
    }
  }


}
