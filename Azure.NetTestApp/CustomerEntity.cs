using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.NetTestApp
{
    public sealed class CustomerEntity : TableEntity
    {
        public CustomerEntity(string lastName)
        {
            PartitionKey = lastName;
            RowKey = Guid.NewGuid().ToString();
        }

        public CustomerEntity()
        {

        }

        public string FirstName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
    }
}
