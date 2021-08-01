using System;
using System.Collections.Generic;
using Hangfire;
using Twilio.Rest.Api.V2010.Account;

namespace DelusionalApi.Service
{
    public class CallService
    {
        public readonly AppSetttings _appSetttings;

        public CallService(AppSetttings appSetttings)
        {
            _appSetttings = appSetttings;
        }

        [Queue("calls")]
        public void ScheduleCall(string phoneNumber, string twiml, Uri callCompletedCallback)
        {
            CallResource.Create(
                record: false,
                twiml: twiml,
                to: new Twilio.Types.PhoneNumber(phoneNumber),
                from: new Twilio.Types.PhoneNumber(_appSetttings.TwilioSettings.CallerId),
                statusCallback: callCompletedCallback,
                statusCallbackEvent: new List<string> { "completed" }
            );
        }

    }
}
