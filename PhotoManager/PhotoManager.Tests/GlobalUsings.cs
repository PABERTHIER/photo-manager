global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging.Testing;
global using NSubstitute;
global using NSubstitute.ExceptionExtensions;
global using NUnit.Framework;
// Projects
global using PhotoManager.Common;
global using PhotoManager.Common.Imaging;
global using PhotoManager.Domain;
global using PhotoManager.Domain.Delegates;
global using PhotoManager.Domain.Entities;
global using PhotoManager.Domain.Enums;
global using PhotoManager.Domain.Extensions;
global using PhotoManager.Domain.Interfaces;
global using PhotoManager.Domain.Models;
global using PhotoManager.Domain.Pipelines;
global using PhotoManager.Domain.Services;
global using PhotoManager.Domain.UserConfigurationSettings;
global using PhotoManager.Domain.ValueObjects;
global using PhotoManager.Infrastructure;
global using PhotoManager.Persistence.Sqlite;
global using PhotoManager.Tests.Integration.Infrastructure.AssetRepositoryTests;
global using PhotoManager.UI.Converters;
global using PhotoManager.UI.ViewModels;
// System
global using System.IO;
