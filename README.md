# DatingApp - Angular and .NET7 Web Application

Welcome to DatingApp, a modern web application built with Angular14 for the frontend and .NET7 for the API backend. This application aims to connect people looking for meaningful relationships in a user-friendly and secure environment.

## Getting Started

### Prerequisites

To run the application locally, ensure you have the following installed:

1. [.NET7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
2. [Node.js](https://nodejs.org/) (v14 or higher)
3. [Angular CLI](https://angular.io/cli) (v14 or higher)
4. [PostgresSQL](https://postgresql.org/)
5. [Docker](https://www.docker.com/) (optional - if you prefer running the app using Docker)

### Launching the Application Locally

1. Clone this repository to your local machine:

2. Build the Angular frontend => cd to the `client folder` and then do : `npm install && ng build`

3. Install .NET dependencies: cd to the `API folder` then do : `dotnet run`

### Running with Docker
Alternatively, you can use Docker to run the application by following these steps:

0. pull and run and Postgres image.

1. Pull the application image by : `docker pull neokyuubi/datingapp`

2. then build it : `docker build -t neokyuubi/datingapp`

3. then in the root folder do : `docker run --rm -it -p 8080:80 neokyuubi/datingapp:latest`

### SSL Certificate (Local Setup Only)

If you choose to run the application locally and wish to use SSL, follow these steps:

Install the certificate located at `Client/ssl` folder

If you don't want to install and run without SSL then :

Uncomment line 47 and comment out line 48 in `API/Program.cs`.

and then rebuild the application by `ng build`

### Demonstration

![Demo 1](./Demos/Desktop%208-2-2023%205-12-35%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-20-32%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-14-02%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-26-06%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-09-49%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-23-10%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-19-06%20PM.gif)

