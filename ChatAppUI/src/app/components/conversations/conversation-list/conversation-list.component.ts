import { CommonModule } from '@angular/common';
import { UserConversation } from '../../../models/conversations/responses/user-conversations-response';
import { ConversationItemComponent } from '../conversation-item/conversation-item.component';
import { RouterModule } from '@angular/router';
import { Subject, debounceTime } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ConversationType } from '../../../enums/conversation-type';
import { AuthenticationService } from '../../../services/authentication.service';
import { Component, DestroyRef, effect, Input, input, output } from '@angular/core';


@Component({
  selector: 'app-conversation-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ConversationItemComponent],
  templateUrl: './conversation-list.component.html',
  styleUrl: './conversation-list.component.css',
})
export class ConversationListComponent {

  conversations = input.required<UserConversation[]>();
  @Input({ required: true }) others!: UserConversation[];
  conversationSelected = output<UserConversation>();
  searchChanged = output<string>();
  filteredConversations: UserConversation[] = [];
  // selectedConversation: UserConversation | null = null;
  selectedConversation = input<UserConversation | null>(null);

  searchValue: string = '';
  error: string | null = null;


  private searchSubject = new Subject();

  constructor(private destroyRef: DestroyRef, private authService: AuthenticationService) {
    this.searchSubject.pipe(
      debounceTime(300),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.filterConversations();
    });


    effect(() => {
      this.filterConversations();
    });
  }

  onSelectConversation(conversation: UserConversation) {
    // this.selectedConversation = conversation;
    this.conversationSelected.emit(conversation);
  }

  onSearchInput(event: Event) {
    const target = event.target as HTMLInputElement;
    this.searchValue = target.value.trim().toLowerCase();
    this.searchSubject.next(this.searchValue);
  }

  private filterConversations() {
    // 3andk mo4kla hya enk t5ly el filter fe el parent a7sn


    if (!this.searchValue) {
      this.filteredConversations = [...this.conversations()];
      this.others = [];
      return;
    }

    this.filteredConversations = this.conversations().filter(c =>
      c.title.toLowerCase().includes(this.searchValue) || (c.type === ConversationType.Direct &&
        c.participants.some(p => p.userId !== this.authService.getCurrentUserId() && p.userName.toLowerCase().includes(this.searchValue))));


    if (this.searchValue == this.authService.getCurrentUser()?.userName.toLowerCase())
      return;

    var directConverationByUserName = this.filteredConversations.find(c =>
      c.type === ConversationType.Direct &&
      c.participants.some(p => p.userId !== this.authService.getCurrentUserId() && p.userName.toLowerCase() === this.searchValue)
    );


    if (!directConverationByUserName) {
      this.searchChanged.emit(this.searchValue);
    }
  }

  isSelected(conversation: UserConversation): boolean {
    if (this.selectedConversation()?.id)
      return this.selectedConversation()!.id === conversation.id;

    if (this.selectedConversation)
      return this.selectedConversation() === conversation;

    return false;
  }

}

