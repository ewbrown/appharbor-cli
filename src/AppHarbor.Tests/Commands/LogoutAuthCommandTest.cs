﻿using System.IO;
using AppHarbor.Commands;
using Moq;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace AppHarbor.Tests.Commands
{
	public class LogoutAuthCommandTest
	{
		[Theory, AutoCommandData]
		public void ShouldLogoutUser([Frozen]Mock<IAccessTokenConfiguration> accessTokenConfigurationMock, [Frozen]Mock<TextWriter> writer, LogoutAuthCommand logoutCommand)
		{
			logoutCommand.Execute(new string[0]);

			writer.Verify(x => x.WriteLine("Successfully logged out."));
			accessTokenConfigurationMock.Verify(x => x.DeleteAccessToken(), Times.Once());
		}
	}
}
