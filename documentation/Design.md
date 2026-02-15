> **DEPRECATED**: This document describes the original Blazor WebAssembly client, which was removed in MGG-90. The frontend is being replaced by a React/Vite SPA (MGG-32). ViewModels referenced below have been replaced by NSwag-generated Request/Response DTOs (MGG-88).

# Receipt Management Client Application Design Document

## Table of Contents
1. [Introduction](#introduction)
2. [Architecture Overview](#architecture-overview)
3. [Technology Stack](#technology-stack)
4. [Page Structure and CRUD Operations](#page-structure-and-crud-operations)
    - [Accounts Management](#accounts-management)
    - [Receipts Management](#receipts-management)
    - [Transactions Management](#transactions-management)
    - [Trips Management](#trips-management)
5. [User Flow](#user-flow)
    - [Creating a New Receipt with Items](#creating-a-new-receipt-with-items)
    - [Creating a New Transaction and Associating with an Account](#creating-a-new-transaction-and-associating-with-an-account)
    - [Associating Receipts and Transactions into a Trip](#associating-receipts-and-transactions-into-a-trip)
6. [UI Components and Interactions](#ui-components-and-interactions)
    - [Custom Components](#custom-components)
    - [Reusable Components](#reusable-components)
    - [Forms and Validation](#forms-and-validation)
7. [Navigation](#navigation)
8. [State Management](#state-management)
9. [Real-Time Features](#real-time-features)
10. [API Integration](#api-integration)
11. [Error Handling and Notifications](#error-handling-and-notifications)
12. [Responsive Design](#responsive-design)
13. [Accessibility](#accessibility)
14. [Testing Strategy](#testing-strategy)
15. [Conclusion](#conclusion)

---

## Introduction

This document provides a comprehensive design blueprint for the `Receipts` Client application. It details the pages and components necessary to implement Create, Read, Update, and Delete (CRUD) operations for various view models, including `AccountVM`, `ReceiptVM`, `TransactionVM`, and `ReceiptItemVM`. The application is designed to offer an intuitive and efficient user experience for managing receipts, accounts, transactions, and trips. It leverages aggregate view models like `ReceiptWithItemsVM` and `TransactionAccountVM` to streamline data associations and ensure data integrity.

## Architecture Overview

The Client application follows a modular architecture, promoting separation of concerns and scalability. The key architectural components include:

- **Presentation Layer**: Built using Blazor for constructing interactive web UI with C#.
- **UI Framework**: Utilizes MudBlazor for consistent and responsive UI components.
- **Real-Time Communication**: Implements SignalR to facilitate real-time updates and notifications.
- **State Management**: Managed through scoped services and context providers to maintain application state efficiently.
- **API Integration**: Communicates with the backend API to perform CRUD operations and data synchronization.

### Component Hierarchy

- **Pages**: High-level components representing different sections of the application (e.g., Home, Manage Trips).
- **Layouts**: Define the common structure (e.g., Navigation Menu, Header) applied across multiple pages.
- **Components**: Reusable UI elements (e.g., DateOnlyPicker, Tables, Forms).
- **Services**: Handle business logic, API communication, and real-time data handling (e.g., SignalRService).
- **ViewModels**: Represent data structures used across the application for data binding and manipulation.

## Technology Stack

- **Framework**: ASP.NET Core Blazor WebAssembly
- **UI Library**: MudBlazor
- **Real-Time Communication**: SignalR
- **State Management**: Scoped Services
- **HTTP Client**: HttpClient with API Clients for data fetching
- **Dependency Injection**: Built-in DI container for service management
- **Testing Framework**: xUnit and Moq for unit testing

## Page Structure and CRUD Operations

### Accounts Management

#### Pages:
1. **Accounts List Page**
    - **Description**: Displays a comprehensive list of all financial accounts.
    - **Features**:
        - Search and filter functionality.
        - Pagination for scalability.
        - Options to view, edit, or delete each account.
        - Button to add a new account.

2. **Create/Edit Account Page**
    - **Description**: Provides a form to create a new account or edit an existing one.
    - **Features**:
        - Form fields for `AccountCode`, `Name`, and `IsActive`.
        - Form validation to ensure data integrity.
        - Submit and cancel buttons.
        - Dynamic title indicating creation or editing mode.

#### CRUD Operations:
- **Create**:
    - User navigates to the Accounts List Page and clicks on "Add Account".
    - A modal or dedicated page displays a form to input `AccountCode`, `Name`, and `IsActive` status.
    - Upon submission, the account is created via the API.
    - Real-time update reflects the new account in the list.

- **Read**:
    - Accounts List Page fetches and displays all accounts.
    - Users can click on an account to view detailed information.

- **Update**:
    - From the Accounts List Page, users select "Edit" for a specific account.
    - The Create/Edit Account Page loads pre-filled with the account's current details.
    - Users modify the necessary fields and submit to update.
    - Changes are saved via the API and reflected in real-time.

- **Delete**:
    - Users select "Delete" on a specific account.
    - A confirmation dialog appears to prevent accidental deletions.
    - Upon confirmation, the account is removed via the API.
    - The Accounts List Page updates in real-time to reflect the deletion.

### Receipts Management

#### Pages:
1. **Receipts List Page**
    - **Description**: Displays all receipts with options to view, edit, or delete.
    - **Features**:
        - Search by description, date, or location.
        - Filters for date range and location.
        - Pagination and sorting capabilities.
        - Buttons to add a new receipt.

2. **Create/Edit Receipt Page**
    - **Description**: Allows users to create a new receipt or edit an existing one, including adding receipt items.
    - **Features**:
        - Form fields for `Description`, `Location`, `Date`, and `TaxAmount`.
        - Dynamic addition and removal of `ReceiptItemVM` entries.
        - Validation to ensure all required fields are filled correctly.
        - Submit and cancel buttons.

#### CRUD Operations:
- **Create**:
    - User navigates to the Receipts List Page and clicks on "Add Receipt".
    - The Create/Edit Receipt Page appears with empty form fields.
    - User fills in `Description`, `Location`, selects `Date` using `MudDateOnlyPicker`, enters `TaxAmount`, and adds receipt items.
    - Upon submission, the receipt and its items are created via the API.
    - The Receipts List Page updates in real-time.

- **Read**:
    - Receipts List Page retrieves and displays all receipts.
    - Clicking on a receipt shows detailed information, including associated receipt items.

- **Update**:
    - Users select "Edit" on a specific receipt from the Receipts List Page.
    - The Create/Edit Receipt Page loads with existing receipt details and items.
    - Users make necessary changes and submit to update.
    - Updates are saved via the API and reflected in real-time.

- **Delete**:
    - Users select "Delete" on a specific receipt.
    - A confirmation prompt ensures deliberate action.
    - Upon confirmation, the receipt and optionally its items are deleted via the API.
    - The Receipts List Page updates in real-time.

### Transactions Management

#### Pages:
1. **Transactions List Page**
    - **Description**: Lists all transactions with options to view, edit, or delete.
    - **Features**:
        - Search by amount, date, or associated account.
        - Filters for amount range and date range.
        - Pagination and sorting.
        - Buttons to add a new transaction.

2. **Create/Edit Transaction Page**
    - **Description**: Provides a form to create or edit transactions, including selecting an associated account.
    - **Features**:
        - Form fields for `Amount`, `Date`, and account selection via dropdown.
        - Validation for numeric and date inputs.
        - Submit and cancel buttons.

#### CRUD Operations:
- **Create**:
    - User navigates to the Transactions List Page and clicks on "Add Transaction".
    - The Create/Edit Transaction Page appears with empty form fields.
    - User inputs `Amount`, selects `Date` using `MudDateOnlyPicker`, and associates the transaction with an `AccountVM`.
    - Upon submission, the transaction is created via the API.
    - The Transactions List Page updates in real-time.

- **Read**:
    - Transactions List Page retrieves and displays all transactions.
    - Clicking on a transaction displays detailed information, including the associated account.

- **Update**:
    - Users select "Edit" on a specific transaction from the Transactions List Page.
    - The Create/Edit Transaction Page loads with existing transaction details.
    - Users modify the necessary fields and submit to update.
    - Changes are saved via the API and reflected in real-time.

- **Delete**:
    - Users select "Delete" on a specific transaction.
    - A confirmation dialog appears to prevent accidental deletions.
    - Upon confirmation, the transaction is removed via the API.
    - The Transactions List Page updates in real-time.

### Trips Management

#### Pages:
1. **Trips List Page**
    - **Description**: Displays all trips with options to view, edit, or delete.
    - **Features**:
        - Search by trip name, date, or associated accounts.
        - Filters for date range.
        - Pagination and sorting capabilities.
        - Buttons to add a new trip.

2. **Create/Edit Trip Page**
    - **Description**: Allows users to create a new trip or edit an existing one by associating receipts and transactions.
    - **Features**:
        - Selection interface (e.g., multi-select lists or checkboxes) to choose existing receipts and transactions.
        - Form fields for trip-specific details if any (e.g., trip name, destination).
        - Validation to ensure at least one receipt and transaction are associated.
        - Submit and cancel buttons.

#### CRUD Operations:
- **Create**:
    - User navigates to the Trips List Page and clicks on "Add Trip".
    - The Create/Edit Trip Page appears with selection options for receipts and transactions.
    - User selects the desired receipts and transactions to associate with the trip.
    - Upon submission, the trip is created via the API.
    - The Trips List Page updates in real-time.

- **Read**:
    - Trips List Page retrieves and displays all trips.
    - Clicking on a trip shows detailed information, including associated receipts and transactions.

- **Update**:
    - Users select "Edit" on a specific trip from the Trips List Page.
    - The Create/Edit Trip Page loads with existing trip associations.
    - Users modify the selection of receipts and transactions and submit to update.
    - Changes are saved via the API and reflected in real-time.

- **Delete**:
    - Users select "Delete" on a specific trip.
    - A confirmation prompt ensures deliberate action.
    - Upon confirmation, the trip is deleted via the API.
    - The Trips List Page updates in real-time.

## User Flow

### Creating a New Receipt with Items

1. **Navigate to Receipts List Page**:
    - **Action**: User selects "Manage Receipts" from the navigation menu.
    - **Result**: Receipts List Page loads, displaying existing receipts.

2. **Initiate Creation**:
    - **Action**: User clicks on the "Add Receipt" button.
    - **Result**: Create/Edit Receipt Page opens with empty form fields.

3. **Fill Receipt Form**:
    - **Action**: User enters `Description`, `Location`, selects `Date` using `MudDateOnlyPicker`, and inputs `TaxAmount`.
    - **Result**: Form captures all necessary receipt details.

4. **Add Receipt Items**:
    - **Action**: User clicks "Add Item" to insert a new receipt item.
    - **Form Fields**: `ReceiptItemCode`, `Description`, `Quantity`, `UnitPrice`, `Category`, `Subcategory`.
    - **Validation**: Ensures all required fields are filled and values are valid.
    - **Result**: Multiple receipt items can be added dynamically.

5. **Submit**:
    - **Action**: User reviews the form and clicks "Submit".
    - **Process**: Form data is validated and sent to the API to create the receipt and its items.
    - **Result**: Receipt is created, and the user receives a success notification.

6. **Confirmation**:
    - **Action**: System displays a success message.
    - **Real-Time Update**: Receipts List Page refreshes via SignalR to include the new receipt without a manual reload.

### Creating a New Transaction and Associating with an Account

1. **Navigate to Transactions List Page**:
    - **Action**: User selects "Manage Transactions" from the navigation menu.
    - **Result**: Transactions List Page loads, displaying existing transactions.

2. **Initiate Creation**:
    - **Action**: User clicks on the "Add Transaction" button.
    - **Result**: Create/Edit Transaction Page opens with empty form fields.

3. **Fill Transaction Form**:
    - **Action**: User inputs `Amount` and selects `Date` using `MudDateOnlyPicker`.
    - **Validation**: Ensures `Amount` is a valid number and `Date` is selected.
    - **Result**: Form captures necessary transaction details.

4. **Associate Account**:
    - **Action**: User selects an existing account from a dropdown list populated with `AccountVM` entries.
    - **Result**: Transaction is associated with the selected account.

5. **Submit**:
    - **Action**: User reviews the form and clicks "Submit".
    - **Process**: Form data is validated and sent to the API to create the transaction.
    - **Result**: Transaction is created, and the user receives a success notification.

6. **Confirmation**:
    - **Action**: System displays a success message.
    - **Real-Time Update**: Transactions List Page refreshes via SignalR to include the new transaction without a manual reload.

### Associating Receipts and Transactions into a Trip

1. **Navigate to Trips List Page**:
    - **Action**: User selects "Manage Trips" from the navigation menu.
    - **Result**: Trips List Page loads, displaying existing trips.

2. **Initiate Creation**:
    - **Action**: User clicks on the "Add Trip" button.
    - **Result**: Create/Edit Trip Page opens with selection options for receipts and transactions.

3. **Select Receipts**:
    - **Action**: User selects one or more receipts from a list or multi-select component.
    - **Validation**: Ensures at least one receipt is selected.
    - **Result**: Selected receipts are marked for association with the trip.

4. **Select Transactions**:
    - **Action**: User selects one or more transactions from a list or multi-select component.
    - **Validation**: Ensures at least one transaction is selected.
    - **Result**: Selected transactions are marked for association with the trip.

5. **Submit**:
    - **Action**: User reviews the selections and clicks "Submit".
    - **Process**: Selections are validated and sent to the API to create the trip with associated receipts and transactions.
    - **Result**: Trip is created, and the user receives a success notification.

6. **Confirmation**:
    - **Action**: System displays a success message.
    - **Real-Time Update**: Trips List Page refreshes via SignalR to include the new trip without a manual reload.

## UI Components and Interactions

### Custom Components

1. **MudDateOnlyPicker**
    - **Purpose**: Customized date picker component capable of handling `DateOnly` types.
    - **Features**:
        - Simplified date selection without time components.
        - Integrates seamlessly with MudBlazor forms.
        - Reusable across various forms for consistency.

2. **SignalRService**
    - **Purpose**: Manages real-time communication between the client and server.
    - **Features**:
        - Establishes and maintains a SignalR hub connection.
        - Handles registration of message handlers for real-time updates.
        - Provides methods to send messages to the server.
        - Ensures automatic reconnection and error handling.

### Reusable Components

1. **Data Tables**
    - **Description**: Tables to display lists of accounts, receipts, transactions, and trips.
    - **Features**:
        - Sorting, filtering, and pagination.
        - Action buttons for view, edit, and delete.
        - Responsive design for various screen sizes.

2. **Forms**
    - **Description**: Forms used for creating and editing entities.
    - **Features**:
        - Input validation and error messages.
        - Consistent styling using MudBlazor components.
        - Dynamic fields for adding/removing related items (e.g., receipt items).

3. **Dialogs and Modals**
    - **Description**: Modal dialogs for confirmation prompts and detailed forms.
    - **Features**:
        - Prevents users from navigating away during critical operations.
        - Reusable across different CRUD operations.
        - Integrated with MudBlazor for consistent UI.

### Forms and Validation

- **Implementation**:
    - Utilize MudBlazor's `MudForm` for form layout and validation.
    - Define validation rules for each input field (e.g., required fields, data formats).
    - Display validation messages inline to guide user input.
    - Disable submit buttons until forms are valid to prevent erroneous submissions.

- **User Experience**:
    - Immediate feedback on input errors.
    - Clear labeling and placeholders for form fields.
    - Logical tab order for keyboard navigation.

## Navigation

### Sidebar Navigation Menu (`NavMenu.razor`)

- **Structure**:
    - **Top Row**: Contains the application logo or name.
    - **Navigation Links**: Vertical list of links to different pages.
        - **Home**: Overview and quick links.
        - **Manage Trips**: Access to the Trips List Page.
        - **Manage Receipts**: Access to the Receipts List Page.
        - **Manage Accounts**: Access to the Accounts List Page.
        - **Manage Transactions**: Access to the Transactions List Page.
        - **Generate Reports**: Access to reporting tools and pages.

- **Features**:
    - **Responsive Design**: Collapses into a hamburger menu on smaller screens.
    - **Active Link Highlighting**: Indicates the current page for better user orientation.
    - **Icons**: Utilizes Bootstrap Icons for visual cues next to each navigation link.
    - **Accessibility**: Keyboard navigable and screen reader friendly.

- **CSS Styling**:
    - Custom styles ensure a cohesive look with the overall application design.
    - Hover effects and active state styles enhance usability.

### Top Row Navigation

- **Components**:
    - **Hamburger Menu**: Toggles the visibility of the sidebar on smaller screens.
    - **About Link**: Directs users to external documentation or about page.
    - **Responsive Adjustments**: Alters layout and visibility based on screen size.

## State Management

### Scoped Services

- **Description**: Services that maintain state and handle data operations within a user's session.
- **Implementation**:
    - **ReceiptClient**: Manages API interactions for receipts.
    - **AccountClient**: Manages API interactions for accounts.
    - **TransactionClient**: Manages API interactions for transactions.
    - **TripClient**: Manages API interactions for trips.
    - **SignalRService**: Handles real-time data synchronization.

### SignalR Integration

- **Purpose**: Ensures that all connected clients receive real-time updates when data changes occur.
- **Functionality**:
    - Listens for events such as receipt creation, transaction updates, or trip deletions.
    - Triggers UI updates automatically without requiring manual refreshes.
    - Enhances collaborative features by maintaining data consistency across multiple users.

### Component State

- **Management**:
    - Each page component maintains its own local state for form inputs and displayed data.
    - Shared state is managed through scoped services to allow data sharing across components.

- **Techniques**:
    - Two-way data binding using Blazor's `@bind`.
    - State changes trigger UI re-renders to reflect the latest data.

## Real-Time Features

### Live Updates

- **Description**: Provides immediate reflection of data changes across all connected clients.
- **Examples**:
    - When a new receipt is created by one user, all other users see the receipt list update instantly.
    - Updates to transactions or accounts are propagated in real-time.

### Notifications

- **Implementation**:
    - Utilizes MudBlazor's `ISnackbar` service to display transient messages.
    - Notifies users of successful operations, errors, or important updates instantly.

### Collaborative Editing

- **Purpose**: Allows multiple users to interact with the same data concurrently without conflicts.
- **Mechanism**:
    - Real-time synchronization of data states via SignalR.
    - Conflict resolution strategies to handle simultaneous edits.

### Error Broadcasting

- **Functionality**:
    - Real-time error notifications ensure users are immediately informed of issues.
    - Enhances debugging and user experience by providing instant feedback.

## API Integration

### API Clients

- **Overview**: Dedicated client classes to interact with backend API endpoints for different entities.
- **Components**:
    - **ReceiptClient**: Handles CRUD operations for receipts.
    - **AccountClient**: Manages CRUD operations for accounts.
    - **TransactionClient**: Facilitates CRUD operations for transactions.
    - **TripClient**: Manages CRUD operations for trips and their associations.

### HTTP Communication

- **Techniques**:
    - Utilizes `HttpClient` for sending HTTP requests.
    - Implements asynchronous programming patterns to ensure non-blocking operations.
    - Handles response parsing and error management effectively.

### Error Handling

- **Strategies**:
    - Captures and processes HTTP errors gracefully.
    - Provides meaningful error messages to users through UI notifications.
    - Retries failed requests where appropriate using Polly or similar libraries.

## Error Handling and Notifications

### Global Error Handler

- **Description**: Catches unhandled exceptions and displays user-friendly error messages.
- **Implementation**:
    - Custom error boundaries in Blazor.
    - Utilizes MudBlazor's `ISnackbar` to notify users of critical errors.

### Form Validation

- **Features**:
    - Real-time validation feedback on form inputs.
    - Prevents form submission until all validation rules are satisfied.
    - Highlights erroneous fields and provides descriptive messages.

### User Notifications

- **Examples**:
    - Success messages upon successful creation, update, or deletion of entities.
    - Error messages when operations fail due to network issues or invalid data.
    - Informational messages for actions like saving drafts or warnings before deletions.

## Responsive Design

### Mobile Compatibility

- **Features**:
    - Sidebar navigation collapses into a hamburger menu on smaller screens.
    - Forms and tables adjust layout for optimal readability and usability.
    - Touch-friendly interactions for mobile users.

### Desktop Layout

- **Features**:
    - Persistent sidebar navigation for easy access to different sections.
    - Multi-column layouts for complex forms and data displays.
    - Enhanced spacing and sizing for better aesthetics on larger screens.

### Breakpoints

- **Implementation**:
    - Utilizes CSS media queries to adjust layouts based on screen width.
    - Ensures consistent user experience across devices with varying resolutions.

## Accessibility

### Compliance

- **Standards**: Adheres to WCAG 2.1 guidelines to ensure the application is accessible to all users.
- **Features**:
    - Semantic HTML elements for better screen reader compatibility.
    - Keyboard navigable interfaces.
    - Sufficient color contrast for text and UI elements.

### ARIA Attributes

- **Usage**:
    - Implements ARIA roles and labels to enhance accessibility.
    - Ensures dynamic content updates are announced to assistive technologies.

### Testing

- **Tools**:
    - Automated accessibility testing using tools like Axe or Lighthouse.
    - Manual testing with screen readers and keyboard navigation.

## Testing Strategy

### Unit Testing

- **Scope**:
    - Test individual components and services to ensure they function as expected.
    - Utilize xUnit and Moq for creating mock dependencies and verifying interactions.

### Integration Testing

- **Scope**:
    - Test interactions between different components and services.
    - Ensure seamless communication between the client and backend API.

### End-to-End (E2E) Testing

- **Scope**:
    - Simulate user interactions to validate entire user flows.
    - Utilize tools like Selenium or Playwright to automate browser-based testing.

### Continuous Integration

- **Implementation**:
    - Integrate testing into the CI pipeline to ensure code quality and reliability.
    - Run automated tests on every commit to catch issues early.

## Conclusion

This detailed design document outlines the comprehensive structure and functionalities of the `Receipts` Client application. By leveraging Blazor, MudBlazor, and SignalR, the application aims to deliver a responsive, intuitive, and real-time user experience for managing receipts, accounts, transactions, and trips. The structured approach to CRUD operations, combined with robust state management and real-time features, ensures that the application is scalable, maintainable, and user-friendly. Adherence to best practices in UI design, accessibility, and testing further enhances the application's quality and reliability. This document serves as a foundational blueprint for developers and stakeholders to implement and understand the application's architecture and user interactions thoroughly.