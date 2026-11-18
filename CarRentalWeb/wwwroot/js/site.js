const modal = document.getElementById("vehicleModal");

function openVehicleModal(card) {
    if (!modal) return;

    document.getElementById("modalImage").src = card.dataset.image;
    document.getElementById("modalImage").alt = card.dataset.model;
    document.getElementById("modalClass").textContent = card.dataset.class;
    document.getElementById("modalTitle").textContent = card.dataset.model;
    document.getElementById("modalDescription").textContent = card.dataset.description;
    document.getElementById("modalDrivetrain").textContent = card.dataset.drivetrain;
    document.getElementById("modalTransmission").textContent = card.dataset.transmission;
    document.getElementById("modalEngine").textContent = card.dataset.engine;
    document.getElementById("modalFuel").textContent = card.dataset.fuel;
    document.getElementById("modalSeats").textContent = card.dataset.seats;
    document.getElementById("modalMileage").textContent = card.dataset.mileage + " км";
    document.getElementById("modalPrice").textContent = card.dataset.price;

    const select = document.querySelector('[name="VehicleId"]');
    document.getElementById("modalBookButton").onclick = () => {
        if (select) {
            select.value = card.dataset.id || "";
        }

        const booking = document.getElementById("booking");
        closeVehicleModal();
        booking?.scrollIntoView({ behavior: "smooth", block: "start" });
    };

    modal.classList.add("open");
    modal.setAttribute("aria-hidden", "false");
    document.body.classList.add("modal-open");
}

function closeVehicleModal() {
    if (!modal) return;
    modal.classList.remove("open");
    modal.setAttribute("aria-hidden", "true");
    document.body.classList.remove("modal-open");
}

document.querySelectorAll(".vehicle-card").forEach(card => {
    card.addEventListener("click", event => {
        if (event.target.closest("button, form")) return;
        openVehicleModal(card);
    });
    card.addEventListener("keydown", event => {
        if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            openVehicleModal(card);
        }
    });
});

document.querySelectorAll("[data-close-modal]").forEach(element => {
    element.addEventListener("click", closeVehicleModal);
});

document.addEventListener("keydown", event => {
    if (event.key === "Escape") closeVehicleModal();
});
