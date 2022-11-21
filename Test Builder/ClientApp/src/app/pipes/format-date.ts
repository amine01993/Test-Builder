import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
  name: 'formatDate'
})
export class FormatDatePipe implements PipeTransform {
  transform(dateStr: Date): string {
    const date = new Date(dateStr);
    return date.getFullYear() + '-' + (date.getMonth() + 1).toString().padStart(2, '0') + '-' + date.getDate()
      + ' ' + date.getHours() + ':' + date.getMinutes();
  }
}
