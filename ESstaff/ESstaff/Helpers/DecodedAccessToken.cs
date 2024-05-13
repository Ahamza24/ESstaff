using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESstaff.Models;
using JWT;

namespace ESstaff.Helpers
{
    public class DecodedAccessToken
    {
        //shared secret. The API has the exact same secret and they MUST be the same for this to work, obviously!
        private static readonly string secretKey = "ByYM000OLlMQG6VVVp1OH7Xzyr7gHuw1qvUC5dcGt3SNM";

        private string accessToken;
        public string AccessToken
        {
            get
            {
                return accessToken;
            }
            set
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = value;
                }
                else
                {
                    throw new Exception("Cannot set AccessTokenData again once set, create new instance instead!");
                }
            }
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("Firstname")]
        public string FirstName { get; set; }

        [JsonProperty("Surename")]
        public string SureName { get; set; }

        [JsonProperty("email")]
        public string EmailAddress { get; set; }

        [JsonProperty("jti")]
        public string Jti { get; set; }

        //when a single role exists in the JSON, role comes across as a single string value but when there's more than one, it comes across as a string array.
        //this converter deals with that complexity and "promotes" the single string role to a list of just 1 string so that it always appears a list of strings regardless.
        [JsonConverter(typeof(SingleOrArrayJsonConverter<string>))]
        [JsonProperty("role")]
        public IList<string> Role { get; set; }

        [JsonProperty("exp")]
        public int Exp { get; set; }

        [JsonProperty("iss")]
        public string Iss { get; set; }

        [JsonProperty("aud")]
        public string Aud { get; set; }

        protected bool HasRole(string roleToCheck)
        {
            if (Role != null)
            {
                return Role.Any(r => r == roleToCheck);
            }
            return false;
        }

        public static T Decode<T>(Token token)
            where T : DecodedAccessToken
        {
            string jsonPayload = JsonWebToken.Decode(token.Data, secretKey);
            T dat = JsonConvert.DeserializeObject<T>(jsonPayload);
            dat.AccessToken = token.Data;
            return dat;
        }
    }
}
