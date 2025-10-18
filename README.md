This project aims to implement order creation and order tracking functionalities. The application operates using the Code-First approach and features an N-Layer Architecture utilizing .Net Core technology.

By utilizing Swagger, you can test the API requests and obtain the necessary JSON format information.
If required, you need to edit the **launchSettings.json** file located under **MyOrderProjectAPI/Properties** to configure the Swagger port settings.
The project also includes small unit tests to enable basic functionality checks before compilation.

**For DB creation and connection:**

1.) You need to modify the **DefaultConnection** line within the **appsettings.json** file.

2.) The necessary bash commands to create the DB, in order, are:

        A) `dotnet ef migrations add InitialMigration --project MyOrderProjectAPI`
        B) `dotnet ef database update --project MyOrderProjectAPI`

**Flow Diagram:**

1.  **Client (Web/Mobile App)** Sends Request: After the user confirms the order contents, a POST request is sent to the API (`/api/orders`).
2.  **MyOrderProjectAPI (Controller)** Receives Request: The endpoint captures the incoming request and initiates the authentication/authorization check.
3.  **MyOrderProjectAPI (Service Layer)** Applies Business Logic: Validates order details, checks stock availability, calculates the price, and executes necessary business rules.
4.  **MyOrderProjectAPI (Repository Layer)** Persists Data: Transmits the command to the Database to save the processed and validated order data.
5.  **Database (DB)** Makes Data Persistent: Saves the order and sends the successful operation result back to the API.
6.  **MyOrderProjectAPI (Controller)** Generates Response: Returns a successful status code (HTTP 201 Created or 200 OK) and a JSON response containing the details of the created order to the client."
