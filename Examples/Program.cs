using SampleApp.Examples;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var query = "What is quantum mechanics?";

            // Uncomment the example you want to run:

            await Example1_QuickStart.Run(query);
            // await Example2_VectorStores.Run(query);
            // await Example3_LoadingData.Run(query);
            // await Example4_LowLevel.Run();
        }
    }
}