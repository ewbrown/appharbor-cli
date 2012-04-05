﻿using System;
using System.IO;
using System.Linq;
using AppHarbor.Commands;
using AppHarbor.Model;
using Moq;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;

namespace AppHarbor.Tests.Commands
{
	public class AppCreateCommandTest
	{
		[Theory, AutoCommandData]
		public void ShouldThrowWhenNoArguments(AppCreateCommand command)
		{
			var exception = Assert.Throws<CommandException>(() => command.Execute(new string[0]));
			Assert.Equal("An application name must be provided to create an application", exception.Message);
		}

		[Theory, AutoCommandData]
		public void ShouldCreateApplicationWithOnlyName([Frozen]Mock<IAppHarborClient> client, AppCreateCommand command)
		{
			var arguments = new string[] { "foo" };
			VerifyArguments(client, command, arguments);
		}

		[Theory, AutoCommandData]
		public void ShouldCreateApplicationWithRegion([Frozen]Mock<IAppHarborClient> client, AppCreateCommand command, string[] arguments)
		{
			VerifyArguments(client, command, arguments);
		}

		private static void VerifyArguments(Mock<IAppHarborClient> client, AppCreateCommand command, string[] arguments)
		{
			command.Execute(arguments);
			client.Verify(x => x.CreateApplication(arguments.First(), arguments.Skip(1).FirstOrDefault()), Times.Once());
		}

		[Theory, AutoCommandData]
		public void ShouldSetupApplicationLocallyAfterCreation([Frozen]Mock<IApplicationConfiguration> applicationConfiguration, [Frozen]Mock<IAppHarborClient> client, AppCreateCommand command, CreateResult<string> result, User user, string[] arguments)
		{
			client.Setup(x => x.CreateApplication(It.IsAny<string>(), It.IsAny<string>())).Returns(result);
			client.Setup(x => x.GetUser()).Returns(user);

			command.Execute(arguments);
			applicationConfiguration.Verify(x => x.SetupApplication(result.ID, user), Times.Once());
		}

		[Theory, AutoCommandData]
		public void ShouldPrintSuccessMessageAfterCreatingApplication([Frozen]Mock<IAppHarborClient> client, Mock<AppCreateCommand> command, string[] arguments, string applicationSlug)
		{
			client.Setup(x => x.CreateApplication(arguments[0], arguments[1])).Returns(new CreateResult<string> { ID = applicationSlug });
			using (var writer = new StringWriter())
			{
				Console.SetOut(writer);
				command.Object.Execute(arguments);

				Assert.Contains(string.Format(string.Concat("Created application \"{0}\" | URL: https://{0}.apphb.com", Environment.NewLine), applicationSlug), writer.ToString());
			}
		}
	}
}