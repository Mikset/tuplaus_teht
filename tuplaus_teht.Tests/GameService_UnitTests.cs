using System;
using tuplaus_teht.Service;

namespace tuplaus_teht.Tests
{
    public class GameService_UnitTests
    {
        private readonly GameService service;
        private readonly DBHandler dbHandler;

        public GameService_UnitTests ()
        { 
            service = new GameService();
            dbHandler = DBHandler.Instance;
        }

        [Theory]
        [InlineData(Guess.SMALL)]
        [InlineData(Guess.LARGE)]
        public async void ServiceTuplausSuccessTest(string choice)
        {
            Player testPlayer = new()
            {
                PlayerID = 20,
                PlayerName = "Test20",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);

            TuplausActionData action = new()
            {
                PlayerID = testPlayer.PlayerID,
                Choice = choice,
                Stake = 100,
                firstGame = true
            };
            TuplausResponseData response = await service.Tuplaus(action);

            if (response.IsWin)
            {
                Assert.Equal(action.Stake * 2, response.Prize);
            }
            else
            {
                Assert.Equal(0, response.Prize);
            }
            Assert.Equal(0, response.Status);

            await dbHandler.DeletePlayerTuplausGameEvents(testPlayer.PlayerID);
            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Fact] 
        public async void ServiceTuplausBadStakeFailureTest()
        {
            TuplausActionData action = new()
            {
                PlayerID = 21,
                Choice = Guess.LARGE,
                Stake = -100,
                firstGame = true
            };
            TuplausResponseData response = await service.Tuplaus(action);
            Assert.Contains($"Can't have a stake of {action.Stake}.", response.Message);
            Assert.Equal(-1, response.Status);
        }

        [Fact]
        public async void ServiceTuplausPlayTwiceSuccessTest()
        {
            TuplausResponseData response;

            Player testPlayer = new()
            {
                PlayerID = 22,
                PlayerName = "Test22",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);

            TuplausActionData action = new()
            {
                PlayerID = testPlayer.PlayerID,
                Choice = Guess.SMALL,
                Stake = 100,
                firstGame = true
            };

            TransactionData transaction = new()
            {
                PlayerID = testPlayer.PlayerID,
                Amount = 100
            };

            do
            {
                response = await service.Tuplaus(action);

                if (!response.IsWin)
                {
                    await dbHandler.Deposit(transaction);
                }
            }
            while (!response.IsWin);

            action.firstGame = false;
            action.Stake = response.Prize;
            response = await service.Tuplaus(action);

            if (response.IsWin)
            {
                Assert.Equal(action.Stake * 2, response.Prize);
            }
            else
            {
                Assert.Equal(0, response.Prize);
            }
            Assert.Equal("Success", response.Message);

            await dbHandler.DeletePlayerTuplausGameEvents(testPlayer.PlayerID);
            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Fact]
        public async void ServiceDepositSuccessTest()
        {
            Player testPlayer = new()
            {
                PlayerID = 23,
                PlayerName = "Test23",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);
            TransactionData data = new()
            {
                PlayerID = testPlayer.PlayerID,
                Amount = 100,
            };
            TransactionResponseData response = await service.Deposit(data);

            Assert.Equal("Success", response.Message);
            Assert.Equal(testPlayer.Saldo + data.Amount, response.Amount);

            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Fact]
        public async void ServiceWithdrawSuccessTest()
        {
            Player testPlayer = new()
            {
                PlayerID = 24,
                PlayerName = "Test24",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);
            TransactionData data = new()
            {
                PlayerID = testPlayer.PlayerID,
                Amount = 100,
            };
            TransactionResponseData response = await service.Withdraw(data);

            Assert.Equal("Success", response.Message);
            Assert.Equal(testPlayer.Saldo - data.Amount, response.Amount);

            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Fact]
        public async void ServiceDepositFailureTest()
        {
            TransactionData data = new()
            {
                PlayerID = 25,
                Amount = -100
            };
            TransactionResponseData response = await service.Deposit(data);
            Assert.Contains($"Can't deposit {data.Amount}.", response.Message);
            Assert.Equal(-1, response.Status);
        }

        [Fact]
        public async void ServiceWithdrawFailureTest()
        {
            TransactionData data = new()
            {
                PlayerID = 25,
                Amount = -100
            };
            TransactionResponseData response = await service.Withdraw(data);
            Assert.Contains($"Can't withdraw {data.Amount}.", response.Message);
            Assert.Equal(-1, response.Status);
        }
    }
}
