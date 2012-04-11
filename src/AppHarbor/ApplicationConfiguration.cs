﻿using System;
using System.IO;
using AppHarbor.Model;

namespace AppHarbor
{
	public class ApplicationConfiguration : IApplicationConfiguration
	{
		private readonly IFileSystem _fileSystem;
		private readonly IGitRepositoryConfigurer _repositoryConfigurer;
		private readonly TextWriter _writer;

		public ApplicationConfiguration(IFileSystem fileSystem, IGitRepositoryConfigurer repositoryConfigurer, TextWriter writer)
		{
			_fileSystem = fileSystem;
			_repositoryConfigurer = repositoryConfigurer;
			_writer = writer;
		}

		public string GetApplicationId()
		{
			try
			{
				using (var stream = _fileSystem.OpenRead(ConfigurationFile.FullName))
				{
					using (var reader = new StreamReader(stream))
					{
						return reader.ReadToEnd();
					}
				}
			}
			catch (FileNotFoundException)
			{
			}

			try
			{
				return _repositoryConfigurer.GetApplicationId();
			}
			catch (RepositoryConfigurationException)
			{
			}

			throw new ApplicationConfigurationException("Application is not configured");
		}

		public virtual void SetupApplication(string id, User user)
		{
			try
			{
				_repositoryConfigurer.Configure(id, user);
				return;
			}
			catch (RepositoryConfigurationException exception)
			{
				_writer.WriteLine(exception.Message);
			}

			using (var stream = _fileSystem.OpenWrite(ConfigurationFile.FullName))
			{
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(id);
				}
			}

			_writer.WriteLine("Wrote application configuration to {0}", ConfigurationFile.FullName);
		}

		private static DirectoryInfo CurrentDirectory
		{
			get
			{
				return new DirectoryInfo(Directory.GetCurrentDirectory());
			}
		}

		private static FileInfo ConfigurationFile
		{
			get
			{
				var directory = Directory.GetCurrentDirectory();
				var appharborConfigurationFile = new FileInfo(Path.Combine(directory, ".appharbor"));
				return appharborConfigurationFile;
			}
		}

		public void DeleteApplication()
		{
			_repositoryConfigurer.Unconfigure();
		}
	}
}
