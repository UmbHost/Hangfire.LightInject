using LightInject;
using System;
using System.Web;
using System.IO;
using System.Linq;
using Hangfire;

public class LightInjectJobActivator : JobActivator
{
    private readonly ServiceContainer _container;
    internal static readonly object LifetimeScopeTag = new object();

    public LightInjectJobActivator(ServiceContainer container, bool selfReferencing = false)
    {
        if (container == null)
            throw new ArgumentNullException("container");

        this._container = container;

    }

    public override object ActivateJob(Type jobType)
    {
        // IMPORTANT: HACK to create fake http context for job to allow the LightInject PerWebRequestScopeManager to work correctly when running in background jobs
        // Umbraco is hardcoded to using MixedLightInjectScopeManagerProvider so its really really hard to get around so this hack is the easiest way to handle this.
        if (HttpContext.Current == null)
        {
            HttpContext.Current = new HttpContext(new HttpRequest("PerWebRequestScopeManager", "https://localhost/PerWebRequestScopeManager", string.Empty),
                new HttpResponse(new StringWriter()));
        }

        // this will fail if you do self referencing job queues on a class with an interface:
        //  BackgroundJob.Enqueue(() => this.SendSms(message)); 
        var instance = _container.TryGetInstance(jobType);

        // since it fails we can try to get the first interface and request from container
        if (instance == null && jobType.GetInterfaces().Count() > 0)
            instance = _container.GetInstance(jobType.GetInterfaces().FirstOrDefault());

        return instance;

    }

    public override JobActivatorScope BeginScope()
    {
        // IMPORTANT: HACK to create fake http context for job to allow the LightInject PerWebRequestScopeManager to work correctly when running in background jobs
        // Umbraco is hardcoded to using MixedLightInjectScopeManagerProvider so its really really hard to get around so this hack is the easiest way to handle this.
        if (HttpContext.Current == null)
        {
            HttpContext.Current = new HttpContext(new HttpRequest("PerWebRequestScopeManager", "https://localhost/PerWebRequestScopeManager", string.Empty),
                new HttpResponse(new StringWriter()));
        }

        return new LightInjecterScope(_container);
    }

}

class LightInjecterScope : JobActivatorScope
{
    private readonly ServiceContainer _container;
    private readonly Scope _scope;

    public LightInjecterScope(ServiceContainer container)
    {

        _container = container;

        _scope = _container.BeginScope();
    }

    public override object Resolve(Type jobType)
    { // IMPORTANT: HACK to create fake http context for job to allow the LightInject PerWebRequestScopeManager to work correctly when running in background jobs
        // Umbraco is hardcoded to using MixedLightInjectScopeManagerProvider so its really really hard to get around so this hack is the easiest way to handle this.
        if (HttpContext.Current == null)
        {
            HttpContext.Current = new HttpContext(new HttpRequest("PerWebRequestScopeManager", "https://localhost/PerWebRequestScopeManager", string.Empty),
                new HttpResponse(new StringWriter()));
        }

        var instance = _container.TryGetInstance(jobType);

        // since it fails we can try to get the first interface and request from container
        if (instance == null && jobType.GetInterfaces().Count() > 0)
            instance = _container.GetInstance(jobType.GetInterfaces().FirstOrDefault());

        return instance;

    }

    public override void DisposeScope()
    {
        // IMPORTANT: HACK to create fake http context for job to allow the LightInject PerWebRequestScopeManager to work correctly when running in background jobs
        // Umbraco is hardcoded to using MixedLightInjectScopeManagerProvider so its really really hard to get around so this hack is the easiest way to handle this.
        if (HttpContext.Current == null)
        {
            HttpContext.Current = new HttpContext(new HttpRequest("PerWebRequestScopeManager", "https://localhost/PerWebRequestScopeManager", string.Empty),
                new HttpResponse(new StringWriter()));
        }

        _scope?.Dispose();
    }
}