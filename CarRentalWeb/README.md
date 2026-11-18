# DriveRental

Вебзастосунок для перегляду автопарку та бронювання автомобілів. Реалізований на ASP.NET Core Razor Pages (.NET 10).

## Запуск

```powershell
dotnet run --project .\CarRentalWeb.csproj
```

Після запуску відкрийте адресу, яку покаже `dotnet run` (зазвичай `https://localhost:7xxx` або `http://localhost:5xxx`).

## Можливості

- Каталог автомобілів із фото та характеристиками.
- Бронювання з перевіркою мінімального віку водія, пробігу та доступності авто.
- Розрахунок вартості оренди та знижка для оренди понад 7 днів.
- Список бронювань із клієнтом, автомобілем, періодом, вартістю і статусом.
- Повернення автомобіля з оновленням статусу бронювання.

Дані бронювань зберігаються в пам’яті процесу і очищуються після перезапуску застосунку.

## Діаграма класів

```mermaid
classDiagram
    direction LR

    class VehicleClass {
        <<enumeration>>
        Economy
        Comfort
        Business
    }

    class Vehicle {
        +string Id
        +string Model
        +VehicleClass Class
        +decimal DailyRate
        +int Mileage
        +bool IsAvailable
        +bool IsRoadworthy
        +string ImageUrl
        +string Transmission
        +string Drivetrain
        +string Engine
        +int Seats
        +string Fuel
        +string Description
        +SetAvailability(bool)
        +SetRoadworthiness(bool)
    }

    class Client {
        +string FullName
        +DateTime DateOfBirth
        +int Age
    }

    class RentalAgreement {
        +Vehicle Vehicle
        +Client Client
        +DateTime StartDate
        +DateTime EndDate
        +decimal TotalCost
        +bool IsReturned
        +MarkReturned()
    }

    class RentalService {
        -List~RentalAgreement~ agreements
        +List~Vehicle~ Vehicles
        +Rent(Vehicle, Client, DateTime, DateTime) RentalAgreement
        +ReturnVehicle(Vehicle)
        +Agreements IReadOnlyCollection
    }

    class VehicleUnavailableException
    class UnderageDriverException

    Vehicle --> VehicleClass : class
    RentalAgreement --> Vehicle : rents
    RentalAgreement --> Client : rented by
    RentalService o-- Vehicle : manages
    RentalService o-- RentalAgreement : records
    RentalService ..> VehicleUnavailableException : throws
    RentalService ..> UnderageDriverException : throws
```
