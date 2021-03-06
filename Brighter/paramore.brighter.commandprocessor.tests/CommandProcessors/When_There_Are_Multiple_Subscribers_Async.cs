﻿#region Licence
/* The MIT License (MIT)
Copyright © 2014 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System;
using FakeItEasy;
using Machine.Specifications;
using Nito.AsyncEx;
using paramore.brighter.commandprocessor;
using paramore.brighter.commandprocessor.Logging;
using paramore.commandprocessor.tests.CommandProcessors.TestDoubles;
using TinyIoC;

namespace paramore.commandprocessor.tests.CommandProcessors
{
    [Subject(typeof(CommandProcessor))]
    public class When_There_Are_Multiple_Subscribers_Async
    {
        private static CommandProcessor s_commandProcessor;
        private static readonly MyEvent s_myEvent = new MyEvent();
        private static Exception s_exception;

        private Establish _context = () =>
        {
            var logger = A.Fake<ILog>();

            var registry = new SubscriberRegistry();
            registry.RegisterAsync<MyEvent, MyEventHandlerAsync>();
            registry.RegisterAsync<MyEvent, MyOtherEventHandlerAsync>();

            var container = new TinyIoCContainer();
            var handlerFactory = new TinyIocHandlerFactoryAsync(container);
            container.Register<IHandleRequestsAsync<MyEvent>, MyEventHandlerAsync>("MyEventHandlerAsync");
            container.Register<IHandleRequestsAsync<MyEvent>, MyOtherEventHandlerAsync>("MyOtherHandlerAsync");
            container.Register<ILog>(logger);

            s_commandProcessor = new CommandProcessor(registry, handlerFactory, new InMemoryRequestContextFactory(), new PolicyRegistry(), logger);
        };

        //Ignore any errors about adding System.Runtime from the IDE. See https://social.msdn.microsoft.com/Forums/en-US/af4dc0db-046c-4728-bfe0-60ceb93f7b9f/vs2012net-45-rc-compiler-error-when-using-actionblock-missing-reference-to?forum=tpldataflow
        private Because _of = () => s_exception = Catch.Exception(() => AsyncContext.Run(async () => await s_commandProcessor.PublishAsync(s_myEvent)));

        private It _should_not_throw_an_exception = () => s_exception.ShouldBeNull();
        private It _should_publish_the_command_to_the_first_event_handler = () => MyEventHandlerAsync.ShouldReceive(s_myEvent).ShouldBeTrue();
        private It _should_publish_the_command_to_the_second_event_handler = () => MyOtherEventHandlerAsync.ShouldReceive(s_myEvent).ShouldBeTrue();
    }
}
