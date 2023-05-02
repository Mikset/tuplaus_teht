using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tuplaus_teht.Common.DTO;
using tuplaus_teht.Common;

namespace tuplaus_teht.Service
{
    public class GameEngine
    {
        private readonly Random rand;
        private readonly int deckSize = 13;
        private readonly int centre = 7;
        private readonly int winMultiplier = 2;

        public GameEngine() 
        { 
            rand = new Random();
        }
        /// <summary>
        /// <para>Takes the player's action data and checks if the player won or lost. Based on the outcome, the player is awarded double their given stake. </para>
        /// <para>Saves the player's new saldo and the game event into the database.</para>
        /// </summary>
        /// <param name="action"><c>TuplausActionData</c> object with all the data about the action the player took.</param>
        /// <returns> <c>TuplausResponseData</c> with the drawn card and whether the player won the round filled in. Prize and Saldo are 0 </returns>
        public TuplausResponseData TuplausGame(TuplausActionData action)
        {
            
            int card = rand.Next(deckSize) + 1;
            bool isWin;
            int prize = 0;
            
            if (action.Choice == Guess.SMALL)
            {
                isWin = card < centre;
            }
            else if (action.Choice == Guess.LARGE)
            {
                isWin = card > centre;
            }
            else
            {
                throw new Exception($"Choice {action.Choice} not recognised. Should be {Guess.SMALL} or {Guess.LARGE}");
            }

            if (isWin)
            {
                prize = action.Stake * winMultiplier;
            }

            return new TuplausResponseData
            {
                Card = card,
                IsWin = isWin,
                Prize = prize,
                Status = 0,
                Message = "Success"
            };
        }
    }
}
