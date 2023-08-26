using Autofac;

namespace PhotoManager.Tests;

public static class DatabaseServiceConfigurationMock
{
    public static void RegisterDatabaseTypes(this ContainerBuilder containerBuilder)
    {
        containerBuilder.RegisterType<ObjectListStorage>().As<IObjectListStorage>().SingleInstance();
        //containerBuilder.RegisterType<DataTableStorage>().As<IDataTableStorage>().SingleInstance();
        containerBuilder.RegisterType<BlobStorage>().As<IBlobStorage>().SingleInstance();
        containerBuilder.RegisterType<BackupStorage>().As<IBackupStorage>().SingleInstance();
        containerBuilder.RegisterType<Database>().As<IDatabase>().SingleInstance();
    }
}
