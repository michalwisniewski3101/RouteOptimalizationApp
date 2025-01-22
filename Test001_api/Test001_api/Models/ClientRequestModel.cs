namespace Test001_api.Models
{
    public class ClientRequestModel
    {
        public string Parent { get; set; } // Pole 'Parent' w głównym modelu
        public Model Model { get; set; } // Pole 'Model' zawierające 'Shipments' i 'Vehicles'

    }

    public class Model
    {
        public List<ShipmentModell> Shipments { get; set; } = new List<ShipmentModell>(); // Lista 'Shipments'
        public List<VehicleModel> Vehicles { get; set; } = new List<VehicleModel>(); // Lista 'Vehicles'
        public string GlobalEndTime { get; set; }
        public string GlobalStartTime { get; set; }
    }

    public class ShipmentModell
    {
        public List<Delivery> Deliveries { get; set; } = new List<Delivery>(); // Lista 'Deliveries' w ramach dostawy
        public int ShipmentIndex { get; set; }
    }

    public class Delivery
    {
        public ArrivalLocation ArrivalLocation { get; set; } // Pole 'ArrivalLocation' zawierające współrzędne geograficzne
        public string Label { get; set; }

    }

    public class ArrivalLocation
    {
        public double Latitude { get; set; } // Szerokość geograficzna
        public double Longitude { get; set; } // Długość geograficzna
    }

    public class VehicleModel
    {
        public Location? StartLocation { get; set; } // Lokalizacja początkowa pojazdu
        public Location? EndLocation { get; set; } // Lokalizacja końcowa pojazdu
        public double CostPerKilometer { get; set; } // Koszt na kilometr
        public int VehicleIndex { get; set; }
        public string Label { get; set; }
        public string StartLabel { get; set; }
        public string EndLabel { get; set; }
        
    }

    public class Location
    {
        public double Latitude { get; set; } // Szerokość geograficzna
        public double Longitude { get; set; } // Długość geograficzna
    }
}
