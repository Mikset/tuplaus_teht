namespace tuplaus_teht.Tests
{
    public class GameEngine_UnitTests
    {
        private readonly GameEngine engine;

        public GameEngine_UnitTests() 
        { 
            engine = new GameEngine();
        }

        [Theory]
        [InlineData(Guess.SMALL)]
        [InlineData(Guess.LARGE)]
        public void TuplausSuccessTest(string choice)
        {
            TuplausActionData action = new()
            {
                PlayerID = 10,
                Choice = choice,
                Stake = 100,
                firstGame = true
            };
            TuplausResponseData response = engine.TuplausGame(action);

            if (response.IsWin)
            {
                Assert.Equal(action.Stake * 2, response.Prize);
            }
            else
            {
                Assert.Equal(0, response.Prize);
            }
            Assert.Equal(0, response.Status);
        }

        [Fact] 
        public void TuplausBadGuessFailureTest()
        {
            TuplausActionData action = new()
            {
                PlayerID = 11,
                Choice = "väärä",
                Stake = 100,
                firstGame = true
            };

            Exception ex = Assert.Throws<Exception>(() => engine.TuplausGame(action));
            Assert.Contains($"Choice {action.Choice} not recognised.", ex.Message);
        }
    }
}
