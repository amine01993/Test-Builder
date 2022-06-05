import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
  name: 'array'
})
export class ArrayPipe implements PipeTransform {
  transform(arr: any[]): number[] {
    const res = Array(arr.length);
    for (let i = 0; i < res.length; i++) {
      res[i] = i;
    }
    return res;
  }
}
