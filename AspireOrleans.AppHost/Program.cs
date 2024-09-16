namespace AspireOrleans.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var server = builder.AddProject<Projects.OrleansServer>("server");

        var client = builder.AddProject<Projects.OrleansClient>("client");

        builder.Build().Run();
    }
}