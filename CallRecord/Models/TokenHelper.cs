using Azure;
using Azure.Communication;
using Azure.Communication.Identity;
using Azure.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AdaniCall.Models
{
    public class TokenHelper
    {
        string endpoint = ConfigurationManager.AppSettings["CallEndPoint"].ToString();
        string accessKey = ConfigurationManager.AppSettings["CallKey"].ToString();

        public AccessToken RefreshTokenAsync(string userCallID)
        {
            var client = new CommunicationIdentityClient(new Uri(endpoint), new AzureKeyCredential(accessKey));

            ////To refresh an access token, pass an instance of the CommunicationUserIdentifier object into GetTokenAsync. If you've stored this Id and need to create a new CommunicationUserIdentifier, you can do so by passing your stored Id into the CommunicationUserIdentifier constructor as follows:
            var identityToRefresh = new CommunicationUserIdentifier(userCallID);
            return client.GetToken(identityToRefresh, scopes: new[] { CommunicationTokenScope.VoIP });
        }
    }
}