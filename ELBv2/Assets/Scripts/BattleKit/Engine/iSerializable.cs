using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleKit.Engine {
	interface iSerializable {
		JToken Serialize();
	}
}
