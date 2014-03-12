﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SmartStore.Utilities;

namespace SmartStore.Core.Data
{
	
	public class SqlFileTokenizer
	{

		public SqlFileTokenizer(string fileName, Assembly assembly = null, string location = null)
		{
			Guard.ArgumentNotEmpty(() => fileName);

			this.FileName = fileName;
			this.Assembly = assembly;
			this.Location = location;
		}

		public string FileName 
		{ 
			get; 
			private set; 
		}

		public Assembly Assembly 
		{ 
			get; 
			private set; 
		}

		public string Location 
		{ 
			get; 
			private set; 
		}

		public IEnumerable<string> Tokenize()
		{
			if (this.Assembly == null)
			{
				this.Assembly = Assembly.GetCallingAssembly();
			}
			
			using (var reader = ReadSqlFile())
			{
				var statement = string.Empty;
				while ((statement = ReadNextSqlStatement(reader)) != null)
				{
					yield return statement;
				}
			}
		}

		protected virtual StreamReader ReadSqlFile()
		{

			var fileName = this.FileName;

			if (fileName.StartsWith("~") || fileName.StartsWith("/"))
			{
				string path = CommonHelper.MapPath(fileName);
				if (!File.Exists(path))
				{
					return StreamReader.Null;
				}

				return new StreamReader(File.OpenRead(path));
			}

			// SQL file is obviously an embedded resource
			var assembly = this.Assembly;
			var asmName = assembly.FullName.Substring(0, assembly.FullName.IndexOf(','));
			var location = this.Location ?? asmName + ".Sql";
			var name = String.Format("{0}.{1}", location, fileName);
			var stream = assembly.GetManifestResourceStream(name);
			Debug.Assert(stream != null);
			return new StreamReader(stream);
		}

		private string ReadNextSqlStatement(TextReader reader)
		{
			var sb = new StringBuilder();

			string lineOfText;

			while (true)
			{
				lineOfText = reader.ReadLine();
				if (lineOfText == null)
				{
					if (sb.Length > 0)
						return sb.ToString();
					else
						return null;
				}

				if (lineOfText.TrimEnd().ToUpper() == "GO")
					break;

				sb.Append(lineOfText + Environment.NewLine);
			}

			return sb.ToString();
		}

	}

}