using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
// ReSharper disable All

namespace SIBAS_PN
{
	class JsonValidator {
		private readonly string _jsonFile;
		private readonly string _schemaFile;
		private const string _DummySchema = @"{'$schema' : 'https://json-schema.org/draft/2019-09/schema'}";
		
		private JSchema jschema = null;
		private JObject jobject = null;

		private IList<ValidationError> messages;
		
		private StreamReader streamreader;
		private JsonTextReader textreader = null;

		public JsonValidator(string[] args) {
			_jsonFile = args[0];
			if (args.Length == 2)
				_schemaFile = args[1];
		}

		~JsonValidator() {
			textreader?.Close();
		}

		private JSchema SibasPnSchema {
			get {
				if (jschema == null) {
					// use given schema
					if (_schemaFile != null) {
						using (streamreader = File.OpenText(_schemaFile))
							try {
								textreader = new JsonTextReader(streamreader) { 
									CloseInput = true 
								};
								jschema = JSchema.Load(textreader = new JsonTextReader(streamreader));
							}
							catch (JsonReaderException error) {
								Console.WriteLine(error);
							}
					}
					// use default (build-in) schema
					else {
						jschema = JSchema.Parse(_DummySchema);
					}
				}
				return jschema;
			}
		}

		private JObject SibasPNJsonfile {
			get {
				if (jobject == null) {
					using (streamreader = File.OpenText(_jsonFile))
						try {
							textreader = new JsonTextReader(streamreader) { 
								CloseInput = true 
							};
							jobject = JToken.ReadFrom(textreader = new JsonTextReader(streamreader)) as JObject;
						}
						catch (JsonReaderException error) {
							Console.WriteLine(error);
						}
				}
				return jobject;
			}
		}

		public bool SchemaValid {
			get {
				try {
					return SibasPNJsonfile.IsValid(SibasPnSchema, out messages);
				}
				catch (Exception error) {
					Console.WriteLine($"{error}");
				}
				return false;
			}
		}

		public IList<ValidationError> Messages { 
			get => messages; 
			set => messages = value; 
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
				JsonValidator jsonVal = new JsonValidator(args);
				Console.WriteLine("Validation results:");
				Console.WriteLine("Validation was {0}.", jsonVal.SchemaValid.ToString().ToLower());
				Console.WriteLine(jsonVal.Messages == null ? jsonVal.Messages.ToString() : "No schema errors.");
			}
		}
	}
}
