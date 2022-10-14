import { HttpClient, HttpEventType, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Subscription } from 'rxjs';
import { requiredFileType } from './requiredFileType.validator';

@Component({
  selector: 'app-import-modal',
  templateUrl: './import-modal.component.html',
  styleUrls: ['./import-modal.component.scss']
})
export class ImportModalComponent implements OnInit {

  import: FormGroup = new FormGroup({
    file: new FormControl(null, [Validators.required, requiredFileType('application/json')]),
  });

  get file(): FormControl {
    return this.import.get('file') as FormControl;
  }

  serverErrors: { [key: string]: string } = {};
  fileName: string = '';

  uploadSub: Subscription | undefined;
  uploadProgress: number = 0;

  constructor(
    private httpClient: HttpClient,
    public activeModal: NgbActiveModal,
  ) { }

  ngOnInit(): void {
  }

  OnSubmit(): void {
    console.log(this.import);
    if (this.import.valid) {
      const file: File = this.import.value.file;

      this.fileName = file.name;
      const formData = new FormData();
      formData.append("importedFile", file);

      const headers = new HttpHeaders();
      headers.append('Content-Type', 'application/json');

      const upload$ = this.httpClient.post("api/question/import-json", formData, {
        reportProgress: true,
        observe: 'events',
        params: { auth: true },
        headers,
      });

      this.uploadSub = upload$.subscribe((event: any) => {
        console.log('upload$.subscribe', event);
        if (event.type == HttpEventType.UploadProgress) {
          this.uploadProgress = Math.round(100 * (event.loaded / event.total));
        }
        else if (event.type == HttpEventType.Sent) {
          // finalize
          this.Reset();
        }
      });
    }
  }

  CancelUpload() {
    this.uploadSub?.unsubscribe();
    this.Reset();
  }

  Reset() {
    this.uploadProgress = 0;
    this.uploadSub = undefined;
  }
}
