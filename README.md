# IdeaFlow [live Preview](https://ideaflow-client.vercel.app/)

IdeaFlow is a collaborative mind mapping platform that enables users to create, publish, and manage mind maps. Users can favorite mind maps, clone existing diagrams, and share their ideas seamlessly.

## Features
- **User Authentication**: Secure login and registration with identity and SMTP support (email confirmation, forgot password, reset password).
- **Mind Map Management**: Users can create, edit, and delete mind maps.
- **Publishing & Sharing**: Users can publish mind maps for public access.
- **Favorites**: Users can save mind maps to their favorites.
- **Cloning**: Users can duplicate an existing mind map for further customization.
- **Resilient API Calls**: Implemented using Polly for handling transient failures.
- **Validation**: Fluent Validation ensures data integrity.
- **CQRS Pattern**: Clean separation of concerns with Command and Query Responsibility Segregation.
- **AI-Enhanced**: Utilizes Gemini API and DeepSeek API for AI-powered enhancements.
- **Automated Mapping**: AutoMapper simplifies data transformations.
- **Containerization**: Fully Dockerized setup with Docker Compose for easy deployment.

## Tech Stack
- **Backend**: ASP.NET Web API
- **Database**: Entity Framework Core
- **Architecture**: Onion Architecture
- **AI Integration**: Gemini API, DeepSeek API
- **Resilience & Fault Handling**: Polly
- **Validation**: Fluent Validation
- **Object Mapping**: AutoMapper
- **Containerization**: Docker / Docker Compose
- **Security**: Identity & SMTP for authentication and email verification

## Database Schema
The core entities in IdeaFlow include:

1. **User**: Represents the application users with authentication details.
2. **Diagram**: Stores the mind maps, their content, and publishing status.
3. **Favorite**: Allows users to mark and save diagrams as favorites.

### Relationships:
- A **User** can create multiple **Diagrams**.
- A **Diagram** can be marked as a favorite by multiple users.
- Users can **clone** existing diagrams for further modifications.

## Installation & Setup
1. Clone the repository:
   ```sh
   git clone https://github.com/ahmed-ateya1/IdeaFlow.git
   cd IdeaFlow
   ```
2. Build and run the project using Docker Compose:
   ```sh
   docker-compose up --build
   ```
3. Configure environment variables for Identity & SMTP (email services) and AI API keys.

## Schema
![Screenshot 2025-04-01 035526](https://github.com/user-attachments/assets/779b1559-f9a9-409d-9492-0f3a5d0d2ba3)

## License
This project is licensed under the MIT License.

