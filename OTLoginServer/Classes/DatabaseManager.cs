﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using OTLoginServer.Models;
using static OTLoginServer.Models.Character;

namespace OTLoginServer.Classes
{
    public class DatabaseManager
    {
        private readonly MySqlConnection _connection;

        private const string connectionString = "host = {0}; userid = {1}; password = {2}; database = {3}; port = {4}";

        public DatabaseManager()
        {
            _connection = new MySqlConnection();
        }

        public async Task<bool> Setup()
        {
            _connection.ConnectionString = String.Format(connectionString, ConfigLoader.GetString("mysqlHost"), ConfigLoader.GetString("mysqlUser"), 
                ConfigLoader.GetString("mysqlPass"), ConfigLoader.GetString("mysqlDatabase"), ConfigLoader.GetInteger("mysqlPort"));

            try
            {
                await _connection.OpenAsync();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            Console.WriteLine($"Connected to {_connection.Database} at {ConfigLoader.GetString("mysqlHost")}:{ConfigLoader.GetInteger("mysqlPort")} via {ConfigLoader.GetString("mysqlUser")}");
            return true;
        }

        public string Hash(string stringToHash)
        {
            using (var sha1 = new SHA1Managed())
            {
                return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(stringToHash))).Replace("-", "").ToLower();
            }
        }

        public async Task<Account> GetAccount(string name, string password)
        {
            var cmd = new MySqlCommand($"SELECT `id`, `premdays`, `lastday` FROM `accounts` WHERE `name` = '{name}' AND `password` = '{Hash(password)}'", _connection);
            using (var dataReader = await cmd.ExecuteReaderAsync())
            {
                if (dataReader.Read())
                {
                    Account account = new Account();

                    if (!dataReader.HasRows)
                    {
                        return null;
                    }

                    account.Id = dataReader.GetInt32(dataReader.GetOrdinal("id"));

                    int premDays = dataReader.GetInt32(dataReader.GetOrdinal("premdays"));
                    account.IsPremium = premDays > 0;
                    account.LastLoginTime = dataReader.GetInt64(dataReader.GetOrdinal("lastday"));
                    account.PremiumUntil = DateTime.UtcNow.ToTimestamp().Seconds + premDays * 86400;

                    return account;
                }
            }
            return null;
        }

        public ICollection<World> GetWorlds()
        {
            // this is just a dummy method, cause tfs does not support more than one world
            List<World> worlds = new List<World>();
            World world = new World()
            {
                Name = ConfigLoader.GetString("serverName"),
                ExternalAddressProtected = ConfigLoader.GetString("ip"),
                ExternalPortUnprotected = ConfigLoader.GetInteger("loginProtocolPort").ToString(),
                ExternalPortProtected = ConfigLoader.GetInteger("loginProtocolPort").ToString(),
                ExternalAddressUnprotected = ConfigLoader.GetString("ip"),
            };

            string pvpType = ConfigLoader.GetString("worldType");
            if (pvpType == "pvp")
            {
                world.PvpType = 0;
            }
            else if (pvpType == "no-pvp")
            {
                world.PvpType = 1;
            }
            else if (pvpType == "pvp-enforced")
            {
                world.PvpType = 2;
            }

            worlds.Add(world);
            return worlds;
        }

        public async Task<ICollection<Character>> GetAccountCharacters(int id)
        {
            var cmd = new MySqlCommand($"SELECT `name`, `level`, `vocation`, `sex`, `looktype`, `lookhead`, `lookbody`, `looklegs`, `lookfeet`, `lookaddons`  FROM `players` WHERE `account_id` = {id}", _connection);
            using (var dataReader = await cmd.ExecuteReaderAsync())
            {
                List<Character> characters = new List<Character>();
                while (await dataReader.ReadAsync())
                {
                    characters.Add(new Character()
                    {
                        Name = dataReader.GetString(dataReader.GetOrdinal("name")),
                        Level = dataReader.GetInt32(dataReader.GetOrdinal("level")),
                        Vocation = (VocationEnum)System.Enum.Parse(typeof(VocationEnum), dataReader.GetInt32(dataReader.GetOrdinal("vocation")).ToString()),
                        IsMale = dataReader.GetInt32(dataReader.GetOrdinal("sex")) == 0,
                        OutfitId = dataReader.GetInt32(dataReader.GetOrdinal("looktype")),
                        HeadColor = dataReader.GetInt32(dataReader.GetOrdinal("lookhead")),
                        TorsoColor = dataReader.GetInt32(dataReader.GetOrdinal("lookbody")),
                        LegsColor = dataReader.GetInt32(dataReader.GetOrdinal("looklegs")),
                        DetailColor = dataReader.GetInt32(dataReader.GetOrdinal("lookfeet")),
                        AddonsFlags = dataReader.GetInt32(dataReader.GetOrdinal("lookaddons")),
                    });
                }

                return characters;
            }
        }

        public EventsScheduleResponse GetScheduledEvents()
        {
            // you can use this method to parse events from your database

            EventsScheduleResponse eventsResponse = new EventsScheduleResponse();
            eventsResponse.EventList = new List<Event>();
            eventsResponse.EventList.Add(new Event()
            {
                StartDate = DateTime.UtcNow.ToTimestamp().Seconds,
                EndDate = DateTime.UtcNow.ToTimestamp().Seconds + 5 * 86400,
                ColorLight = "#7a1b34",
                ColorDark = "#64162b",
                Name = "Sample Event",
                Description = "Nekiro was here"
            });

            return eventsResponse;
        }

        public BoostedCreatureResponse GetBoostedCreature()
        {
            // you can use this method to parse boosted creature from your database
            return new BoostedCreatureResponse() { RaceId = 1496 };
        }

        public CacheInfoResponse GetCacheInfo()
        {
            // you can use this method to parse info stuff from your database
            return new CacheInfoResponse()
            {
                PlayersOnline = 123,
                TwitchStreams = 456,
                TwitchViewer = 789,
                GamingYoutubeStreams = 1000,
                GamingYoutubeViewer = 24444,
            };
        }
    }
}
