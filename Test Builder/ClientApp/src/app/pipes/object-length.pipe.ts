import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
  name: 'objectLength'
})
export class ObjectLengthPipe implements PipeTransform {
  transform(obj: { [key: string]: any }, separator: string): number {
    let count = Object.keys(obj).length;
    return count;
  }
}
