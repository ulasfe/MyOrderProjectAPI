## 1\. Project Overview

This project implements core functionalities for **order creation** and **order tracking**. It is designed as a high-performance backend service to securely handle e-commerce operations.

### Key Features

  * Securely handle new order requests.
  * Track the status of existing orders.
  * Robust error handling and validation.

## 2\. Technical Stack and Architecture

This section highlights the technologies and design patterns used, demonstrating a commitment to industry best practices.

### Technologies

| Category | Technology | Purpose |
| :--- | :--- | :--- |
| **Backend** | `.NET Core` / `C#` | Primary development framework for high-performance API. |
| **Database** | `Entity Framework Core (EF Core)` | ORM for data access and manipulation. |
| **API Testing** | `Swagger` / `OpenAPI` | API documentation and request testing interface. |
| **Unit Testing** | `xUnit` (or NUnit/MSTest) | Framework for ensuring code quality and reliability. |

### Architecture and Design

  * **Architecture:** **N-Layered Architecture** (Separation of concerns between Presentation, Service, and Data Access layers).
  * **Database Approach:** **Code-First** development using EF Core for model definition and migration management.
  * **Authentication/Authorization:** **JWT (JSON Web Token)** based security for validating and authorizing user access.

## 3\. Getting Started

Follow these steps to set up and run the project locally.

### Prerequisites

  * .NET SDK (Version e.g., 8.0)
  * Git
  * SQL Server / SQL Server LocalDB

### Installation and Setup

1.  **Clone the Repository:**

    ```bash
    git clone [YOUR-REPO-URL]
    cd [Your-Project-Folder]
    ```

2.  **Configure Database Connection:**

      * Open the **`appsettings.json`** file in the `MyOrderProjectAPI` project.
      * Update the **`DefaultConnection`** string to point to your local database instance.

3.  **Database Migration (Code-First):**
    Use the Entity Framework Core CLI commands to create the database schema:

    ```bash
    # A) Create the initial migration (if not already done)
    dotnet ef migrations add InitialMigration --project MyOrderProjectAPI

    # B) Apply migrations to update the database
    dotnet ef database update --project MyOrderProjectAPI
    ```

4.  **Run the API:**

    ```bash
    dotnet run --project MyOrderProjectAPI
    ```

## 4\. API Documentation and Testing

The API is documented using Swagger for easy testing and exploration of endpoints.

  * **Swagger UI Access:** Once the application is running, navigate to: `https://localhost:<Port-Number>/swagger` (The default port is typically configured in `launchSettings.json`).
  * **Port Configuration:** To change the port, edit the **`launchSettings.json`** file under `MyOrderPojectAPI/Properties`.

### Core Endpoints

| HTTP Method | Endpoint | Description | Status Code |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/orders` | Creates a new customer order. | `201 Created` |
| `GET` | `/api/orders/{id}` | Retrieves the details and current status of a specific order. | `200 OK` |

## 5\. Architectural Flow and Logic

This diagram illustrates the process from a client request to data persistence, emphasizing the separation of duties across the N-Layers.

### Order Creation Flow

| Step | Layer | Action | Description |
| :--- | :--- | :--- | :--- |
| **1.** | **Client (Web/Mobile App)** | Sends Request | The user confirms the order and sends a `POST` request to the API (`/api/orders`). |
| **2.** | **Controller (Presentation Layer)** | Receives Request | The endpoint captures the request, performs basic model binding, and starts **Authentication/Authorization** checks. |
| **3.** | **Service Layer (Business Logic)** | Executes Logic | The service validates order details, checks stock availability, calculates the final price, and enforces all necessary business rules. |
| **4.** | **Repository Layer (Data Access)** | Persists Data | The repository prepares and transmits the command to the Database to save the processed and validated order data. |
| **5.** | **Database (DB)** | Saves Data\*\* | Persists the order and returns the success result to the API. |
| **6.** | **Controller (Presentation Layer)** | Generates Response | Returns a successful status code (`HTTP 201 Created`) and a JSON response containing the details of the newly created order. |

### Unit Tests

The project includes small unit tests primarily focused on the **Service Layer** to ensure the reliability and correctness of the core business logic before deployment.
