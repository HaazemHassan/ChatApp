import { ChatHubService } from './../../services/chat-hub.service';
import { Component, DestroyRef } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { StringInitialsPipe } from "../../pipes/string-initials.pipe";
import { User } from "../../models/interfaces/userInterface";
import { debounceTime, Subject } from "rxjs";
import { ApplicationUserService } from "../../services/application-user.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { AuthenticationService } from '../../services/authentication.service';
import { ConversationType } from '../../enums/conversation-type';
import { Router } from '@angular/router';


@Component({
  selector: 'app-create-group',
  standalone: true,
  imports: [FormsModule, StringInitialsPipe],
  templateUrl: './create-group.component.html',
  styleUrl: './create-group.component.css'
})
export class CreateGroupComponent {

  groupName: string = '';
  searchValue: string = '';
  searchResults: User[] = [];
  selectedUsers: User[] = [];
  isSearching: boolean = false;

  private searchSubject = new Subject<string>();

  constructor(
    private applicationUserService: ApplicationUserService,
    private authservices: AuthenticationService,
    private chatHubService: ChatHubService,
    private router: Router,
    private destroyRef: DestroyRef
  ) {
    this.searchSubject.pipe(
      debounceTime(300),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((searchTerm) => {
      if (searchTerm.trim()) {
        this.searchUsers(searchTerm);
      } else {
        this.searchResults = [];
      }
    });
  }

  onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchValue = target.value.trim();
    this.searchSubject.next(this.searchValue);
  }

  private searchUsers(username: string): void {
    this.isSearching = true;
    this.applicationUserService.searchUsers(username).subscribe({
      next: (users) => {
        // Filter out already selected users
        this.searchResults = users.filter(user =>
          !this.selectedUsers.some(selected => selected.id === user.id)
        );
        this.isSearching = false;
      },
      error: (error) => {
        console.error('Error searching users:', error);
        this.searchResults = [];
        this.isSearching = false;
      }
    });
  }

  addUser(user: User): void {
    if (!this.selectedUsers.some(selected => selected.id === user.id)) {
      this.selectedUsers.push(user);
      this.searchResults = this.searchResults.filter(result => result.id !== user.id);
    }
  }

  removeUser(user: User): void {
    this.selectedUsers = this.selectedUsers.filter(selected => selected.id !== user.id);
    if (this.searchValue.trim()) {
      this.searchSubject.next(this.searchValue);
    }
  }

  createGroup(): void {
    var participantIds = this.selectedUsers.map(user => user.id);
    participantIds.push(this.authservices.currentUser!.id);
    this.chatHubService.createConversation(participantIds, this.groupName, ConversationType.Group).then(() => {
      this.router.navigate(['/conversations']);
    }).catch(err => {
      console.error('Error creating new conversation:', err);
    });
  }

  cancel() {
    this.router.navigate(['/conversations']);
  }
}
