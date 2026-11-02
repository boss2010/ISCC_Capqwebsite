using Newtonsoft.Json;

namespace ViewModels
{
    public static class ResponseAcquirerCode
    {
        private static readonly HttpClient BankClient = new HttpClient();

        public static string Response(string order)
        {
            var url = "https://nbe.gateway.mastercard.com/api/rest/version/100/merchant/AGRICULTURE/order/" + order;
            return QueryBank(url, "Basic TWVyY2hhbnQuQUdSSUNVTFRVUkU6ZWVkZTUzOGY2N2RlZDE5OTBkYmYwMTllYzM3Mzk0ODk=");
        }

        public static string Response_Inspection(string order)
        {
            var url = "https://nbe.gateway.mastercard.com/api/rest/version/100/merchant/AGRICULTURE2/order/" + order;
            return QueryBank(url, "Basic bWVyY2hhbnQuQUdSSUNVTFRVUkUyOjRiMDQwZTVkOTIwZDA5MDc5ZjdkMTllZWQxZmRmM2Jh");
        }

        private static string QueryBank(string url, string authorization)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("Authorization", authorization);
                request.Headers.TryAddWithoutValidation("Accept", "application/json");

                using var response = BankClient.Send(request);
                if (!response.IsSuccessStatusCode)
                {
                    return "NF";
                }

                var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var data = JsonConvert.DeserializeObject<ResponseDTO.Root>(result);
                return data?.transaction?.LastOrDefault()?.response?.acquirerCode ?? "NF";
            }
            catch (Exception)
            {
                return "NF";
            }
        }
    }
}
