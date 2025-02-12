import { BrowserModule } from '@angular/platform-browser';
import { ApplicationRef, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AngularEditorModule } from '@kolkov/angular-editor';
import { DragDropModule } from '@angular/cdk/drag-drop';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { QuestionAddComponent } from './question-add/question-add.component';
import { SplitPipe } from './pipes/split.pipe';
import { MultipleChoiceAddComponent } from './question-add/multiple-choice/multiple-choice.component';
import { TrueFalseAddComponent } from './question-add/true-false/true-false.component';
import { MatchingAddComponent } from './question-add/matching-add/matching-add.component';
import { FreeTextAddComponent } from './question-add/free-text-add/free-text-add.component';
import { EssayAddComponent } from './question-add/essay-add/essay-add.component';
import { LoginComponent } from './login/login.component';
import { NgbActiveModal, NgbModalModule, NgbPopoverModule } from '@ng-bootstrap/ng-bootstrap';
import { RegisterComponent } from './register/register.component';
import { CategoryComponent } from './category/category.component';
import { CategoryManageComponent } from './category/category-manage/category-manage.component';
import { AuthenticationInteceptor } from './authentication.interceptor';
import { ObjectLengthPipe } from './pipes/object-length.pipe';
import { ArrayPipe } from './pipes/array.pipe';
import { SubcategoryManageComponent } from './category/subcategory-manage/subcategory-manage.component';
import { ConfirmationModalComponent } from './confirmation-modal/confirmation-modal.component';
import { TestComponent } from './test/test.component';
import { TestAddComponent } from './test/test-add/test-add.component';
import { TestEditComponent } from './test/test-edit/test-edit.component';
import { PageSettingsComponent } from './test/page-settings/page-settings.component';
import { PageEditComponent } from './test/page-edit/page-edit.component';
import { QuestionBankComponent } from './question-bank/question-bank.component';
import { QuestionItemComponent } from './question-bank/question-item/question-item.component';
import { SettingCreateComponent } from './setting-create/setting-create.component';
import { PreviewComponent } from './preview/preview.component';
import { AdminComponent } from './admin/admin.component';
import { PreviewItemComponent } from './preview/preview-item/preview-item.component';
import { ImportModalComponent } from './import-modal/import-modal.component';
import { FileUploadComponent } from './file-upload/file-upload.component';
import { FormatDatePipe } from './pipes/format-date';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    QuestionAddComponent,
    SplitPipe,
    ObjectLengthPipe,
    FormatDatePipe,
    ArrayPipe,
    MultipleChoiceAddComponent,
    TrueFalseAddComponent,
    MatchingAddComponent,
    FreeTextAddComponent,
    EssayAddComponent,
    LoginComponent,
    RegisterComponent,
    CategoryComponent,
    CategoryManageComponent,
    SubcategoryManageComponent,
    ConfirmationModalComponent,
    TestComponent,
    TestAddComponent,
    TestEditComponent,
    PageSettingsComponent,
    PageEditComponent,
    QuestionBankComponent,
    QuestionItemComponent,
    SettingCreateComponent,
    PreviewComponent,
    AdminComponent,
    PreviewItemComponent,
    ImportModalComponent,
    FileUploadComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot([
      { path: '', pathMatch: 'full', redirectTo: 'admin' },
      {
        path: 'admin', component: AdminComponent,
        children: [
          { path: '', component: HomeComponent, pathMatch: 'full' },
          {
            path: 'add-question/:page-id', component: QuestionAddComponent,
            children: [
              { path: '', pathMatch: 'full', redirectTo: 'multiple-choice' },
              { path: 'multiple-choice/:id', component: MultipleChoiceAddComponent },
              { path: 'multiple-choice', component: MultipleChoiceAddComponent },
              { path: 'true-false/:id', component: TrueFalseAddComponent },
              { path: 'true-false', component: TrueFalseAddComponent },
              { path: 'matching/:id', component: MatchingAddComponent },
              { path: 'matching', component: MatchingAddComponent },
              { path: 'free-text/:id', component: FreeTextAddComponent },
              { path: 'free-text', component: FreeTextAddComponent },
              { path: 'essay/:id', component: EssayAddComponent },
              { path: 'essay', component: EssayAddComponent }
            ]
          },
          { path: 'category', component: CategoryComponent },
          { path: 'test-add', component: TestAddComponent },
          { path: 'test-edit/:id', component: TestEditComponent },
          { path: 'tests', component: TestComponent },
          { path: 'page-edit/:page-id', component: PageEditComponent },
          { path: 'question-bank/:page-id', component: QuestionBankComponent },
          { path: 'question-bank', component: QuestionBankComponent },
          { path: 'setting-create', component: SettingCreateComponent },
        ]
      },      { path: 'preview/test/:id', component: PreviewComponent }
    ]),
    AngularEditorModule,
    NgbModalModule,
    NgbPopoverModule,
    DragDropModule,
  ],
  providers: [
    NgbActiveModal,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthenticationInteceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    LoginComponent,
    RegisterComponent,
    PageSettingsComponent,
    ImportModalComponent,
    ConfirmationModalComponent,
    //AppComponent,
  ]
})
export class AppModule {
  //ngDoBootstrap(app: ApplicationRef) {
  //  console.log('bgDoBootstrap');
  //  app.bootstrap(AppComponent);
  //}
}
