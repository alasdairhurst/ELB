using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ELB.Data {
	
	class ELBDataService : Engine.Data.DataService {
		private const string dbFile = "db.s3db";

		public ELBDataService() : base(dbFile) {}

	};
}
