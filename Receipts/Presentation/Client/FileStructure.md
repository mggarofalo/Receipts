# Receipt Management Client Application - Hierarchical File Structure

This outline presents a comprehensive hierarchical structure of the required files for the `Receipts` Client application. It is designed to facilitate efficient development, maintainability, and scalability. The structure is organized by feature areas, adhering to best practices in Blazor application architecture.

## Root Directory
Receipts/Presentation/Client/
├── Components/
│ ├── DateOnlyPicker.razor
│ └── ConfirmationDialog.razor
├── Interfaces/
│ └── Services/
│   ├── Core/
│   │ ├── IAccountService.cs
│   │ ├── IReceiptItemService.cs
│   │ ├── IReceiptService.cs
│   │ └── ITransactionService.cs
│   └── Aggregates/
│     ├── IReceiptWithItemsService.cs
│     ├── ITransactionAccountService.cs
│     └── ITripService.cs
├── Services/
│ ├── Core/
│ │ ├── AccountService.cs
│ │ ├── ReceiptItemService.cs
│ │ ├── ReceiptService.cs
│ │ └── TransactionService.cs
│ ├── Aggregates/
│ │ ├── ReceiptWithItemsService.cs
│ │ ├── TransactionAccountService.cs
│ │ └── TripService.cs
│ └── SignalRService.cs
├── Pages/
│ ├── Home.razor
│ ├── Accounts/
│ │ ├── AccountsList.razor
│ │ ├── AccountCreate.razor
│ │ └── AccountEdit.razor
│ ├── Receipts/
│ │ ├── ReceiptsList.razor
│ │ ├── ReceiptCreate.razor
│ │ └── ReceiptEdit.razor
│ ├── Transactions/
│ │ ├── TransactionsList.razor
│ │ ├── TransactionCreate.razor
│ │ └── TransactionEdit.razor
│ ├── Trips/
│ │ ├── TripsList.razor
│ │ ├── TripCreate.razor
│ │ └── TripEdit.razor
│ └── Reports/
│   └── ReportsPage.razor
├── Layout/
│ ├── MainLayout.razor
│ └── NavMenu.razor
├── wwwroot/
│ ├── css/
│ │ ├── app.css
│ │ └── bootstrap/
│ ├── favicon.png
│ └── index.html
├── App.razor
├── Program.cs
├── _Imports.razor
└── Client.csproj

## Detailed Breakdown

### 1. Components

#### 1.1 Pages

Each feature area has its own folder containing pages related to that feature. This modular approach enhances organization and scalability.

- **Home**
  - **Home.razor**: Landing page providing an overview and quick links to other sections.

- **Accounts**
  - **AccountsList.razor**: Displays a list of all accounts with options to view, edit, or delete.
  - **AccountCreate.razor**: Form for creating a new account.
  - **AccountEdit.razor**: Form for editing an existing account.

- **Receipts**
  - **ReceiptsList.razor**: Displays all receipts with options to view, edit, or delete.
  - **ReceiptCreate.razor**: Form for creating a new receipt, including adding receipt items.
  - **ReceiptEdit.razor**: Form for editing an existing receipt and its items.

- **Transactions**
  - **TransactionsList.razor**: Displays all transactions with options to view, edit, or delete.
  - **TransactionCreate.razor**: Form for creating a new transaction and associating it with an account.
  - **TransactionEdit.razor**: Form for editing an existing transaction and its account association.

- **Trips**
  - **TripsList.razor**: Displays all trips with options to view, edit, or delete.
  - **TripCreate.razor**: Form for creating a new trip by associating receipts and transactions.
  - **TripEdit.razor**: Form for editing an existing trip and its associations.

- **Reports**
  - **ReportsPage.razor**: Page for generating and viewing detailed reports.

#### 1.2 Shared

Shared components and services that are reused across multiple pages.

- **Components**
  - **DateOnlyPicker.razor**: Custom date picker component capable of handling `DateOnly` types.
  - **ConfirmationDialog.razor**: Reusable dialog for confirming actions like deletions.
  - **Notification.razor**: Component for displaying real-time notifications to users.

- **Services**
  - **SignalRService.cs**: Manages real-time communication between the client and server using SignalR.
  - **ReceiptClient.cs**: Handles API interactions for receipts.
  - **AccountClient.cs**: Handles API interactions for accounts.
  - **TransactionClient.cs**: Handles API interactions for transactions.
  - **TripClient.cs**: Handles API interactions for trips.

#### 1.3 Layout

Layout components define the common structure applied across multiple pages.

- **MainLayout.razor**: Defines the main layout structure, including sidebar and main content area.
- **NavMenu.razor**: Sidebar navigation menu with links to different sections of the application.
- **Header.razor**: Top navigation bar containing elements like the About link.

#### 1.4 Imports.razor

Imports common namespaces and components to be used across all Razor components.

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using Client
@using Client.Components
@using MudBlazor
```

#### 1.5 Routes.razor

Defines the routing configuration for the application.

```razor
<Router AppAssembly="typeof(Program).Assembly">
	<Found Context="routeData">
		<RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
		<FocusOnNavigate RouteData="routeData" Selector="h1" />
	</Found>
	<NotFound>
		<LayoutView Layout="typeof(Layout.MainLayout)">
			<p>Sorry, there's nothing at this address.</p>
		</LayoutView>
	</NotFound>
