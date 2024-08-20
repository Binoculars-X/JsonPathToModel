using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.ModelData;

public class SampleClientModel
{
    public string Id { get; set; }
    public string? BusinessId { get; set; }
    public Person Person { get; set; }
    public Role[] Roles { get; set; }
    public bool IsDeleted { get; set; }
}

public class Person
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<Email> Emails { get; set; } = [];
    public List<Address> Addresses { get; set; } = [];
    public Contact PrimaryContact { get; set; }
}

public class Contact
{
    public Email Email { get; set; }
    public Phone Phone { get; set; }
}

public class Email
{
    public string Value { get; set; }
}
public class Phone
{
    public string Value { get; set; }
}

public class Address
{
    public string AddressType { get; set; }
    public string StreetNumber { get; set; }
    public string StreetType { get; set; }
    public string[] AddressLine { get; set; }
    public string CityName { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
}

public class Holding
{
    public string Code { get; set; }
    public string MarketCode { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
}

public class Role
{
    public string RoleId { get; set; }
    public string Name { get; set; }
}
