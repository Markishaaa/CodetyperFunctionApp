CREATE TABLE Users (
    UserId NVARCHAR(255),          
    Username NVARCHAR(255) NOT NULL PRIMARY KEY,     
    PasswordHash NVARCHAR(255) NOT NULL,        
    Email NVARCHAR(255) NOT NULL UNIQUE,        
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()  
);

CREATE TABLE Languages (
    Name NVARCHAR(255) PRIMARY KEY
);

CREATE TABLE Tasks (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Shown BIT NOT NULL,
    CreatorId NVARCHAR(255) NOT NULL,
    FOREIGN KEY (CreatorId) REFERENCES Users(UserId)  
);

CREATE TABLE Snippets (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Content NVARCHAR(MAX) NOT NULL,
    Shown BIT NOT NULL,
    LanguageName NVARCHAR(255) NOT NULL,
    TaskId INT NOT NULL,
    CreatorId NVARCHAR(255) NOT NULL,
    FOREIGN KEY (LanguageName) REFERENCES Languages(Name),
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
    FOREIGN KEY (CreatorId) REFERENCES Users(UserId)       
);

CREATE TABLE Scores (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Wpm INT NOT NULL,
    Accuracy INT NOT NULL,
    UserId NVARCHAR(255) NOT NULL,
    SnippetId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (SnippetId) REFERENCES Snippets(Id)  
);

CREATE TABLE Snippet_Contributor (
    SnippetId INT NOT NULL,
    UserId NVARCHAR(255) NOT NULL,
    FOREIGN KEY (SnippetId) REFERENCES Snippets(Id),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)     
);

CREATE TABLE ArchivedTasks (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    DeniedAt DATETIME NOT NULL DEFAULT GETDATE(),
    Reason NVARCHAR(MAX),
    CreatorId NVARCHAR(255) NOT NULL,
    StaffId NVARCHAR(255) NOT NULL,
    FOREIGN KEY (CreatorId) REFERENCES Users(UserId),
    FOREIGN KEY (StaffId) REFERENCES Users(UserId)      
);

CREATE TABLE ArchivedSnippets (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Content NVARCHAR(MAX) NOT NULL,
    DeniedAt DATETIME NOT NULL DEFAULT GETDATE(),
    Reason NVARCHAR(MAX),
    LanguageName NVARCHAR(255) NOT NULL,
    TaskId INT NOT NULL,
    CreatorId NVARCHAR(255) NOT NULL,
    StaffId NVARCHAR(255) NOT NULL,
    FOREIGN KEY (LanguageName) REFERENCES Languages(Name),
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
    FOREIGN KEY (CreatorId) REFERENCES Users(UserId),
    FOREIGN KEY (StaffId) REFERENCES Users(UserId)       
);

CREATE TABLE Roles (
    RoleName NVARCHAR(50) NOT NULL PRIMARY KEY
);

INSERT INTO Roles (RoleName) VALUES 
('SuperAdmin'),
('Admin'),
('Moderator'),
('User');

ALTER TABLE Users
ADD RoleName NVARCHAR(50) NOT NULL;

ALTER TABLE Users
ADD CONSTRAINT FK_User_Role FOREIGN KEY (RoleName) REFERENCES Roles(RoleName);