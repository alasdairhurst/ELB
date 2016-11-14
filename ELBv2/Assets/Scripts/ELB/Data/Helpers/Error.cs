using System;
using System.Runtime.Serialization;

namespace ELB.Data.Helpers {
	[Serializable]
	class Error : Exception {
		public Error() {
		}

		public Error(string message) : base(message) {
		}

		public Error(string message, Exception innerException) : base(message, innerException) {
		}

		protected Error(SerializationInfo info, StreamingContext context) : base(info, context) {
		}
	}
}