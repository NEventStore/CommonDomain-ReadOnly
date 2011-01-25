using System;
using CommonDomain.Core;
using Machine.Specifications;

namespace CommonDomain.AcceptanceTests
{
    [Subject("Routing Events")]
    public class When_dispatching_a_message_with_a_registered_aggregate_that_has_a_matching_apply_method :
        With_a_convention_event_router_and_registered_aggregate<object>
    {
        Because of = () =>
            router.Dispatch(new MessageA());

        It should_call_the_apply_method = () =>
            aggregate.ApplyForMessageACalled.ShouldBeTrue();
    }

    [Subject("Routing Events")]
    public class When_dispatching_a_message_with_a_registered_aggregate_that_has_a_matching_apply_method_that_is_private :
        With_a_convention_event_router_and_registered_aggregate<object>
    {
        Because of = () =>
            router.Dispatch(new MessageB());

        It should_call_the_apply_method = () =>
            aggregate.ApplyForMessageBCalled.ShouldBeTrue();
    }

    [Subject("Routing Events")]
    public class When_dispatching_a_message_with_a_registered_aggregate_that_has_a_matching_apply_method_that_is_static :
        With_a_convention_event_router_and_registered_aggregate<object>
    {
        Establish context = () =>
            MockAggregate.ApplyForMessageCCalled = false;

        Because of = () => router.Dispatch(new MessageC());

        It should_not_call_the_apply_method = () => MockAggregate.ApplyForMessageCCalled.ShouldBeFalse();
    }

    [Subject("Routing Events")]
    public class When_dispatching_a_message_with_a_registered_aggregate_that_has_a_matching_apply_method_with_multiple_parameters :
        With_a_convention_event_router_and_registered_aggregate<object>
    {
        Because of = () => 
            router.Dispatch(new MessageD());

        It should_not_call_the_apply_method = () => 
            aggregate.ApplyForMessageDCalled.ShouldBeFalse();
    }

    [Subject("Routing Events")]
    public class When_dispatching_a_message_with_a_registered_aggregate_that_has_a_matching_apply_method_that_does_not_return_void :
        With_a_convention_event_router_and_registered_aggregate<object>
    {
        Because of = () => 
            router.Dispatch(new MessageE());

        It should_not_call_the_apply_method = () => 
            aggregate.ApplyForMessageECalled.ShouldBeFalse();
    }

    [Subject("Routing Events")]
    public class When_dispatching_a_message_with_a_registered_aggregate_that_does_not_have_a_matching_apply_method :
        With_a_convention_event_router_and_registered_aggregate<object>
    {
        Establish context = () =>
            MockAggregate.ApplyForMessageCCalled = false;

        Because of = () => router.Dispatch(new MessageF());

        It should_not_call_any_apply_methods = () =>
        {
            aggregate.ApplyForMessageACalled.ShouldBeFalse();
            aggregate.ApplyForMessageBCalled.ShouldBeFalse();
            MockAggregate.ApplyForMessageCCalled.ShouldBeFalse();
        };
    }

    [Subject("Routing Events")]
    public class When_dispatching_a_message_with_a_registered_aggregate_that_has_a_matching_apply_method_and_a_manually_registered_handler :
        With_a_convention_event_router_and_registered_aggregate<object>
    {
        static bool manualHandlerCalled;

        Establish context = () => 
            router.Register<MessageA>(m => manualHandlerCalled = true);

        Because of = () =>
            router.Dispatch(new MessageA());

        It should_not_call_the_apply_method = () =>
            aggregate.ApplyForMessageACalled.ShouldBeFalse();

        It should_call_the_manual_handler = () =>
            aggregate.ApplyForMessageACalled.ShouldBeFalse();
    }
    
    public class With_a_convention_event_router_and_registered_aggregate<TEvent>
    {
        protected static ConventionEventRouter<TEvent> router;
        protected static MockAggregate aggregate;

        Establish context = () =>
        {
            router = new ConventionEventRouter<TEvent>();
            aggregate = new MockAggregate();
            router.Register(aggregate);
        };
    }

    public class MockAggregate : AggregateBase<object>
    {
        public bool ApplyForMessageACalled { get; set; }
        public bool ApplyForMessageBCalled { get; set; }
        public static bool ApplyForMessageCCalled { get; set; }
        public bool ApplyForMessageDCalled { get; set; }
        public bool ApplyForMessageECalled { get; set; }

        public void Apply(MessageA message)
        {
            ApplyForMessageACalled = true;
        }

        void Apply(MessageB message)
        {
            ApplyForMessageBCalled = true;
        }

        public static void Apply(MessageC message)
        {
            ApplyForMessageCCalled = true;
        }

        public void Apply(MessageD message, object otherParameter)
        {
            ApplyForMessageCCalled = true;
        }

        public int Apply(MessageE message)
        {
            ApplyForMessageCCalled = true;

            return 1;
        }
    }

    public class MessageA
    {
    }

    public class MessageB
    {
    }

    public class MessageC
    {
    }

    public class MessageD
    {
    }

    public class MessageE
    {
    }

    public class MessageF
    {
    }
}