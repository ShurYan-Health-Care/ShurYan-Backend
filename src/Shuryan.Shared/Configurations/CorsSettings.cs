using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Shared.Configurations
{
	public class CorsSettings
	{
		/// Policy Name
		public string PolicyName { get; set; } = null!;

		/// Domains allowed to call API
		public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

		/// Allowed HTTP Methods (GET, POST, etc.)
		public string[] AllowedMethods { get; set; } = Array.Empty<string>();

		/// Headers allowed in a Request
		public string[] AllowedHeaders { get; set; } = Array.Empty<string>();

		/// Do we allow Cookies and Authentication Headers?
		public bool AllowCredentials { get; set; } = true;
	}
}
