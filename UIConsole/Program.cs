using CommandsMediatR = Application.CommandsMediatR;
using Application.Interfaces;
using QueriesMediatR = Application.QueriesMediatR;

using Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;

namespace UIConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test CQRS MetiatoR Pattern!");

            #region MediatR
            RunCQRSMediatR();
            #endregion
                                 
            Console.ReadKey();   

        }


        private static void RunCQRSMediatR()
        {
            try
            {
                var mediator = BuildMediator();

                //Add new Product
                var product = new CommandsMediatR.AddNewProductCommand { Id = Guid.NewGuid(), Name = "iPhone 11", Description = "Apple iphone 11" };
                var res = mediator.Send(product);

                //Update Product Unit Price
                mediator.Send(new CommandsMediatR.UpdateProductUnitPriceCommand { Id = product.Id, UnitPrice = 200 });

                //Update Product Current Stock
                mediator.Send(new CommandsMediatR.UpdateProductCurrentStockCommand { Id = product.Id, CurrentStock = 600 });


                //Finde Products By Name
                Console.WriteLine("Productos iPhones:");
                var productsByName = mediator.Send(new QueriesMediatR.GetProductsByNameQuery { Name = "iPhone" });
                foreach (var item in productsByName.Result)
                {
                    Console.WriteLine(item.ToString());
                }

                //Finde Products Out Of Stock
                Console.WriteLine("Productos sin Stock:");
                var outOfStockProducts = mediator.Send(new QueriesMediatR.FindOutOfStockProductsQuery());
                foreach (var item in outOfStockProducts.Result)
                {
                    Console.WriteLine(item.ToString());
                }

                //Delete Product
                mediator.Send(new CommandsMediatR.DeleteProductCommand { Id = product.Id });

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            
        }
       

        #region Private Builders
       
        //https://github.com/jbogard/MediatR/blob/master/samples/MediatR.Examples.AspNetCore/Program.cs
        private static IMediator BuildMediator()
        {
            
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationContextInMemoryDB>(opt => opt.UseInMemoryDatabase(databaseName:"CQRS-MediatR"), ServiceLifetime.Singleton);
            services.AddSingleton<IApplicationContextInMemoryDB>(p => p.GetService<ApplicationContextInMemoryDB>());
            services.AddMediatR(new Type[] { typeof(CommandsMediatR.AddNewProductCommand),
            typeof(CommandsMediatR.UpdateProductUnitPriceCommand),
            typeof(CommandsMediatR.UpdateProductCurrentStockCommand),
            typeof(QueriesMediatR.GetProductsByNameQuery),
            typeof(QueriesMediatR.FindOutOfStockProductsQuery)
            
            });


            var serilogLogger = new LoggerConfiguration()
                        .WriteTo .File($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}Logs{Path.DirectorySeparatorChar}application.log")
                        .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(logger: serilogLogger, dispose: true);
            });


            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IMediator>();
        }
        #endregion

        


    }
}
