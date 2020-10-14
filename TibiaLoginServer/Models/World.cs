﻿namespace TibiaLoginServer.Models
{
    public class World
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "World";
        public string ExternalAddressProtected { get; set; }
        public string ExternalPortProtected { get; set; }
        public string ExternalAddressUnprotected { get; set; }
        public string ExternalPortUnprotected { get; set; }
        public int PreviewState { get; set; } = 0;
        public string Location { get; set; } = "EUR"; // todo make it enum
        public bool AntiCheatProtection { get; set; } = false;
        public int PvpType { get; set; } // enum?
        public bool IsTournamentWorld { get; set; } = false;
        public bool RestrictedStore { get; set; } = false;
    }
}