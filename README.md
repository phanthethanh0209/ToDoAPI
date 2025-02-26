# Project Name: Todo List API

## Description  
The Todo List API is a backend RESTful API that allows users to efficiently manage their task lists. It provides a platform for users to register, log in, create, update, delete, and view their tasks. Designed to be scalable and easy to use, this API employs JSON Web Tokens (JWT) for user authentication.

This project serves as a backend solution for a Todo List application, making it an ideal choice for developers looking to integrate task management features into their web or mobile applications.  

You can view the project details on link here: [Project_URL](https://roadmap.sh/projects/todo-list-api). 

## Features  
- **User Registration**: Allows new users to create an account.  
- **User Login**: Authenticates users and issues JWT tokens.  
- **Task List Management**:  
  - Create new tasks  
  - View task list with pagination and search by title 
  - Update tasks  
  - Delete tasks
- **Security**: Uses JWT for authentication and access control. Implements refresh tokens to allow users to obtain new access tokens without requiring them to log in again, enhancing security and user experience.
 
## Technologies  
- **ASP.NET Core API 8**
- **SQL Server**

## Database  
This project uses the Code First approach with Entity Framework Core to create and manage the database. The database schema is defined using C# classes, and migrations are used to apply changes to the database.  

## Usage Instructions
- During registration, users need to provide a name, email, and password.
- After successful login, users will receive a JWT that must be sent along with requests to secure endpoints.
- To retrieve the task list with pagination, use the /api/todos endpoint and pass the page and limit query parameters to specify the page number and the number of tasks per page.

## Contributing
Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request.
