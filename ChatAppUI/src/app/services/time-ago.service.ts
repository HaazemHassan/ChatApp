import { Injectable } from '@angular/core';
import { interval, map, Observable, startWith } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TimeAgoService {

  getTimeAgo(date: string | Date): Observable<string> {
    const valueStr = typeof date === 'string' ? date : date.toISOString();
    const messageDate = valueStr.endsWith('Z')
      ? new Date(valueStr)
      : new Date(valueStr + 'Z');

    return interval(30000).pipe(
      startWith(0),
      map(() => this.calculateTimeAgo(messageDate))
    );
  }

  private calculateTimeAgo(messageDate: Date): string {
    const now = new Date();
    const diffInMs = now.getTime() - messageDate.getTime();
    const diffInHours = diffInMs / (1000 * 60 * 60);
    const diffInDays = diffInMs / (1000 * 60 * 60 * 24);

    if (diffInHours < 1) {
      const minutes = Math.floor(diffInMs / (1000 * 60));
      return `${minutes}m`;
    } else if (diffInHours < 24) {
      const hours = Math.floor(diffInHours);
      return `${hours}h`;
    } else if (diffInDays < 7) {
      const days = Math.floor(diffInDays);
      return `${days}d`;
    } else {
      return messageDate.toLocaleDateString('en-US', {
        year: '2-digit',
        month: 'short',
        day: 'numeric'
      });
    }
  }
}
