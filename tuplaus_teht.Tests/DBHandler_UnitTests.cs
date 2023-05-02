namespace tuplaus_teht.Tests
{
    public class DBHandler_UnitTests
    {
        private readonly DBHandler dbHandler;

        public DBHandler_UnitTests()
        {
            dbHandler = DBHandler.Instance;
        }

        [Fact]
        public async void AddPlayerTest()
        {
            Player testPlayer = new()
            {
                PlayerID = 0,
                PlayerName = "Test",
                Saldo = 1000
            };
            Player resultPlayer = await dbHandler.SavePlayerData(testPlayer);
            Assert.Equal(testPlayer.PlayerName, resultPlayer.PlayerName);
            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Fact]
        public async void DeletePlayerTest()
        {
            Player testPlayer = new()
            {
                PlayerID = 1,
                PlayerName = "Test1",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);

            Player resultPlayer = await dbHandler.DeletePlayerData(testPlayer.PlayerID);
            Assert.Equal(testPlayer.PlayerName, resultPlayer.PlayerName);
        }

        [Fact]
        public async void GetPlayerTest()
        {
            Player testPlayer = new()
            {
                PlayerID = 2,
                PlayerName = "Test2",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);

            Player resultPlayer = await dbHandler.GetPlayerData(testPlayer.PlayerID);
            Assert.Equal(testPlayer.PlayerName, resultPlayer.PlayerName);

            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Theory]
        [InlineData(100, Guess.SMALL, 1, 200)]
        [InlineData(200, Guess.LARGE, 13, 400)]
        public async void EventTests(int stake, string choice, int card, int prize)
        {
            Player testPlayer = new()
            {
                PlayerID = 3,
                PlayerName = "Test3",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);

            Assert.True(await dbHandler.SaveTuplausGameEvent(new TuplausGameEvent
            {
                PlayerID = testPlayer.PlayerID,
                Stake = stake,
                Choice = choice,
                Card = card,
                Prize = prize
            }));

            Assert.Equal(choice, (await dbHandler.GetLastTuplausGameEvent(testPlayer.PlayerID)).Choice);

            Assert.True(await dbHandler.DeletePlayerTuplausGameEvents(testPlayer.PlayerID));

            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Fact]
        public async void DepositSuccessTest()
        {
            Player testPlayer = new()
            {
                PlayerID = 4,
                PlayerName = "Test4",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);

            Assert.Equal(testPlayer.Saldo + 100, await dbHandler.Deposit(new TransactionData
            {
                PlayerID = testPlayer.PlayerID,
                Amount = 100
            }));

            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Fact]
        public async void DepositNoUserFailureTest()
        {
            Player testPlayer = new()
            {
                PlayerID = 5,
                PlayerName = "Test5",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);

            var ex = await Assert.ThrowsAsync<Exception>(async () => await dbHandler.Deposit(new TransactionData
            {
                PlayerID = -1,
                Amount = 100
            }));

            Assert.Contains("Failed to find account with playerID", ex.Message);

            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Fact]
        public async void WithdrawSuccessTest()
        {
            Player testPlayer = new()
            {
                PlayerID = 6,
                PlayerName = "Test6",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);

            Assert.Equal(testPlayer.Saldo - 100, await dbHandler.Withdraw(new TransactionData
            {
                PlayerID = testPlayer.PlayerID,
                Amount = 100
            }));

            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Fact]
        public async void WithdrawNoUserFailureTest()
        {
            Player testPlayer = new()
            {
                PlayerID = 7,
                PlayerName = "Test7",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);

            var ex = await Assert.ThrowsAsync<Exception>(async () => await dbHandler.Withdraw(new TransactionData
            {
                PlayerID = -1,
                Amount = 100
            }));

            Assert.Contains("Failed to find account with playerID", ex.Message);

            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }

        [Fact]
        public async void WithdrawOverdrawFailureTest()
        {
            Player testPlayer = new()
            {
                PlayerID = 8,
                PlayerName = "Test8",
                Saldo = 1000
            };
            await dbHandler.SavePlayerData(testPlayer);

            var ex = await Assert.ThrowsAsync<Exception>(async () => await dbHandler.Withdraw(new TransactionData
            {
                PlayerID = testPlayer.PlayerID,
                Amount = 10000
            }));

            Assert.Contains("Cannot overdraw", ex.Message);

            await dbHandler.DeletePlayerData(testPlayer.PlayerID);
        }
    }
}