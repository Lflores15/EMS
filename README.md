# Using .env  
  - To use .env create it by duplicating .env.dev and if wanting to change password do so in there for default Admin account, changing name to .env
  - Example:
    ```
    DEFAULT_ADMIN_EMAIL=admin@admin.com
    ADMIN_PASSWORD=StrongPassword123
    ```
# Creating Database
  1. Add a Migration
    Run the following command to create the initial migration for your database schema:
     ```
     dotnet ef migrations add InitialCreate
     ```
     
  3. Apply the Migration
    Use the following command to update the database and apply the migration:
      ```
      dotnet ef database update
      ```
     
  4. Verify the Database
    Check your database server (e.g., SQL Server Management Studio, Azure Data Studio, etc.) to ensure the database has been created.

  5. Update the Database Schema (if needed)
    If you make changes to the models, you can create a new migration and update the database:
      ```
      dotnet ef migrations add UpdateSomeFeature
      dotnet ef database update
      ```
     
Notes:
  Ensure your appsettings.json file contains the correct connection string for your database.
  For troubleshooting, make sure Microsoft.EntityFrameworkCore.Tools is installed in your project. You can add it with:
    ```
    dotnet add package Microsoft.EntityFrameworkCore.Tools
    ```

## Building and Running the Application

### Build the Application

1. Compile the application using the following command:
    ```
    dotnet build EMS.csproj
    ```

2. Start the application with:
    ```
    dotnet run
    ```

3. Once the application is running, navigate to the provided URL (e.g., http://localhost:5000) in your web browser to access it.
