import { CommonModule } from '@angular/common';
import { UserConversation } from '../../../models/conversations/responses/user-conversations-response';
import { ConversationItemComponent } from '../conversation-item/conversation-item.component';
import { RouterModule } from '@angular/router';
import { Subject, debounceTime } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ConversationType } from '../../../enums/conversation-type';
import { AuthenticationService } from '../../../services/authentication.service';
import { Component, DestroyRef, effect, Input, input, output, computed } from '@angular/core';
import { MessageResponse } from '../../../models/conversations/responses/conversation-messages-response';


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
  selectedConversation = input<UserConversation | null>(null);

  conversationSelected = output<UserConversation>();
  searchChanged = output<string>();
  filteredConversations: UserConversation[] = [];

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
    console
    this.conversationSelected.emit(conversation);
  }

  onSearchInput(event: Event) {
    const target = event.target as HTMLInputElement;
    this.searchValue = target.value.trim().toLowerCase();
    this.searchSubject.next(this.searchValue);
  }

  private filterConversations() {
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


    if (!directConverationByUserName)
      this.searchChanged.emit(this.searchValue);

  }

  isSelectedConversation = computed(() => {
    return (conv: UserConversation) => {
      const selected = this.selectedConversation();
      return selected?.id ? selected?.id === conv.id : selected === conv;
    };
  });
}

