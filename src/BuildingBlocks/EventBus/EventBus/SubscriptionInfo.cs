using System;

namespace OPMMetamodel.BuildingBlocks.EventBus
{
    public class SubscriptionInfo
    {
        public bool IsDynamic { get; }
        
        public Type HandlerType { get; }
        
        private SubscriptionInfo(bool isDynamic, Type handlerType)
        {
            IsDynamic = isDynamic;
            HandlerType = handlerType;
        }

        public static SubscriptionInfo Typed(Type handlerType)
        {
            return new SubscriptionInfo(false, handlerType);
        }

    }
}