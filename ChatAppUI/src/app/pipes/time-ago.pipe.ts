import { Pipe, PipeTransform } from '@angular/core';
import { Observable } from 'rxjs';
import { TimeAgoService } from '../services/time-ago.service';

@Pipe({
  name: 'timeAgo',
  pure: true
})
export class TimeAgoPipe implements PipeTransform {
  constructor(private timeAgoService: TimeAgoService) { }

  transform(date: string | Date | null): Observable<string> {
    if (!date) {
      return new Observable(observer => observer.next(''));
    }
    return this.timeAgoService.getTimeAgo(date);
  }
}