</Router>
```


### 2. Services

Scoped services responsible for handling business logic and API communications.

- **ReceiptService.cs**: Contains methods for managing receipts, including creation, retrieval, updating, and deletion.
- **AccountService.cs**: Contains methods for managing accounts.
- **TransactionService.cs**: Contains methods for managing transactions.
- **TripService.cs**: Contains methods for managing trips and their associations.

### 3. wwwroot

Static assets served by the application.

- **css/**
  - **app.css**: Global application styles.
  - **Client.styles.css**: Client-specific styles.
  - **...**: Additional CSS files as needed.

- **js/**
  - **...**: JavaScript files if any additional client-side scripting is required.

- **images/**
  - **favicon.png**: Favicon for the application.
  - **...**: Additional images as needed.

### 4. Layout Styling

CSS files associated with layout components to ensure consistent styling across the application.

- **MainLayout.razor.css**

  ```css
  .page {
      position: relative;
      display: flex;
      flex-direction: column;
  }

  main {
      flex: 1;
  }

  .sidebar {
      background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);
  }

  .top-row {
      background-color: #f7f7f7;
      border-bottom: 1px solid #d6d5d5;
      justify-content: flex-end;
      height: 3.5rem;
      display: flex;
      align-items: center;
  }

      .top-row ::deep a, .top-row ::deep .btn-link {
          white-space: nowrap;
          margin-left: 1.5rem;
          text-decoration: none;
      }

      .top-row ::deep a:hover, .top-row ::deep .btn-link:hover {
          text-decoration: underline;
      }

      .top-row ::deep a:first-child {
          overflow: hidden;
          text-overflow: ellipsis;
      }

  @media (max-width: 640.98px) {
      .top-row {
          justify-content: space-between;
      }

      .top-row ::deep a, .top-row ::deep .btn-link {
          margin-left: 0;
      }
  }

  @media (min-width: 641px) {
      .page {
          flex-direction: row;
      }

      .sidebar {
          width: 250px;
          height: 100vh;
          position: sticky;
          top: 0;
      }

      .top-row {
          position: sticky;
          top: 0;
          z-index: 1;
      }

      .top-row.auth ::deep a:first-child {
          flex: 1;
          text-align: right;
          width: 0;
      }

      .top-row, article {
          padding-left: 2rem !important;
          padding-right: 1.5rem !important;
      }
  }

  #blazor-error-ui {
      background: lightyellow;
      bottom: 0;
      box-shadow: 0 -1px 2px rgba(0, 0, 0, 0.2);
      display: none;
      left: 0;
      padding: 0.6rem 1.25rem 0.7rem 1.25rem;
      position: fixed;
      width: 100%;
      z-index: 1000;
  }

      #blazor-error-ui .dismiss {
          cursor: pointer;
          position: absolute;
          right: 0.75rem;
          top: 0.5rem;
      }
  ```

- **NavMenu.razor.css**

  ```css
  @media (min-width: 641px) {
      .navbar-toggler {
          display: none;
      }
  }

  .bi-list-nested-nav-menu {
      margin-right: 0.5rem;
  }
  ```

## Additional Considerations

### 1. Data Models

Ensure that the shared view models (`AccountVM`, `ReceiptVM`, `TransactionVM`, `ReceiptItemVM`, `ReceiptWithItemsVM`, `TransactionAccountVM`, `TripVM`) are accurately defined in the `Shared` project and properly referenced within the Client application.

### 2. Real-Time Communication

- **SignalR Integration**: The `SignalRService.cs` should establish and manage the SignalR connection, handling events such as data updates and broadcasting notifications.

### 3. Form Handling and Validation

- Utilize MudBlazor's `MudForm` for creating responsive and validated forms.
- Implement validation rules within each create/edit page to ensure data integrity.

### 4. Navigation and Routing

- Ensure all pages are correctly routed and accessible via the sidebar navigation menu.
- Implement active link highlighting to help users understand their current location within the application.

### 5. State Management

- Manage shared state using scoped services to allow different components and pages to access and modify shared data seamlessly.
- Leverage two-way data binding (`@bind`) to synchronize UI elements with underlying data models.

### 6. Error Handling

- Implement global error handling using Blazor's error boundaries.
- Display user-friendly error messages and notifications using MudBlazor's `ISnackbar`.

### 7. Responsive and Accessible Design

- Ensure all components are responsive, providing a consistent experience across various device sizes.
- Adhere to accessibility standards (WCAG 2.1), implementing ARIA attributes and ensuring keyboard navigability.

### 8. Testing

- Develop unit tests for individual components and services using frameworks like xUnit and Moq.
- Create integration tests to verify interactions between components and the backend API.
- Implement end-to-end (E2E) tests using tools like Selenium or Playwright to validate complete user flows.

### 9. Continuous Integration and Deployment

- Set up a CI/CD pipeline to automate building, testing, and deploying the application.
- Include automated tests in the pipeline to ensure code quality and reliability.

## Conclusion

This hierarchical file structure serves as a structured blueprint for developing the `Receipts` Client application. It ensures a modular, organized, and maintainable codebase, facilitating efficient development and scalability. By adhering to this structure, developers can implement robust CRUD functionalities, real-time features, and an intuitive user interface, delivering a high-quality receipt management solution.
