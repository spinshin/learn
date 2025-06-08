using Bogus;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ChannelExperiment;

class Program
{
    static async Task Main(string[] args)
    {
        const string connectionUri =
            "Server=destruction.cwy2cgzdjbif.ap-south-1.rds.amazonaws.com;Database=test;User Id=admin;Password=ktByHYWcMwxsZfNQgkKh;TrustServerCertificate=True;";

        // var tasks = new List<Task>();
        //
        // const int taskCount = 1;
        const int repetitionCount = 5000;
        const int entryCount = 1500;

        // for (var i = 0; i < taskCount; i++)
        // {
        //     var task = Task.Run(async () =>
        //     {

        var faker = new Faker();
        var timestamp = new DateTime(2025, 1, 1);

        for (var j = 0; j < repetitionCount; j++)
        {
            List<SensorEntry> data = [];

            for (var k = 0; k < entryCount; k++)
            {
                data.Add(new SensorEntry
                {
                    Id = k,
                    Value = faker.Random.Double(0, 200),
                    SourceTime = timestamp.AddSeconds(j),
                    StatusCode = faker.PickRandom(1, 2, 3)
                });
            }

            await using var connection = new SqlConnection(connectionUri);
            var sql = """
                      INSERT INTO OpcData
                      VALUES (@Id, @Value, @StatusCode, @SourceTime)
                      """;

            await connection.ExecuteAsync(sql, data);
        }

        //     });
        //
        //     tasks.Add(task);
        // }
        //
        // await Task.WhenAll(tasks);
    }
}

public class SensorEntry
{
    public SensorEntry()
    {
    }

    public SensorEntry(int id,
        double value,
        DateTime sourceTime,
        int statusCode)
    {
        Id = id;
        Value = value;
        SourceTime = sourceTime;
        StatusCode = statusCode;
    }

    public int Id { get; set; }
    public double Value { get; set; }
    public DateTime SourceTime { get; set; }
    public int StatusCode { get; set; }
}
