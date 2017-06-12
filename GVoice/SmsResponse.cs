namespace GVoice
{
    public class SmsResponse
    {
        public Response response;
        public class Response
        {
            public LcResponse.Response.Status status;
            public string conversationId;
            public string callId;
            public string timestampMs;
        }
    }
}