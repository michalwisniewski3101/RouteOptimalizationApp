using Google.Api.Gax.Grpc;
using Google.Maps.RouteOptimization.V1;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Test001_api.Models;

namespace CashFlow.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    public class GoogleController : Controller
    {
        // POST api/google/optimize
        [HttpPost("optimize")]
        public async Task<IActionResult> OptimizeTours([FromBody] ClientRequestModel requestModel)
        {
            // Inicjalizacja klienta do Route Optimization API
            var client = RouteOptimizationClient.Create();
            var responseModel = new ServerResponseModel();

            // Tworzenie obiektu OptimizeToursRequest na podstawie otrzymanego modelu
            var request = new OptimizeToursRequest
            {
                Parent = requestModel.Parent,
                PopulatePolylines=true,
                PopulateTransitionPolylines=false,
                ConsiderRoadTraffic=true,
                Model = new ShipmentModel()
            };
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddHours(24);

            request.Model.GlobalStartTime = Timestamp.FromDateTime(startTime);
            request.Model.GlobalEndTime = Timestamp.FromDateTime(endTime);

            // Upewnij się, że nanos nie jest ustawiony lub jest ustawiony na 0
            request.Model.GlobalStartTime.Nanos = 0;
            request.Model.GlobalEndTime.Nanos = 0;
            // Przekazywanie dostaw (shipments)
            foreach (var shipmentModel in requestModel.Model.Shipments)
            {
                var shipment = new Shipment();

                foreach (var deliveryModel in shipmentModel.Deliveries)
                {
                    shipment.Deliveries.Add(new Shipment.Types.VisitRequest
                    {
                        ArrivalLocation = new Google.Type.LatLng
                        {
                            Latitude = deliveryModel.ArrivalLocation.Latitude,
                            Longitude = deliveryModel.ArrivalLocation.Longitude
                        },
                        Label=deliveryModel.Label
                    });
                }

                request.Model.Shipments.Add(shipment);
            }

            // Przekazywanie pojazdów (vehicles)
            foreach (var vehicleModel in requestModel.Model.Vehicles)
            {
                var vehicle = new Vehicle();
                
                if (vehicleModel.StartLocation != null) {
                    vehicle.StartLocation = new Google.Type.LatLng
                    {
                        Latitude = vehicleModel.StartLocation.Latitude,
                        Longitude = vehicleModel.StartLocation.Longitude
                    };
                }
                if (vehicleModel.EndLocation != null)
                {
                    vehicle.EndLocation = new Google.Type.LatLng
                    {
                        Latitude = vehicleModel.EndLocation.Latitude,
                        Longitude = vehicleModel.EndLocation.Longitude
                    };
                 }
                vehicle.CostPerKilometer = vehicleModel.CostPerKilometer;
                vehicle.Label = vehicleModel.Label;
                

                request.Model.Vehicles.Add(vehicle);
            }

            try
            {
                // Wysłanie zapytania OptimizeTours do API
                var response = client.OptimizeTours(request);

                // Mapowanie odpowiedzi z Google Route Optimization API do ServerResponseModel
                foreach (var route in response.Routes)
                {

                    var vehicle = requestModel.Model.Vehicles.FirstOrDefault(v => v.VehicleIndex == route.VehicleIndex);
                    var routeModel = new ServerResponseModel.Route();



                    routeModel.Start = new ServerResponseModel.Start
                    {

                        VehicleStartTime = route.VehicleStartTime?.ToDateTime().ToString("o"),
                        ArrivalLocation = new ServerResponseModel.ArrivalLocation
                        {
                            Latitude = vehicle.StartLocation.Latitude,
                            Longitude = vehicle.StartLocation.Longitude,
                        },
                        VisitLabel = vehicle.StartLabel

                    };
                    if (vehicle.EndLocation != null)
                    {
                        routeModel.End = new ServerResponseModel.End
                        {
                            VehicleEndTime = route.VehicleEndTime?.ToDateTime().ToString("o"),
                            ArrivalLocation = new ServerResponseModel.ArrivalLocation
                            {
                                Latitude = vehicle.EndLocation.Latitude,
                                Longitude = vehicle.EndLocation.Longitude,
                            },
                            VisitLabel = vehicle.EndLabel
                        };
                    }
                    routeModel.RouteTotalCost = route.RouteTotalCost;
                    routeModel.VehicleLabel = route.VehicleLabel;
                        routeModel.RoutePolyline = new ServerResponseModel.EncodedPolyline
                        {
                            Points = route.RoutePolyline?.Points // Ensure RoutePolyline is included if present
                        };
                    

                    // Mapowanie wizyt i lokalizacji dostawy
                    foreach (var visit in route.Visits)
                    {
                        var visitModel = new ServerResponseModel.Visit
                        {
                            ShipmentIndex = visit.ShipmentIndex,
                            StartTime = visit.StartTime.ToDateTime().ToString("o"),
                            Detour = visit.Detour.ToString(),
                            VisitLabel = visit.VisitLabel
                        };

                        // Znajdowanie odpowiedniej lokalizacji dostawy na podstawie ShipmentIndex
                        var shipment = requestModel.Model.Shipments.FirstOrDefault(s => s.ShipmentIndex == visit.ShipmentIndex);
                        if (shipment != null)
                        {
                            var delivery = shipment.Deliveries.FirstOrDefault();
                            if (delivery != null)
                            {
                                visitModel.ArrivalLocation = new ServerResponseModel.ArrivalLocation
                                {
                                    Latitude = delivery.ArrivalLocation.Latitude,
                                    Longitude = delivery.ArrivalLocation.Longitude
                                };
                            }
                        }

                        routeModel.Visits.Add(visitModel);
                    }

                    // Dodawanie przejść
                    foreach (var transition in route.Transitions)
                    {
                        routeModel.Transitions.Add(new ServerResponseModel.Transition
                        {
                            TravelDuration = transition.TravelDuration.ToString(),
                            TravelDistanceMeters = transition.TravelDistanceMeters,
                            WaitDuration = transition.WaitDuration.ToString(),
                            TotalDuration = transition.TotalDuration.ToString(),
                            StartTime = transition.StartTime.ToDateTime().ToString("o"),
                            RoutePolyline = new ServerResponseModel.EncodedPolyline
                            {
                                Points = transition.RoutePolyline?.Points // Ensure RoutePolyline is included if present
                            }
                        });
                    }

                    responseModel.Routes.Add(routeModel);
                }

                responseModel.Metrics = new ServerResponseModel.Metricss
                {
                    AggregatedRouteMetrics = new ServerResponseModel.AggregatedRouteMetrics
                    {
                        PerformedShipmentCount = response.Metrics.AggregatedRouteMetrics.PerformedShipmentCount,
                        TravelDuration = response.Metrics.AggregatedRouteMetrics.TravelDuration.ToString(),
                        WaitDuration = response.Metrics.AggregatedRouteMetrics.WaitDuration.ToString(),
                        DelayDuration = response.Metrics.AggregatedRouteMetrics.DelayDuration.ToString(),
                        BreakDuration = response.Metrics.AggregatedRouteMetrics.BreakDuration.ToString(),
                        VisitDuration = response.Metrics.AggregatedRouteMetrics.VisitDuration.ToString(),
                        TotalDuration = response.Metrics.AggregatedRouteMetrics.TotalDuration.ToString(),
                        TravelDistanceMeters = response.Metrics.AggregatedRouteMetrics.TravelDistanceMeters
                    },
                    UsedVehicleCount = response.Metrics.UsedVehicleCount,
                    EarliestVehicleStartTime = response.Metrics.EarliestVehicleStartTime.ToDateTime().ToString("o"),
                    LatestVehicleEndTime = response.Metrics.LatestVehicleEndTime.ToDateTime().ToString("o"),
                    TotalCost = response.Metrics.TotalCost,
                   /* Costs = new ServerResponseModel.Costs
                    {
                        ModelVehiclesCostPerKilometer = response.Metrics.Costs.model
                    }*/
                };

                return Ok(responseModel);
            }
            catch (Grpc.Core.RpcException ex)
            {
                // Obsługa błędów z gRPC
                return BadRequest($"gRPC error: {ex.StatusCode} - {ex.Message}");
            }
        }

    }
}
