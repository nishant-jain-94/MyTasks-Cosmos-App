using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Security.Authentication;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MyTaskConsoleApp
{
    public class MyTask
    {
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Category")]
        public string Category { get; set; }

        [BsonElement("Date")]
        public DateTime Date { get; set; }

        [BsonElement("CreatedDate")]
        public DateTime CreatedDate { get; set; }
    }

    class MyTaskController
    {
        private string userName;
        private string host;
        private string password;


        private string dbName = "Tasks";
        private string collectionName = "TasksList";

        public MyTaskController()
        {
            userName = Environment.GetEnvironmentVariable("COSMOS_MONGODB_USERNAME");
            host = Environment.GetEnvironmentVariable("COSMOS_MONGODB_HOST");
            password = Environment.GetEnvironmentVariable("COSMOS_MONGODB_PASSWORD");
        }

        private IMongoCollection<MyTask> GetTasksCollection()
        {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(host, 10255);
            settings.UseSsl = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;

            MongoIdentity identity = new MongoInternalIdentity(dbName, userName);
            MongoIdentityEvidence evidence = new PasswordEvidence(password);

            settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            MongoClient client = new MongoClient(settings);
            var database = client.GetDatabase(dbName);
            var todoTaskCollection = database.GetCollection<MyTask>(collectionName);
            return todoTaskCollection;
        }

        private IMongoCollection<MyTask> GetTasksCollectionForEdit()
        {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(host, 10255);
            settings.UseSsl = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;

            MongoIdentity identity = new MongoInternalIdentity(dbName, userName);
            MongoIdentityEvidence evidence = new PasswordEvidence(password);

            settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            MongoClient client = new MongoClient(settings);
            var database = client.GetDatabase(dbName);
            var todoTaskCollection = database.GetCollection<MyTask>(collectionName);
            return todoTaskCollection;
        }

        public List<MyTask> GetAllTasks()
        {
            try
            {
                var collection = GetTasksCollection();
                return collection.Find(new BsonDocument()).ToList();
            }
            catch (MongoConnectionException)
            {
                return new List<MyTask>();
            }
        }

        public void CreateTask(MyTask task)
        {
            var collection = GetTasksCollectionForEdit();
            try
            {
                collection.InsertOne(task);
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            // Read data from MongoDB
            MyTaskController taskController = new MyTaskController();
            var taskList = taskController.GetAllTasks();
            var json = JsonSerializer.Serialize(taskList);
            Console.WriteLine("State of Collection Before adding Tasks");
            Console.WriteLine("#------------------------------------------------------------#");
            Console.WriteLine(json);

            MyTask t = new MyTask();
            t.Name = "Write Documentation";
            t.Category = "Developers";
            t.CreatedDate = DateTime.Now;
            t.Date = DateTime.Now;

            // Write data to MongoDB
            MyTaskController taskController1 = new MyTaskController();
            taskController1.CreateTask(t);
            taskList = taskController1.GetAllTasks();
            json = JsonSerializer.Serialize(taskList);
            Console.WriteLine("State of Collection After adding Tasks");
            Console.WriteLine("#------------------------------------------------------------#");
            Console.WriteLine(json);
        }
    }
}
