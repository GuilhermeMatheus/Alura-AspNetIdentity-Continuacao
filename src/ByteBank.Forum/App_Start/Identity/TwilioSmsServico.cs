using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ByteBank.Forum.App_Start.Identity
{
    public class TwilioSmsServico : IIdentityMessageService
    {
        private readonly string TWILIO_SID = ConfigurationManager.AppSettings["twilio:sid"];
        private readonly string TWILIO_TOKEN = ConfigurationManager.AppSettings["twilio:auth_token"];
        private readonly string TWILIO_NUM = ConfigurationManager.AppSettings["twilio:number"];

        public async Task SendAsync(IdentityMessage message)
        {
            TwilioClient.Init(TWILIO_SID, TWILIO_TOKEN);

            var twilio_message = await MessageResource.CreateAsync(
                new PhoneNumber(message.Destination),
                from: TWILIO_NUM,
                body: message.Body
            );
        }
    }
}