namespace CarRentalWeb.Models;

public enum VehicleClass
{
    Economy,
    Comfort,
    Business
}

public sealed class Vehicle
{
    public string Id { get; }
    public string Model { get; }
    public VehicleClass Class { get; }
    public decimal DailyRate { get; }
    public int Mileage { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public bool IsRoadworthy { get; private set; } = true;
    public string ImageUrl { get; }
    public string Transmission { get; }
    public string Drivetrain { get; }
    public string Engine { get; }
    public int Seats { get; }
    public string Fuel { get; }
    public string Description { get; }

    public Vehicle(
        string id,
        string model,
        VehicleClass vehicleClass,
        decimal dailyRate,
        int mileage,
        string imageUrl,
        string transmission,
        string drivetrain,
        string engine,
        int seats,
        string fuel,
        string description)
    {
        Id = id;
        Model = model;
        Class = vehicleClass;
        DailyRate = dailyRate;
        Mileage = mileage;
        ImageUrl = imageUrl;
        Transmission = transmission;
        Drivetrain = drivetrain;
        Engine = engine;
        Seats = seats;
        Fuel = fuel;
        Description = description;
    }

    public void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;

    public void SetRoadworthiness(bool isRoadworthy) => IsRoadworthy = isRoadworthy;
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
            throw new ArgumentOutOfRangeException(nameof(dateOfBirth));

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
    public bool IsReturned { get; private set; }

    public RentalAgreement(Vehicle vehicle, Client client, DateTime startDate, DateTime endDate, decimal totalCost)
    {
        Vehicle = vehicle;
        Client = client;
        StartDate = startDate;
        EndDate = endDate;
        TotalCost = totalCost;
    }

    public void MarkReturned() => IsReturned = true;
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
    private const int MinimumDriverAge = 21;
    private readonly List<RentalAgreement> agreements = new();

    private static readonly IReadOnlyDictionary<VehicleClass, int> MaximumMileageByClass =
        new Dictionary<VehicleClass, int>
        {
            [VehicleClass.Economy] = 150_000,
            [VehicleClass.Comfort] = 120_000,
            [VehicleClass.Business] = 100_000
        };

    public List<Vehicle> Vehicles { get; } =
    [
        new("toyota-corolla", "Toyota Corolla", VehicleClass.Economy, 1200m, 45_000,
            "/images/toyota-corolla.jpg",
            "Автомат", "Передній привід", "1.8 л, 140 к.с.", 5, "Бензин",
            "Надійний міський автомобіль із помірною витратою та комфортним багажником."),
        new("skoda-octavia", "Skoda Octavia", VehicleClass.Comfort, 1800m, 130_000,
            "/images/skoda-octavia.jpg",
            "DSG", "Передній привід", "2.0 л TDI, 150 к.с.", 5, "Дизель",
            "Практичний універсал для дальніх поїздок і бізнес-перевезень."),
        new("bmw-5", "BMW 5 Series", VehicleClass.Business, 3200m, 82_000,
            "/images/bmw-5.jpg",
            "Автомат 8 передач", "Задній привід", "2.0 л TwinPower, 249 к.с.", 5, "Бензин",
            "Комфортний бізнес-седан із високим рівнем оснащення."),
        new("hyundai-elantra", "Hyundai Elantra", VehicleClass.Economy, 1100m, 38_000,
            "/images/hyundai-elantra.jpg",
            "Автомат", "Передній привід", "1.6 л, 128 к.с.", 5, "Бензин",
            "Економний седан для щоденних поїздок містом."),
        new("volkswagen-passat", "Volkswagen Passat", VehicleClass.Comfort, 1900m, 91_000,
            "/images/volkswagen-passat.jpg",
            "Автомат 7 передач", "Передній привід", "2.0 л TSI, 190 к.с.", 5, "Бензин",
            "Німецький седан із простором для пасажирів і багажу."),
        new("mercedes-e", "Mercedes-Benz E-Class", VehicleClass.Business, 3500m, 68_000,
            "/images/mercedes-e.jpg",
            "Автомат 9 передач", "Задній привід", "2.0 л Mild Hybrid, 197 к.с.", 5, "Бензин",
            "Представницький седан із преміальним інтер'єром."),
        new("toyota-rav4", "Toyota RAV4", VehicleClass.Comfort, 2200m, 74_000,
            "/images/toyota-rav4.jpg",
            "Варіатор", "Повний привід AWD", "2.5 л Hybrid, 219 к.с.", 5, "Гібрид",
            "Повнопривідний кросовер для міста та подорожей."),
        new("bmw-x5", "BMW X5", VehicleClass.Business, 4200m, 55_000,
            "/images/bmw-x5.jpg",
            "Автомат 8 передач", "Повний привід xDrive", "3.0 л, 340 к.с.", 5, "Бензин",
            "Потужний преміальний позашляховик із повним приводом."),
        new("tesla-model-3", "Tesla Model 3", VehicleClass.Comfort, 2400m, 29_000,
            "/images/tesla-model-3.jpg",
            "Одноступенева", "Задній привід", "Електромотор, 283 к.с.", 5, "Електро",
            "Електроседан із автоматизованою системою та великим запасом ходу.")
    ];

    public RentalAgreement Rent(Vehicle vehicle, Client client, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentNullException.ThrowIfNull(client);

        if (client.Age < MinimumDriverAge)
            throw new UnderageDriverException(
                $"Вік водія — {client.Age} років. Мінімальний вік — {MinimumDriverAge}.");

        if (startDate.Date > endDate.Date)
            throw new ArgumentException("Дата початку не може бути пізнішою за дату завершення.");

        if (!vehicle.IsAvailable)
            throw new VehicleUnavailableException($"{vehicle.Model} вже заброньовано.");

        if (!vehicle.IsRoadworthy)
            throw new VehicleUnavailableException($"{vehicle.Model} не пройшов технічний огляд.");

        if (vehicle.Mileage > MaximumMileageByClass[vehicle.Class])
            throw new VehicleUnavailableException(
                $"{vehicle.Model} перевищив допустимий пробіг для класу {vehicle.Class}.");

        int rentalDays = (endDate.Date - startDate.Date).Days + 1;
        decimal totalCost = vehicle.DailyRate * rentalDays;
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

        var activeAgreement = agreements.LastOrDefault(
            agreement => agreement.Vehicle.Id == vehicle.Id && !agreement.IsReturned);
        activeAgreement?.MarkReturned();
    }

    public IReadOnlyCollection<RentalAgreement> Agreements => agreements.AsReadOnly();
}
