import { AngularEditorConfig } from '@kolkov/angular-editor';

export const editorConfig: AngularEditorConfig = {
  editable: true,
  spellcheck: true,
  height: 'auto',
  minHeight: '0',
  maxHeight: 'auto',
  width: 'auto',
  minWidth: '0',
  translate: 'yes',
  enableToolbar: true,
  showToolbar: true,
  placeholder: 'Enter question here...',
  defaultParagraphSeparator: '',
  defaultFontName: '',
  defaultFontSize: '',
  fonts: [
    { class: 'arial', name: 'Arial' },
    { class: 'times-new-roman', name: 'Times New Roman' },
    { class: 'calibri', name: 'Calibri' },
    { class: 'comic-sans-ms', name: 'Comic Sans MS' }
  ],
  //customClasses: [
  //  {
  //    name: 'quote',
  //    class: 'quote',
  //  },
  //  {
  //    name: 'redText',
  //    class: 'redText'
  //  },
  //  {
  //    name: 'titleText',
  //    class: 'titleText',
  //    tag: 'h1',
  //  },
  //],
  uploadWithCredentials: false,
  sanitize: true,
  toolbarPosition: 'top',
  toolbarHiddenButtons: [
    ['undo', 'redo', 'indent', 'outdent', 'heading'],
    ['link', 'unlink', 'insertImage', 'insertVideo', 'toggleEditorMode']
  ]
};
