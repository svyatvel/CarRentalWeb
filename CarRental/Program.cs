using System;

namespace CarRental;

public enum VehicleClass
{
    Economy,
    Comfort,
    Business
}

public sealed class MaintenanceRecord
{
    public DateTime Date { get; }
    public int Mileage { get; }
    public bool IsPassed { get; }
    public string Description { get; }

    public MaintenanceRecord(DateTime date, int mileage, bool isPassed, string description)
    {
        Date = date;
        Mileage = mileage;
        IsPassed = isPassed;
        Description = description;
    }
}

public sealed class Vehicle
{
    public string LicensePlate { get; }
    public string Model { get; }
    public VehicleClass Class { get; }
    public decimal DailyRate { get; }
    public int Mileage { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public bool IsRoadworthy { get; private set; } = true;
    public List<MaintenanceRecord> MaintenanceRecords { get; } = new();

    public Vehicle(string licensePlate, string model, VehicleClass vehicleClass, decimal dailyRate, int mileage)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            throw new ArgumentException("Номерний знак є обов'язковим.", nameof(licensePlate));
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Модель є обов'язковою.", nameof(model));
        if (dailyRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(dailyRate), "Тариф має бути більше нуля.");
        if (mileage < 0)
            throw new ArgumentOutOfRangeException(nameof(mileage), "Пробіг не може бути від'ємним.");

        LicensePlate = licensePlate;
        Model = model;
        Class = vehicleClass;
        DailyRate = dailyRate;
        Mileage = mileage;
    }

    public void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;

    public void SetRoadworthiness(bool isRoadworthy) => IsRoadworthy = isRoadworthy;

    public void AddMileage(int kilometers)
    {
        if (kilometers < 0)
            throw new ArgumentOutOfRangeException(nameof(kilometers), "Кількість кілометрів не може бути від'ємною.");
        Mileage += kilometers;
    }

    public void AddMaintenanceRecord(MaintenanceRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        MaintenanceRecords.Add(record);
        IsRoadworthy = record.IsPassed;
    }
}

public sealed class Client
{
    public string FullName { get; }
    public DateTime DateOfBirth { get; }
    public int Age => CalculateAge(DateOfBirth, DateTime.Today);

    public Client(string fullName, DateTime dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("ПІБ є обов'язковим.", nameof(fullName));
        if (dateOfBirth > DateTime.Today)
            throw new ArgumentOutOfRangeException(nameof(dateOfBirth), "Дата народження не може бути в майбутньому.");

        FullName = fullName;
        DateOfBirth = dateOfBirth;
    }

    private static int CalculateAge(DateTime birthDate, DateTime onDate)
    {
        int age = onDate.Year - birthDate.Year;
        if (birthDate.AddYears(age) > onDate)
            age--;
        return age;
    }
}

public sealed class RentalAgreement
{
    public Vehicle Vehicle { get; }
    public Client Client { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public decimal TotalCost { get; }

    public RentalAgreement(Vehicle vehicle, Client client, DateTime startDate, DateTime endDate, decimal totalCost)
    {
        Vehicle = vehicle;
        Client = client;
        StartDate = startDate;
        EndDate = endDate;
        TotalCost = totalCost;
    }
}

public sealed class VehicleUnavailableException : Exception
{
    public VehicleUnavailableException(string message) : base(message) { }
}

public sealed class UnderageDriverException : Exception
{
    public UnderageDriverException(string message) : base(message) { }
}

public sealed class RentalService
{
    private readonly List<RentalAgreement> agreements = new();
    private const int MinimumDriverAge = 21;

    private static readonly IReadOnlyDictionary<VehicleClass, int> MaximumMileageByClass =
        new Dictionary<VehicleClass, int>
        {
            [VehicleClass.Economy] = 150_000,
            [VehicleClass.Comfort] = 120_000,
            [VehicleClass.Business] = 100_000
        };

    public RentalAgreement Rent(Vehicle vehicle, Client client, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentNullException.ThrowIfNull(client);

        if (client.Age < MinimumDriverAge)
            throw new UnderageDriverException(
                $"Клієнт {client.FullName} має лише {client.Age} років. Мінімальний вік водія — {MinimumDriverAge}.");

        if (startDate.Date > endDate.Date)
            throw new ArgumentException("Дата початку оренди не може бути пізнішою за дату завершення.");

        if (!vehicle.IsAvailable)
            throw new VehicleUnavailableException($"Автомобіль {vehicle.Model} ({vehicle.LicensePlate}) вже заброньовано.");

        if (!vehicle.IsRoadworthy)
            throw new VehicleUnavailableException($"Автомобіль {vehicle.Model} не пройшов технічний огляд.");

        if (vehicle.Mileage > MaximumMileageByClass[vehicle.Class])
            throw new VehicleUnavailableException(
                $"Автомобіль класу {vehicle.Class} перевищив допустимий пробіг " +
                $"{MaximumMileageByClass[vehicle.Class]:N0} км.");

        int rentalDays = (endDate.Date - startDate.Date).Days + 1;
        decimal totalCost = vehicle.DailyRate * rentalDays;

        // Знижка за довгострокову оренду: понад 7 діб — 10%.
        if (rentalDays > 7)
            totalCost *= 0.90m;

        var agreement = new RentalAgreement(vehicle, client, startDate, endDate, totalCost);
        agreements.Add(agreement);
        vehicle.SetAvailability(false);
        return agreement;
    }

    public void ReturnVehicle(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        vehicle.SetAvailability(true);
    }

    public IReadOnlyCollection<RentalAgreement> GetAgreementList() => agreements.AsReadOnly();
}

internal static class ProgramEntry
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var economyCar = new Vehicle("AA1234BC", "Toyota Corolla", VehicleClass.Economy, 1200m, 45_000);
        economyCar.AddMaintenanceRecord(
            new MaintenanceRecord(DateTime.Today.AddMonths(-1), 44_500, true, "Чергове технічне обслуговування пройдено"));

        var comfortCar = new Vehicle("KA7777XX", "Skoda Octavia", VehicleClass.Comfort, 1800m, 130_000);

        var rentalService = new RentalService();
        var adultClient = new Client("Іваненко Петро Сергійович", new DateTime(1998, 5, 12));

        try
        {
            var agreement = rentalService.Rent(
                economyCar,
                adultClient,
                DateTime.Today,
                DateTime.Today.AddDays(9));

            Console.WriteLine($"Договір: {adultClient.FullName}");
            Console.WriteLine($"Автомобіль: {agreement.Vehicle.Model}, {agreement.Vehicle.LicensePlate}");
            Console.WriteLine($"Період: {agreement.StartDate:d} - {agreement.EndDate:d}");
            Console.WriteLine($"Вартість: {agreement.TotalCost:N2} грн");

            rentalService.ReturnVehicle(economyCar);

            var youngClient = new Client("Мельник Олег", new DateTime(2010, 1, 1));
            rentalService.Rent(economyCar, youngClient, DateTime.Today, DateTime.Today.AddDays(2));
        }
        catch (UnderageDriverException ex)
        {
            Console.WriteLine($"Помилка віку: {ex.Message}");
        }
        catch (VehicleUnavailableException ex)
        {
            Console.WriteLine($"Автомобіль недоступний: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Некоректні дані: {ex.Message}");
        }
    }
}
