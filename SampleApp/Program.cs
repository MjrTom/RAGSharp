using SampleApp.Examples;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter query to search: ");
                var query = Console.ReadLine(); //var query = "What is quantum mechanics?";
                                                // Uncomment the example you want to run:

                // await Example1_QuickStart.Run(query);
                // await Example2_FilesSearch.Run(query);
                // await Example3_WebDocSearch.Run(query);
                // await Example4_Barebones.Run();
                await Example5_Advanced.Run(query);

            }
        }
    }
}