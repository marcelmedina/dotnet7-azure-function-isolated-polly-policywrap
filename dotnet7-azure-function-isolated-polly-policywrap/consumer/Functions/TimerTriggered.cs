using consumer.TypedHttpClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace consumer.Functions
{
    public class TimerTriggered
    {
        private readonly IConfiguration _configuration;
        private readonly StateCounterHttpClient _httpClient;

        public TimerTriggered(IConfiguration configuration, StateCounterHttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [Function("TimerTriggered")]
        public async Task Run([TimerTrigger("*/10 * * * * *")] FunctionContext context)
        {
            var logger = context.GetLogger<TimerTriggered>();

            try
            {
                var endpoint = _configuration.GetValue<string>(Constants.ProducerEndpoint);

                var increment = await _httpClient.GetStateCounter(endpoint);

                logger.LogInformation($"### Increment = {increment}");
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}
