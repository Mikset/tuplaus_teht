using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using tuplaus_teht.Common;
using tuplaus_teht.Common.DTO;
using tuplaus_teht.Service.DTO;
using static System.Collections.Specialized.BitVector32;

namespace tuplaus_teht.Service
{
    public class GameService : IGameService
    {
        private readonly GameEngine engine;
        private readonly DBHandler dbHandler;

        public GameService()
        { 
            engine = new GameEngine();
            dbHandler = DBHandler.Instance;
        }

        /// <summary>
        /// Plays a round of Tuplaus
        /// </summary>
        /// <param name="data"><c>TuplausActionData</c> with the player's ID and necessary data for the game</param>
        /// <returns><c>TuplausResponseData</c> with the drawn card, if the player won, the possible prize, and a status report</returns>
        public async Task<TuplausResponseData> Tuplaus(TuplausActionData data)
        {
            TuplausResponseData response;
            if (data.firstGame)
            {
                if (data.Stake > 0)
                {
                    TransactionData withdrawStake = new TransactionData
                    {
                        PlayerID = data.PlayerID,
                        Amount = data.Stake
                    };
                    int newSaldo = await dbHandler.Withdraw(withdrawStake);

                    if (newSaldo >= 0)
                    {
                        try
                        {
                            response = engine.TuplausGame(data);
                            Console.WriteLine("Player " + data.PlayerID + " played Tuplaus");
                        }
                        catch (Exception ex)
                        {
                            response = new TuplausResponseData
                            {
                                Status = -1,
                                Message = $"Failed to play tuplaus: {ex.Message}"
                            };
                            Console.WriteLine("Player " + data.PlayerID + " failed to play Tuplaus");
                        }
                    }
                    else
                    {
                        response = new TuplausResponseData
                        {
                            Status = -1,
                            Message = $"Failed to play tuplaus: Failed to withdraw stake from user."
                        };
                    }
                }
                else
                {
                    response = new TuplausResponseData
                    {
                        Status = -1,
                        Message = $"Failed to play tuplaus: Can't have a stake of {data.Stake}."
                    };
                }
            }
            else
            {
                Console.WriteLine("Player " + data.PlayerID + " continued to play Tuplaus");
                TuplausGameEvent previousRound = await dbHandler.GetLastTuplausGameEvent(data.PlayerID);

                if (previousRound.Prize == data.Stake)
                {
                    response = engine.TuplausGame(data);
                }
                else
                {
                    response = new TuplausResponseData
                    {
                        Status = -1,
                        Message = $"Failed to play tuplaus: Could not continue game. Stake was {data.Stake} when it should have been {previousRound.Prize}"
                    };
                }
            }

            _ = await dbHandler.SaveTuplausGameEvent(new TuplausGameEvent
            {
                PlayerID = data.PlayerID,
                Stake = data.Stake,
                Choice = data.Choice,
                Card = response.Card,
                Prize = response.Prize
            });
            return response; 
        }

        /// <summary>
        /// Deposits money into the player's account
        /// </summary>
        /// <param name="data"><c>TransactionData</c> with the player's ID and deposit amount</param>
        /// <returns><c>TransactionResponseData</c> with the player's new saldo and a status report</returns>
        public async Task<TransactionResponseData> Deposit(TransactionData data)
        {
            TransactionResponseData response;
            if (data.Amount > 0)
            {
                try
                {
                    int newSaldo = await dbHandler.Deposit(data);
                    response = new TransactionResponseData
                    {
                        Amount = newSaldo,
                        Status = 0,
                        Message = "Success"
                    };
                    Console.WriteLine($"Player {data.PlayerID} deposited money");
                }
                catch (Exception ex)
                {
                    response = new TransactionResponseData
                    {
                        Status = -1,
                        Message = ex.Message
                    };
                    Console.WriteLine($"Player {data.PlayerID} failed to deposit money");
                }
            }
            else
            {
                response = new TransactionResponseData
                {
                    Status = -1,
                    Message = $"Can't deposit {data.Amount}."
                };
                Console.WriteLine($"Player {data.PlayerID} failed to deposit money");
            }
            return response;
        }

        /// <summary>
        /// Withdraws money from the player's account
        /// </summary>
        /// <param name="data"><c>TransactionData</c> with the player's ID and withdrawal amount</param>
        /// <returns><c>TransactionResponseData</c> with the player's new saldo and a status report</returns>
        public async Task<TransactionResponseData> Withdraw(TransactionData data)
        {
            TransactionResponseData response;
            if (data.Amount > 0)
            {
                try
                {
                    int newSaldo = await dbHandler.Withdraw(data);
                    response = new TransactionResponseData
                    {
                        Amount = newSaldo,
                        Status = 0,
                        Message = "Success"
                    };
                    Console.WriteLine($"Player {data.PlayerID} withdrew money");
                }
                catch (Exception ex)
                {
                    response = new TransactionResponseData
                    {
                        Status = -1,
                        Message = ex.Message
                    };
                    Console.WriteLine($"Player {data.PlayerID} failed to withdraw money");
                }
            }
            else
            {
                response = new TransactionResponseData
                {
                    Status = -1,
                    Message = $"Can't withdraw {data.Amount}."
                };
                Console.WriteLine($"Player {data.PlayerID} failed to withdraw money");
            }
            return response;
        }
    }
}