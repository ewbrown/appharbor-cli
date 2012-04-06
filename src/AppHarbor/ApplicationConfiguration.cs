﻿using System;
using System.IO;
using System.Linq;
using AppHarbor.Model;

namespace AppHarbor
{
	public class ApplicationConfiguration : IApplicationConfiguration
	{
		private readonly IFileSystem _fileSystem;
		private readonly IGitExecutor _gitExecutor;
		private readonly IGitRepositoryConfigurer _repositoryConfigurer;

		public ApplicationConfiguration(IFileSystem fileSystem, IGitExecutor gitExecutor, IGitRepositoryConfigurer repositoryConfigurer)
		{
			_fileSystem = fileSystem;
			_gitExecutor = gitExecutor;
			_repositoryConfigurer = repositoryConfigurer;
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

			var url = _gitExecutor.Execute("config remote.appharbor.url", CurrentDirectory).FirstOrDefault();
			if (url != null)
			{
				return url.Split(new string[] { "/", ".git" }, StringSplitOptions.RemoveEmptyEntries).Last();
			}

			throw new ApplicationConfigurationException("Application is not configured");
		}

		public virtual void SetupApplication(string id, User user)
		{
			var repositoryUrl = string.Format("https://{0}@appharbor.com/{1}.git", user.Username, id);

			try
			{
				_gitExecutor.Execute(string.Format("remote add appharbor {0}", repositoryUrl),
					CurrentDirectory);

				Console.WriteLine("Added \"appharbor\" as a remote repository. Push to AppHarbor with git push appharbor master");
				return;
			}
			catch (InvalidOperationException)
			{
				Console.WriteLine("Couldn't add appharbor repository as a git remote. Repository URL is: {0}", repositoryUrl);
			}

			using (var stream = _fileSystem.OpenWrite(ConfigurationFile.FullName))
			{
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(id);
				}
			}

			Console.WriteLine("Wrote application configuration to {0}", ConfigurationFile);
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
	}
}
