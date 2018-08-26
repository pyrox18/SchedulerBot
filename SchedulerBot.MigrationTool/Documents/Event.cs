using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SchedulerBot.MigrationTool.Documents
{
    public class Event
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("startDate")]
        public DateTime StartTimestamp { get; set; }

        [BsonElement("endDate")]
        public DateTime EndTimestamp { get; set; }

        [BsonElement("repeat")]
        public string Repeat { get; set; }
    }
}
