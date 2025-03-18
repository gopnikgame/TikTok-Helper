using MediatR;

using NLog;
using NLog.Targets;

using TTStreamer.WPF.Notify;

namespace TTStreamer.WPF.Logging
{
    [Target("UITarget")]
    public sealed class UITarget : TargetWithLayout  //or inherit from Target
    {
        protected override async void Write(LogEventInfo logEvent)
        {
            var msg = Layout.Render(logEvent);
            var mediator = App.GetService<IMediator>(); //было бы неплохо найти способ внедрения через конструктор
            await mediator.Publish(new LogNotify(logEvent, msg));
        }
    }

}