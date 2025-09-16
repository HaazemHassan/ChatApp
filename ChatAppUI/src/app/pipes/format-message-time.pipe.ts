import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'formatMessageTime'
})
export class FormatMessageTimePipe implements PipeTransform {

  transform(sentAt: string): string {
    const utcDate = new Date(sentAt);
    const date = sentAt.endsWith('Z')
      ? utcDate
      : new Date(sentAt + 'Z');
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

}
