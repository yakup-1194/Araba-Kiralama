using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace RentACar.Auth
{
    public class AuthProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            // context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" }); // Farklı domainlerden istek sorunu yaşamamak için

            //Burada kendi authentication yöntemimizi belirleyebiliriz.Veritabanı bağlantısı vs...
            var customerService = new CustomerService();
            var customer = customerService.customerLogin(context.UserName, context.Password);
            List<string> customerAuth = new List<string>();

            if (customer != null)
            {
                string auth = "";
                if (customer.Admin == 1)
                {
                    auth = "Admin";

                }
                else
                {
                    auth = "Customer";
                }
                customerAuth.Add(auth);

                var identity = new ClaimsIdentity(context.Options.AuthenticationType);

                identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
                identity.AddClaim(new Claim(ClaimTypes.Role, auth));
                identity.AddClaim(new Claim(ClaimTypes.PrimarySid, customer.customerId.ToString()));

                AuthenticationProperties property = new AuthenticationProperties(new Dictionary<string, string>
                {
                    { "customerId", customer.customerId.ToString() },
                    { "customerUsername", customer.customerUsername },
                    { "customerAuth",Newtonsoft.Json.JsonConvert.SerializeObject(customerAuth) }

               });
                AuthenticationTicket ticket = new AuthenticationTicket(identity, property);


                context.Validated(ticket);
            }
            else
            {
                context.SetError("Geçersiz istek", "Hatalı kullanıcı bilgisi");
            }



        }
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }
    }
}