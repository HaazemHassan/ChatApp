import { Component, input, output } from '@angular/core';
import { UserConversation } from '../../../models/conversations/responses/user-conversations-response';

@Component({
  selector: 'app-conversation-item',
  standalone: true,
  imports: [],
  templateUrl: './conversation-item.component.html',
  styleUrl: './conversation-item.component.css',
})
export class ConversationItemComponent {
  conversation = input.required<UserConversation>();
  isSelected = input<boolean>(false);
  selectedConversation = output<UserConversation>();


  onSelect() {
    if (this.conversation())
      this.selectedConversation.emit(this.conversation());
    console.log('Selected conversation in conversation-Item:', this.conversation());
  }

  // Get initials from the title (first letter of up to two words)
  extractInitialsFromTitle(title: string | null | undefined): string {
    if (!title) return '?';
    const parts = title.trim().split(' ').slice(0, 2);
    return parts
      .map((p) => p.charAt(0))
      .join('')
      .toUpperCase();
  }
}
