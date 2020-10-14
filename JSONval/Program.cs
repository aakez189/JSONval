using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace SIBAS_PN
{
	class JSONValidator {
		private readonly string jsonFile;
		private readonly string schemaFile;
		private const string basicSchema = @"{'$schema' : 'https://json-schema.org/draft/2019-09/schema'}";
		private JSchema jschema = null;
		private JObject jobject = null;
		private IList<string> messages;
		private StreamReader streamreader;
		private JsonTextReader textreader = null;

		public JSONValidator(string[] args) {
			jsonFile = args[0];
			if (args.Length == 2)
				schemaFile = args[1];
		}

		~JSONValidator() {
			if (textreader != null)
				textreader.Close();
		}

		public JSchema SibasPN_Schema {
			get {
				if (jschema == null) {
					// use given schema
					if (schemaFile != null) {
						using (streamreader = File.OpenText(schemaFile))
							try {
								textreader = new JsonTextReader(streamreader) { CloseInput = true };
								jschema = JSchema.Load(textreader = new JsonTextReader(streamreader));
							}
							catch (JsonReaderException error) {
								Console.WriteLine(error);
								Console.WriteLine(messages);
							}
					}
					// use default (build-in) schema
					else {
						jschema = JSchema.Parse(basicSchema);
					}
				}
				return jschema;
			}
		}

		public JObject SibasPN_JSONfile {
			get {
				if (jobject == null) {
					using (streamreader = File.OpenText(jsonFile)) {
						textreader = new JsonTextReader(streamreader) { CloseInput = true };
						jobject = JToken.ReadFrom(textreader = new JsonTextReader(streamreader)) as JObject;
					}
				}
				return jobject;
			}
		}

		public bool SchemaValid {
			get {
				try {
					return SibasPN_JSONfile.IsValid(SibasPN_Schema, out messages);
				}
				catch (Exception error) {
					if (schemaFile == null) {
						Console.WriteLine("Schema file is missing!");
						Console.WriteLine($"{error}");
					}
					return false;
				}
			}
		}
	}

	class Start {
		/// <summary>
		/// args[0] = .json file (mandatory)
		/// args[1] = schema file (optional)
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			if (args.Length == 0) {
				Console.WriteLine("No .json file available!");
				return;
			}
			else {
				JSONValidator jsonVal = new JSONValidator(args);
				Console.WriteLine("Validation result:");
				Console.WriteLine(jsonVal.SchemaValid);
			}
		}
	}
}
