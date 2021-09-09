﻿using System;

namespace TryingKafka.KafkaService.Models
{
    public class Report
    {
        public Guid Id { get; set; }
        public Order Order { get; set; }
        public string Details { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedOn { get; set; }

        public override string ToString()
        {
            return $"{Id}; Status:{Status}; Details:{Details}; On:{CreatedOn}. For Order: {Order.ToString()}";
        }
    }
}
