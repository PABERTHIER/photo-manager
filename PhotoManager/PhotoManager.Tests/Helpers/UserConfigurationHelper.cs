﻿using PhotoManager.Infrastructure;

namespace PhotoManager.Tests.Helpers;

public class UserConfigurationHelper
{
    public static string GetApplicationBackUpTestsFolder()
    {
        return Constants.PathBackUpTests;
    }
}