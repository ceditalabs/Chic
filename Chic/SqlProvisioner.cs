using Chic.Abstractions;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Chic
{
    public class SqlProvisioner : IDatabaseProvisioner
    {
        private readonly IServiceProvider services;
        private readonly Queue<string> sqlSteps;

        public SqlProvisioner(IServiceProvider services)
        {
            this.services = services;

            sqlSteps = new Queue<string>();
        }

        public void AddStep(object step)
        {
            if (step == null)
            {
                throw new ArgumentNullException(nameof(step));
            }
            if (!(step is string))
            {
                throw new ArgumentException("step is not a SQL string.", nameof(step));
            }

            var sqlStatement = step as string;
            sqlSteps.Enqueue(sqlStatement);
        }

        public void AddStepFromFile(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            var fileExists = File.Exists(fileName);
            if (!fileExists)
            {
                throw new IOException($"File {fileName} does not exist or is not readable.");
            }
            var sqlStatement = File.ReadAllText(fileName);
            AddStep(sqlStatement);
        }

        /// <summary>
        /// Add SQL files for provisioning where name is *.provision.sql
        /// </summary>
        /// <param name="assembly"></param>
        public void AddStepsFromAssemblyResources(Assembly assembly)
        {
            var sqlResources = assembly.GetManifestResourceNames().Where(m => m.Contains(".provision.sql")).OrderBy(m => m);
            foreach (var sqlResource in sqlResources)
            {
                var stream = assembly.GetManifestResourceStream(sqlResource);
                var sqlStatement = new StreamReader(stream).ReadToEnd();
                AddStep(sqlStatement);
            }
        }

        public bool Provision()
        {
            using (var scope = services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<IDbConnection>();
                try
                {
                    try
                    {
                        db.Open();
                    }
                    catch (SqlException)
                    {
                        return false;
                    }

                    do
                    {
                        var sql = sqlSteps.Dequeue();
                        try
                        {
                            db.Execute(sql);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Could not Provision database, caught exception whilst executing SQL: {sql}", ex);
                        }
                    } while (sqlSteps.Count > 0);
                }
                finally
                {
                    db?.Close();
                    db?.Dispose();
                }

                return true;
            }
        }
    }
}
