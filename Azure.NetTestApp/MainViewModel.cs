using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Azure.NetTestApp
{
    public sealed class MainViewModel : BindableBase
    {
        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private CloudTable customersTable;

        public MainViewModel()
        {
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            tableClient = storageAccount.CreateCloudTableClient();
            customersTable = tableClient.GetTableReference("Customers");
            customersTable.CreateIfNotExists();

            Customers = new ObservableCollection<Customer>();
        }


        private ICommand addCommand;
        public ICommand AddCommand
        {
            get
            {
                if (addCommand == null)
                {
                    addCommand = new RelayCommand(ex => Add());
                }
                return addCommand;
            }
        }

        public void Add()
        {
            var customer = new Customer();
            Customers.Add(customer);
            Customer = customer;
        }

        private ICommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new RelayCommand(async ex => await Save());
                }
                return saveCommand;
            }
        }

        public async Task Save()
        {
            foreach (var item in Customers.Where(c => c.RowKey != Guid.Empty))
            {
                var entity = TableCustomers.FirstOrDefault(c => c.PartitionKey == item.LastName && c.RowKey == item.RowKey.ToString());
                entity.FirstName = item.FirstName;
                entity.EmailAddress = item.EmailAddress;
                entity.PhoneNumber = item.PhoneNumber;

                var update = TableOperation.Replace(entity);
                await customersTable.ExecuteAsync(update);
            }


            var batchOperation = new TableBatchOperation();
            foreach (var item in Customers.Where(c => c.RowKey == Guid.Empty))
            {
                var customer = new CustomerEntity(item.LastName);
                customer.FirstName = item.FirstName;
                customer.EmailAddress = item.EmailAddress;
                customer.PhoneNumber = item.PhoneNumber;
                batchOperation.Insert(customer);
                item.RowKey = Guid.Parse(customer.RowKey);
            }
            if (batchOperation.Any())
            {
                await customersTable.ExecuteBatchAsync(batchOperation);
            }
            
        }

        public void Load()
        {
            var query = new TableQuery<CustomerEntity>();
            var results = customersTable.ExecuteQuery(query);
            TableCustomers = new List<CustomerEntity>(results);


            foreach(var item in TableCustomers)
            {
                Customers.Add(new Customer
                {
                    EmailAddress = item.EmailAddress,
                    FirstName = item.FirstName,
                    LastName = item.PartitionKey,
                    PhoneNumber = item.PhoneNumber,
                    RowKey = Guid.Parse(item.RowKey)
                });
            }

            if(Customers.Any())
            {
                Customer = Customers.FirstOrDefault();
            }
        }

        public List<CustomerEntity> TableCustomers { get; set; }

        private ObservableCollection<Customer> customers;
        public ObservableCollection<Customer> Customers
        {
            get { return customers; }
            set { SetProperty(ref customers, value); }
        }

        private Customer customer;
        public Customer Customer
        {
            get { return customer; }
            set { SetProperty(ref customer, value); }
        }
    }
}
