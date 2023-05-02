using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using tuplaus_teht.Common.DTO;
using tuplaus_teht.Service.DTO;
using Npgsql;
using System.Globalization;

namespace tuplaus_teht.Service
{
    public class DBHandler
    {
        private static DBHandler instance;
        private NpgsqlDataSource dataSource;

        public static DBHandler Instance
        {
            get
            {
                if (instance == null)
                    instance = new DBHandler();

                return instance;
            }
        }
        private DBHandler() 
        {
            string connectionString = "host=localhost;port=5433;database=tuplaus;username=postgres;password=****";
            dataSource = NpgsqlDataSource.Create(connectionString);
        }

        /// <summary>
        /// Saves a tuplaus game event into the database 
        /// </summary>
        /// <param name="gameEvent"><c>TuplausGameEvent</c> object with the to be saved data</param>
        /// <returns>true if the query worked, false if not</returns>
        public async Task<bool> SaveTuplausGameEvent(TuplausGameEvent gameEvent) 
        {
            try
            {
                //Unsafe and should be done with EF core anyway
                string sql = $"INSERT INTO tuplausGameEvents(playerid, stake, choice, card, prize) VALUES ({gameEvent.PlayerID},  {gameEvent.Stake} , '{gameEvent.Choice}',  {gameEvent.Card}, {gameEvent.Prize});";
                NpgsqlCommand command = dataSource.CreateCommand(sql);

                await command.ExecuteNonQueryAsync();
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// Delete's player's game events from the database
        /// </summary>
        /// <param name="playerID">The ID of the player who's data is being deleted</param>
        /// <returns>true if the query worked, false if not</returns>
        public async Task<bool> DeletePlayerTuplausGameEvents(int playerID)
        {
            try
            {
                //Unsafe and should be done with EF core anyway
                string sql = $"DELETE FROM tuplausGameEvents WHERE playerid = {playerID};";
                NpgsqlCommand command = dataSource.CreateCommand(sql);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves a player's last tuplaus game event from the database
        /// </summary>
        /// <param name="id">The ID of the player</param>
        /// <returns><see cref="TuplausGameEvent"/> object with the found data</returns>
        public async Task<TuplausGameEvent> GetLastTuplausGameEvent(int id)
        {
            try
            {
                string sql = $"SELECT * FROM TuplausGameEvents WHERE playerid = {id} ORDER BY eventtime DESC LIMIT 1;";
                NpgsqlCommand command = dataSource.CreateCommand(sql);

                var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    reader.Read();
                    return new TuplausGameEvent
                    {
                        PlayerID = (int)reader["playerid"],
                        Stake = (int)reader["stake"],
                        Choice = (string)reader["choice"],
                        Card = (int)reader["card"],
                        Prize = (int)reader["prize"]
                    };
                }
                else
                {
                    throw new Exception($"Could not find a game event from player {id}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get game event data: {ex.Message}");
            }

        }

        /// <summary>
        /// Retrieves a player's information from the database
        /// </summary>
        /// <param name="playerID">The ID of the searched player</param>
        /// <returns><see cref="Player"/> object with the found data</returns>
        public async Task<Player> GetPlayerData(int playerID) 
        {
            try
            {
                string sql = $"SELECT * FROM players WHERE playerid = {playerID};";
                NpgsqlCommand command = dataSource.CreateCommand(sql);

                var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    reader.Read();
                    return new Player
                    {
                        PlayerID = (int)reader["playerid"],
                        PlayerName = (string)reader["playername"],
                        Saldo = (int)reader["saldo"]
                    };
                }
                else
                {
                    throw new Exception($"Could not find a player with ID {playerID}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get player data: {ex.Message}");
            }
            
        }

        /// <summary>
        /// Saves a player into the database 
        /// </summary>
        /// <param name="player"><c>Player</c> object with the to be saved data</param>
        /// <returns>true if the query worked, false if not</returns>
        public async Task<Player> SavePlayerData(Player player)
        {
            try
            {
                string sql = $"INSERT INTO players(playerid, playername, saldo) VALUES ({player.PlayerID},  '{player.PlayerName}' , {player.Saldo}) RETURNING *;";
                NpgsqlCommand command = dataSource.CreateCommand(sql);

                var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    reader.Read();
                    return new Player
                    {
                        PlayerID = (int)reader["playerid"],
                        PlayerName = (string)reader["playername"],
                        Saldo = (int)reader["saldo"]
                    };
                }
                else
                {
                    throw new Exception($"Reader returned no rows");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add player: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a player from the database 
        /// </summary>
        /// <param name="playerID">ID of the player to be deleted data</param>
        /// <returns>true if the query worked, false if not</returns>
        public async Task<Player> DeletePlayerData(int playerID)
        {
            try
            {
                string sql = $"DELETE FROM players WHERE playerid = {playerID} RETURNING *;";
                NpgsqlCommand command = dataSource.CreateCommand(sql);

                var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    reader.Read();
                    return new Player
                    {
                        PlayerID = (int)reader["playerid"],
                        PlayerName = (string)reader["playername"],
                        Saldo = (int)reader["saldo"]
                    };
                }
                else
                {
                    throw new Exception($"Could not find a player with ID {playerID}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception ($"Failed to delete player: {ex.Message}");
            }
        }

        /// <summary>
        /// Deposits the amount specified in the <c>TransactionData</c> object into the specified player's account
        /// </summary>
        /// <param name="data"><c>TransactionData</c> object with the transaction amount and target</param>
        /// <returns>The player's new saldo</returns>
        public async Task<int> Deposit(TransactionData data)
        {
            try
            {
                string sql = $"SELECT saldo FROM players WHERE playerid = {data.PlayerID};";
                NpgsqlCommand command = dataSource.CreateCommand(sql);

                var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    int saldo = reader.GetInt32(0);

                    sql = $"UPDATE players SET saldo = {saldo + data.Amount} WHERE playerid = {data.PlayerID} RETURNING *";
                    command = dataSource.CreateCommand(sql);

                    reader = await command.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        saldo = (int)reader["saldo"];
                    }
                    else
                    {
                        throw new Exception("Failed to update saldo");
                    }
                    
                    return saldo;
                }
                else
                {
                    throw new Exception($"Failed to find account with playerID {data.PlayerID}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed Deposit: {ex.Message}");
            }
        }

            /// <summary>
            /// Withdraws the amount specified in the <c>TransactionData</c> object out of the specified player's account
            /// </summary>
            /// <param name="data"><c>TransactionData</c> object with the transaction amount and target</param>
            /// <returns>The player's new saldo</returns>
        public async Task<int> Withdraw(TransactionData data)
        {
            try
            {
                string sql = $"SELECT saldo FROM players WHERE playerid = {data.PlayerID};";
                NpgsqlCommand command = dataSource.CreateCommand(sql);

                var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    int saldo = reader.GetInt32(0);

                    if (saldo - data.Amount >= 0)
                    {
                        sql = $"UPDATE players SET saldo = {saldo - data.Amount} WHERE playerid = {data.PlayerID} RETURNING *";
                        command = dataSource.CreateCommand(sql);

                        reader = await command.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            saldo = (int)reader["saldo"];
                        }
                        else
                        {
                            throw new Exception("Failed to update saldo.");
                        }

                        return saldo;
                    }
                    else
                    {
                        throw new Exception("Cannot overdraw.");
                    }
                }
                else
                {
                    throw new Exception($"Failed to find account with playerID {data.PlayerID}.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed Withdrawal: {ex.Message}.");
            }
        }
    }
}
