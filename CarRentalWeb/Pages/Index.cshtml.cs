using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CarRentalWeb.Models;

namespace CarRentalWeb.Pages;

public class IndexModel : PageModel
{
    private readonly RentalService rentalService;

    [BindProperty] public string FullName { get; set; } = "";
    [BindProperty] public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-25);
    [BindProperty] public string VehicleId { get; set; } = "";
    [BindProperty] public DateTime StartDate { get; set; } = DateTime.Today;
    [BindProperty] public DateTime EndDate { get; set; } = DateTime.Today.AddDays(3);

    public IReadOnlyCollection<Vehicle> Vehicles => rentalService.Vehicles;
    public IReadOnlyCollection<RentalAgreement> Agreements =>
        rentalService.Agreements.OrderByDescending(agreement => agreement.StartDate).ToArray();
    public RentalAgreement? Agreement { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public IndexModel(RentalService rentalService)
    {
        this.rentalService = rentalService;
    }

    public void OnGet() { }

    public IActionResult OnPostRent()
    {
        try
        {
            var vehicle = rentalService.Vehicles.FirstOrDefault(v => v.Id == VehicleId)
                ?? throw new ArgumentException("Оберіть автомобіль.");

            Agreement = rentalService.Rent(
                vehicle,
                new Client(FullName, DateOfBirth),
                StartDate,
                EndDate);

            SuccessMessage = "Автомобіль успішно заброньовано!";
        }
        catch (Exception ex) when (ex is UnderageDriverException or VehicleUnavailableException or ArgumentException)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }

    public IActionResult OnPostReturn(string vehicleId)
    {
        var vehicle = rentalService.Vehicles.FirstOrDefault(v => v.Id == vehicleId);
        if (vehicle is not null)
        {
            rentalService.ReturnVehicle(vehicle);
            SuccessMessage = $"{vehicle.Model} повернено в парк.";
        }
        return Page();
    }
}
