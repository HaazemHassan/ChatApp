import { ApiResponse } from './../models/api-response';
import { Injectable, } from '@angular/core';
import { map, Observable } from 'rxjs';
import { User } from '../models/interfaces/userInterface';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApplicationUserService {
  apiUrl: string = environment.apiUrl;
  constructor(private http: HttpClient) { }


  searchUsers(username: string): Observable<User[]> {
    return this.http
      .get<ApiResponse<User[]>>(`${this.apiUrl}/ApplicationUser/search?username=${username}`)
      .pipe(
        map((res) => res.data ?? []),
      );
  }

}

