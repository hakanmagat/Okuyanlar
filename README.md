# Okuyanlar - Library Management System


## Project Overview
Okuyanlar is a robust Library Management System engineered with Clean Architecture principles and Test-Driven Development (TDD). By enforcing a strict separation of concerns, the project delivers a highly modular, scalable solution that supports long-term maintainability.

## Architecture
The solution implements **Onion (Clean) Architecture**, ensuring that the domain logic remains independent of external frameworks and databases. Dependencies flow inwards, with the Core layer at the centre.

### Solution Structure
The solution is divided into the following specialised projects:

* **`Okuyanlar.Core` (Domain Layer)**
    * **Role:** The heart of the application.
    * **Contents:** Domain entities, interfaces (contracts), and DTOs.
    * **Dependencies:** None. Completely independent.

* **`Okuyanlar.Data` (Infrastructure Layer)**
    * **Role:** Handles data persistence and external concerns.
    * **Contents:** Entity Framework Core DB Context, repository implementations, and database configurations (SQLite).
    * **Dependencies:** `Okuyanlar.Core`.

* **`Okuyanlar.Service` (Business Layer)**
    * **Role:** Encapsulates business logic and application rules.
    * **Contents:** Service implementations, validations, and domain logic processing.
    * **Dependencies:** `Okuyanlar.Core` and `Okuyanlar.Data` (via interfaces).

* **`Okuyanlar.Web` (Presentation Layer)**
    * **Role:** The user interface and entry point.
    * **Contents:** ASP.NET Core MVC Controllers, Views, and ViewModels.
    * **Dependencies:** `Okuyanlar.Service` and `Okuyanlar.Core`.

* **`Okuyanlar.Tests` (Testing Layer)**
    * **Role:** Ensures code reliability and correctness.
    * **Contents:** xUnit tests covering unit and integration scenarios following the TDD cycle.

## Technologies & Methodologies
* **Framework:** .NET 10 (ASP.NET Core)
* **Language:** C#
* **Database:** SQLite (Entity Framework Core)
* **Testing:** xUnit
* **Design Pattern:** Clean Architecture, Repository Pattern
* **Development Approach:** Test-First (TDD)
* 
