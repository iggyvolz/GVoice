namespace GVoice
{
    public class LcResponse
    {
        public Response response;
        public class Response
        {
            public Status status;
            public class Status
            {
                public string kind;
                public int status;
            }
            public Conversation[] conversation;
            public class Conversation
            {
                public string id;
                public string conversationTime;
                public bool read;
                public string[] label;
                public string pagination_token;
                public PhoneCall[] phoneCall;
                public class PhoneCall
                {
                    public string id;
                    public string startTime;
                    public string did;
                    public HeadingContact contact;
                    public string type;
                    public int status;
                    public string messageText;
                    public int coarseType;
                    public int transcriptStatus;
                    public bool isArtificialErrorMessage;
                    public override bool Equals(object obj)
                    {
                        return (obj is PhoneCall && obj.GetHashCode() == GetHashCode());

                    }
                    public override int GetHashCode()
                    {
                        return int.Parse(id, System.Globalization.NumberStyles.HexNumber);
                    }
                }
                public HeadingContact[] headingContact;
                public class HeadingContact
                {
                    public string name;
                    public string phoneNumber;
                    public int phoneType;
                    public string id;
                    public string phoneNumberFormatted;
                    public bool blocked;
                    public override bool Equals(object obj)
                    {
                        return (obj is HeadingContact && obj.GetHashCode() == GetHashCode());
                    }
                    public override int GetHashCode()
                    {
                        return int.Parse(id);
                    }
                }
            }
        }
    }
}