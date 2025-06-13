// See https://aka.ms/new-console-template for more information

using System.Dynamic;
using System.Text.Json;

var data = new
{
    Int = 1,
    Double = 1.1,
    String = "Hello",
    DateTime = DateTime.Now,
    DateOnly = DateOnly.FromDateTime(DateTime.Now),
    TimeOnly = TimeOnly.FromDateTime(DateTime.Now),
    Enum = DayOfWeek.Monday,
    Guid = Guid.NewGuid(),
    Decimal = 1.1m,
    Class = new
    {
        Id = 1,
        Name = "<UNK>",
    },
    Dict = new Dictionary<string, object>
    {
        { "First", 1 },
        { "Second", 2 }
    },
    List = new List<int> { 1, 2, 3 }
};

var json = JsonSerializer.Serialize(data);  

var obj = JsonSerializer.Deserialize<DynamicObject>(json);

Console.WriteLine(obj);