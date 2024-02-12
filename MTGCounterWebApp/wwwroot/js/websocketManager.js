window.websocketManager = {
    connectAndSendMessage: function (url, message) {
        const socket = new WebSocket(url);

        socket.onopen = function (event) {
            console.log("Connection opened");
            socket.send(message);
        };

        socket.onmessage = function (event) {
            console.log("Message from server ", event.data);
        };

        socket.onclose = function (event) {
            console.log("Connection closed");
        };

        socket.onerror = function (error) {
            console.error("WebSocket error: ", error);
        };
    }
};
