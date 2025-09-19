import { Component, computed, input, output } from '@angular/core';
import { UserConversation } from '../../../models/conversations/responses/user-conversations-response';
import { TimeAgoPipe } from "../../../pipes/time-ago.pipe";
import { AsyncPipe } from '@angular/common';
import { StringInitialsPipe } from "../../../pipes/string-initials.pipe";
import { MessageResponse } from '../../../models/conversations/responses/conversation-messages-response';
import { ConversationType } from '../../../enums/conversation-type';

@Component({
  selector: 'app-conversation-item',
  standalone: true,
  imports: [TimeAgoPipe, AsyncPipe, StringInitialsPipe],
  templateUrl: './conversation-item.component.html',
  styleUrl: './conversation-item.component.css',
})
export class ConversationItemComponent {
  conversation = input.required<UserConversation>();
  isSelected = input<boolean>(false);
  selectedConversation = output<UserConversation>();
  ConversationType = ConversationType;

  onSelect() {
    if (this.conversation())
      this.selectedConversation.emit(this.conversation());
  }

  lastMessagePreview = computed((): string => {
    const lastMessage = this.conversation().lastMessage;
    if (!lastMessage || !lastMessage.content)
      return 'No messages yet';

    const maxLength = 50;
    const content = lastMessage.content.trim();

    return content.length <= maxLength ? content : content.substring(0, maxLength) + '...';
  });

}
