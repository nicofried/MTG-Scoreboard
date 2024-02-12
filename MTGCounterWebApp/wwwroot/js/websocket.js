// Define a variable to hold the WebSocket instance
let webSocket = null;

// Function to establish WebSocket connection
function connectToWebSocket(url) {
    webSocket = new WebSocket(url);

    webSocket.onopen = function(event) {
        console.log("Connection established");
    };

    webSocket.onmessage = function(event) {
        console.log("Message from server: ", event.data);
    };

    webSocket.onclose = function(event) {
        console.log("Connection closed");
    };

    webSocket.onerror = function(error) {
        console.error("WebSocket error: ", error);
    };
}

// Function to send a message through the WebSocket
function sendMessageToWebSocket(message) {
    if (webSocket && webSocket.readyState === WebSocket.OPEN) {
        webSocket.send(message);
    } else {
        console.error("WebSocket is not connected.");
    }
}

// Export the functions to be accessible from Blazor
window.websocketInterop = {
    connect: connectToWebSocket,
    sendMessage: sendMessageToWebSocket
};
