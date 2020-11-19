using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;


namespace JSONval {

	internal class JsonValidator {
		private readonly string _jsonFile;
		private string _schemaFile;
		private const string DummySchema = @"{'$schema' : 'https://json-schema.org/draft/2019-09/schema'}";

		private JSchema jschema;
		private JObject jobject;
		private StreamReader streamreader;
		private JsonTextReader textreader;


		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="args"></param>
		public JsonValidator(IReadOnlyList<string> args) {
			_jsonFile = args[0];
			if (args.Count > 1)
				_schemaFile = args[1];	// ignore the rest, if any
		}


		/// <summary>
		/// dtor
		/// </summary>
		~JsonValidator() {
			textreader?.Close();
		}


		/// <summary>
		/// get JSON schema
		/// </summary>
		private JSchema JsonSchema {
			get {
				if (jschema == null) {
					if (_schemaFile == null)
						_schemaFile = DummySchema;

					using (streamreader = File.OpenText(_schemaFile))
						try {
							textreader = new JsonTextReader(streamreader) { 
								CloseInput = true 
							};
							jschema = JSchema.Load(textreader);
						}
						catch (JsonReaderException error) { 
							Console.WriteLine(error); 
						}
				}
				return jschema;
			}
		}


		/// <summary>
		/// get JSON file
		/// </summary>
		private JObject JsonFile {
			get {
				if (jobject == null)
					using (streamreader = File.OpenText(_jsonFile))
						try {
							textreader = new JsonTextReader(streamreader) { 
								CloseInput = true 
							};
							jobject = JToken.ReadFrom(textreader) as JObject;
						}
						catch (JsonReaderException error) {
							Console.WriteLine(error);
						}
				return jobject;
			}
		}


		/// <summary>
		/// validate JSON file against JSON schema
		/// </summary>
		public bool JsonFileIsValid {
			get {
				try {
					bool valid = JsonFile.IsValid(JsonSchema, out IList<ValidationError> messages);
					Messages = messages;
					return valid;
				}
				catch (Exception error) {
					Console.WriteLine($"{error}");
				}
				return false;
			}
		}


		public IList<ValidationError> Messages {
			get; 
			private set;
		}
	}


	public static class Start {
		/// <summary>
		/// args[0] = .json file (mandatory)
		/// args[1] = schema file (optional)
		/// </summary>
		/// <param name="args"></param>
		private static void Main(string[] args) {
			if (args.Length == 0) {
				Console.WriteLine("No .json file available!");
			}
			else {
				JsonValidator jsonVal = new JsonValidator(args);
				Console.WriteLine("Validation results:");
				Console.WriteLine("Validation was {0}.", jsonVal.JsonFileIsValid.ToString().ToLower());
				Console.WriteLine(jsonVal.Messages != null ? jsonVal.Messages.ToString() : "No schema errors.");
			}
		}
	}
}