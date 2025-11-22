# Building Real-Time Chat with React and SignalR

## Introduction

In this article, we will explore how to build a real-time chat application using React, SignalR, and Markdown streaming. This guide will walk you through the process of setting up a React application, integrating SignalR for live messaging, and implementing Markdown streaming for dynamic content rendering.

## Prerequisites

Before we begin, ensure you have the following tools and knowledge:

- **Node.js and npm**: Make sure you have Node.js and npm installed on your machine.
- **Basic knowledge of React**: Familiarity with React components and hooks.
- **Understanding of TypeScript**: Basic understanding of TypeScript is recommended.
- **SignalR basics**: Some experience with SignalR will be helpful.

## Setting Up the React Application

1. **Create a new React app**:

    ```bash
    npx create-react-app real-time-chat --template typescript
    ```

2. **Install necessary packages**:
    ```bash
    npm install @microsoft/signalr marked
    ```

## Integrating SignalR for Real-Time Messaging

SignalR is a library that simplifies adding real-time web functionality to applications. It allows server-side code to push content to clients instantly.

### Setting Up SignalR Client

1. **Create a SignalR connection**:

    ```typescript
    import * as signalR from "@microsoft/signalr";

    const connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").configureLogging(signalR.LogLevel.Information).build();
    ```

2. **Start the connection**:

    ```typescript
    connection.start().catch((err) => console.error(err.toString()));
    ```

3. **Handle incoming messages**:
    ```typescript
    connection.on("ReceiveMessage", (user, message) => {
        const msg = document.createElement("div");
        msg.textContent = `${user}: ${message}`;
        document.getElementById("messagesList").appendChild(msg);
    });
    ```

## Implementing Markdown Streaming

Markdown allows users to format text using simple syntax. We will use the `marked` library to parse and render Markdown.

1. **Render Markdown messages**:

    ```typescript
    import { marked } from "marked";

    function renderMarkdown(message: string): string {
        return marked(message);
    }
    ```

2. **Display Markdown in the chat**:
    ```typescript
    const markdownMessage = renderMarkdown(message);
    document.getElementById("messagesList").innerHTML += `<div>${markdownMessage}</div>`;
    ```

## Conclusion

By following these steps, you have successfully created a real-time chat application using React, SignalR, and Markdown streaming. This application allows users to send and receive messages in real-time while rendering Markdown content dynamically.

## Additional Resources

- [React Documentation](https://reactjs.org/docs/getting-started.html)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-5.0)
- [Marked Documentation](https://marked.js.org/)

## Next Steps

Consider adding more features such as user authentication, message history, and emoji support to enhance your chat application.
