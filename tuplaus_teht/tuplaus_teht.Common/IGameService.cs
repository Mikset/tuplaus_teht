using System.ServiceModel;
using System.Threading.Tasks;
using tuplaus_teht.Common.DTO;

namespace tuplaus_teht.Common
{
    [ServiceContract]
    [XmlSerializerFormat]
    public interface IGameService
    {
        [OperationContract]
        Task<TuplausResponseData> Tuplaus(TuplausActionData data);
        [OperationContract]
        Task<TransactionResponseData> Deposit(TransactionData data);
        [OperationContract]
        Task<TransactionResponseData> Withdraw(TransactionData data);
    }
}
