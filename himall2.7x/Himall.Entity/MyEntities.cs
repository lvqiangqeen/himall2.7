using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Entity
{
	[DbConfigurationType(typeof(DbContextConfiguration))]
	public class MyEntities:Entities
	{
		static MyEntities()
		{
			EntityFramework.Container container = new EntityFramework.Container();
			EntityFramework.Locator.RegisterDefaults(container);
			container.Register<EntityFramework.Batch.IBatchRunner>(() => new EntityFramework.Batch.MySqlBatchRunner());
			EntityFramework.Locator.SetContainer(container);
		}

		protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("");
		}
	}

	public class DbContextConfiguration : MySql.Data.Entity.MySqlEFConfiguration
	{
		public DbContextConfiguration()
		{
			EntityFramework.Locator.Current.Register<EntityFramework.Batch.IBatchRunner>(() => new EntityFramework.Batch.MySqlBatchRunner());
		}
	}
}
