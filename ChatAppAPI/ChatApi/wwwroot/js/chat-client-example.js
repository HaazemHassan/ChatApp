// Example JavaScript SignalR Client Usage
// This file shows how to use the ChatHub from the frontend

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub", {
        accessTokenFactory: () => localStorage.getItem("accessToken")
    })
    .build();

// Start connection
connection.start().then(function () {
    console.log("Connected to ChatHub");
}).catch(function (err) {
    console.error(err.toString());
});

// ===== CONNECTION EVENTS =====
connection.on("UserConnected", function (userId) {
    console.log(`User ${userId} came online`);
    // Update UI to show user as online
});

connection.on("UserDisconnected", function (userId) {
    console.log(`User ${userId} went offline`);
    // Update UI to show user as offline
});

// ===== CONVERSATION EVENTS =====
connection.on("ConversationCreated", function (conversation) {
    console.log("New conversation created:", conversation);
    // Add conversation to UI
});

connection.on("ParticipantAdded", function (conversationId, userId, role) {
    console.log(`User ${userId} added to conversation ${conversationId}`);
    // Update conversation participants in UI
});

connection.on("ParticipantRemoved", function (conversationId, userId) {
    console.log(`User ${userId} removed from conversation ${conversationId}`);
    // Update conversation participants in UI
});

connection.on("AddedToConversation", function (conversationId) {
    console.log(`You were added to conversation ${conversationId}`);
    // Add conversation to user's conversation list
    joinConversation(conversationId);
});

// ===== MESSAGE EVENTS =====
connection.on("ReceiveMessage", function (message) {
    console.log("New message received:", message);
    // Add message to conversation UI
    displayMessage(message);
    
    // Automatically mark message as delivered when received
    markMessageAsDelivered(message.Id);
});

connection.on("MessageDelivered", function (messageId, userId) {
    console.log(`Message ${messageId} delivered to user ${userId}`);
    // Update message delivery status in UI
    updateMessageDeliveryStatus(messageId, userId);
});

connection.on("MessageEdited", function (data) {
    console.log("Message edited:", data);
    // Update message in UI
    updateMessageInUI(data.MessageId, data.NewContent, data.EditedAt);
});

connection.on("MessageDeleted", function (messageId) {
    console.log("Message deleted:", messageId);
    // Remove or mark message as deleted in UI
    markMessageAsDeletedInUI(messageId);
});

connection.on("MessageRead", function (messageId, userId) {
    console.log(`Message ${messageId} read by user ${userId}`);
    // Update message read status in UI
});

// ===== TYPING EVENTS =====
connection.on("UserStartedTyping", function (conversationId, userId) {
    console.log(`User ${userId} started typing in conversation ${conversationId}`);
    // Show typing indicator in UI
    showTypingIndicator(conversationId, userId);
});

connection.on("UserStoppedTyping", function (conversationId, userId) {
    console.log(`User ${userId} stopped typing in conversation ${conversationId}`);
    // Hide typing indicator in UI
    hideTypingIndicator(conversationId, userId);
});

// ===== ERROR HANDLING =====
connection.on("Error", function (message) {
    console.error("Hub Error:", message);
    // Show error to user
    showErrorMessage(message);
});

// ===== CLIENT METHODS TO CALL HUB =====

// Join a conversation
function joinConversation(conversationId) {
    connection.invoke("JoinConversation", conversationId);
}

// Leave a conversation
function leaveConversation(conversationId) {
    connection.invoke("LeaveConversation", conversationId);
}

// Create new conversation
function createConversation(title, type, participantIds) {
    connection.invoke("CreateConversation", title, type, participantIds);
}

// Send message
function sendMessage(conversationId, content, messageType = 1, replyToMessageId = null) {
    connection.invoke("SendMessage", conversationId, content, messageType, replyToMessageId);
}

// Edit message
function editMessage(messageId, newContent) {
    connection.invoke("EditMessage", messageId, newContent);
}

// Delete message
function deleteMessage(messageId) {
    connection.invoke("DeleteMessage", messageId);
}

// Mark message as delivered
function markMessageAsDelivered(messageId) {
    connection.invoke("MessageDelivered", messageId);
}

// Mark message as read
function markMessageAsRead(messageId) {
    connection.invoke("MarkMessageAsRead", messageId);
}

// Add participant to conversation
function addParticipant(conversationId, userId, role = 2) { // 2 = Member
    connection.invoke("AddParticipant", conversationId, userId, role);
}

// Remove participant from conversation
function removeParticipant(conversationId, userId) {
    connection.invoke("RemoveParticipant", conversationId, userId);
}

// Start typing
function startTyping(conversationId) {
    connection.invoke("StartTyping", conversationId);
}

// Stop typing
function stopTyping(conversationId) {
    connection.invoke("StopTyping", conversationId);
}

// ===== UTILITY FUNCTIONS =====
function displayMessage(message) {
    // Implementation depends on your UI framework
    console.log("Displaying message:", message);
}

function updateMessageInUI(messageId, newContent, editedAt) {
    // Implementation depends on your UI framework
    console.log(`Updating message ${messageId} with content: ${newContent}`);
}

function markMessageAsDeletedInUI(messageId) {
    // Implementation depends on your UI framework
    console.log(`Marking message ${messageId} as deleted`);
}

function updateMessageDeliveryStatus(messageId, userId) {
    // Implementation depends on your UI framework
    console.log(`Updating delivery status for message ${messageId} delivered to user ${userId}`);
}

function showTypingIndicator(conversationId, userId) {
    // Implementation depends on your UI framework
    console.log(`Showing typing indicator for user ${userId} in conversation ${conversationId}`);
}

function hideTypingIndicator(conversationId, userId) {
    // Implementation depends on your UI framework
    console.log(`Hiding typing indicator for user ${userId} in conversation ${conversationId}`);
}

function showErrorMessage(message) {
    // Implementation depends on your UI framework
    alert("Error: " + message);
}

/* 
ENUM VALUES FOR REFERENCE:
ConversationType: { Direct: 1, Group: 2 }
MessageType: { Text: 1, Image: 2, File: 3, Audio: 4 }
ConversationParticipantRole: { Owner: 1, Member: 2 }
DeliveryStatus: { Sent: 1, Delivered: 2, Read: 3 }
*/