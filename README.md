# Codetyper

**Codetyper** is a web application designed to improve your coding speed and accuracy through a fun and interactive typing game. Users can practice typing code snippets, track their performance, and explore a growing library of programming challenges.

---

## Features

### Core Functionality

- **Typing Game**: Users are presented with random code snippets and can start typing to initiate a timer. Once the code is successfully typed, the user's Words Per Minute (WPM) is calculated, and a new snippet is displayed.
- **Code Library**: Browse a comprehensive list of programming tasks and solutions across various languages.

### User Roles

1. **Guest**:
   - Access the game and library without registration.
2. **Registered Users**:
   - Submit requests for new code snippets or programming tasks.
3. **Moderators**:
   - Approve or reject user-submitted content to ensure quality and relevance.
4. **Admins**:
   - Add new programming languages supported by the platform. This is restricted due to the use of the React-Codemirror 5 library for syntax highlighting.

---

## Technology Stack

### Database

- **Azure SQL**: Stores application data, including users, code snippets, programming tasks, and roles.
- **Key Tables**:
  - `Users`: User information and roles.
  - `Languages`: Supported programming languages.
  - `Tasks`: Programming challenges.
  - `Snippets`: Code snippets for typing.
  - `Archived_Tasks` and `Archived_Snippets`: Rejected tasks and snippets.

### Backend

- **Azure Function App**: A serverless architecture using C#.
- **Structure**:
  - Functions Layer: Handles HTTP requests and routes them to the appropriate services.
  - Services Layer: Contains business logic, data validation, and operations.
  - Repository Layer: Manages database interactions using Dapper ORM.

### Frontend

- **React (with TypeScript)**: Provides an interactive and responsive user interface.
- **React-Codemirror 5**: Used for syntax highlighting in the code editor.

---

## Getting Started

### Prerequisites

- **Node.js**: For running the frontend.
- **Azure Account**: For setting up the backend and database.

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/Markishaaa/CodetyperFunctionApp.git
   ```
2. Navigate to the project folder and install dependencies for the frontend:
   ```bash
   cd codetyper/frontend
   npm install
   ```
3. Set up the Azure Function backend and database according to the provided documentation in the `/docs` folder.

### Running the Application

1. Start the frontend:
   ```bash
   npm start
   ```
2. Deploy or run the Azure Function App locally.
3. Access the application at `http://localhost:3000`.

---

## License

This project is licensed under the [MIT License](LICENSE.txt).
