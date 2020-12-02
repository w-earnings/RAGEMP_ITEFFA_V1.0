using GTANetworkAPI;
using System.Collections.Generic;

namespace iTeffa.Houses
{
    class GarageType
    {
        public Vector3 Position { get; }
        public List<Vector3> VehiclesPositions { get; }
        public List<Vector3> VehiclesRotations { get; }
        public int MaxCars { get; }

        public GarageType(Vector3 position, List<Vector3> vehiclesPositions, List<Vector3> vehiclesRotations, int maxCars)
        {
            Position = position;
            VehiclesPositions = vehiclesPositions;
            VehiclesRotations = vehiclesRotations;
            MaxCars = maxCars;
        }
    }
}