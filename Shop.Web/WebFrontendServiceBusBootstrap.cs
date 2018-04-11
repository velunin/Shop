using System;
using System.Collections.Generic;
using System.Linq;

using Rds.CaraBus.RequestResponse;

using Shop.Domain.Commands;
using Shop.Domain.Commands.Cart;
using Shop.Domain.Commands.Order;
using Shop.Domain.Sagas;
using Shop.Infrastructure;
using Shop.Infrastructure.SubscriptionConfigurators;

namespace Shop.Web
{
    internal class WebFrontendServiceBusBootstrap : AppServiceBusBootstrapBase
    {
        public WebFrontendServiceBusBootstrap(
            ICorrelationValueResolver correlationValueResolver, 
            Func<SubscribersType, ISubscriptionConfigurator> subscriptionConfiguratorFactory) 
            : base(correlationValueResolver, subscriptionConfiguratorFactory)
        {
        }

        protected override IEnumerable<Type> AllowedCommands
        {
            get
            {
                yield return typeof(AddOrUpdateProductInCart);
                yield return typeof(CreateOrderCommand);
            }
        }
        protected override IEnumerable<Type> AllowedSagas
        {
            get
            {
                yield return typeof(OrderSaga);
            }
        }
        protected override IEnumerable<Type> AllowedEvents => Enumerable.Empty<Type>();

    }
}