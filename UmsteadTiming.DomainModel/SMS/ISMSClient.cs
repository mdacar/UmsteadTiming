using System.Collections.Generic;

namespace UltimateTiming.DomainModel.SMS
{
    public interface ISMSClient
    {

        SMSResult SendMessage(string phoneNumber, string message);


        Dictionary<string, string> RetrieveStopMessages();

    }
}
