using SampleApp.Examples;

class Program
{
    static async Task Main()
    {
        while (true)
        {
            Console.WriteLine("Enter your query to search:");
            string query = Console.ReadLine();

            // Pick which example to run by uncommenting:
            // await Example1_Basic.Run(query);
            // await Example2_FileStore.Run();
            // await Example3_Directory.Run(); 
            await Example4_Url.Run(query);
            // await Example5_Manual.Run();
        }
    }
}
