using static Google.Maps.RouteOptimization.V1.ShipmentRoute.Types;

namespace Test001_api.Models
{
    public class ServerResponseModel
    {
        public List<Route> Routes { get; set; } = new List<Route>();
        public Metricss Metrics { get; set; }
       

        public class Route
        {
            public int VehicleIndex { get; set; }
            public string VehicleLabel { get; set; }
            public Start Start { get; set; }
           public End End { get; set; }
            public List<Visit> Visits { get; set; } = new List<Visit>();
            public List<Transition> Transitions { get; set; } = new List<Transition>();
            public RouteMetrics RouteMetrics { get; set; }
            public RouteCosts RouteCosts { get; set; }
            public double RouteTotalCost { get; set; }
            public EncodedPolyline RoutePolyline { get; set; }
        }
        public class Start
        {
            public string VehicleStartTime { get; set; }
            public ArrivalLocation ArrivalLocation { get; set; }
            public string VisitLabel { get; set; }

        }

        public class End
        {
            public string VehicleEndTime { get; set; }
            public ArrivalLocation ArrivalLocation { get; set; }
            public string VisitLabel { get; set; }
        }

        public class Visit
        {
            public int ShipmentIndex { get; set; }
            public string StartTime { get; set; }
            public string Detour { get; set; }
            public string VisitLabel { get; set; }

            public ArrivalLocation ArrivalLocation { get; set; }
        }
        public class ArrivalLocation
        {
            public double Latitude { get; set; } // Szerokość geograficzna
            public double Longitude { get; set; } // Długość geograficzna
        }
        public class Transition
        {
            public string TravelDuration { get; set; }
            public double TravelDistanceMeters { get; set; }
            public string WaitDuration { get; set; }
            public string TotalDuration { get; set; }
            public string StartTime { get; set; }
            public EncodedPolyline RoutePolyline { get; set; }

        }
        public class EncodedPolyline
        {
            public string Points { get; set; }
        }
        public class RouteMetrics
        {
            public int PerformedShipmentCount { get; set; }
            public string TravelDuration { get; set; }
            public string WaitDuration { get; set; }
            public string DelayDuration { get; set; }
            public string BreakDuration { get; set; }
            public string VisitDuration { get; set; }
            public string TotalDuration { get; set; }
            public int TravelDistanceMeters { get; set; }
        }

        public class RouteCosts
        {
            public decimal ModelVehiclesCostPerKilometer { get; set; }
        }

        public class Metricss
        {
            public AggregatedRouteMetrics AggregatedRouteMetrics { get; set; }
            public int UsedVehicleCount { get; set; }
            public string EarliestVehicleStartTime { get; set; }
            public string LatestVehicleEndTime { get; set; }
            public double TotalCost { get; set; }

            public Costs Costs { get; set; }
        }

        public class AggregatedRouteMetrics
        {
            public int PerformedShipmentCount { get; set; }
            public string TravelDuration { get; set; }
            public string WaitDuration { get; set; }
            public string DelayDuration { get; set; }
            public string BreakDuration { get; set; }
            public string VisitDuration { get; set; }
            public string TotalDuration { get; set; }
            public double TravelDistanceMeters { get; set; }
        }

        public class Costs
        {
            public decimal ModelVehiclesCostPerKilometer { get; set; }
        }
    }



}


//niech server z requesta tworzy server response model, a następnie sortuje po index z odp google i niech zwraca
