﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PaymentService.Api.Models;

public class Account
{
    public const string CollectionName = "Accounts";

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    public string AccountType { get; set; }

    public List<Transaction> Transactions { get; set; }
    
    // In coins
    public long Money { get; set; }
    
    public DateTimeOffset CreationAt { get; set; }
}

public class AccountDto
{
    public Guid UserId { get; set; }
    public long Money { get; set; }
}