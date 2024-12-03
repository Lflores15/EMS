# Using .env  
  - To use .env create it by duplicating .env.dev and if wanting to change password do so in there for default Admin account, changing name to .env
  - Example: DEFAULT_ADMIN_EMAIL=admin@admin.com        ADMIN_PASSWORD=StrongPassword123

# Creating the Database
  - To create the database start by using dotnet ef migrations add
  - Example: dotnet ef migrations add initialCreate



# GitHub Branch Naming Conventions

## Branch Naming Guide

- **Develop/{ITEM}**
  - Used for changes that **modify major parts** of the codebase.
  - Example: `Develop/LoginSystem` for significant updates to the login functionality.

- **Update/{ITEM}**
  - Used for changes that **update smaller parts** of the codebase.
  - Example: `Update/ReadmeFile` for minor documentation or code adjustments.

- **Feature/{ITEM}**
  - Used for adding **new features** that aren't major changes but enhance the app.
  - Example: `Feature/UserProfile` for adding a new user profile page.

- **Fix/{ITEM}**
  - Used for **bug fixes** or small corrections in the code.
  - Example: `Fix/LoginButton` for fixing an issue with the login button.

- **Hotfix/{ITEM}**
  - Used for **urgent fixes** applied directly to the main branch in production, usually reserved for critical bugs.
  - Example: `Hotfix/SecurityPatch` for a security-related issue that needs immediate attention.

---

Using these branch naming conventions helps keep development organized and distinguishes between major changes, minor updates, and fixes.
