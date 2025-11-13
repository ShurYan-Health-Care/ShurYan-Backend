using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Shared.Configurations
{
	public class CorsSettings
	{
		public string PolicyName { get; set; } = null!;
		public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
		public string[] AllowedMethods { get; set; } = Array.Empty<string>();
		public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
		public bool AllowCredentials { get; set; } = true;
	}
}
