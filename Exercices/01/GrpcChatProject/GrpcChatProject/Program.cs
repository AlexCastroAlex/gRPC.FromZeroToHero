using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Exemple.Project;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Beginning Proto");

var emp = new Employee();
emp.FirstName = "Alex";
emp.LastName = "CASTRO";
emp.IsRetired = false;
var birthdate = new DateTime(1980, 1, 1);
birthdate = DateTime.SpecifyKind(birthdate,DateTimeKind.Utc);
emp.BirthDate = Timestamp.FromDateTime(birthdate);
emp.Age = 10;
emp.MaritalStatus = Employee.Types.MaritalStatus.Other;
emp.PreviousEmployers.Add("XXXX");
emp.PreviousEmployers.Add("YYYYYY");
var address = new Address();
address.City = "Lyon";
address.StreetName = "XXXx";
emp.CurrentAddress = address;
emp.FamilyLinks.Add("father", "XXXX");
emp.FamilyLinks.Add("son", "YYYY");
emp.FamilyLinks.Add("daughter", "ZZZZZZ");

using (var output = File.Create("emp.dat"))
{
    emp.WriteTo(output);
}

Employee employeeFromfile;
using (var input = File.OpenRead("emp.dat"))
{
    employeeFromfile = Employee.Parser.ParseFrom(input);
}

Console.WriteLine(employeeFromfile);

