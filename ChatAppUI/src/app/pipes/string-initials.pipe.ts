import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'stringInitials'
})
export class StringInitialsPipe implements PipeTransform {

  transform(value: string): string {
    if (!value) return '?';
    const parts = value.trim().split(' ').slice(0, 2);
    return parts
      .map((p) => p.charAt(0))
      .join('')
      .toUpperCase();
  }
}
