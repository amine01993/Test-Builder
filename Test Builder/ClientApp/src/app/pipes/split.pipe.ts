import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
  name: 'split'
})
export class SplitPipe implements PipeTransform {
  transform(str: string, separator: string): string[] {
    let arr = str.split(separator);
    return arr;
  }
}
