using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using MediatR;
using ServerMonitor.Helpers;

namespace ServerMonitor
{
    public static class Bootstrapper
    {
        public static void Run()
        {
            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerRequest();

            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            var mediatorOpenTypes = new[]
            {
                typeof(IRequestHandler<,>),
                typeof(INotificationHandler<>),
            };

            foreach (var mediatorOpenType in mediatorOpenTypes)
            {
                builder
                    .RegisterAssemblyTypes(typeof(BaseApi).GetTypeInfo().Assembly)
                    .AsClosedTypesOf(mediatorOpenType)
                    .AsImplementedInterfaces();
            }

            var container = builder.Build();
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}